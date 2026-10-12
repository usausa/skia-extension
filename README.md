# SkiaExtension - Extensions for SkiaSharp

[![NuGet](https://img.shields.io/nuget/v/SkiaExtension.svg)](https://www.nuget.org/packages/SkiaExtension/)

## 🧩 What is this?

Extensions for SkiaSharp, grouped by namespace.  
The first one, `SkiaExtension.Fonts`, draws text with per character font fallback, so Japanese, emoji and other characters the primary font lacks are drawn with a font that has the glyphs.  

| Namespace | Contents |
|---|---|
| `SkiaExtension.Fonts` | Per character font fallback, text truncated with an ellipsis, drawing with an alignment and line heights |

Each feature has its own namespace under `SkiaExtension`.  
Features that need other dependencies come as separate packages named after their namespace.  

## 🚀 Quick Start

Create a `FontResolver` for the primary family, create the text and draw it with the `SKCanvas` extension.  

```csharp
using SkiaExtension.Fonts;

using var resolver = new FontResolver("sans-serif");
var text = resolver.CreateText("見出し 🍎 text", 16, false, maxWidth: 120);
canvas.DrawText(text, 70, 40, SKTextAlign.Center, paint);
```

Characters the primary family lacks are resolved per character, first through `FallbackFonts.Typefaces` and then through the system fonts with the language hints of `FallbackFonts.Languages`.  
`Languages` defaults to the UI language of the device.  
Set it to always prefer the glyph variants of a language for shared CJK ideographs, and add a bundled font when the system fonts lack characters.  

```csharp
FallbackFonts.Languages = ["ja"];
FallbackFonts.Typefaces = [SKTypeface.FromFile("NotoSansJP-Regular.ttf")];
```

The settings are read when a resolver is created, so set them at startup.  

## ✅ Supported features

| Category | Detail |
|---|---|
| **Fallback** | Configured typefaces, then the system fonts with BCP-47 language hints |
| **Emoji** | Emoji presentation sequences use the emoji font; variation selectors and zero width joiners are not drawn |
| **Text** | Font choice per text element, truncation with an ellipsis, bold by emboldening, cached results |
| **Drawing** | `SKCanvas.DrawText` extension with left, center and right alignment |
| **Metrics** | Width of the text, line height of the primary font, or of every font resolved so far |

## 📖 API

See the [API reference](Document/API.md).  
