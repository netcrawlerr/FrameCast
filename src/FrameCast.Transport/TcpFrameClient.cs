using System.Net.Sockets;
using FrameCast.Protocol;

namespace FrameCast.Transport;

public class TcpFrameClient : IFrameTransport
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;

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
        Console.WriteLine($"Connected to server {_host}:{_port}");
    }

    public Task StopAsync()
    {
        _stream?.Close();
        _client?.Close();
        return Task.CompletedTask;
    }

    public async Task SendFrameAsync(FrameMessage frame)
    {
        if (_stream == null)
            throw new InvalidOperationException("Client not started");

        var bytes = frame.ToBytes();
        await _stream.WriteAsync(bytes, 0, bytes.Length);
        await _stream.FlushAsync();
    }
}
