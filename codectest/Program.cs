// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

using System.Runtime.InteropServices;
using CSJ2K.Util;

namespace codectest
{
    using System;
    using System.IO;
    using SkiaSharp;

    using CSJ2K;

    internal class Program
    {
        private static void Main(string[] args)
        {
            SKBitmapImageCreator.Register();
            
            File.Delete("file11.jp2");
            File.Delete("file12.jp2");
            File.Delete("file13.jp2");
            File.Delete("file14.jp2");
            File.Delete("file15.jp2");
            foreach (var f in Directory.EnumerateFiles(".",$"file*_histogram.png"))
            {
                File.Delete(f);
            }

            using (var ppm = File.OpenRead("a1_mono.ppm"))
            {
                var enc = J2kImage.ToBytes(J2kImage.CreateEncodableSource(ppm));
                File.WriteAllBytes("file11.jp2", enc);
            }

            using (var ppm = File.OpenRead("a2_colr.ppm"))
            {
                var enc = J2kImage.ToBytes(J2kImage.CreateEncodableSource(ppm));
                File.WriteAllBytes("file12.jp2", enc);
            }

            using (var pgx = File.OpenRead("c1p0_05_0.pgx"))
            {
                var enc = J2kImage.ToBytes(J2kImage.CreateEncodableSource(pgx));
                File.WriteAllBytes("file13.jp2", enc);
            }
            
            using (var bitmap = SKBitmap.Decode("racoon.png"))
            {
                var src = J2kImage.CreateEncodableSource(bitmap);
                var enc = J2kImage.ToBytes(src);
                File.WriteAllBytes("file14.jp2", enc);
            }
            using (var bitmap = SKBitmap.Decode("dog.jpeg"))
            {
                var src = J2kImage.CreateEncodableSource(bitmap);
                var enc = J2kImage.ToBytes(src);
                File.WriteAllBytes("file15.jp2", enc);
            }

            for (var i = 1; i <= 15; i++)
            {
                try
                {
                    SKBitmap image;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var timer = new HiPerfTimer();
                        timer.Start();
                        image = J2kImage.FromFile($"file{i}.jp2").As<SKBitmap>();
                        timer.Stop();
                        Console.WriteLine($"file{i}: {timer.Duration} seconds");
                    }
                    else
                    {
                        image = J2kImage.FromFile($"file{i}.jp2").As<SKBitmap>();
                    }

                    var histogram = GenerateHistogram(image);
                    var encoded = histogram.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes($"file{i}_histogram.png", encoded.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"file{i}:\r\n{e.Message}");
                    if (e.InnerException != null)
                    {
                        Console.WriteLine(e.InnerException.Message);
                        Console.WriteLine(e.InnerException.StackTrace);
                    }
                    else Console.WriteLine(e.StackTrace);

                }
            }
        }

        private static SKBitmap GenerateHistogram(SKBitmap image)
        {
            var histogram = new SKBitmap(256, 100, true);

            var colorcounts = new int[256];

            // This is ungodly slow, but it's just for diagnostics.
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var c = image.GetPixel(x, y);
                    colorcounts[c.Red]++;
                    colorcounts[c.Green]++;
                    colorcounts[c.Blue]++;
                }
            }

            var maxval = 0;
            for (var i = 0; i < 256; i++) if (colorcounts[i] > maxval) maxval = colorcounts[i];
            for (var i = 1; i < 255; i++)
            {
                //Console.WriteLine(i + ": " + histogram[i] + "," + (((float)histogram[i] / (float)maxval) * 100F));
                colorcounts[i] = (int)Math.Round((colorcounts[i] / (double)maxval) * 100D);
            }
            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < 100; y++)
                {
                    histogram.SetPixel(x, y, colorcounts[x] >= (100 - y) ? SKColors.Black : SKColors.White);
                }
            }
            return histogram;
        }
    }
}
