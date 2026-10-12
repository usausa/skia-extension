namespace SkiaExtension.Tests.Fonts;

public sealed class FallbackFontsTests
{
    [Fact]
    public void DefaultsFollowTheDeviceLanguageWithoutExtraTypefaces()
    {
        // Act
        var languages = FallbackFonts.Languages;
        var typefaces = FallbackFonts.Typefaces;

        // Assert
        Assert.Null(languages);
        Assert.Empty(typefaces);
    }
}
