namespace SkiaExtension.Tests;

public sealed class CodecExtensionsTests
{
    private static readonly SKColor Red = new(255, 0, 0);
    private static readonly SKColor Green = new(0, 255, 0);
    private static readonly SKColor Blue = new(0, 0, 255);
    private static readonly SKColor Yellow = new(255, 255, 0);

    [Theory]
    [InlineData(SKEncodedOrigin.TopLeft, 64, 32, "RGBY")]
    [InlineData(SKEncodedOrigin.TopRight, 64, 32, "GRYB")]
    [InlineData(SKEncodedOrigin.BottomRight, 64, 32, "YBGR")]
    [InlineData(SKEncodedOrigin.BottomLeft, 64, 32, "BYRG")]
    [InlineData(SKEncodedOrigin.LeftTop, 32, 64, "RBGY")]
    [InlineData(SKEncodedOrigin.RightTop, 32, 64, "BRYG")]
    [InlineData(SKEncodedOrigin.RightBottom, 32, 64, "YGBR")]
    [InlineData(SKEncodedOrigin.LeftBottom, 32, 64, "GYRB")]
    public void ImageIsTurnedUprightByTheEncodedOrigin(SKEncodedOrigin origin, int width, int height, string quadrants)
    {
        // Arrange
        using var stream = new MemoryStream(CreateJpeg(origin));
        using var codec = SKCodec.Create(stream);

        // Act
        using var bitmap = codec.DecodeOriented();

        // Assert
        Assert.Equal(origin, codec.EncodedOrigin);
        Assert.Equal(width, bitmap.Width);
        Assert.Equal(height, bitmap.Height);
        AssertColor(quadrants[0], bitmap.GetPixel(width / 4, height / 4));
        AssertColor(quadrants[1], bitmap.GetPixel(width * 3 / 4, height / 4));
        AssertColor(quadrants[2], bitmap.GetPixel(width / 4, height * 3 / 4));
        AssertColor(quadrants[3], bitmap.GetPixel(width * 3 / 4, height * 3 / 4));
    }

    private static byte[] CreateJpeg(SKEncodedOrigin origin)
    {
        using var bitmap = new SKBitmap(64, 32);
        bitmap.Erase(Red, new SKRectI(0, 0, 32, 16));
        bitmap.Erase(Green, new SKRectI(32, 0, 64, 16));
        bitmap.Erase(Blue, new SKRectI(0, 16, 32, 32));
        bitmap.Erase(Yellow, new SKRectI(32, 16, 64, 32));
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100);
        var jpeg = data.ToArray();
        byte[] exif =
        [
            0xFF, 0xE1, 0x00, 0x22, (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0x00, 0x00,
            0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, (byte)origin, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ];
        return [.. jpeg[..2], .. exif, .. jpeg[2..]];
    }

    private static void AssertColor(char code, SKColor actual)
    {
        var expected = code switch
        {
            'R' => Red,
            'G' => Green,
            'B' => Blue,
            _ => Yellow
        };
        Assert.InRange(Math.Abs(expected.Red - actual.Red), 0, 48);
        Assert.InRange(Math.Abs(expected.Green - actual.Green), 0, 48);
        Assert.InRange(Math.Abs(expected.Blue - actual.Blue), 0, 48);
    }
}
