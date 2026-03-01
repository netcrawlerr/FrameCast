using System;
using System.Threading.Tasks;
using FrameCast.Capture.Windows;
using FrameCast.Encoding;
using FrameCast.Protocol;
using FrameCast.Transport;

class Program
{
    static async Task Main()
    {
        int port = 5000;

        //   TCP Server

        var server = new TcpFrameServer(port);
        server.FrameReceived += frame =>
        {
            Console.WriteLine($"Server: Frame {frame.Timestamp} received, broadcasting...");
        };
        _ = Task.Run(() => server.StartAsync());

        //  Capture Client

        var client = new TcpFrameClient("127.0.0.1", port);
        await client.StartAsync();

        var captureService = new WindowsScreenCaptureService();
        var encoder = new JpegFrameEncoder();

        Console.WriteLine("Streaming at ~30 FPS...");
        Console.WriteLine("Run FrameCast.App to view the stream.");
        Console.WriteLine("Press any key to stop.\n");

        bool running = true;

        var streamingTask = Task.Run(async () =>
        {
            var frameDelay = TimeSpan.FromMilliseconds(33); // ~30 FPS
            int frameCount = 0;

            while (running)
            {
                try
                {
                    var bmp = captureService.CaptureFullScreen();
                    var jpeg = encoder.Encode(bmp);

                    var frame = new FrameMessage
                    {
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Data = jpeg,
                    };

                    await client.SendFrameAsync(frame);

                    frameCount++;
                    Console.WriteLine($"Client: Sent frame {frameCount}");

                    await Task.Delay(frameDelay);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Streaming error: {ex.Message}");
                }
            }
        });

        Console.ReadKey();
        running = false;

        await streamingTask;

        await client.StopAsync();
        await server.StopAsync();

        Console.WriteLine("Streaming stopped.");
    }
}
