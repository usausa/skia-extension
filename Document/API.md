# SkiaExtension API reference

## 🔤 SkiaExtension.Fonts

### FallbackFonts

Global font fallback settings, read when a `FallbackFontResolver` is created.  
Set them at startup, before the first resolver is created.  

| Name | Type | Default | Description |
|---|---|---|---|
| `Languages` | `IReadOnlyList<string>?` | `null` | BCP-47 tags for the per character system font lookup in priority order; the tag selects the glyph variant of shared CJK ideographs. `null` uses the UI language of the device (`CultureInfo.CurrentUICulture`) and an empty list passes no hint |
| `Fallbacks` | `IReadOnlyList<SKTypeface>` | `[]` | Typefaces tried before the system lookup, for example a bundled font; they stay owned by the caller |

### FallbackFontResolver

Resolves a font per character and shapes text into runs that share a font.  
Characters are resolved in this order: the primary family, the fallback typefaces, then the system fonts with the language hints.  
Emoji presentation sequences (a character followed by U+FE0F) are matched against the emoji font first.  
Variation selectors and zero width joiners only steer the font choice and are not drawn.  
Fonts and shaped runs are owned by the resolver and stay valid until it is disposed.  

| Constructor | Description |
|---|---|
| `FallbackFontResolver(string familyName)` | Uses `FallbackFonts.Languages` and `FallbackFonts.Fallbacks` |
| `FallbackFontResolver(string familyName, IReadOnlyList<string>? languages, IReadOnlyList<SKTypeface> fallbacks)` | Uses the given settings; `languages` follows the same rules as `FallbackFonts.Languages` |

| Name | Returns | Description |
|---|---|---|
| `Shape(string text, float size, bool bold, float maxWidth = ∞)` | `FallbackTextRun` | Splits the text into runs that share a font and truncates it with an ellipsis when it is wider than `maxWidth`; line breaks become spaces and results are cached |
| `GetMetrics(float size, bool bold)` | `SKFontMetrics` | Metrics of the primary font |
| `GetLineHeight(float size, bool bold)` | `float` | Line height of the primary font |
| `GetMaxLineHeight(float size, bool bold)` | `float` | Largest line height of the primary font, the fallback typefaces and the system fonts resolved so far |
| `ClearCache()` | `void` | Discards the shaped runs |
| `Measurements` | `int` | Number of text measurements so far, for diagnostics |
| `Dispose()` | `void` | Releases the fonts and the system typefaces |

`bold` draws with an emboldened font.  

### FallbackTextRun

A shaped text made of parts that share a font.  

| Name | Type | Description |
|---|---|---|
| `Parts` | `IReadOnlyList<FallbackTextRunPart>` | Parts in drawing order |
| `Width` | `float` | Total advance width |
| `Empty` | `FallbackTextRun` | Run without parts |
| `Draw(SKCanvas canvas, float x, float baseline, SKPaint paint)` | `float` | Draws the parts from left to right and returns the x after the last part |

### FallbackTextRunPart

| Name | Type | Description |
|---|---|---|
| `Text` | `string` | Text drawn with the font |
| `Width` | `float` | Advance width |
| `Font` | `SKFont` | Font of the part, owned by the resolver |
