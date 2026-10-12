namespace SkiaExtension.Fonts;

using System.Buffers;
using System.Globalization;
using System.Text;

using SkiaSharp;

public sealed class FallbackFontResolver : IDisposable
{
    private const int EmojiFlag = 1 << 24;
    private const int CacheLimit = 2048;
    private const float ProbeSize = 12;

    private static readonly string[] EmojiLanguages = ["und-Zsye"];
    private static readonly SearchValues<char> Selectors = SearchValues.Create("︎️‍");

    private readonly string familyName;
    private readonly SKTypeface primary;
    private readonly string[]? languages;
    private readonly SKTypeface[] fallbacks;
    private readonly Dictionary<string, SKTypeface> systemByFamily = [with(StringComparer.Ordinal)];
    private readonly Dictionary<int, SKTypeface> byCodepoint = [];
    private readonly Dictionary<(SKTypeface Typeface, float Size, bool Bold), SKFont> fonts = [];
    private readonly Dictionary<(string Text, float Size, bool Bold, float MaxWidth), FallbackTextRun> cache = [];

    public int Measurements { get; private set; }

    public FallbackFontResolver(string familyName)
        : this(familyName, FallbackFonts.Languages, FallbackFonts.Fallbacks)
    {
    }

    public FallbackFontResolver(string familyName, IReadOnlyList<string>? languages, IReadOnlyList<SKTypeface> fallbacks)
    {
        this.familyName = familyName;
        primary = SKTypeface.FromFamilyName(familyName) ?? SKTypeface.Default;
        this.languages = ResolveLanguages(languages);
        this.fallbacks = fallbacks.ToArray();
    }

    public void Dispose()
    {
        foreach (var font in fonts.Values)
        {
            font.Dispose();
        }

        fonts.Clear();
        foreach (var typeface in systemByFamily.Values)
        {
            typeface.Dispose();
        }

        systemByFamily.Clear();
        byCodepoint.Clear();
        cache.Clear();
        primary.Dispose();
    }

    public SKFontMetrics GetMetrics(float size, bool bold) => GetFont(primary, size, bold).Metrics;

    public float GetLineHeight(float size, bool bold) => GetLineHeight(primary, size, bold);

    public float GetMaxLineHeight(float size, bool bold)
    {
        var height = GetLineHeight(primary, size, bold);
        foreach (var typeface in fallbacks.Concat(systemByFamily.Values))
        {
            height = Math.Max(height, GetLineHeight(typeface, size, bold));
        }

        return height;
    }

    public FallbackTextRun Shape(string text, float size, bool bold, float maxWidth = Single.PositiveInfinity)
    {
        if (text.Length == 0)
        {
            return FallbackTextRun.Empty;
        }

        var key = (text, size, bold, maxWidth);
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var run = ShapeCore(Sanitize(text), size, bold, maxWidth);
        if (cache.Count >= CacheLimit)
        {
            cache.Clear();
        }

        cache[key] = run;
        return run;
    }

    public void ClearCache() => cache.Clear();

    private static string[]? ResolveLanguages(IReadOnlyList<string>? languages)
    {
        if (languages is null)
        {
            var name = CultureInfo.CurrentUICulture.Name;
            return name.Length > 0 ? [name] : null;
        }

        return languages.Count > 0 ? languages.ToArray() : null;
    }

    private static string Sanitize(string text) => text.Replace('\r', ' ').Replace('\n', ' ');

    private static string StripSelectors(string text) => text.AsSpan().IndexOfAny(Selectors) < 0 ? text : text.Replace("️", String.Empty, StringComparison.Ordinal).Replace("︎", String.Empty, StringComparison.Ordinal).Replace("‍", String.Empty, StringComparison.Ordinal);

    private float GetLineHeight(SKTypeface typeface, float size, bool bold)
    {
        var metrics = GetFont(typeface, size, bold).Metrics;
        return metrics.Descent - metrics.Ascent;
    }

    private FallbackTextRun ShapeCore(string text, float size, bool bold, float maxWidth)
    {
        var segments = Segment(text, size, bold);
        var widths = new float[segments.Count];
        var width = 0f;
        for (var i = 0; i < segments.Count; i++)
        {
            Measurements++;
            widths[i] = segments[i].Font.MeasureText(segments[i].Text);
            width += widths[i];
        }

        var parts = new List<FallbackTextRunPart>(segments.Count + 1);
        if (width <= maxWidth)
        {
            for (var i = 0; i < segments.Count; i++)
            {
                parts.Add(new FallbackTextRunPart(segments[i].Text, widths[i], segments[i].Font));
            }

            return new FallbackTextRun(parts, width);
        }

        var ellipsisFont = GetFont(primary, size, bold);
        Measurements++;
        var ellipsisWidth = ellipsisFont.MeasureText("…");
        if (ellipsisWidth > maxWidth)
        {
            return FallbackTextRun.Empty;
        }

        width = 0f;
        for (var i = 0; i < segments.Count; i++)
        {
            if (width + widths[i] + ellipsisWidth <= maxWidth)
            {
                parts.Add(new FallbackTextRunPart(segments[i].Text, widths[i], segments[i].Font));
                width += widths[i];
                continue;
            }

            var segmentText = segments[i].Text;
            var boundaries = StringInfo.ParseCombiningCharacters(segmentText);
            var low = 0;
            var high = boundaries.Length;
            var lowWidth = 0f;
            while (low < high)
            {
                var middle = low + ((high - low + 1) / 2);
                var end = middle == boundaries.Length ? segmentText.Length : boundaries[middle];
                Measurements++;
                var prefixWidth = segments[i].Font.MeasureText(segmentText.AsSpan(0, end));
                if (width + prefixWidth + ellipsisWidth <= maxWidth)
                {
                    low = middle;
                    lowWidth = prefixWidth;
                }
                else
                {
                    high = middle - 1;
                }
            }

            if (low > 0)
            {
                var end = low == boundaries.Length ? segmentText.Length : boundaries[low];
                parts.Add(new FallbackTextRunPart(segmentText[..end], lowWidth, segments[i].Font));
                width += lowWidth;
            }

            break;
        }

        parts.Add(new FallbackTextRunPart("…", ellipsisWidth, ellipsisFont));
        return new FallbackTextRun(parts, width + ellipsisWidth);
    }

    private List<FallbackTextRunPart> Segment(string text, float size, bool bold)
    {
        var segments = new List<FallbackTextRunPart>();
        var boundaries = StringInfo.ParseCombiningCharacters(text);
        SKTypeface? current = null;
        var start = 0;
        for (var i = 0; i < boundaries.Length; i++)
        {
            var index = boundaries[i];
            var end = i + 1 < boundaries.Length ? boundaries[i + 1] : text.Length;
            var emoji = text.AsSpan(index, end - index).Contains('️');
            var typeface = Rune.TryGetRuneAt(text, index, out var rune) ? ResolveTypeface(rune.Value, emoji) : primary;
            if (ReferenceEquals(typeface, current))
            {
                continue;
            }

            if (current is not null)
            {
                segments.Add(new FallbackTextRunPart(StripSelectors(text[start..index]), 0, GetFont(current, size, bold)));
            }

            current = typeface;
            start = index;
        }

        if (current is not null)
        {
            segments.Add(new FallbackTextRunPart(StripSelectors(text[start..]), 0, GetFont(current, size, bold)));
        }

        return segments;
    }

    private SKTypeface ResolveTypeface(int codepoint, bool emoji)
    {
        if ((codepoint <= 127) && !emoji)
        {
            return primary;
        }

        var key = emoji ? codepoint | EmojiFlag : codepoint;
        if (byCodepoint.TryGetValue(key, out var typeface))
        {
            return typeface;
        }

        typeface = emoji ? MatchSystemFont(codepoint, EmojiLanguages) : null;
        if (typeface is null)
        {
            typeface = primary;
            if (!ContainsGlyph(primary, codepoint))
            {
                typeface = Array.Find(fallbacks, fallback => ContainsGlyph(fallback, codepoint)) ?? MatchSystemFont(codepoint, languages) ?? primary;
            }
        }

        byCodepoint[key] = typeface;
        return typeface;
    }

    private SKTypeface? MatchSystemFont(int codepoint, string[]? hints)
    {
        var typeface = SKFontManager.Default.MatchCharacter(familyName, SKFontStyle.Normal, hints, codepoint);
        if (typeface is null)
        {
            return null;
        }

        if (systemByFamily.TryGetValue(typeface.FamilyName, out var existing))
        {
            typeface.Dispose();
            return existing;
        }

        systemByFamily[typeface.FamilyName] = typeface;
        return typeface;
    }

    private bool ContainsGlyph(SKTypeface typeface, int codepoint) => GetFont(typeface, ProbeSize, false).ContainsGlyph(codepoint);

    private SKFont GetFont(SKTypeface typeface, float size, bool bold)
    {
        var key = (typeface, size, bold);
        if (!fonts.TryGetValue(key, out var font))
        {
            font = new SKFont(typeface, size) { Embolden = bold };
            fonts[key] = font;
        }

        return font;
    }
}
