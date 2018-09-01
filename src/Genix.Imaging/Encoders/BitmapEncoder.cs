// -----------------------------------------------------------------------
// <copyright file="BitmapEncoder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Encoders
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class BitmapEncoder
    {
        /// <summary>
        /// The file signature.
        /// </summary>
        private const ushort Signature = 0x4d42;

        /// <summary>
        /// Limitations.
        /// </summary>
        private const int MaxAllowedWidth = 1000000;
        private const int MaxAllowedHeight = 1000000;
        private const long MaxAllowedPixels = 400000000L;

        public void Save(Stream stream, Image image)
        {
            uint fsize = 0;
            uint offbytes = 0;
            uint imagesize = 0;
            int ncolors = image.BitsPerPixel <= 8 ? 1 << image.BitsPerPixel : 0;

            if (image.Width > BitmapEncoder.MaxAllowedWidth)
            {
                throw new InvalidOperationException("Cannot save the image. The image width is too large.");
            }

            if (image.Height > BitmapEncoder.MaxAllowedHeight)
            {
                throw new InvalidOperationException("Cannot save the image. The image height is too large.");
            }

            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                // write file header
                writer.Write(BitmapEncoder.Signature);
                writer.Write(fsize);
                writer.Write(0U);
                writer.Write(offbytes);

                // write header
                writer.Write(40);   // size of header
                writer.Write(image.Width);
                writer.Write(image.Height);
                writer.Write((ushort)1);
                writer.Write((ushort)image.BitsPerPixel);
                writer.Write(imagesize);
                writer.Write((int)((39.37 * image.HorizontalResolution) + 0.5));
                writer.Write((int)((39.37 * image.VerticalResolution) + 0.5));
                writer.Write(ncolors);
                writer.Write(ncolors);

                // write colors if necessary
                if (ncolors > 0)
                {
                }

                // write image data
                if (image.BitsPerPixel <= 8)
                {
                    /*ulong[] bits = image.Bits;
                    int stride8 = image.Stride8;
                    data = (l_uint8*)pixGetData(pix) + pixBpl * (h - 1);
                    for (int i = 0, ii = image.Height, offset = ; i < ii; i++)
                    {
                        writer.Write()
                        memcpy(fmdata, data, fBpl);
                        data -= pixBpl;
                        fmdata += fBpl;
                    }*/
                }
            }
        }

        /*[StructLayout(LayoutKind.Sequential)]
        private struct BMPINFOHEADER
        {
            public int biSize;          // size of the BMP_InfoHeader struct
            public int biWidth;         // bitmap width in pixels
            public int biHeight;        // bitmap height in pixels
            public short biPlanes;      // number of bitmap planes
            public short biBitCount;    // number of bits per pixel
            public int biCompression;   // compress format (0 == uncompressed)
            public int biSizeImage;     // size of image in bytes
            public int biXPelsPerMeter; // pixels per meter in x direction
            public int biYPelsPerMeter; // pixels per meter in y direction
            public int biClrUsed;       // number of colors used
            public int biClrImportant;  // number of important colors used
        };*/

        ////typedef struct BMP_InfoHeader  BMP_IH;

        /*! Number of bytes in a BMP info header */
        ////#define BMP_IHBYTES  sizeof(BMP_IH)
    }
}
