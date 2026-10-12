# SkiaExtension API reference

Every type is in the `SkiaExtension` namespace.  
Angles are degrees clockwise from 3 o'clock, as in SkiaSharp.  

## 🔤 FallbackFonts

Global font fallback settings, read when a `FontResolver` is created.  
Set them at startup, before the first resolver is created.  

| Name | Type | Default | Description |
|---|---|---|---|
| `Languages` | `IReadOnlyList<string>?` | `null` | BCP-47 tags for the per character system font lookup in priority order; the tag selects the glyph variant of shared CJK ideographs. `null` uses the UI language of the device (`CultureInfo.CurrentUICulture`) and an empty list passes no hint |
| `Typefaces` | `IReadOnlyList<SKTypeface>` | `[]` | Typefaces tried before the system lookup, for example a bundled font; they stay owned by the caller |

## 🔤 FontResolver

Resolves a font per character and creates texts split into segments that share a font.  
Characters are resolved in this order: the primary family, the fallback typefaces, then the system fonts with the language hints.  
Emoji presentation sequences (a character followed by U+FE0F) are matched against the emoji font first.  
Variation selectors and zero width joiners only steer the font choice and are not drawn.  
Fonts and created texts are owned by the resolver and stay valid until it is disposed.  

| Constructor | Description |
|---|---|
| `FontResolver(string familyName)` | Uses `FallbackFonts.Languages` and `FallbackFonts.Typefaces` |
| `FontResolver(string familyName, IReadOnlyList<string>? languages, IEnumerable<SKTypeface> typefaces)` | Uses the given settings; `languages` follows the same rules as `FallbackFonts.Languages` |

| Name | Returns | Description |
|---|---|---|
| `CreateText(string text, float size, bool bold, float maxWidth = ∞)` | `DrawableText` | Splits the text into segments that share a font and truncates it with an ellipsis when it is wider than `maxWidth`; line breaks become spaces and results are cached |
| `GetMetrics(float size, bool bold)` | `SKFontMetrics` | Metrics of the primary font |
| `GetLineHeight(float size, bool bold)` | `float` | Line height of the primary font |
| `GetMaxLineHeight(float size, bool bold)` | `float` | Largest line height of the primary font, the fallback typefaces and the system fonts resolved so far |
| `ClearCache()` | `void` | Discards the created texts |
| `Measurements` | `int` | Number of text measurements so far, for diagnostics |
| `Dispose()` | `void` | Releases the fonts and the system typefaces |

`bold` draws with an emboldened font.  

## 🔤 DrawableText

A text split into segments that share a font, ready to draw.  

| Name | Type | Description |
|---|---|---|
| `Segments` | `IReadOnlyList<TextSegment>` | Segments in drawing order |
| `Width` | `float` | Total advance width, for example to place the text next to another one |
| `Empty` | `DrawableText` | Text without segments |

## 🔤 TextSegment

| Name | Type | Description |
|---|---|---|
| `Text` | `string` | Text drawn with the font |
| `Width` | `float` | Advance width |
| `Font` | `SKFont` | Font of the segment, owned by the resolver |

## 🖌 CanvasExtensions

The `DrawText` overloads follow the `SKCanvas.DrawText` overloads that SkiaSharp 4 keeps, so the alignment is always given.  
`DrawArc` has the signature of the `SKCanvas.DrawArc` that SkiaSharp 4 removed.  

| Name | Description |
|---|---|
| `DrawText(this SKCanvas canvas, DrawableText text, float x, float y, SKTextAlign textAlign, SKPaint paint)` | Draws the text on the baseline `y` with its left edge, center or right edge at `x` |
| `DrawText(this SKCanvas canvas, DrawableText text, SKPoint p, SKTextAlign textAlign, SKPaint paint)` | Same with the position given as a point |
| `DrawArc(this SKCanvas canvas, SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)` | Draws the arc of the oval; `useCenter` closes it through the center into a wedge |
| `DrawArc(this SKCanvas canvas, SKPoint center, float radius, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)` | Same for a circle |

## 🔤 FontMetricsExtensions

| Name | Returns | Description |
|---|---|---|
| `GetLineHeight(this SKFontMetrics metrics)` | `float` | `Descent - Ascent` |
| `GetTopBaseline(this SKFontMetrics metrics, float top)` | `float` | Baseline that puts the top of the text at `top` |
| `GetCenterBaseline(this SKFontMetrics metrics, float centerY)` | `float` | Baseline that centers the text on `centerY` |
| `GetBottomBaseline(this SKFontMetrics metrics, float bottom)` | `float` | Baseline that puts the bottom of the text at `bottom` |

## 🎨 ColorExtensions

| Name | Returns | Description |
|---|---|---|
| `WithOpacity(this SKColor color, float opacity)` | `SKColor` | Color with the alpha from an opacity between `0` and `1`; the opacity is clamped |
| `Lerp(this SKColor color, SKColor to, float amount)` | `SKColor` | Color between the two, alpha included; `amount` is clamped between `0` and `1`, and a color mixed with black or white gets darker or lighter |

## 🖼 CodecExtensions

| Name | Returns | Description |
|---|---|---|
| `DecodeOriented(this SKCodec codec)` | `SKBitmap` | Decodes the image and turns it upright by the EXIF orientation (`SKCodec.EncodedOrigin`), all eight orientations included; the caller owns the bitmap |

## 🖼 BitmapExtensions

| Name | Returns | Description |
|---|---|---|
| `Resize(this SKBitmap bitmap, float scale)` | `SKBitmap` | New bitmap scaled by `scale` with the Mitchell cubic resampler |

## 📐 AngleHelper

| Name | Returns | Description |
|---|---|---|
| `ToSkiaAngle(float clockAngle)` | `float` | Converts degrees clockwise from 12 o'clock to the SkiaSharp angle |
| `GetPoint(SKPoint center, float radius, float angle)` | `SKPoint` | Point on the circle at the angle |
| `CreateArcGradient(SKPoint center, float startAngle, float sweepAngle, SKColor[] colors, float[]? colorPos = null)` | `SKShader` | Sweep gradient spread over the arc from `startAngle`; outside the arc it keeps the end colors, and the caller disposes the shader |
