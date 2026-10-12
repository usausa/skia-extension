namespace SkiaExtension.Fonts;

using SkiaSharp;

public sealed record FallbackTextRunPart(string Text, float Width, SKFont Font);
