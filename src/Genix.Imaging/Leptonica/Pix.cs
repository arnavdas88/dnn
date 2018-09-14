// -----------------------------------------------------------------------
// <copyright file="Pix.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Leptonica
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Genix.Core;

    /// <summary>
    /// Represents the Leptonica's Pix object.
    /// </summary>
    public sealed partial class Pix : DisposableObject
    {
        /// <summary>
        /// Creates a new Leptonica's <see cref="Pix"/> object from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// The <see cref="Pix"/> object this method creates.
        /// </returns>
        public static Pix FromImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            SafePixHandle handle = NativeMethods.pixCreate(image.Width, image.Height, image.BitsPerPixel);
            try
            {
                NativeMethods.pixSetResolution(handle, image.HorizontalResolution, image.VerticalResolution);
                IntPtr dst = NativeMethods.pixGetData(handle);
                int wpl = NativeMethods.pixGetWpl(handle);

                unsafe
                {
                    fixed (ulong* src = image.Bits)
                    {
                        Arrays.CopyStrides(image.Height, new IntPtr(src), image.Stride8, dst, wpl * sizeof(uint));

                        int count = image.Height * wpl;
                        BitUtils32.BiteSwap(count, dst);

                        if (image.BitsPerPixel < 8)
                        {
                            BitUtils32.BitSwap(count, image.BitsPerPixel, dst);
                        }
                    }
                }
            }
            catch
            {
                handle?.Dispose();
                throw;
            }

            return new Pix(handle);
        }

#if false
        /// <summary>
        /// Creates an <see cref="System.Drawing.Bitmap"/> object from this Leptonica's <see cref="Pix"/> object.
        /// </summary>
        /// <returns>
        /// The <see cref="System.Drawing.Bitmap"/> this method creates.
        /// </returns>
        public System.Drawing.Bitmap ToBitmap()
        {
            if (NativeMethods.pixWriteMemBmp(out IntPtr data, out IntPtr size, this.handle) != 0)
            {
                throw new InvalidOperationException("Cannot convert pix object to bitmap.");
            }

            try
            {
                unsafe
                {
                    byte[] bytes = new byte[size.ToInt64()];
                    Marshal.Copy(data, bytes, 0, bytes.Length);
                    File.WriteAllBytes("d:\\xxxxx.bmp", bytes);

                    ////return System.Drawing.Bitmap.FromFile("d:\\xxxxx.bmp") as System.Drawing.Bitmap;

                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        return System.Drawing.Bitmap.FromStream(stream, true, true) as System.Drawing.Bitmap;
                        ////return new System.Drawing.Bitmap(stream);
                    }

                    /*using (UnmanagedMemoryStream stream = new UnmanagedMemoryStream((byte*)data.ToPointer(), size.ToInt64()))
                    {
                        return System.Drawing.Bitmap.FromStream(stream) as System.Drawing.Bitmap;
                    }*/
                }
            }
            finally
            {
                NativeMethods.lept_free(data);
            }
        }
#endif

        /// <summary>
        /// Creates an <see cref="Image"/> object from this Leptonica's <see cref="Pix"/> object.
        /// </summary>
        /// <returns>
        /// The <see cref="Image"/> object this method creates.
        /// </returns>
        public Image ToImage()
        {
            NativeMethods.pixGetDimensions(this.handle, out int w, out int h, out int d);
            NativeMethods.pixGetResolution(this.handle, out int xres, out int yres);
            IntPtr src = NativeMethods.pixGetData(this.handle);
            int wpl = NativeMethods.pixGetWpl(this.handle);

            Image image = new Image(w, h, d, xres == 0 ? 200 : xres, yres == 0 ? 200 : yres);

            unsafe
            {
                fixed (ulong* bits = image.Bits)
                {
                    IntPtr dst = new IntPtr(bits);
                    Arrays.CopyStrides(image.Height, src, wpl * sizeof(uint), dst, image.Stride8);

                    int count = image.Bits.Length * 2; // work with 32-bit words
                    BitUtils32.BiteSwap(count, dst);

                    if (image.BitsPerPixel < 8)
                    {
                        BitUtils32.BitSwap(count, image.BitsPerPixel, dst);
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// Converts this <see cref="Pix"/> from gray scale to black-and-white using Otsu binarization algorithm.
        /// </summary>
        /// <param name="adaptiveThreshold">Indicates whether to use adaptive threshold algorithm.</param>
        /// <returns>
        /// A new binary <see cref="Pix"/>.
        /// </returns>
        public Pix BinarizeOtsu(bool adaptiveThreshold)
        {
            NativeMethods.pixGetDimensions(this.handle, out int w, out int h, out int d);

            /*if (w < 16 || h < 16)
            {
                adaptiveThreshold = false;
            }*/

            if (w < 32 || h < 32)
            {
                adaptiveThreshold = true;
            }

            int minSize = adaptiveThreshold ? 16 : 4;

            int sx = Core.MinMax.Max(minSize, Core.MinMax.Min(64, w / 5));        // tile size in pixels
            int sy = Core.MinMax.Max(minSize, Core.MinMax.Min(128, h / 5));
            const int Thresh = 100;                                 // threshold for determining foreground
            int mincount = sx * sy / 3;                             // min threshold on counts in a tile
            const int Smoothx = 2;                                  // half-width of block convolution kernel width
            const int Smoothy = 2;                                  // half-width of block convolution kernel height
            const float Scorefact = 0.0f;                           // fraction of the max Otsu score; typ. 0.1

            SafePixHandle pixd;
            if (adaptiveThreshold)
            {
                NativeMethods.pixOtsuAdaptiveThreshold(this.handle, sx, sy, Smoothx, Smoothy, Scorefact, out SafePixHandle pixth, out pixd);
                pixth?.Dispose();
            }
            else
            {
                pixd = NativeMethods.pixOtsuThreshOnBackgroundNorm(this.handle, null, sx, sy, Thresh, mincount, 255, Smoothx, Smoothy, Scorefact, out int thresh);
            }

            if (pixd == null || pixd.IsInvalid)
            {
                throw new InvalidOperationException();
            }

            return new Pix(pixd);
        }

        /// <summary>
        /// Finds connected components on this <see cref="Pix"/>.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <returns>
        /// The <see cref="Boxa"/> that contains bounding boxes for found c.c.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Boxa FindConnectedComponents(int connectivity)
        {
            SafeBoxaHandle boxaHandle = NativeMethods.pixConnComp(this.handle, out SafePixaHandle pixaHandle, connectivity);
            pixaHandle?.Dispose();
            return boxaHandle.IsInvalid ? null : new Boxa(boxaHandle);
        }

        /// <summary>
        /// Finds connected components on this <see cref="Pix"/>.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="pixa">The <see cref="Boxa"/> that contains c.c. images.</param>
        /// <returns>
        /// The <see cref="Boxa"/> that contains bounding boxes for found c.c.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Boxa FindConnectedComponents(int connectivity, out Pixa pixa)
        {
            SafeBoxaHandle boxaHandle = NativeMethods.pixConnComp(this.handle, out SafePixaHandle pixaHandle, connectivity);
            pixa = pixaHandle.IsInvalid ? null : new Pixa(pixaHandle);
            return boxaHandle.IsInvalid ? null : new Boxa(boxaHandle);
        }

        /// <summary>
        /// Computes the distance of each pixel from the nearest background pixel.
        /// </summary>
        /// <param name="connectivity">The pixel connectivity (4 or 8).</param>
        /// <param name="outdepth">The depth of destination <see cref="Image"/> (8 or 16).</param>
        /// <param name="boundcond">The boundary conditions (1 for L_BOUNDARY_BG, 2, for L_BOUNDARY_FG).</param>
        /// <returns>The destination <see cref="Pix"/> this method creates.</returns>
        public Pix DistanceFunction(int connectivity, int outdepth, int boundcond)
        {
            return new Pix(NativeMethods.pixDistanceFunction(this.handle, connectivity, outdepth, boundcond));
        }
    }
}
