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
            OnPropertyChanged();
        }
    }

    private string _serverIp = "";
    public string ServerIp
    {
        get => _serverIp;
        set
        {
            _serverIp = value;
            OnPropertyChanged();
        }
    }

    private int _serverPort = 5000;
    public int ServerPort
    {
        get => _serverPort;
        set
        {
            _serverPort = value;
            OnPropertyChanged();
        }
    }

    private TcpFrameClient? _client;
    public RelayCommand ConnectCommand { get; }

    public MainWindowViewModel()
    {
        ConnectCommand = new RelayCommand(async _ => await ConnectAsync());
    }

    private async Task ConnectAsync()
    {
        try
        {
            if (_client != null)
            {
                await _client.StopAsync();
                _client = null;
            }

            _client = new TcpFrameClient(ServerIp, ServerPort);
            _client.FrameReceived += OnFrameReceived;

            Console.WriteLine($"Connecting to {ServerIp}:{ServerPort}...");
            await _client.StartAsync();
            Console.WriteLine("Connected.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }

    private void OnFrameReceived(FrameMessage frame)
    {
        try
        {
            using var ms = new MemoryStream(frame.Data);
            var bitmap = new Bitmap(ms);
            Dispatcher.UIThread.Post(() => CurrentFrame = bitmap);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bitmap error: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
