using System.Drawing;
using FrameCast.Capture.Windows;
using FrameCast.Encoding;

Console.WriteLine("Capturing screen and encoding to JPEG...");

// Capture full screen
var captureService = new WindowsScreenCaptureService();
using var bitmap = captureService.CaptureFullScreen();

// Encode to JPEG
var encoder = new JpegFrameEncoder(85); // quality
var jpegBytes = encoder.Encode(bitmap);

// Save the encoded JPEG
File.WriteAllBytes("./outputs/screenshot.jpg", jpegBytes);

Console.WriteLine("Screenshot saved as screenshot.jpg");
