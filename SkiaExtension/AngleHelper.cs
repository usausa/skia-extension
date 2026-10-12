namespace SkiaExtension;

using SkiaSharp;

public static class AngleHelper
{
    public static float ToSkiaAngle(float clockAngle) => clockAngle - 90;

    public static SKPoint GetPoint(SKPoint center, float radius, float angle)
    {
        var radian = angle * MathF.PI / 180;
        return new SKPoint(center.X + (radius * MathF.Cos(radian)), center.Y + (radius * MathF.Sin(radian)));
    }

    public static SKShader CreateArcGradient(SKPoint center, float startAngle, float sweepAngle, SKColor[] colors, float[]? colorPos = null) =>
        SKShader.CreateSweepGradient(center, colors, colorPos, SKShaderTileMode.Clamp, 0, sweepAngle, SKMatrix.CreateRotationDegrees(startAngle, center.X, center.Y));
}
