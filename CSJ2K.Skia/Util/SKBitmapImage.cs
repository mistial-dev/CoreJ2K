// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2024 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using System.Runtime.InteropServices;
using SkiaSharp;

namespace CSJ2K.Util
{
    internal class SKBitmapImage : ImageBase<SKBitmap>
    {
        internal SKBitmapImage(int width, int height, byte[] bytes)
            : base(width, height, bytes)
        { }

        protected override object GetImageObject()
        {
            var bitmap = new SKBitmap();

            var gcHandle = GCHandle.Alloc(Bytes, GCHandleType.Pinned);
            var info = new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
            bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes,
                delegate { gcHandle.Free(); }, null);

            return bitmap;
        }
    }
}