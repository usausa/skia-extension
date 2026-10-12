namespace SkiaExtension.Fonts;

public sealed record DrawableText(IReadOnlyList<TextSegment> Segments, float Width)
{
    public static DrawableText Empty { get; } = new([], 0);
}
