# CSJ2K - A Managed and Portable JPEG2000 Codec


Copyright (c) 1999-2000 JJ2000 Partners; original C# port (c) 2007-2012 Jason S. Clary; C# encoding and adaptation to Portable Class Library with platform specific support (c) 2013-2016 Anders Gustafsson, Cureos AB; Copyright (c) 2024 Sjofn LLC.   

Licensed and distributable under the terms of the [BSD license](http://www.opensource.org/licenses/bsd-license.php)

## Summary

This is an adaptation of [CSJ2K](http://csj2k.codeplex.com/), which provides JPEG 2000 decoding and encoding functionality to .NET based platforms. *CSJ2K* is by itself a C# port of the Java 
package *jj2000*, version 5.1. This is a fork of *CSJ2K* for .NET Standard 2.1 making it possible to implement JPEG decoding and encoding on any platform supporting *SkiaSharp*.

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

## Links

* [Guide to the practical implementation of JPEG2000](http://www.jpeg.org/jpeg2000guide/guide/contents.html)

[![CSJ2K.Skia NuGet-Release](https://img.shields.io/nuget/v/CSJ2K.Skia.svg?label=CSJ2K.Skia)](https://www.nuget.org/packages/CSJ2K.Skia/)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/CSJ2K.Skia?label=NuGet%20downloads)](https://www.nuget.org/packages/CSJ2K.Skia/)  
[![Commits per month](https://img.shields.io/github/commit-activity/m/cinderblocks/csj2k/master)](https://www.github.com/cinderblocks/csj2k/)  
[![Build status](https://ci.appveyor.com/api/projects/status/0uki76pd02mb8l4j?svg=true)](https://ci.appveyor.com/project/cinderblocks57647/csj2k)  
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/5704c7b134b249b3ac8ba3ca9a76dbbb)](https://app.codacy.com/gh/cinderblocks/csj2k/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)  
[![ZEC](https://img.shields.io/keybase/zec/cinder)](https://keybase.io/cinder) [![BTC](https://img.shields.io/keybase/btc/cinder)](https://keybase.io/cinder)  
