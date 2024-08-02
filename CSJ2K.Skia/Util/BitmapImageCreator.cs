// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

using System.Runtime.InteropServices;
using CSJ2K.j2k.image;

namespace CSJ2K.Util
{
    public class BitmapImageCreator : IImageCreator
    {
        #region FIELDS

        private static readonly IImageCreator Instance = new BitmapImageCreator();

        #endregion

        #region PROPERTIES

        public bool IsDefault
        {
            get
            {
                return false;
            }
        }

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

        public IImage Create(int width, int height, byte[] bytes)
        {
            return new BitmapImage(width, height, bytes);
        }

        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
            return BitmapImageSource.Create(imageObject);
        }

        #endregion
    }
}
