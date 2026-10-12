namespace SkiaExtension.Tests;

public sealed class AngleHelperTests
{
    [Theory]
    [InlineData(0f, -90f)]
    [InlineData(90f, 0f)]
    [InlineData(225f, 135f)]
    public void ClockAngleIsMeasuredFromTwelveOClock(float clockAngle, float skiaAngle)
    {
        // Act & Assert
        Assert.Equal(skiaAngle, AngleHelper.ToSkiaAngle(clockAngle));
    }

    [Theory]
    [InlineData(0f, 60f, 50f)]
    [InlineData(90f, 50f, 60f)]
    [InlineData(180f, 40f, 50f)]
    [InlineData(270f, 50f, 40f)]
    public void PointIsOnTheCircleClockwiseFromThreeOClock(float angle, float x, float y)
    {
        // Act
        var point = AngleHelper.GetPoint(new SKPoint(50, 50), 10, angle);

        // Assert
        Assert.Equal(x, point.X, 0.001f);
        Assert.Equal(y, point.Y, 0.001f);
    }

    [Fact]
    public void ArcGradientStartsAtTheStartAngle()
    {
        // Arrange
        var center = new SKPoint(50, 50);
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        using var shader = AngleHelper.CreateArcGradient(center, 90, 180, [SKColors.Red, SKColors.Blue]);
        using var paint = new SKPaint();
        paint.Shader = shader;
        canvas.Clear(SKColors.Transparent);

        // Act
        canvas.DrawCircle(center, 45, paint);

        // Assert
        var start = AngleHelper.GetPoint(center, 30, 100);
        var end = AngleHelper.GetPoint(center, 30, 260);
        var startColor = bitmap.GetPixel((int)start.X, (int)start.Y);
        var endColor = bitmap.GetPixel((int)end.X, (int)end.Y);
        Assert.True(startColor.Red > startColor.Blue);
        Assert.True(endColor.Blue > endColor.Red);
    }
}
