namespace SkiaExtension.Tests.Fonts;

public sealed class CanvasExtensionsTests
{
    [Theory]
    [InlineData(SKTextAlign.Left, 0f)]
    [InlineData(SKTextAlign.Center, 0.5f)]
    [InlineData(SKTextAlign.Right, 1f)]
    public void TextIsAlignedToTheGivenPosition(SKTextAlign textAlign, float ratio)
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var text = resolver.CreateText("見出し abc", 20, false);

        // Act
        using var left = Render((canvas, paint) => canvas.DrawText(text, 40, 30, paint));
        using var aligned = Render((canvas, paint) => canvas.DrawText(text, 40 + (text.Width * ratio), 30, textAlign, paint));

        // Assert
        Assert.InRange(LeftmostInk(aligned) - LeftmostInk(left), -1, 1);
    }

    [Fact]
    public void EmptyTextDrawsNothing()
    {
        // Act
        using var bitmap = Render(static (canvas, paint) => canvas.DrawText(DrawableText.Empty, 10, 30, SKTextAlign.Center, paint));

        // Assert
        Assert.Equal(-1, LeftmostInk(bitmap));
    }

    private static SKBitmap Render(Action<SKCanvas, SKPaint> draw)
    {
        var bitmap = new SKBitmap(300, 50);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint();
        paint.Color = SKColors.Black;
        paint.IsAntialias = true;
        canvas.Clear(SKColors.Transparent);
        draw(canvas, paint);
        return bitmap;
    }

    private static int LeftmostInk(SKBitmap bitmap)
    {
        for (var x = 0; x < bitmap.Width; x++)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                {
                    return x;
                }
            }
        }

        return -1;
    }
}
