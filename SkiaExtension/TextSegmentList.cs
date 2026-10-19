namespace SkiaExtension;

using System.Collections;

using SkiaSharp;

internal sealed class TextSegmentList : IReadOnlyList<TextSegment>
{
    private readonly List<TextSegment> segments;
    private SKTextBlob?[]? blobs;

    public TextSegmentList(List<TextSegment> segments)
    {
        this.segments = segments;
        blobs = new SKTextBlob?[segments.Count];
    }

    public int Count => segments.Count;

    public TextSegment this[int index] => segments[index];

    public SKTextBlob? GetBlob(int index)
    {
        if (blobs is null)
        {
            return null;
        }

        var segment = segments[index];
        return blobs[index] ??= SKTextBlob.Create(segment.Text, segment.Font);
    }

    public void Release()
    {
        if (blobs is null)
        {
            return;
        }

        foreach (var blob in blobs)
        {
            blob?.Dispose();
        }

        blobs = null;
    }

    public IEnumerator<TextSegment> GetEnumerator() => segments.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
