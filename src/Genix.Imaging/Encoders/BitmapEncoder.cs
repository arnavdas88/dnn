// -----------------------------------------------------------------------
// <copyright file="BitmapEncoder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Encoders
{
    using System;
    using System.IO;
    using System.Text;
    using Genix.Core;

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
            if (image.Width > BitmapEncoder.MaxAllowedWidth)
            {
                throw new InvalidOperationException("Cannot save the image. The image width is too large.");
            }

            if (image.Height > BitmapEncoder.MaxAllowedHeight)
            {
                throw new InvalidOperationException("Cannot save the image. The image height is too large.");
            }

            if ((long)image.Width * image.Height > BitmapEncoder.MaxAllowedPixels)
            {
                throw new InvalidOperationException("Cannot save the image. The number of pixels in the image is too large.");
            }

            int stride32 = ((image.Width * image.BitsPerPixel) + 31) / 32;
            int stride8 = stride32 * 4;
            int imagesize = stride8 * image.Height;
            int ncolors = image.BitsPerPixel <= 8 ? 1 << image.BitsPerPixel : 0;

            int offbytes = 14 /* file header */ + 40 /* bmp header */ + (ncolors * 4) /* colors */;
            int fsize = offbytes + imagesize;

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
                writer.Write(0);    // compression (0 == uncompressed)
                writer.Write(imagesize);
                writer.Write((int)((39.37 * image.HorizontalResolution) + 0.5));
                writer.Write((int)((39.37 * image.VerticalResolution) + 0.5));
                writer.Write(ncolors);
                writer.Write(ncolors);

                // write colors if necessary
                if (ncolors > 0)
                {
                    Color[] palette = Image.CreatePalette(image.BitsPerPixel);
                    for (int i = 0, ii = palette.Length; i < ii; i++)
                    {
                        writer.Write(palette[i].Argb);
                    }
                }

                // write bits
                byte[] bitsdst = new byte[imagesize];
                unsafe
                {
                    // positive height indicates that bitmap is bottom-up
                    fixed (ulong* src = &image.Bits[(image.Height - 1) * image.Stride])
                    {
                        fixed (byte* dst = bitsdst)
                        {
                            Arrays.CopyStrides(image.Height, new IntPtr(src), -image.Stride8, new IntPtr(dst), stride8);

                            if (image.BitsPerPixel < 8)
                            {
                                // make bits big-endian
                                BitUtils32.BitSwap(image.Height * stride32, image.BitsPerPixel, new IntPtr(dst));
                            }
                        }
                    }
                }

                writer.Write(bitsdst);
                writer.Flush();
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
    }
}
