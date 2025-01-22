// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024-2025 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using CoreJ2K.j2k.image;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace CoreJ2K.Util
{
    internal class WindowsBitmapImageSource : PortableImageSource
    {
        #region CONSTRUCTORS

        private WindowsBitmapImageSource(Bitmap bitmap)
            : base(
                bitmap.Width,
                bitmap.Height,
                GetNumberOfComponents(bitmap.PixelFormat),
                GetRangeBits(bitmap.PixelFormat),
                GetSignedArray(bitmap.PixelFormat),
                GetComponents(bitmap))
        {
        }

        #endregion

        #region METHODS


        internal static BlkImgDataSrc Create(object imageObject)
        {
            return !(imageObject is Bitmap bitmap) ? null : new WindowsBitmapImageSource(bitmap);
        }

        private static int GetNumberOfComponents(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 3;
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Format64bppArgb:
                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelFormat));
            }
        }

        private static int GetRangeBits(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format16bppGrayScale:
                    return 16;
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format4bppIndexed:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 8;
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppPArgb:
                case PixelFormat.Format64bppArgb:
                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelFormat));
            }
        }

        private static bool[] GetSignedArray(PixelFormat pixelFormat)
        {
            return Enumerable.Repeat(false, GetNumberOfComponents(pixelFormat)).ToArray();
        }

        private static int[][] GetComponents(Bitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;
            var nc = GetNumberOfComponents(bitmap.PixelFormat);

            var comps = new int[nc][];
            for (var c = 0; c < nc; ++c) comps[c] = new int[w * h];

            for (int y = 0, xy = 0; y < h; ++y)
            {
                for (var x = 0; x < w; ++x, ++xy)
                {
                    var color = bitmap.GetPixel(x, y);
                    for (var c = 0; c < nc; ++c)
                    {
                        comps[c][xy] = c == 0 ? color.R : c == 1 ? color.G : color.B;
                    }
                }
            }

            return comps;
        }

        #endregion
    }
}