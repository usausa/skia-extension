namespace SkiaExtension.Tests.Fonts;

public sealed class FallbackFontResolverTests
{
    [Fact]
    public void ResolverShapesMixedScriptsAndTruncatesWithAnEllipsis()
    {
        // Arrange
        using var resolver = new FallbackFontResolver("sans-serif", ["ja"], []);

        // Act
        var run = resolver.Shape("見出し 🍎 ▲ text", 14, false);
        var truncated = resolver.Shape("A long title that will not fit", 14, false, 40);
        var empty = resolver.Shape(String.Empty, 14, false);
        var tooNarrow = resolver.Shape("abc", 14, false, 1);
        var cached = resolver.Shape("見出し 🍎 ▲ text", 14, false);

        // Assert
        Assert.True(run.Width > 0);
        Assert.NotEmpty(run.Parts);
        Assert.True(resolver.Measurements > 0);
        Assert.Equal("…", truncated.Parts[^1].Text);
        Assert.True(truncated.Width <= 40);
        Assert.Empty(empty.Parts);
        Assert.Empty(tooNarrow.Parts);
        Assert.Same(run, cached);
        Assert.True(resolver.GetLineHeight(14, false) > 0);
    }

    [Fact]
    public void VariationSelectorsAreNotDrawn()
    {
        // Arrange
        using var resolver = new FallbackFontResolver("sans-serif", ["ja"], []);

        // Act
        var run = resolver.Shape("✈️", 20, false);

        // Assert
        Assert.DoesNotContain(run.Parts, static part => part.Text.Contains('️', StringComparison.Ordinal));
    }

    [Fact]
    public void JapaneseAndEmojiResolveToFontsThatHaveTheGlyphs()
    {
        // Arrange
        using var resolver = new FallbackFontResolver("sans-serif", ["ja"], []);

        // Act
        var run = resolver.Shape("回転/分 🍎", 12, false);

        // Assert
        Assert.All(run.Parts, static part => Assert.All(part.Text.EnumerateRunes(), rune => Assert.True(part.Font.ContainsGlyph(rune.Value))));
    }

    [Theory]
    [InlineData("ja-JP")]
    [InlineData("zh-CN")]
    [InlineData("")]
    public void DeviceLanguageIsUsedWhenNoLanguagesAreGiven(string culture)
    {
        // Arrange
        string[] languages = culture.Length > 0 ? [culture] : [];
        using var automatic = CreateResolver(CultureInfo.GetCultureInfo(culture));
        using var specified = new FallbackFontResolver("sans-serif", languages, []);

        // Act
        var actual = automatic.Shape("直", 20, false);
        var expected = specified.Shape("直", 20, false);

        // Assert
        Assert.Equal(expected.Parts[0].Font.Typeface.FamilyName, actual.Parts[0].Font.Typeface.FamilyName);
    }

    [Fact]
    public void MaxLineHeightIncludesFontsResolvedWhileShaping()
    {
        // Arrange
        using var resolver = new FallbackFontResolver("sans-serif", ["ja"], []);

        // Act
        var before = resolver.GetMaxLineHeight(16, false);
        resolver.Shape("見出し 🍎", 16, false);
        var after = resolver.GetMaxLineHeight(16, false);

        // Assert
        Assert.Equal(resolver.GetLineHeight(16, false), before);
        Assert.True(after >= before);
    }

    [Fact]
    public void ClearCacheShapesTheTextAgain()
    {
        // Arrange
        using var resolver = new FallbackFontResolver("sans-serif", ["ja"], []);
        var first = resolver.Shape("abc", 14, false);
        var measurements = resolver.Measurements;

        // Act
        resolver.ClearCache();
        var second = resolver.Shape("abc", 14, false);

        // Assert
        Assert.NotSame(first, second);
        Assert.Equal(first.Width, second.Width);
        Assert.True(resolver.Measurements > measurements);
    }

    [Fact]
    public void DrawAdvancesByTheWidthOfTheRun()
    {
        // Arrange
        using var resolver = new FallbackFontResolver("sans-serif", ["ja"], []);
        using var bitmap = new SKBitmap(200, 50);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { Color = SKColors.Black };
        var run = resolver.Shape("見出し abc", 14, false);

        // Act
        var end = run.Draw(canvas, 10, 30, paint);

        // Assert
        Assert.Equal(10 + run.Width, end, 0.01f);
    }

    private static FallbackFontResolver CreateResolver(CultureInfo culture)
    {
        var current = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            return new FallbackFontResolver("sans-serif", null, []);
        }
        finally
        {
            CultureInfo.CurrentUICulture = current;
        }
    }
}
