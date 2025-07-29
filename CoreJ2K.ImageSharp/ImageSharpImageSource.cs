// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024-2025 Sjofn LLC.
// Copyright (c) 2025 Mistial Developer.
// Licensed under the BSD 3-Clause License.

using System.Linq;
using CoreJ2K.j2k.image;
using CoreJ2K.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CoreJ2K.ImageSharp
{
    internal class ImageSharpImageSource : PortableImageSource
    {

        private ImageSharpImageSource(Image image, int numComponents, int rangeBits, bool[] signed, int[][] components)
            : base(
                image.Width,
                image.Height,
                numComponents,
                rangeBits,
                signed,
                components)
        {
        }

        internal static BlkImgDataSrc Create(object imageObject)
        {
            switch (imageObject)
            {
                case Image<Rgba32> rgba32:
                    return CreateFromRgba32(rgba32);
                case Image<Rgb24> rgb24:
                    return CreateFromRgb24(rgb24);
                case Image<L8> l8:
                    return CreateFromL8(l8);
                case Image<La16> la16:
                    return CreateFromLa16(la16);
                default:
                    return null;
            }
        }

        private static ImageSharpImageSource CreateFromRgba32(Image<Rgba32> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 4;
            var rangeBits = 8;
            var signed = Enumerable.Repeat(false, nc).ToArray();
            var components = GetRgba32Components(image);

            return new ImageSharpImageSource(image, nc, rangeBits, signed, components);
        }

        private static ImageSharpImageSource CreateFromRgb24(Image<Rgb24> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 3;
            var rangeBits = 8;
            var signed = Enumerable.Repeat(false, nc).ToArray();
            var components = GetRgb24Components(image);

            return new ImageSharpImageSource(image, nc, rangeBits, signed, components);
        }

        private static ImageSharpImageSource CreateFromL8(Image<L8> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 1;
            var rangeBits = 8;
            var signed = Enumerable.Repeat(false, nc).ToArray();
            var components = GetL8Components(image);

            return new ImageSharpImageSource(image, nc, rangeBits, signed, components);
        }

        private static ImageSharpImageSource CreateFromLa16(Image<La16> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 2;
            var rangeBits = 8;
            var signed = Enumerable.Repeat(false, nc).ToArray();
            var components = GetLa16Components(image);

            return new ImageSharpImageSource(image, nc, rangeBits, signed, components);
        }

        private static int[][] GetRgba32Components(Image<Rgba32> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 4;

            var comps = new int[nc][];
            for (var c = 0; c < nc; ++c) comps[c] = new int[w * h];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0, xy = 0; y < h; ++y)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (var x = 0; x < w; ++x, ++xy)
                    {
                        var pixel = pixelRow[x];
                        comps[0][xy] = pixel.R;
                        comps[1][xy] = pixel.G;
                        comps[2][xy] = pixel.B;
                        comps[3][xy] = pixel.A;
                    }
                }
            });

            return comps;
        }

        private static int[][] GetRgb24Components(Image<Rgb24> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 3;

            var comps = new int[nc][];
            for (var c = 0; c < nc; ++c) comps[c] = new int[w * h];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0, xy = 0; y < h; ++y)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (var x = 0; x < w; ++x, ++xy)
                    {
                        var pixel = pixelRow[x];
                        comps[0][xy] = pixel.R;
                        comps[1][xy] = pixel.G;
                        comps[2][xy] = pixel.B;
                    }
                }
            });

            return comps;
        }

        private static int[][] GetL8Components(Image<L8> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 1;

            var comps = new int[nc][];
            for (var c = 0; c < nc; ++c) comps[c] = new int[w * h];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0, xy = 0; y < h; ++y)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (var x = 0; x < w; ++x, ++xy)
                    {
                        var pixel = pixelRow[x];
                        comps[0][xy] = pixel.PackedValue;
                    }
                }
            });

            return comps;
        }

        private static int[][] GetLa16Components(Image<La16> image)
        {
            var w = image.Width;
            var h = image.Height;
            var nc = 2;

            var comps = new int[nc][];
            for (var c = 0; c < nc; ++c) comps[c] = new int[w * h];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0, xy = 0; y < h; ++y)
                {
                    var pixelRow = accessor.GetRowSpan(y);
                    for (var x = 0; x < w; ++x, ++xy)
                    {
                        var pixel = pixelRow[x];
                        comps[0][xy] = pixel.L;
                        comps[1][xy] = pixel.A;
                    }
                }
            });

            return comps;
        }

    }
}