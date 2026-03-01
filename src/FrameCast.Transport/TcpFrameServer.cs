using System.Net;
using System.Net.Sockets;
using FrameCast.Protocol;

namespace FrameCast.Transport;

public class TcpFrameServer : IFrameTransport
{
    private readonly int _port;
    private TcpListener? _listener;
    private bool _running = false;

    public event Action<FrameMessage>? FrameReceived;

    public TcpFrameServer(int port)
    {
        _port = port;
    }

    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _running = true;
        Console.WriteLine($"Server started on port {_port}");

        while (_running)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }

    public Task StopAsync()
    {
        _running = false;
        _listener?.Stop();
        return Task.CompletedTask;
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        var buffer = new byte[4096];

        while (_running && client.Connected)
        {
            try
            {
                // Read the first 12 bytes: DataLength (4) + Timestamp (8)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client handler error: {ex.Message}");
                break;
            }
        }
    }

    public Task SendFrameAsync(FrameMessage frame)
    {
        throw new NotImplementedException("Server does not send frames");
    }
}
