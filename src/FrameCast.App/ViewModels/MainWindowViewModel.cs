using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FrameCast.Protocol;
using FrameCast.Transport;

namespace FrameCast.App.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private Bitmap? _currentFrame;
    public Bitmap? CurrentFrame
    {
        get => _currentFrame;
        set
        {
            _currentFrame = value;
            Console.WriteLine("UI: CurrentFrame updated");
            OnPropertyChanged();
        }
    }

    private TcpFrameClient? _client;

    public MainWindowViewModel()
    {
        Console.WriteLine("UI: MainWindowViewModel created");
        _ = ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine("UI: Connecting to server...");

            _client = new TcpFrameClient("127.0.0.1", 5000);
            _client.FrameReceived += OnFrameReceived;

            await _client.StartAsync();

            Console.WriteLine("UI: Connected to server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UI: Connection error: {ex.Message}");
        }
    }

    private void OnFrameReceived(FrameMessage frame)
    {
        Console.WriteLine($"UI: Frame received ({frame.Data.Length} bytes)");

        try
        {
            using var ms = new MemoryStream(frame.Data);
            var bitmap = new Bitmap(ms);
            Console.WriteLine("UI: Bitmap created");

            Dispatcher.UIThread.Post(() =>
            {
                CurrentFrame = bitmap;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UI: Bitmap error: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
