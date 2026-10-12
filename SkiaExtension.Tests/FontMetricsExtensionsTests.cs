namespace SkiaExtension.Tests;

public sealed class FontMetricsExtensionsTests
{
    [Fact]
    public void BaselinesPlaceTheTextAtTheTopCenterAndBottom()
    {
        // Arrange
        using var font = new SKFont(SKTypeface.Default, 20);
        var metrics = font.Metrics;

        // Act
        var lineHeight = metrics.GetLineHeight();
        var top = metrics.GetTopBaseline(10);
        var center = metrics.GetCenterBaseline(50);
        var bottom = metrics.GetBottomBaseline(90);

        // Assert
        Assert.Equal(metrics.Descent - metrics.Ascent, lineHeight);
        Assert.Equal(10, top + metrics.Ascent, 0.001f);
        Assert.Equal(50, (center + metrics.Ascent + center + metrics.Descent) / 2, 0.001f);
        Assert.Equal(90, bottom + metrics.Descent, 0.001f);
    }
}
