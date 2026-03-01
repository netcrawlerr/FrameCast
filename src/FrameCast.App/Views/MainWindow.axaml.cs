using Avalonia.Controls;
using FrameCast.App.ViewModels;

namespace FrameCast.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
