using FrameCast.Capture.Windows;
using FrameCast.Encoding;
using FrameCast.Protocol;
using FrameCast.Transport;

class Program
{
    static async Task Main()
    {
        int port = 5000;

        var server = new TcpFrameServer(port);
        server.FrameReceived += frame =>
        {
            var filename = $"received_{frame.Timestamp}.jpg";
            System.IO.File.WriteAllBytes(filename, frame.Data);
            Console.WriteLine($"Server: Saved frame {filename}");
        };

        _ = server.StartAsync();

        var client = new TcpFrameClient("127.0.0.1", port);
        await client.StartAsync();

        var captureService = new WindowsScreenCaptureService();
        var encoder = new JpegFrameEncoder();

        for (int i = 0; i < 5; i++)
        {
            var bmp = captureService.CaptureFullScreen();

            var jpeg = encoder.Encode(bmp);

            var frame = new FrameMessage
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Data = jpeg,
            };

            await client.SendFrameAsync(frame);
            Console.WriteLine($"Client: Sent frame {i + 1}");

            await Task.Delay(1000);
        }

        await client.StopAsync();
        await server.StopAsync();

        Console.WriteLine("Demo complete. Press any key to exit.");
        Console.ReadKey();
    }
}
