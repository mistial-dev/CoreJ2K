/*
 *
 * Class:                   ImgWriterSkia
 *
 * Description:             Image writer for SKBitmap
 *
 **/

using System;
using SkiaSharp;

namespace CSJ2K.j2k.image.output
{
    public class ImgWriterSkia:ImgWriter
    {
        /// <summary>A DataBlk, just used to avoid allocating a new one each time it is needed </summary>
        private DataBlkInt db = new DataBlkInt();

        public ImgWriterSkia(SKBitmap outmap, BlkImgDataSrc imgSrc, int n1, int n2, int n3)
        {
            
        }
        
        /// <summary>
        /// Flushes the buffered data before the object is garbage collected. If an
        /// exception is thrown the object finalization is halted, but is otherwise
        /// ignored.
        /// </summary>
        /// <exception cref="IOException">If an I/O error occurs. It halts the
        /// finalization of the object, but is otherwise ignored.</exception>
        /// <seealso cref="Object.finalize" />
        ~ImgWriterSkia()
        {
            flush();
        }
        
        /// <summary>
        /// Closes the underlying file or network connection to where the data is
        /// written. The implementing class must write all buffered data before
        /// closing the file or resource. Any call to other methods of the class
        /// become illegal after a call to this one.
        /// </summary>
        /// <exception cref="IOException">If an I/O error occurs.</exception>
        public override void close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes all buffered data to the file or resource. If the implementing
        /// class does not use buffering, nothing should be done.
        /// </summary>
        /// <exception cref="IOException">If an I/O error occurs.</exception>
        public override void flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the source's current tile to the output. The requests of data
        /// issued by the implementing class to the source ImgData object should be
        /// done by blocks or strips, in order to reduce memory usage.
        /// 
        /// The implementing class should only write data that is not
        /// "progressive" (in other words that it is final), see DataBlk for
        /// details.
        /// </summary>
        /// <exception cref="IOException">If an I/O error occurs.</exception>
        /// <seealso cref="DataBlk" />
        public override void write()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the data of the specified area to the file, coordinates are
        /// relative to the current tile of the source.
        /// 
        /// The implementing class should only write data that is not
        /// "progressive" (in other words that is final), see DataBlk for
        /// details.
        /// </summary>
        /// <param name="ulx">The horizontal coordinate of the upper-left corner of the
        /// area to write, relative to the current tile.</param>
        /// <param name="uly">The vertical coordinate of the upper-left corner of the area
        /// to write, relative to the current tile.</param>
        /// <param name="width">The width of the area to write.</param>
        /// <param name="height">The height of the area to write.</param>
        /// <exception cref="IOException">If an I/O error occurs.</exception>
        public override void write(int ulx, int uly, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}