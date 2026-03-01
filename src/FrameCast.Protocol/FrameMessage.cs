namespace FrameCast.Protocol;

public class FrameMessage
{
    public long Timestamp { get; set; }
    public byte[] Data { get; set; }

    public byte[] ToBytes()
    {
        var lenBytes = BitConverter.GetBytes(Data.Length);
        var tsBytes = BitConverter.GetBytes(Timestamp);
        return lenBytes.Concat(tsBytes).Concat(Data).ToArray();
    }

    public static FrameMessage FromBytes(byte[] bytes)
    {
        int len = BitConverter.ToInt32(bytes, 0);
        long ts = BitConverter.ToInt64(bytes, 4);
        byte[] data = new byte[len];
        Array.Copy(bytes, 12, data, 0, len);
        return new FrameMessage { Timestamp = ts, Data = data };
    }
}
