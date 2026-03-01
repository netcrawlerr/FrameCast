using FrameCast.Protocol;

namespace FrameCast.Transport;

public interface IFrameTransport
{
    Task SendFrameAsync(FrameMessage frame);
    Task StartAsync();
    Task StopAsync();
}