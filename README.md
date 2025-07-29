# CoreJ2K - A Managed and Portable JPEG2000 Codec


Copyright (c) 1999-2000 JJ2000 Partners;  
Copyright (c) 2007-2012 Jason S. Clary; 
Copyright (c) 2013-2016 Anders Gustafsson, Cureos AB;  
Copyright (c) 2024-2025 Sjofn LLC.   

Licensed and distributable under the terms of the [BSD license](http://www.opensource.org/licenses/bsd-license.php)

## Summary

This is an adaptation of [CSJ2K](http://csj2k.codeplex.com/), which provides JPEG 2000 decoding and encoding functionality to .NET based platforms. *CSJ2K* is by itself a C# port of the Java 
package *jj2000*, version 5.1. This is a fork of *CSJ2K* for .NET Standard 2.1 making it possible to implement JPEG decoding and encoding on any platform.

## Installation

Apart from building the relevant class libraries from source, pre-built packages for the supported platforms can also be obtained via [NuGet](https://nuget.org/packages/CSJ2K.Skia/).

## Usage

The Library provides interfaces for image rendering, file I/O and logging.

### Decoding

To decode a JPEG 2000 encoded image, call one of the following methods:

```csharp
public class J2kImage
{
	public static PortableImage FromStream(Stream, ParameterList = null);
	public static PortableImage FromBytes(byte[], ParameterList = null);
	public static PortableImage FromFile(string, ParameterList = null);
}
```

The returned `PortableImage` offers a "cast" method `As<T>()` to obtain an image in the type relevant for the platform. When using the `SKBitmapImageCreator` on .NET, a cast to `SKBitmap` or `SKPixmap` would suffice:

    var bitmap = decodedImage.As<SKBitmap>();

### Encoding

To encode an image, the following overloads are available:

```csharp
public class J2kImage
{
	public static byte[] ToBytes(object, ParameterList = null);
	public static byte[] ToBytes(BlkImgDataSrc, ParameterList = null);
}
```

The first overload takes an platform-specific image `object`. This is still works-in-progress, but an implementation is available for `SKBitmap` objects.

The second overload takes an *CSJ2K* specific object implementing the `BlkImgDataSrc` interface. When *Portable Graymap* (PGM), *Portable Pixelmap* (PPM) or JPEG2000 conformance testing format (PGX) objects are available as `Stream`s, 
it is possible to create `BlkImgDataSrc` objects using either of the following methods:

    J2kImage.CreateEncodableSource(Stream);
	J2kImage.CreateEncodableSource(IList<Stream>);
	
For *PGM* and *PPM* images, you would normally use the single `Stream` overload, whereas for *PGX* images, you may enter one `Stream` object per color component.

### ROI (Region of Interest) Support

CoreJ2K supports ROI encoding using the Maxshift method, allowing you to prioritize specific regions of an image during compression. This ensures that important areas maintain higher quality even at lower bitrates.

Basic ROI encoding example:
```csharp
// Create encoding parameters with a rectangular ROI
var parameters = new ParameterList();
parameters["Rroi"] = "R 100 100 200 150"; // Rectangle at (100,100) with width=200, height=150
parameters["rate"] = "1.0"; // Target bitrate

// Encode with ROI
byte[] encodedData = J2kImage.ToBytes(image, parameters);
```

Facial detection ROI example (68-point landmarks):
```csharp
// Define 3 regions for facial encoding
// Region 1: Eyes & eyebrows (highest priority)  
// Region 2: Nose, mouth & jaw (medium priority)
// Region 3: Full face with context (standard compression)
parameters["Rroi"] = "R 180 190 150 60 R 170 250 170 150 R 120 150 280 300";
parameters["Ralign"] = "on"; // Better performance with aligned blocks
```

For detailed ROI documentation including all supported shapes and parameters, see [docs/ROI_ENCODING.md](docs/ROI_ENCODING.md).
For complete facial detection examples, see [Examples/FacialDetectionROIExample.cs](Examples/FacialDetectionROIExample.cs).

## Links

* [Guide to the practical implementation of JPEG2000](http://www.jpeg.org/jpeg2000guide/guide/contents.html)

[![CoreJ2K NuGet-Release](https://img.shields.io/nuget/v/CoreJ2K.svg?label=CoreJ2K)](https://www.nuget.org/packages/CoreJ2K/) 
[![CoreJ2K.Skia NuGet-Release](https://img.shields.io/nuget/v/CoreJ2K.Skia.svg?label=CoreJ2K.Skia)](https://www.nuget.org/packages/CoreJ2K.Skia/)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/CoreJ2K?label=NuGet%20downloads)](https://www.nuget.org/packages/CoreJ2K/)  
[![Commits per month](https://img.shields.io/github/commit-activity/m/cinderblocks/CoreJ2K/master)](https://www.github.com/cinderblocks/CoreJ2K/)  
[![Build status](https://ci.appveyor.com/api/projects/status/9fr2467p5wxt6qxx?svg=true)](https://ci.appveyor.com/project/cinderblocks57647/corej2k)  
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/5704c7b134b249b3ac8ba3ca9a76dbbb)](https://app.codacy.com/gh/cinderblocks/CoreJ2K/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)  
[![ZEC](https://img.shields.io/keybase/zec/cinder)](https://keybase.io/cinder) [![BTC](https://img.shields.io/keybase/btc/cinder)](https://keybase.io/cinder)  
