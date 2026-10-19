namespace SkiaExtension.Tests;

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
        using var left = Render((canvas, paint) => canvas.DrawText(text, 40, 30, SKTextAlign.Left, paint));
        using var aligned = Render((canvas, paint) => canvas.DrawText(text, 40 + (text.Width * ratio), 30, textAlign, paint));

        // Assert
        Assert.InRange(LeftmostInk(aligned) - LeftmostInk(left), -1, 1);
    }

    [Fact]
    public void PointIsTheSameAsTheCoordinates()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var text = resolver.CreateText("見出し abc", 20, false);

        // Act
        using var coordinates = Render((canvas, paint) => canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint));
        using var point = Render((canvas, paint) => canvas.DrawText(text, new SKPoint(150, 30), SKTextAlign.Center, paint));

        // Assert
        Assert.Equal(coordinates.Bytes, point.Bytes);
    }

    [Fact]
    public void EmptyTextDrawsNothing()
    {
        // Act
        using var bitmap = Render(static (canvas, paint) => canvas.DrawText(DrawableText.Empty, 10, 30, SKTextAlign.Center, paint));

        // Assert
        Assert.Equal(-1, LeftmostInk(bitmap));
    }

    [Theory]
    [InlineData(SKTextAlign.Left, Single.PositiveInfinity)]
    [InlineData(SKTextAlign.Center, Single.PositiveInfinity)]
    [InlineData(SKTextAlign.Right, Single.PositiveInfinity)]
    [InlineData(SKTextAlign.Center, 80f)]
    public void TextIsDrawnTheSameAsACopyOfItsSegments(SKTextAlign textAlign, float maxWidth)
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var text = resolver.CreateText("見出し 🍎 abc", 20, false, maxWidth);
        var copy = text with { Segments = [.. text.Segments] };

        // Act
        using var expected = Render((canvas, paint) => canvas.DrawText(copy, 150, 30, textAlign, paint));
        using var first = Render((canvas, paint) => canvas.DrawText(text, 150, 30, textAlign, paint));
        using var second = Render((canvas, paint) => canvas.DrawText(text, 150, 30, textAlign, paint));

        // Assert
        Assert.True(text.Segments.Count > 1);
        Assert.Equal(expected.Bytes, first.Bytes);
        Assert.Equal(expected.Bytes, second.Bytes);
    }

    [Fact]
    public void DrawingTheTextAgainDoesNotAllocate()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var text = resolver.CreateText("見出し 🍎 abc", 20, false);
        using var bitmap = new SKBitmap(300, 50);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint();
        canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint);

        // Act
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10; i++)
        {
            canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint);
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        // Assert
        Assert.Equal(0, allocated);
    }

    [Fact]
    public void TextIsDrawnAfterTheCacheIsCleared()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var text = resolver.CreateText("見出し 🍎 abc", 20, false);
        using var expected = Render((canvas, paint) => canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint));

        // Act
        resolver.ClearCache();
        using var cleared = Render((canvas, paint) => canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint));
        var created = resolver.CreateText("見出し 🍎 abc", 20, false);
        using var recreated = Render((canvas, paint) => canvas.DrawText(created, 150, 30, SKTextAlign.Center, paint));

        // Assert
        Assert.NotSame(text, created);
        Assert.Equal(expected.Bytes, cleared.Bytes);
        Assert.Equal(expected.Bytes, recreated.Bytes);
    }

    [Fact]
    public void TextIsDrawnAfterTheCacheOverflows()
    {
        // Arrange
        using var resolver = new FontResolver("sans-serif", ["ja"], []);
        var text = resolver.CreateText("見出し 🍎 abc", 20, false);
        using var expected = Render((canvas, paint) => canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint));

        // Act
        for (var i = 0; i < 2048; i++)
        {
            resolver.CreateText(i.ToString(CultureInfo.InvariantCulture), 20, false);
        }

        using var overflowed = Render((canvas, paint) => canvas.DrawText(text, 150, 30, SKTextAlign.Center, paint));

        // Assert
        Assert.NotSame(text, resolver.CreateText("見出し 🍎 abc", 20, false));
        Assert.Equal(expected.Bytes, overflowed.Bytes);
    }

    [Fact]
    public void ArcIsDrawnOnlyAlongTheSweep()
    {
        // Arrange
        var center = new SKPoint(150, 25);

        // Act
        using var bitmap = Render((canvas, paint) =>
        {
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 8;
            canvas.DrawArc(center, 20, 0, 90, false, paint);
        });

        // Assert
        Assert.True(Ink(bitmap, AngleHelper.GetPoint(center, 20, 45)));
        Assert.False(Ink(bitmap, AngleHelper.GetPoint(center, 20, 225)));
    }

    [Fact]
    public void UseCenterFillsTheWedge()
    {
        // Arrange
        var center = new SKPoint(150, 25);

        // Act
        using var wedge = Render((canvas, paint) => canvas.DrawArc(center, 20, 0, 90, true, paint));
        using var segment = Render((canvas, paint) => canvas.DrawArc(center, 20, 0, 90, false, paint));

        // Assert
        Assert.True(Ink(wedge, AngleHelper.GetPoint(center, 5, 45)));
        Assert.False(Ink(segment, AngleHelper.GetPoint(center, 5, 45)));
        Assert.True(Ink(segment, AngleHelper.GetPoint(center, 18, 45)));
    }

    [Fact]
    public void CenterIsTheSameAsTheOval()
    {
        // Act
        using var oval = Render(static (canvas, paint) => canvas.DrawArc(new SKRect(130, 5, 170, 45), 30, 200, true, paint));
        using var center = Render(static (canvas, paint) => canvas.DrawArc(new SKPoint(150, 25), 20, 30, 200, true, paint));

        // Assert
        Assert.Equal(oval.Bytes, center.Bytes);
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

    private static bool Ink(SKBitmap bitmap, SKPoint point) => bitmap.GetPixel((int)point.X, (int)point.Y).Alpha > 0;
}
