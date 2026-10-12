namespace SkiaExtension.Fonts;

using SkiaSharp;

public sealed record FallbackTextRun(IReadOnlyList<FallbackTextRunPart> Parts, float Width)
{
    public static FallbackTextRun Empty { get; } = new([], 0);

    public float Draw(SKCanvas canvas, float x, float baseline, SKPaint paint)
    {
        foreach (var part in Parts)
        {
            canvas.DrawText(part.Text, x, baseline, SKTextAlign.Left, part.Font, paint);
            x += part.Width;
        }

        return x;
    }
}
