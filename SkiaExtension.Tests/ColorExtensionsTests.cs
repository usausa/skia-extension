namespace SkiaExtension.Tests;

public sealed class ColorExtensionsTests
{
    [Theory]
    [InlineData(0f, 0)]
    [InlineData(0.5f, 128)]
    [InlineData(1f, 255)]
    [InlineData(-1f, 0)]
    [InlineData(2f, 255)]
    public void OpacityBecomesTheAlpha(float opacity, int alpha)
    {
        // Act
        var color = new SKColor(10, 20, 30, 40).WithOpacity(opacity);

        // Assert
        Assert.Equal(new SKColor(10, 20, 30, (byte)alpha), color);
    }

    [Fact]
    public void LerpMixesEveryChannelIncludingTheAlpha()
    {
        // Arrange
        var from = new SKColor(0, 100, 200, 0);
        var to = new SKColor(255, 0, 100, 255);

        // Act & Assert
        Assert.Equal(from, from.Lerp(to, 0));
        Assert.Equal(to, from.Lerp(to, 1));
        Assert.Equal(new SKColor(128, 50, 150, 128), from.Lerp(to, 0.5f));
        Assert.Equal(to, from.Lerp(to, 2));
    }
}
