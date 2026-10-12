namespace SkiaExtension;

using SkiaSharp;

public static class FontMetricsExtensions
{
    public static float GetLineHeight(this SKFontMetrics metrics) => metrics.Descent - metrics.Ascent;

    public static float GetTopBaseline(this SKFontMetrics metrics, float top) => top - metrics.Ascent;

    public static float GetCenterBaseline(this SKFontMetrics metrics, float centerY) => centerY - ((metrics.Ascent + metrics.Descent) / 2);

    public static float GetBottomBaseline(this SKFontMetrics metrics, float bottom) => bottom - metrics.Descent;
}
