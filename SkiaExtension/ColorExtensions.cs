namespace SkiaExtension;

using SkiaSharp;

public static class ColorExtensions
{
    public static SKColor WithOpacity(this SKColor color, float opacity) => color.WithAlpha(ToByte(Math.Clamp(opacity, 0f, 1f) * 255f));

    public static SKColor Lerp(this SKColor color, SKColor to, float amount)
    {
        var t = Math.Clamp(amount, 0f, 1f);
        return new SKColor(Mix(color.Red, to.Red, t), Mix(color.Green, to.Green, t), Mix(color.Blue, to.Blue, t), Mix(color.Alpha, to.Alpha, t));
    }

    private static byte Mix(byte from, byte to, float amount) => ToByte(from + ((to - from) * amount));

    private static byte ToByte(float value) => (byte)MathF.Round(value);
}
