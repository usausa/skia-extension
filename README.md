# SkiaExtension - Extensions for SkiaSharp

[![NuGet](https://img.shields.io/nuget/v/SkiaExtension.svg)](https://www.nuget.org/packages/SkiaExtension/)

## 🧩 What is this?

Extensions for SkiaSharp, mostly extension methods in the `SkiaExtension` namespace.  
They cover what SkiaSharp 4 leaves to each application: text with per character font fallback, arcs, angles, colors and photos taken in any orientation.  

## 🚀 Quick Start

Create a `FontResolver` for the primary family, create the text and draw it with the `SKCanvas` extension.  
`GetCenterBaseline` gives the baseline that centers the text vertically.  

```csharp
using SkiaExtension;

using var resolver = new FontResolver("sans-serif");
var text = resolver.CreateText("見出し 🍎 text", 16, false, maxWidth: 120);
var metrics = resolver.GetMetrics(16, false);
canvas.DrawText(text, rect.MidX, metrics.GetCenterBaseline(rect.MidY), SKTextAlign.Center, paint);
```

Characters the primary family lacks are resolved per character, first through `FallbackFonts.Typefaces` and then through the system fonts with the language hints of `FallbackFonts.Languages`.  
`Languages` defaults to the UI language of the device.  
Set it to always prefer the glyph variants of a language for shared CJK ideographs, and add a bundled font when the system fonts lack characters.  

```csharp
FallbackFonts.Languages = ["ja"];
FallbackFonts.Typefaces = [SKTypeface.FromFile("NotoSansJP-Regular.ttf")];
```

The settings are read when a resolver is created, so set them at startup.  

Arcs, angles, colors and photos:  

```csharp
using var shader = AngleHelper.CreateArcGradient(center, 135, 270, [SKColors.DodgerBlue, SKColors.Red]);
paint.Shader = shader;
canvas.DrawArc(center, radius, 135, 270, false, paint);
var tip = AngleHelper.GetPoint(center, radius, AngleHelper.ToSkiaAngle(45));
paint.Color = SKColors.Black.WithOpacity(0.4f);

using var codec = SKCodec.Create(stream);
using var photo = codec.DecodeOriented();
using var thumbnail = photo.Resize(0.25f);
```

## ✅ Supported features

| Category | Detail |
|---|---|
| **Fonts** | Per character fallback through configured typefaces and the system fonts with BCP-47 language hints; emoji presentation sequences use the emoji font |
| **Text** | Text split by font, truncated with an ellipsis and drawn with left, center or right alignment; baselines for the top, the center and the bottom |
| **Arcs** | `DrawArc` by a center and a radius, and sweep gradients that start where the arc starts |
| **Angles** | Points on a circle, and angles from 12 o'clock converted to the SkiaSharp angles from 3 o'clock |
| **Colors** | Interpolation of every channel and colors with an opacity |
| **Images** | Decoding with the EXIF orientation applied and resizing by a scale |

## 📖 API

See the [API reference](Document/API.md).  
