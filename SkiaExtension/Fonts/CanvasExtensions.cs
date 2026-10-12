namespace SkiaExtension.Fonts;

using SkiaSharp;

public static class CanvasExtensions
{
    public static void DrawText(this SKCanvas canvas, DrawableText text, SKPoint p, SKTextAlign textAlign, SKPaint paint) =>
        canvas.DrawText(text, p.X, p.Y, textAlign, paint);

    public static void DrawText(this SKCanvas canvas, DrawableText text, float x, float y, SKTextAlign textAlign, SKPaint paint)
    {
        var start = textAlign switch
        {
            SKTextAlign.Center => x - (text.Width / 2),
            SKTextAlign.Right => x - text.Width,
            _ => x
        };
        foreach (var segment in text.Segments)
        {
            canvas.DrawText(segment.Text, start, y, SKTextAlign.Left, segment.Font, paint);
            start += segment.Width;
        }
    }
}
