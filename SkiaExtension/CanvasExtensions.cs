namespace SkiaExtension;

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
        var segments = text.Segments;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            canvas.DrawText(segment.Text, start, y, SKTextAlign.Left, segment.Font, paint);
            start += segment.Width;
        }
    }

    public static void DrawArc(this SKCanvas canvas, SKPoint center, float radius, float startAngle, float sweepAngle, bool useCenter, SKPaint paint) =>
        canvas.DrawArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), startAngle, sweepAngle, useCenter, paint);
}
