namespace SkiaExtension.Fonts;

using SkiaSharp;

public sealed record TextSegment(string Text, float Width, SKFont Font);
