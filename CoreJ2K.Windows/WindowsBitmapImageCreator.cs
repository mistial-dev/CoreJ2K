// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024-2025 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using CoreJ2K.j2k.image;
using System.Runtime.InteropServices;

namespace CoreJ2K.Util
{
    public class WindowsBitmapImageCreator : IImageCreator
    {
        #region FIELDS

        private static readonly IImageCreator Instance = new WindowsBitmapImageCreator();

        #endregion

        #region PROPERTIES

        public bool IsDefault => false;

        #endregion

        #region METHODS

        public static void Register()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ImageFactory.Register(Instance);
            }
            else
            {
                throw new System.ComponentModel.WarningException(
                    "Cannot register BitmapImageCreator as a provider on non-Windows platforms");
            }
        }

        public IImage Create(int width, int height, int numComponents, byte[] bytes)
        {
            return new WindowsBitmapImage(width, height, numComponents, bytes);
        }

        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
            return WindowsBitmapImageSource.Create(imageObject);
        }

        #endregion
    }
}
