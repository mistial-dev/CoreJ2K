/*
 *
 * Class:                   ImgReaderSkia
 *
 * Description:             Image writer for SKBitmap
 *
 **/

using System;
using SkiaSharp;

namespace CSJ2K.j2k.image.input
{
    public class ImgReaderSkia:ImgReader
    {
        /// <summary>DC offset value used when reading image </summary>
        private const int DC_OFFSET = 128;
        
	    /// <summary>The number of bits that determine the nominal dynamic range</summary>
	    private readonly int rb;
        
        /// <summary>Buffer for the components of each pixel(in the current block)</summary>
        private int[][] barr;
	    
        /// <summary>Data block used only to store coordinates of the buffered blocks</summary>
        private readonly DataBlkInt dbi = new DataBlkInt();
        
        /// <summary>Temporary DataBlkInt object (needed when encoder uses floating-point
        /// filters). This avoids allocating new DataBlk at each time.</summary>
        private DataBlkInt intBlk;

        private SKPixmap image;
        
        public ImgReaderSkia(SKBitmap image)
        {
            this.image = image.PeekPixels();
            w = image.Width;
            h = image.Height;
            rb = 8;
            nc = GetNumberOfComponents(image);
        }
        
        public override void Close()
        {
            image.Dispose();
            image = null;
            barr = null;
        }

        /// <summary> Returns the number of bits corresponding to the nominal range of the
        /// data in the specified component. This is the value rb (range bits) that
        /// was specified in the constructor, which normally is 8 for non bilevel
        /// data, and 1 for bi-level data.
        /// 
        /// If this number is <i>b</i> then the nominal range is between
        /// -2^(b-1) and 2^(b-1)-1, since unsigned data is level shifted to have a
        /// nominal average of 0.</summary>
        /// <param name="compIndex">The index of the component.</param>
        /// <returns> The number of bits corresponding to the nominal range of the
        /// data. For floating-point data this value is not applicable and the
        /// return value is undefined.</returns>
        public override int getNomRangeBits(int compIndex)
        {
            // Check component index
            if (compIndex < 0 || compIndex > this.nc)
                throw new ArgumentOutOfRangeException(nameof(compIndex) + " is out of range");
			
            return rb;
        }
        
        /// <summary> Returns the position of the fixed point in the specified component
        /// (i.e. the number of fractional bits), which is always 0 for this
        /// ImgReader.</summary>
        /// <param name="compIndex">The index of the component.</param>
        /// <returns> The position of the fixed-point (i.e. the number of fractional
        /// bits). Always 0 for this ImgReader.</returns>
        public override int GetFixedPoint(int compIndex)
        {
            // Check component index
            if (compIndex < 0 || compIndex > nc)
                throw new ArgumentOutOfRangeException(nameof(compIndex) + " is out of range");
            return 0;
        }

        /// <summary>
        /// Returns, in the blk argument, the block of image data containing the
        /// specified rectangular area, in the specified component. The data is
        /// returned, as a reference to the internal data, if any, instead of as a
        /// copy, therefore the returned data should not be modified.
        /// 
        /// After being read the coefficients are level shifted by subtracting
        /// 2^(nominal bit range - 1)
        /// 
        /// The rectangular area to return is specified by the 'ulx', 'uly', 'w'
        /// and 'h' members of the 'blk' argument, relative to the current
        /// tile. These members are not modified by this method. The 'offset' and
        /// 'scanw' of the returned data can be arbitrary. See the 'DataBlk' class.
        /// 
        /// If the data array in <tt>blk</tt> is <tt>null</tt>, then a new one
        /// is created if necessary. The implementation of this interface may
        /// choose to return the same array or a new one, depending on what is more
        /// efficient. Therefore, the data array in <tt>blk</tt> prior to the
        /// method call should not be considered to contain the returned data, a
        /// new array may have been created. Instead, get the array from
        /// <tt>blk</tt> after the method has returned.
        /// 
        /// The returned data always has its 'progressive' attribute unset
        /// (i.e. false).
        /// 
        /// When an I/O exception is encountered the JJ2KExceptionHandler is
        /// used. The exception is passed to its handleException method. The action
        /// that is taken depends on the action that has been registered in
        /// JJ2KExceptionHandler. See JJ2KExceptionHandler for details.
        /// 
        /// This method implements buffering for the 3 components: When the
        /// first one is asked, all the 3 components are read and stored until they
        /// are needed.
        /// </summary>
        /// <param name="blk">Its coordinates and dimensions specify the area to
        /// return. Some fields in this object are modified to return the data.</param>
        /// <param name="compIndex">The index of the component from which to get the data. Only 0,
        /// 1 and 3 are valid.</param>
        /// <returns> The requested DataBlk</returns>
        /// <seealso cref="GetCompData" />
        /// <seealso cref="JJ2KExceptionHandler" />
        public override DataBlk GetInternCompData(DataBlk blk, int compIndex)
        {
            // Check component index
            if (compIndex < 0 || compIndex > nc)
            {
                throw new ArgumentOutOfRangeException(nameof(compIndex) + " is out of range");
            }

            // Check type of block provided as an argument
            if (blk.DataType != DataBlk.TYPE_INT)
            {
                if (intBlk == null)
                {
                    intBlk = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
                }
                else
                {
                    intBlk.ulx = blk.ulx;
                    intBlk.uly = blk.uly;
                    intBlk.w = blk.w;
                    intBlk.h = blk.h;
                }
                blk = intBlk;
            }

            // If asking a component for the first time for this block, read all the components
            if ((barr == null) || (dbi.ulx > blk.ulx) || (dbi.uly > blk.uly) 
                || (dbi.ulx + dbi.w < blk.ulx + blk.w) || (dbi.uly + dbi.h < blk.uly + blk.h))
            {
                barr = new int[nc][];

                // Reset data arrays if necessary
                if (barr[compIndex] == null || barr[compIndex].Length < blk.w * blk.h)
                {
                    barr[compIndex] = new int[blk.w * blk.h];
                }
                blk.Data = barr[compIndex];

                var i = (compIndex + 1) % nc;
                if (barr[i] == null || barr[i].Length < blk.w * blk.h)
                {
                    barr[i] = new int[blk.w * blk.h];
                }
                i = (compIndex + 2) % nc;
                if (barr[i] == null || barr[i].Length < blk.w * blk.h)
                {
                    barr[i] = new int[blk.w * blk.h];
                }
                if (nc > 3)
                {
                    i = (compIndex + 3) % nc;
                    if (barr[i] == null || barr[i].Length < blk.w * blk.h)
                    {
                        barr[i] = new int[blk.w * blk.h];
                    }
                }

                // set attributes of the DataBlk used for buffering
                dbi.ulx = blk.ulx;
                dbi.uly = blk.uly;
                dbi.w = blk.w;
                dbi.h = blk.h;

                var red = barr[0];
                var green = barr[1];
                var blue = barr[2];
                var alpha = nc > 3 ? barr[3] : null;

                var pixelsAddr = image.GetPixels(blk.ulx, blk.uly);
                
                unsafe
                {
                    var ptr = (byte*)pixelsAddr.ToPointer();
                    
                    var k = 0;
                    for (var j = 0; j < blk.w * blk.h; ++j)
                    {
                        red[k] = (*(ptr + 0) & 0xFF) - DC_OFFSET;
                        green[k] = (*(ptr + 1) & 0xFF) - DC_OFFSET;
                        blue[k] = (*(ptr + 2) & 0xFF) - DC_OFFSET;
                        if (alpha != null) { alpha[k] = (*(ptr + 3) & 0xFF) - DC_OFFSET; }

                        ++k;
                        ptr += image.BytesPerPixel;
                    }
                }
                
                barr[0] = red;
                barr[1] = green;
                barr[2] = blue;
                if (alpha != null) { barr[3] = alpha; }

                // Set buffer attributes
                blk.Data = barr[compIndex];
                blk.offset = 0;
                blk.scanw = blk.w;
            }
            else
            {
                //Asking for the 2nd or 3rd (or 4th) block component
                blk.Data = barr[compIndex];
                blk.offset = (blk.ulx - dbi.ulx) * dbi.w + blk.ulx - dbi.ulx;
                blk.scanw = dbi.scanw;
            }

            // Turn off the progressive attribute
            blk.progressive = false;
            return blk;
        }
        
        /// <summary> Returns, in the blk argument, a block of image data containing the
		/// specified rectangular area, in the specified component. The data is
		/// returned, as a copy of the internal data, therefore the returned data
		/// can be modified "in place".
		/// 
		/// After being read the coefficients are level shifted by subtracting
		/// 2^(nominal bit range - 1)
		/// 
		/// The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' of
		/// the returned data is 0, and the 'scanw' is the same as the block's
		/// width. See the 'DataBlk' class.
		/// 
		/// If the data array in 'blk' is 'null', then a new one is created. If
		/// the data array is not 'null' then it is reused, and it must be large
		/// enough to contain the block's data. Otherwise an 'ArrayStoreException'
		/// or an 'IndexOutOfBoundsException' is thrown by the Java system.
		/// 
		/// The returned data has its 'progressive' attribute unset
		/// (i.e. false).
		/// 
		/// When an I/O exception is encountered the JJ2KExceptionHandler is
		/// used. The exception is passed to its handleException method. The action
		/// that is taken depends on the action that has been registered in
		/// JJ2KExceptionHandler. See JJ2KExceptionHandler for details.</summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to
		/// return. If it contains a non-null data array, then it must have the
		/// correct dimensions. If it contains a null data array a new one is
		/// created. The fields in this object are modified to return the data.</param>
		/// <param name="c">The index of the component from which to get the data. Only
		/// 0,1 and 2 are valid.</param>
		/// <returns>The requested DataBlk</returns>
		/// <seealso cref="GetInternCompData" />
        public override DataBlk GetCompData(DataBlk blk, int c)
        {
            // Check type of block provided as an argument
            if (blk.DataType != DataBlk.TYPE_INT)
            {
                var tmp = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
                blk = tmp;
            }

            var bakarr = (int[])blk.Data;
            // Save requested block size
            var ulx = blk.ulx;
            var uly = blk.uly;
            var width = blk.w;
            var height = blk.h;
            // Force internal data buffer to be different from external
            blk.Data = null;
            GetInternCompData(blk, c);
            // Copy the data
            if (bakarr == null)
            {
                bakarr = new int[width * height];
            }
            if (blk.offset == 0 && blk.scanw == width)
            {
                // Requested and returned block buffer are the same size
                // CONVERSION PROBLEM?
                Array.Copy((Array)blk.Data, 0, bakarr, 0, width * height);
            }
            else
            {
                // Requested and returned block are different
                for (var i = height - 1; i >= 0; i--)
                {
                    // copy line by line
                    // CONVERSION PROBLEM?
                    Array.Copy((Array)blk.Data, blk.offset + i * blk.scanw, bakarr, i * width, width);
                }
            }
            blk.Data = bakarr;
            blk.offset = 0;
            blk.scanw = blk.w;
            return blk;
        }

        /// <summary> Returns true if the data read was originally signed in the specified
        /// component, false if not. This method always returns false since PPM
        /// data is always unsigned.
        /// 
        /// </summary>
        /// <param name="compIndex">The index of the component, from 0 to N-1.
        /// 
        /// </param>
        /// <returns> always false, since PPM data is always unsigned.
        /// 
        /// </returns>
        public override bool IsOrigSigned(int compIndex)
        {
            // Check component index
            if (compIndex < 0 || compIndex > this.nc)
                throw new ArgumentOutOfRangeException(nameof(compIndex) + " is out of range");
            return false;
        }

        private int GetNumberOfComponents(SKBitmap bitmap)
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
                    return image.AlphaType > SKAlphaType.Opaque ? 4 : 3;
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
        
        /// <summary>Returns a string of information about the object, more than 1 line long.</summary>
        /// <returns> A string of information about the object.</returns>
        public override string ToString()
        {
            return "ImgReaderSkia: WxH = " + w + "x" + h + ", Component = 0,1,2";
        }
    }
}
