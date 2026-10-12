namespace SkiaExtension;

using SkiaSharp;

public static class BitmapExtensions
{
    public static SKBitmap Resize(this SKBitmap bitmap, float scale) =>
        bitmap.Resize(new SKSizeI((int)MathF.Round(bitmap.Width * scale), (int)MathF.Round(bitmap.Height * scale)), new SKSamplingOptions(SKCubicResampler.Mitchell));
}
