// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

using System.Runtime.InteropServices;
using CoreJ2K.Util;

namespace codectest
{
    using System;
    using System.IO;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Formats.Png;

    using CoreJ2K;
    using CoreJ2K.ImageSharp;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ImageSharpImageCreator.Register();

            if (Directory.Exists("output"))
            {
                var di = new DirectoryInfo("output");
                di.Delete(true);
            }
            Directory.CreateDirectory("output");
            
            using (var ppm = File.OpenRead(Path.Combine("samples", "a1_mono.ppm")))
            {
                var enc = J2kImage.ToBytes(J2kImage.CreateEncodableSource(ppm));
                File.WriteAllBytes(Path.Combine("output", "file11.jp2"), enc);
            }

            using (var ppm = File.OpenRead(Path.Combine("samples", "a2_colr.ppm")))
            {
                var enc = J2kImage.ToBytes(J2kImage.CreateEncodableSource(ppm));
                File.WriteAllBytes(Path.Combine("output", "file12.jp2"), enc);
            }

            using (var pgx = File.OpenRead(Path.Combine("samples", "c1p0_05_0.pgx")))
            {
                var enc = J2kImage.ToBytes(J2kImage.CreateEncodableSource(pgx));
                File.WriteAllBytes(Path.Combine("output", "file13.jp2"), enc);
            }
            
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "racoon.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file14.jp2"), enc);
            }
            
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "basn0g01.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file16.jp2"), enc);
            }
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "basn0g08.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file17.jp2"), enc);
            }
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "basn3p02.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file18.jp2"), enc);
            }
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "basn3p08.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file17.jp2"), enc);
            }
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "basn4a08.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file18.jp2"), enc);
            }
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "basn6a08.png")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file19.jp2"), enc);
            }
            using (var image = Image.Load<Rgba32>(Path.Combine("samples", "dog.jpeg")))
            {
                var enc = J2kImage.ToBytes(image);
                File.WriteAllBytes(Path.Combine("output", "file20.jp2"), enc);
            }
            
            string[] files = Directory.GetFiles("output", "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    PortableImage portableImage;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var timer = new HiPerfTimer();
                        timer.Start();
                        portableImage = J2kImage.FromFile(file);
                        timer.Stop();
                        Console.WriteLine($"{file}: {timer.Duration} seconds");
                    }
                    else
                    {
                        portableImage = J2kImage.FromFile(file);
                    }

                    // Try to get as RGBA32 first, if that fails, try other formats
                    Image<Rgba32> image = null;
                    try
                    {
                        image = portableImage.As<Image<Rgba32>>();
                    }
                    catch
                    {
                        // Try RGB24 and convert to RGBA32
                        try
                        {
                            var rgbImage = portableImage.As<Image<Rgb24>>();
                            image = ConvertToRgba32(rgbImage);
                            rgbImage.Dispose();
                        }
                        catch
                        {
                            // Try L8 and convert to RGBA32
                            try
                            {
                                var grayImage = portableImage.As<Image<L8>>();
                                image = ConvertGrayToRgba32(grayImage);
                                grayImage.Dispose();
                            }
                            catch
                            {
                                // Try La16 and convert to RGBA32
                                var grayAlphaImage = portableImage.As<Image<La16>>();
                                image = ConvertGrayAlphaToRgba32(grayAlphaImage);
                                grayAlphaImage.Dispose();
                            }
                        }
                    }

                    var histogram = GenerateHistogram(image);
                    histogram.SaveAsPng(Path.Combine("output", $"{Path.GetFileNameWithoutExtension(file)}_histogram.png"));
                    image.SaveAsPng(Path.Combine("output", $"{Path.GetFileNameWithoutExtension(file)}_encoded.png"));
                    
                    histogram.Dispose();
                    image.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{file}:\r\n{e.Message}");
                    if (e.InnerException != null)
                    {
                        Console.WriteLine(e.InnerException.Message);
                        Console.WriteLine(e.InnerException.StackTrace);
                    }
                    else Console.WriteLine(e.StackTrace);

                }
            }
        }

        private static Image<Rgba32> GenerateHistogram(Image<Rgba32> image)
        {
            var histogram = new Image<Rgba32>(256, 100);

            var colorcounts = new int[256];

            // Calculate histogram
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (var x = 0; x < accessor.Width; x++)
                    {
                        var pixel = pixelRow[x];
                        colorcounts[pixel.R]++;
                        colorcounts[pixel.G]++;
                        colorcounts[pixel.B]++;
                    }
                }
            });

            var maxval = 0;
            for (var i = 0; i < 256; i++) if (colorcounts[i] > maxval) maxval = colorcounts[i];
            for (var i = 1; i < 255; i++)
            {
                colorcounts[i] = (int)Math.Round((colorcounts[i] / (double)maxval) * 100D);
            }
            
            // Draw histogram
            histogram.ProcessPixelRows(accessor =>
            {
                for (var x = 0; x < 256; x++)
                {
                    for (var y = 0; y < 100; y++)
                    {
                        var pixelRow = accessor.GetRowSpan(y);
                        pixelRow[x] = colorcounts[x] >= (100 - y) ? 
                            new Rgba32(0, 0, 0, 255) : 
                            new Rgba32(255, 255, 255, 255);
                    }
                }
            });
            
            return histogram;
        }

        private static Image<Rgba32> ConvertToRgba32(Image<Rgb24> source)
        {
            var target = new Image<Rgba32>(source.Width, source.Height);
            
            source.ProcessPixelRows(target, (sourceAccessor, targetAccessor) =>
            {
                for (int y = 0; y < sourceAccessor.Height; y++)
                {
                    var sourceRow = sourceAccessor.GetRowSpan(y);
                    var targetRow = targetAccessor.GetRowSpan(y);
                    
                    for (int x = 0; x < sourceAccessor.Width; x++)
                    {
                        var rgb = sourceRow[x];
                        targetRow[x] = new Rgba32(rgb.R, rgb.G, rgb.B, 255);
                    }
                }
            });
            
            return target;
        }

        private static Image<Rgba32> ConvertGrayToRgba32(Image<L8> source)
        {
            var target = new Image<Rgba32>(source.Width, source.Height);
            
            source.ProcessPixelRows(target, (sourceAccessor, targetAccessor) =>
            {
                for (int y = 0; y < sourceAccessor.Height; y++)
                {
                    var sourceRow = sourceAccessor.GetRowSpan(y);
                    var targetRow = targetAccessor.GetRowSpan(y);
                    
                    for (int x = 0; x < sourceAccessor.Width; x++)
                    {
                        var gray = sourceRow[x].PackedValue;
                        targetRow[x] = new Rgba32(gray, gray, gray, 255);
                    }
                }
            });
            
            return target;
        }

        private static Image<Rgba32> ConvertGrayAlphaToRgba32(Image<La16> source)
        {
            var target = new Image<Rgba32>(source.Width, source.Height);
            
            source.ProcessPixelRows(target, (sourceAccessor, targetAccessor) =>
            {
                for (int y = 0; y < sourceAccessor.Height; y++)
                {
                    var sourceRow = sourceAccessor.GetRowSpan(y);
                    var targetRow = targetAccessor.GetRowSpan(y);
                    
                    for (int x = 0; x < sourceAccessor.Width; x++)
                    {
                        var pixel = sourceRow[x];
                        targetRow[x] = new Rgba32(pixel.L, pixel.L, pixel.L, pixel.A);
                    }
                }
            });
            
            return target;
        }
    }
}