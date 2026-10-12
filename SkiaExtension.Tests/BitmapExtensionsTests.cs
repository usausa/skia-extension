namespace SkiaExtension.Tests;

public sealed class BitmapExtensionsTests
{
    [Theory]
    [InlineData(0.5f, 32, 16)]
    [InlineData(2f, 128, 64)]
    public void BitmapIsResizedByTheScale(float scale, int width, int height)
    {
        // Arrange
        using var bitmap = new SKBitmap(64, 32);
        bitmap.Erase(SKColors.Red);

        // Act
        using var resized = bitmap.Resize(scale);

        // Assert
        Assert.Equal(width, resized.Width);
        Assert.Equal(height, resized.Height);
        var center = resized.GetPixel(width / 2, height / 2);
        Assert.InRange(center.Red, 250, 255);
        Assert.InRange(center.Blue, 0, 5);
    }
}
