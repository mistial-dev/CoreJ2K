// Copyright (c) 2025 Mistial Developer.
// Licensed under the BSD 3-Clause License.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using CoreJ2K;
using CoreJ2K.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CoreJ2K.Examples
{
    /// <summary>
    /// Example demonstrating JPEG2000 encoding/decoding with ImageSharp integration
    /// </summary>
    public static class ImageSharpExample
    {
        public static void BasicUsageExample()
        {
            // Register ImageSharp support
            ImageSharpImageCreator.Register();

            // Load an image using ImageSharp
            using var originalImage = Image.Load<Rgba32>("input.png");
            
            // Encode to JPEG2000
            byte[] jp2Data = J2kImage.ToBytes(originalImage);
            
            // Save the JPEG2000 file
            File.WriteAllBytes("output.jp2", jp2Data);
            
            // Decode back from JPEG2000
            var decodedPortable = J2kImage.FromBytes(jp2Data);
            
            // Convert back to ImageSharp format
            using var decodedImage = decodedPortable.As<Image<Rgba32>>();
            
            // Save the decoded image
            decodedImage.SaveAsPng("decoded.png");
            
            Console.WriteLine("JPEG2000 round-trip completed successfully!");
        }

        public static void MultiFormatExample()
        {
            ImageSharpImageCreator.Register();

            // Test different pixel formats
            var formats = new[]
            {
                ("rgba32", () => new Image<Rgba32>(100, 100)),
                ("rgb24", () => new Image<Rgb24>(100, 100)),
                ("grayscale", () => new Image<L8>(100, 100)),
                ("grayscale_alpha", () => new Image<La16>(100, 100))
            };

            foreach (var (name, factory) in formats)
            {
                using var image = factory();
                
                // Fill with sample data
                FillSampleData(image);
                
                // Encode to JPEG2000
                byte[] jp2Data = J2kImage.ToBytes(image);
                
                Console.WriteLine($"{name}: Encoded {image.Width}x{image.Height} to {jp2Data.Length} bytes");
                
                // Decode and verify
                var decoded = J2kImage.FromBytes(jp2Data);
                Console.WriteLine($"{name}: Successfully decoded back from JPEG2000");
            }
        }

        private static void FillSampleData<TPixel>(Image<TPixel> image) 
            where TPixel : unmanaged, IPixel<TPixel>
        {
            // Fill with a simple gradient pattern
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        var intensity = (byte)((x + y) % 256);
                        
                        if (typeof(TPixel) == typeof(Rgba32))
                        {
                            row[x] = Unsafe.As<Rgba32, TPixel>(ref Unsafe.AsRef(new Rgba32(intensity, intensity, intensity, 255)));
                        }
                        else if (typeof(TPixel) == typeof(Rgb24))
                        {
                            row[x] = Unsafe.As<Rgb24, TPixel>(ref Unsafe.AsRef(new Rgb24(intensity, intensity, intensity)));
                        }
                        else if (typeof(TPixel) == typeof(L8))
                        {
                            row[x] = Unsafe.As<L8, TPixel>(ref Unsafe.AsRef(new L8(intensity)));
                        }
                        else if (typeof(TPixel) == typeof(La16))
                        {
                            row[x] = Unsafe.As<La16, TPixel>(ref Unsafe.AsRef(new La16(intensity, 255)));
                        }
                    }
                }
            });
        }
    }
}