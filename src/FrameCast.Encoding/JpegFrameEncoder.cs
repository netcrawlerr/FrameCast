using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FrameCast.Encoding;

public class JpegFrameEncoder
{
    private readonly long _quality;

    public JpegFrameEncoder(long quality = 90)
    {
        _quality = quality;
    }

    /// <summary>
    /// Encode a Bitmap to JPEG byte array
    /// </summary>
    public byte[] Encode(Bitmap bitmap)
    {
        using var ms = new MemoryStream();

        var encoder = GetEncoder(ImageFormat.Jpeg);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, _quality);

        bitmap.Save(ms, encoder, encoderParams);
        return ms.ToArray();
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        foreach (var codec in ImageCodecInfo.GetImageDecoders())
        {
            if (codec.FormatID == format.Guid)
                return codec;
        }
        throw new InvalidOperationException("JPEG encoder not found");
    }
}