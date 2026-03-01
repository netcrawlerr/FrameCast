using System;
using System.Net.Sockets;
using FrameCast.Protocol;

namespace FrameCast.Transport;

public class TcpFrameClient : IFrameTransport
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _running;

    public event Action<FrameMessage>? FrameReceived;

    public TcpFrameClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task StartAsync()
    {
        _client = new TcpClient();
        await _client.ConnectAsync(_host, _port);
        _stream = _client.GetStream();
        _running = true;

        Console.WriteLine($"Connected to server {_host}:{_port}");

        _ = Task.Run(ReceiveLoop);
    }

    private async Task ReceiveLoop()
    {
        if (_stream == null)
            return;

        var headerBuffer = new byte[12];

        while (_running)
        {
            try
            {
                int headerRead = 0;
                while (headerRead < 12)
                {
                    int n = await _stream.ReadAsync(headerBuffer, headerRead, 12 - headerRead);
                    if (n == 0)
                        return;
                    headerRead += n;
                }

                int dataLen = BitConverter.ToInt32(headerBuffer, 0);
                long ts = BitConverter.ToInt64(headerBuffer, 4);

                var data = new byte[dataLen];
                int read = 0;
                while (read < dataLen)
                {
                    int n = await _stream.ReadAsync(data, read, dataLen - read);
                    if (n == 0)
                        return;
                    read += n;
                }

                var frame = new FrameMessage { Timestamp = ts, Data = data };
                FrameReceived?.Invoke(frame);
            }
            catch
            {
                break;
            }
        }
    }

    public Task SendFrameAsync(FrameMessage frame)
    {
        if (_stream == null)
            throw new InvalidOperationException("Client not started");

        var bytes = frame.ToBytes();
        return _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public Task StopAsync()
    {
        _running = false;
        _stream?.Close();
        _client?.Close();
        return Task.CompletedTask;
    }
}
