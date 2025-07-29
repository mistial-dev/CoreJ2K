// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024-2025 Sjofn LLC.
// Copyright (c) 2025 Mistial Developer.
// Licensed under the BSD 3-Clause License.

using System;
using System.Runtime.CompilerServices;
using CoreJ2K.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using JetBrains.Annotations;

namespace CoreJ2K.ImageSharp
{
    /// <summary>
    /// Represents an implementation of <see cref="ImageBase{TBase}"/> tailored for the ImageSharp library.
    /// This class provides functionality to create and manipulate images with various pixel formats, such
    /// as grayscale, grayscale with alpha, RGB, and RGBA.
    /// </summary>
    internal class ImageSharpImage : ImageBase<Image>
    {
        /// <summary>
        /// Represents an image implementation based on the ImageSharp library.
        /// Provides methods to handle images with varying components and create the corresponding ImageSharp image objects.
        /// </summary>
        /// <remarks>
        /// The class supports the creation of grayscale, grayscale with alpha, RGB, and RGBA image types
        /// depending on the number of components specified.
        /// </remarks>
        internal ImageSharpImage(int width, int height, int numComponents, byte[] bytes)
            : base(width, height, numComponents, bytes)
        { }

        /// <summary>
        /// Generates and returns an object representation of the image, based on its number of color components.
        /// </summary>
        /// <returns>
        /// An image object corresponding to the number of components:
        /// - A grayscale image for 1 component.
        /// - A grayscale image with alpha channel for 2 components.
        /// - An RGB image for 3 components.
        /// - An RGBA image for 4+ components (extra components beyond RGBA are ignored).
        /// </returns>
        protected override object GetImageObject()
        {
            switch (NumComponents)
            {
                case 1:
                    return CreateGrayscaleImage();
                case 2:
                    return CreateGrayscaleAlphaImage();
                case 3:
                    return CreateRgbImage();
                case 4:
                case 5:
                default:
                    // For 4+ components, convert to RGBA by using first 4 components
                    // This handles CMYK, multi-spectral, and other multi-component images
                    return CreateRgbaImage();
            }
        }

        /// <summary>
        /// Creates a grayscale image from the given byte array, setting each pixel based on its corresponding value in the data.
        /// </summary>
        /// <returns>A grayscale image represented by an instance of <see cref="SixLabors.ImageSharp.Image{L8}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Image<L8> CreateGrayscaleImage()
        {
            var image = new Image<L8>(Width, Height);

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < Height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    var sourceOffset = y * Width;
                    for (var x = 0; x < Width; x++)
                    {
                        pixelRow[x] = new L8(Bytes[sourceOffset + x]);
                    }
                }
            });

            return image;
        }

        /// <summary>
        /// Creates a grayscale image with an alpha channel from byte data.
        /// </summary>
        /// <returns>
        /// An image of type <see cref="SixLabors.ImageSharp.Image{La16}"/> representing the grayscale image with alpha channel.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Image<La16> CreateGrayscaleAlphaImage()
        {
            var image = new Image<La16>(Width, Height);
            
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < Height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    var sourceOffset = y * Width * 2;
                    for (var x = 0; x < Width; x++)
                    {
                        var pixelOffset = sourceOffset + x * 2;
                        pixelRow[x] = new La16(Bytes[pixelOffset], Bytes[pixelOffset + 1]);
                    }
                }
            });

            return image;
        }

        /// Creates an RGB image from the provided byte array, width, and height.
        /// Processes the pixel data in the input byte array to create a new Image&lt;Rgb24&gt; object.
        /// Each pixel is represented by three consecutive bytes (for red, green, and blue channels),
        /// which are packed into an `Rgb24` structure for the output image.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Image<Rgb24> CreateRgbImage()
        {
            var image = new Image<Rgb24>(Width, Height);
            
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < Height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    var sourceOffset = y * Width * 3;
                    for (var x = 0; x < Width; x++)
                    {
                        var pixelOffset = sourceOffset + x * 3;
                        pixelRow[x] = new Rgb24(
                            Bytes[pixelOffset],
                            Bytes[pixelOffset + 1], 
                            Bytes[pixelOffset + 2]);
                    }
                }
            });

            return image;
        }

        /// <summary>
        /// Creates an RGBA image with the specified width, height, and representation of color components.
        /// Processes and populates pixel data using the input byte array.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Image{T}"/> of type <see cref="Rgba32"/> representing the RGBA image.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Image<Rgba32> CreateRgbaImage()
        {
            var image = new Image<Rgba32>(Width, Height);
            
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < Height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    var sourceOffset = y * Width * Math.Min(NumComponents, 4);
                    for (var x = 0; x < Width; x++)
                    {
                        var pixelOffset = sourceOffset + x * Math.Min(NumComponents, 4);
                        var alpha = NumComponents >= 4 ? Bytes[pixelOffset + 3] : (byte)255;
                        
                        pixelRow[x] = new Rgba32(
                            Bytes[pixelOffset],
                            Bytes[pixelOffset + 1], 
                            Bytes[pixelOffset + 2],
                            alpha);
                    }
                }
            });

            return image;
        }
    }
}