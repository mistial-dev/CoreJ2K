# CoreJ2K.ImageSharp - A Cross-Platform JPEG2000 Codec

Copyright (c) 1999-2000 JJ2000 Partners;
Copyright (c) 2007-2012 Jason S. Clary;
Copyright (c) 2013-2016 Anders Gustafsson, Cureos AB;
Copyright (c) 2024-2025 Sjofn LLC;
Copyright (c) 2025 Mistial Developer.

Licensed and distributable under the terms of the [BSD license](http://www.opensource.org/licenses/bsd-license.php)

## Summary

CoreJ2K.ImageSharp is a cross-platform JPEG2000 codec for .NET Standard 2.0/2.1 applications. Originally based on CSJ2K (a C# port of the Java jj2000 package), this implementation exclusively uses SixLabors.ImageSharp for image handling, providing consistent behavior across all platforms without Windows-specific dependencies.

## Installation

Install via NuGet Package Manager:

```
Install-Package CoreJ2K.ImageSharp
```

Or using the .NET CLI:

```
dotnet add package CoreJ2K.ImageSharp
```

## Usage

### Setup

First, register ImageSharp support in your application:

```csharp
using CoreJ2K.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// Register ImageSharp support (call once at startup)
ImageSharpImageCreator.Register();
```

### Decoding

To decode a JPEG 2000 encoded image:

```csharp
using CoreJ2K;

// From file
var decodedImage = J2kImage.FromFile("image.jp2");

// From stream
var decodedImage = J2kImage.FromStream(stream);

// From byte array
var decodedImage = J2kImage.FromBytes(jpegData);
```

The returned `PortableImage` can be cast to ImageSharp types:

```csharp
// Cast to specific ImageSharp pixel formats
var rgbaImage = decodedImage.As<Image<Rgba32>>();
var rgbImage = decodedImage.As<Image<Rgb24>>();
var grayImage = decodedImage.As<Image<L8>>();
var grayAlphaImage = decodedImage.As<Image<La16>>();
```

### Encoding

To encode an ImageSharp image to JPEG2000:

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// Load or create an ImageSharp image
var image = Image.Load<Rgba32>("input.png");

// Encode to JPEG2000 bytes
byte[] jp2Data = J2kImage.ToBytes(image);

// Encode with custom parameters
var parameters = new ParameterList();
parameters["rate"] = "2.0"; // Compression ratio
byte[] jp2Data = J2kImage.ToBytes(image, parameters);
```

### Alternative Input Sources

For specialized formats, you can also create encodable sources from streams:

```csharp
// From PGM/PPM streams
var source = J2kImage.CreateEncodableSource(stream);
byte[] jp2Data = J2kImage.ToBytes(source);

// From multiple PGX component streams
var componentStreams = new List<Stream> { stream1, stream2, stream3 };
var source = J2kImage.CreateEncodableSource(componentStreams);
byte[] jp2Data = J2kImage.ToBytes(source);
```

### ROI (Region of Interest) Support

CoreJ2K.ImageSharp supports ROI encoding using the Maxshift method, allowing you to prioritize specific regions of an image during compression. This ensures that important areas maintain higher quality even at lower bitrates.

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

## Resources

* [Guide to the practical implementation of JPEG2000](http://www.jpeg.org/jpeg2000guide/guide/contents.html)
* [SixLabors.ImageSharp Documentation](https://docs.sixlabors.com/index.html)
