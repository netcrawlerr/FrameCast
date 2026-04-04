using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

        // IP for  streaming
        string serverIp = GetStreamingIpAddress() ?? "127.0.0.1";
        Console.WriteLine($"Server IP: {serverIp}");

        // TCP Server
        var server = new TcpFrameServer(port, serverIp);
        _ = Task.Run(() => server.StartAsync());

        // Capture client
        var client = new TcpFrameClient(serverIp, port);
        await client.StartAsync();

        var captureService = new WindowsScreenCaptureService();
        var encoder = new JpegFrameEncoder();

        Console.WriteLine("System initialized. Waiting for a viewer to connect...");
        bool running = true;

        var streamingTask = Task.Run(async () =>
        {
            var frameDelay = TimeSpan.FromMilliseconds(33); // ~30 FPS
            int frameCount = 0;

            while (running)
            {
                try
                {
                    // Check if anyone besides the capture client is connected
                    if (server.ConnectedClientsCount > 1)
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
                        Console.WriteLine($"Streaming: Sent frame {frameCount}");
                        await Task.Delay(frameDelay);
                    }
                    else
                    {
                        // Idle mode - save resources
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Streaming error: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
        });

        Console.ReadKey();
        running = false;
        await streamingTask;
        await client.StopAsync();
        await server.StopAsync();
    }

    public static string? GetStreamingIpAddress()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .ToList();

        // try to find wlan0
        var wlanIp = GetIpFromInterfaceType(interfaces, NetworkInterfaceType.Wireless80211);
        if (wlanIp != null) return wlanIp;

        // fallback to Ethernet
        var ethernetIp = GetIpFromInterfaceType(interfaces, NetworkInterfaceType.Ethernet);
        if (ethernetIp != null) return ethernetIp;

        return null; // fallback to LH
    }

    private static string? GetIpFromInterfaceType(IEnumerable<NetworkInterface> interfaces, NetworkInterfaceType type)
    {
        foreach (var nic in interfaces.Where(n => n.NetworkInterfaceType == type))
        {
            var ipProps = nic.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(addr.Address))
                {
                    return addr.Address.ToString();
                }
            }
        }
        return null;
    }
}