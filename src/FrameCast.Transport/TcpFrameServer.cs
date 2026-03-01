using System;
using System.Net;
using System.Net.Sockets;
using FrameCast.Protocol;

namespace FrameCast.Transport;

public class TcpFrameServer : IFrameTransport
{
    private readonly int _port;
    private readonly string? _ipAddress;
    private TcpListener? _listener;
    private bool _running = false;
    private readonly List<NetworkStream> _clientStreams = new();

    public event Action<FrameMessage>? FrameReceived;

    // accept optional IP
    public TcpFrameServer(int port, string? ipAddress = null)
    {
        _port = port;
        _ipAddress = ipAddress;
    }

    public async Task StartAsync()
    {
        IPAddress bindIp = IPAddress.Any;
        if (
            !string.IsNullOrWhiteSpace(_ipAddress)
            && IPAddress.TryParse(_ipAddress, out var parsedIp)
        )
        {
            bindIp = parsedIp;
        }

        _listener = new TcpListener(bindIp, _port);
        _listener.Start();
        _running = true;

        Console.WriteLine($"Server started on {bindIp}:{_port}");

        while (_running)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                var stream = client.GetStream();
                lock (_clientStreams)
                    _clientStreams.Add(stream);

                Console.WriteLine(
                    $"New client connected: {((IPEndPoint)client.Client.RemoteEndPoint!).Address}"
                );

                _ = Task.Run(() => HandleClientAsync(client, stream));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, NetworkStream stream)
    {
        var buffer = new byte[12];

        while (_running && client.Connected)
        {
            try
            {
                int headerRead = 0;
                while (headerRead < 12)
                {
                    int n = await stream.ReadAsync(buffer, headerRead, 12 - headerRead);
                    if (n == 0)
                        break;
                    headerRead += n;
                }
                if (headerRead < 12)
                    break;

                int dataLen = BitConverter.ToInt32(buffer, 0);
                long ts = BitConverter.ToInt64(buffer, 4);

                var data = new byte[dataLen];
                int read = 0;
                while (read < dataLen)
                {
                    int n = await stream.ReadAsync(data, read, dataLen - read);
                    if (n == 0)
                        break;
                    read += n;
                }
                if (read < dataLen)
                    break;

                var frame = new FrameMessage { Timestamp = ts, Data = data };
                FrameReceived?.Invoke(frame);

                // Broadcast to all clients
                var bytes = frame.ToBytes();
                lock (_clientStreams)
                {
                    foreach (var s in _clientStreams.ToArray())
                    {
                        if (!s.CanWrite)
                            continue;
                        try
                        {
                            s.Write(bytes, 0, bytes.Length);
                        }
                        catch
                        {
                            _clientStreams.Remove(s);
                        }
                    }
                }
            }
            catch
            {
                break;
            }
        }

        lock (_clientStreams)
            _clientStreams.Remove(stream);
        stream.Close();
        client.Close();
        Console.WriteLine("Client disconnected.");
    }

    public Task SendFrameAsync(FrameMessage frame)
    {
        throw new NotImplementedException("Server does not send frames directly");
    }

    public Task StopAsync()
    {
        _running = false;
        _listener?.Stop();
        lock (_clientStreams)
        {
            foreach (var s in _clientStreams)
                s.Close();
            _clientStreams.Clear();
        }
        return Task.CompletedTask;
    }
}
