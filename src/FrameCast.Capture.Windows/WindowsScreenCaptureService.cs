using System.Drawing;
using System.Windows.Forms;

namespace FrameCast.Capture.Windows;

public class WindowsScreenCaptureService
{
    public Bitmap CaptureFullScreen()
    {
        var bounds = Screen.PrimaryScreen.Bounds; // needs System.Windows.Forms
        var bitmap = new Bitmap(bounds.Width, bounds.Height);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
        }

        return bitmap;
    }
}