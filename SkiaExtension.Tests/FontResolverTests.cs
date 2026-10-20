namespace SkiaExtension.Tests;

public sealed class FontResolverTests
{
    [Fact]
    public void TextIsSplitByFontAndTruncatedWithAnEllipsis()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);

        // Act
        var text = resolver.CreateText("見出し 🍎 ▲ text", 14, false);
        var truncated = resolver.CreateText("A long title that will not fit", 14, false, 40);
        var empty = resolver.CreateText(String.Empty, 14, false);
        var tooNarrow = resolver.CreateText("abc", 14, false, 1);
        var cached = resolver.CreateText("見出し 🍎 ▲ text", 14, false);

        // Assert
        Assert.True(text.Width > 0);
        Assert.NotEmpty(text.Segments);
        Assert.True(resolver.Measurements > 0);
        Assert.Equal("…", truncated.Segments[^1].Text);
        Assert.True(truncated.Width <= 40);
        Assert.Empty(empty.Segments);
        Assert.Empty(tooNarrow.Segments);
        Assert.Same(text, cached);
        Assert.True(resolver.GetLineHeight(14, false) > 0);
    }

    [Fact]
    public void VariationSelectorsAreNotDrawn()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);

        // Act
        var text = resolver.CreateText("✈\uFE0F", 20, false);

        // Assert
        Assert.DoesNotContain(text.Segments, static segment => segment.Text.Contains('\uFE0F', StringComparison.Ordinal));
    }

    [Fact]
    public void JapaneseAndEmojiResolveToFontsThatHaveTheGlyphs()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);

        // Act
        var text = resolver.CreateText("回転/分 🍎", 12, false);

        // Assert
        Assert.All(text.Segments, static segment => Assert.All(segment.Text.EnumerateRunes(), rune => Assert.True(segment.Font.ContainsGlyph(rune.Value))));
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
        using var specified = new FontResolver("sans-serif", languages, []);

        // Act
        var actual = automatic.CreateText("直", 20, false);
        var expected = specified.CreateText("直", 20, false);

        // Assert
        Assert.Equal(expected.Segments[0].Font.Typeface.FamilyName, actual.Segments[0].Font.Typeface.FamilyName);
    }

    [Fact]
    public void MaxLineHeightIncludesFontsResolvedWhileCreatingTexts()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);

        // Act
        var before = resolver.GetMaxLineHeight(16, false);
        resolver.CreateText("見出し 🍎", 16, false);
        var after = resolver.GetMaxLineHeight(16, false);

        // Assert
        Assert.Equal(resolver.GetLineHeight(16, false), before);
        Assert.True(after >= before);
    }

    [Fact]
    public void ClearCacheCreatesTheTextAgain()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var first = resolver.CreateText("abc", 14, false);
        var measurements = resolver.Measurements;

        // Act
        resolver.ClearCache();
        var second = resolver.CreateText("abc", 14, false);

        // Assert
        Assert.NotSame(first, second);
        Assert.Equal(first.Width, second.Width);
        Assert.True(resolver.Measurements > measurements);
    }

    [Fact]
    public void FontsUseLinearMetrics()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);

        // Act
        var text = resolver.CreateText("Total 合計", 6, true);

        // Assert
        Assert.NotEmpty(text.Segments);
        Assert.All(text.Segments, static segment => Assert.True(segment.Font.LinearMetrics));
    }

    private static FontResolver CreateResolver(CultureInfo culture)
    {
        var current = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            return new FontResolver("sans-serif", null, []);
        }
        finally
        {
            CultureInfo.CurrentUICulture = current;
        }
    }
}
