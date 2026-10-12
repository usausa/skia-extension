namespace SkiaExtension.Tests.Fonts;

public sealed class FallbackFontsTests
{
    [Fact]
    public void DefaultsFollowTheDeviceLanguageWithoutExtraTypefaces()
    {
        // Act
        var languages = FallbackFonts.Languages;
        var fallbacks = FallbackFonts.Fallbacks;

        // Assert
        Assert.Null(languages);
        Assert.Empty(fallbacks);
    }
}
