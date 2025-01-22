// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024-2025 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using System.Drawing.Imaging;
using System.Drawing;

namespace CoreJ2K.Util
{
    internal class WindowsBitmapImage : ImageBase<Image>
    {
        #region CONSTRUCTORS

        internal WindowsBitmapImage(int width, int height, byte[] bytes)
            : base(width, height, bytes)
        {
        }

        #endregion

        #region METHODS

        protected override object GetImageObject()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            var dstdata = bitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat);

            var ptr = dstdata.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(Bytes, 0, ptr, Bytes.Length);
            bitmap.UnlockBits(dstdata);

            return bitmap;
        }

        #endregion
    }
}
