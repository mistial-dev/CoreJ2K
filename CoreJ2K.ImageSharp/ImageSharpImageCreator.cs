// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024 Sjofn LLC.
// Copyright (c) 2025 Mistial Developer.
// Licensed under the BSD 3-Clause License.

using CoreJ2K.j2k.image;
using CoreJ2K.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using JetBrains.Annotations;
using IImage = CoreJ2K.Util.IImage;

namespace CoreJ2K.ImageSharp
{
    public class ImageSharpImageCreator : IImageCreator
    {

        private static readonly IImageCreator Instance = new ImageSharpImageCreator();

        public bool IsDefault
        {
            get
            {
                return false;
            }
        }

        [PublicAPI]
        public static void Register()
        {
            ImageFactory.Register(Instance);
        }

        public IImage Create(int width, int height, int numComponents, byte[] bytes)
        {
            return new ImageSharpImage(width, height, numComponents, bytes);
        }

        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
            switch (imageObject)
            {
                case Image<Rgba32> img:
                    return new ImgReaderImageSharp(img);
                case Image<Rgb24> rgb:
                    return new ImgReaderImageSharp(rgb);
                case Image<L8> gray:
                    return new ImgReaderImageSharp(gray);
                case Image<La16> grayAlpha:
                    return new ImgReaderImageSharp(grayAlpha);
                default:
                    return null;
            }
        }

    }
}