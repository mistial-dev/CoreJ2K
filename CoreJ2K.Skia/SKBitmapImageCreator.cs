// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using CoreJ2K.j2k.image;
using CoreJ2K.j2k.image.input;
using SkiaSharp;

namespace CoreJ2K.Util
{
    public class SKBitmapImageCreator : IImageCreator
    {
        #region FIELDS

        private static readonly IImageCreator Instance = new SKBitmapImageCreator();

        #endregion

        #region PROPERTIES

        public bool IsDefault => false;

        #endregion

        #region METHODS

        public static void Register()
        {
            ImageFactory.Register(Instance);
        }

        public IImage Create(int width, int height, byte[] bytes)
        {
            return new SKBitmapImage(width, height, bytes);
        }

        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
            switch (imageObject)
            {
                case SKBitmap bmp:
                    return new ImgReaderSkia(bmp);
                case SKPixmap pmp:
                    return new ImgReaderSkia(pmp);
                default:
                    return null;
            }
        }

        #endregion
    }
}