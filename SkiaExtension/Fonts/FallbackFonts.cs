namespace SkiaExtension.Fonts;

using SkiaSharp;

public static class FallbackFonts
{
    public static IReadOnlyList<string>? Languages { get; set; }

    public static IReadOnlyList<SKTypeface> Typefaces { get; set; } = [];
}
