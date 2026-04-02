using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FrameCast.Protocol;
using FrameCast.Transport;

namespace FrameCast.App.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    /// <TODO>
    /// needs a big refactoring or may be rewriting
    /// </TODO>
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

    public RelayCommand DisconnectCommand { get; }

    private bool _isSidebarOpen = true;

    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set
        {
            _isSidebarOpen = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand ToggleSidebarCommand { get; }

    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        ConnectCommand = new RelayCommand(async _ => await ConnectAsync());
        ToggleSidebarCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);
        DisconnectCommand = new RelayCommand(async _ => await DisconnectAsync());
    }

    private async Task ConnectAsync()
    {
        try
        {
            if (_client != null)
            {
                await _client.StopAsync();
                _client = null;
                Dispatcher.UIThread.Post(() => IsConnected = false);
            }

            _client = new TcpFrameClient(ServerIp, ServerPort);
            _client.FrameReceived += OnFrameReceived;

            Console.WriteLine($"Connecting to {ServerIp}:{ServerPort}...");
            await _client.StartAsync();
            Dispatcher.UIThread.Post(() => IsConnected = true);
            Console.WriteLine("Connected.");
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() => IsConnected = false);
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }


    private async Task DisconnectAsync()
    {
        try
        {
            if (_client != null)
            {
                await _client.StopAsync();
                _client = null;
            }

            Dispatcher.UIThread.Post(() =>
            {
                IsConnected = false;
                CurrentFrame = null; // last frzn frame
            });

            Console.WriteLine("Disconnected from server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during disconnect: {ex.Message}");
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