namespace SkiaExtension;

using SkiaSharp;

public static class CodecExtensions
{
    public static SKBitmap DecodeOriented(this SKCodec codec)
    {
        var bitmap = SKBitmap.Decode(codec);
        var origin = codec.EncodedOrigin;
        if (origin == SKEncodedOrigin.TopLeft)
        {
            return bitmap;
        }

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var oriented = new SKBitmap(bitmap.Info.WithSize(swap ? bitmap.Height : bitmap.Width, swap ? bitmap.Width : bitmap.Height));
        var matrix = GetOriginMatrix(origin, bitmap.Width, bitmap.Height);
        using (var canvas = new SKCanvas(oriented))
        {
            canvas.Clear();
            canvas.SetMatrix(in matrix);
            canvas.DrawBitmap(bitmap, 0, 0, SKSamplingOptions.Default);
        }

        bitmap.Dispose();
        return oriented;
    }

    private static SKMatrix GetOriginMatrix(SKEncodedOrigin origin, int width, int height) => origin switch
    {
        SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, width, 0, 1, 0, 0, 0, 1),
        SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, width, 0, -1, height, 0, 0, 1),
        SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, height, 0, 0, 1),
        SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
        SKEncodedOrigin.RightTop => new SKMatrix(0, -1, height, 1, 0, 0, 0, 0, 1),
        SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, height, -1, 0, width, 0, 0, 1),
        SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, width, 0, 0, 1),
        _ => SKMatrix.Identity
    };
}
