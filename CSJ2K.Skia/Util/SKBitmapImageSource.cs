// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using System;
using System.Linq;
using CSJ2K.j2k.image;
using SkiaSharp;

namespace CSJ2K.Util
{
    public class SKBitmapImageSource : PortableImageSource
    {
        
#region CONSTRUCTORS

        private SKBitmapImageSource(SKBitmap bitmap) 
            : base(bitmap.Width, bitmap.Height
            , GetNumberOfComponents(bitmap)
            , bitmap.Info.BytesPerPixel
            , GetSignedArray(bitmap)
            , GetComponents(bitmap))
        { }

        #endregion

        #region METHODS


        internal static BlkImgDataSrc Create(object imageObject)
        {
            return !(imageObject is SKBitmap bitmap) ? null : new SKBitmapImageSource(bitmap);
        }

        private static int GetNumberOfComponents(SKBitmap bitmap)
        {
            switch (bitmap.ColorType)
            {
                case SKColorType.Alpha8:
                case SKColorType.Alpha16:
                case SKColorType.Gray8:
                    return 1;
                case SKColorType.Rg88:
                case SKColorType.Rg1616:
                    return 2;
                case SKColorType.Rgb888x:
                case SKColorType.Rgb565:
                case SKColorType.Rgb101010x:
                case SKColorType.Bgr101010x:
                    return 3;
                case SKColorType.Argb4444:
                case SKColorType.Bgra8888:
                case SKColorType.Rgba8888:
                case SKColorType.Rgba1010102:
                case SKColorType.Bgra1010102:
                case SKColorType.Rgba16161616:
                    return bitmap.AlphaType > SKAlphaType.Opaque ? 4 : 3;
                case SKColorType.RgbaF16:
                case SKColorType.RgbaF16Clamped:
                case SKColorType.RgbaF32:
                case SKColorType.RgF16:
                case SKColorType.AlphaF16:
                    throw new ArgumentException("Floating point color types unsupported at this time.");
                case SKColorType.Unknown:
                default:
                    throw new ArgumentException(
                        "Image colortype is unknown, number of components cannot be determined.");
            }
        }

        public static int[][] GetComponents(SKBitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;
            var nc = GetNumberOfComponents(bitmap);

            var comps = new int[nc][];
            for (var c = 0; c < nc; ++c) { comps[c] = new int[w * h]; }

            for (int y = 0, xy = 0; y < h; ++y)
            {
                for (var x = 0; x < w; ++x, ++xy)
                {
                    var color = bitmap.GetPixel(x, y);
                    for (var c = 0; c < nc; ++c)
                    {
                        comps[c][xy] = c == 0 ? color.Red : c == 1 ? color.Green : color.Blue;
                    }
                }
            }

            return comps;
        }

        private static bool[] GetSignedArray(SKBitmap bitmap)
        {
            return Enumerable.Repeat(false, GetNumberOfComponents(bitmap)).ToArray();
        }
        
        #endregion
    }
}