// -----------------------------------------------------------------------
// <copyright file="LeptonicaUtils.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if false
namespace Genix.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Leptonica;

    /// <summary>
    /// Provides integration with Leptonica.NET library.
    /// </summary>
    internal static class LeptonicaUtils
    {
        /// <summary>
        /// Creates a new Leptonica's <see cref="Pix"/> object out of the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/>.</param>
        /// <returns>
        /// The <see cref="Pix"/> object this method creates.
        /// </returns>
        public static Pix CreatePix(this Image image)
        {
            Pix pix = Pix.Create(image.Width, image.Height, image.BitsPerPixel);
            try
            {
                pix.pixSetResolution(image.HorizontalResolution, image.VerticalResolution);

                LeptonicaUtils.CopyBits(
                    image.Height,
                    image.Bits,
                    image.Stride8,
                    pix.pixGetData(),
                    pix.Wpl * sizeof(uint),
                    image.BitsPerPixel == 1);
            }
            catch
            {
                pix.Dispose();
                throw;
            }

            return pix;
        }

        public static Image CreateImage(this Pix pix, int xres, int yres)
        {
            Image image = new Image(
                pix.Width,
                pix.Height,
                pix.Depth,
                xres,
                yres);

            LeptonicaUtils.CopyBits(
                image.Height,
                pix.pixGetData(),
                pix.Wpl * sizeof(uint),
                image.Bits,
                image.Stride8,
                image.BitsPerPixel == 1);

            return image;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Pix pixOtsu(this Pix pixs, bool adaptiveThreshold)
        {
            pixs.pixGetDimensions(out int w, out int h, out int d);

            /*if (w < 16 || h < 16)
            {
                adaptiveThreshold = false;
            }*/

            if (w < 32 || h < 32)
            {
                adaptiveThreshold = true;
            }

            int minSize = adaptiveThreshold ? 16 : 4;

            int sx = MinMax.Max(minSize, MinMax.Min(64, w / 5));        // tile size in pixels
            int sy = MinMax.Max(minSize, MinMax.Min(128, h / 5));
            const int Thresh = 100;                                 // threshold for determining foreground
            int mincount = sx * sy / 3;                             // min threshold on counts in a tile
            const int Smoothx = 2;                                  // half-width of block convolution kernel width
            const int Smoothy = 2;                                  // half-width of block convolution kernel height
            const float Scorefact = 0.0f;                           // fraction of the max Otsu score; typ. 0.1

            if (adaptiveThreshold)
            {
                Pix pixth = null;
                try
                {
                    pixs.pixOtsuAdaptiveThreshold(sx, sy, Smoothx, Smoothy, Scorefact, out pixth, out Pix pixd);
                    return pixd;
                }
                finally
                {
                    pixth?.Dispose();
                }
            }
            else
            {
                return pixs.pixOtsuThreshOnBackgroundNorm(null, sx, sy, Thresh, mincount, 255, Smoothx, Smoothy, Scorefact, out int pthresh);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, ulong[] src, int strideSrc, IntPtr dst, int strideDst, bool swapBits)
        {
            unsafe
            {
                uint* udst = (uint*)dst;

                if (strideDst == strideSrc)
                {
                    NativeMethods.copy_m2u(height * strideSrc, src, 0, udst, 0);
                }
                else
                {
                    for (int i = 0, offsrc = 0, offdst = 0; i < height; i++, offsrc += strideSrc, offdst += strideDst)
                    {
                        NativeMethods.copy_m2u(strideDst, src, offsrc, udst, offdst);
                    }
                }

                NativeMethods.bytesswap_ip_32(height * strideDst / sizeof(uint), udst, 0);

                if (swapBits)
                {
                    NativeMethods.bits_reverse_ip_32(height * strideDst / sizeof(uint), udst, 0);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, IntPtr src, int strideSrc, ulong[] dst, int strideDst, bool swapBits)
        {
            unsafe
            {
                uint* usrc = (uint*)src;

                // !!!! after this operation Leptonica image is invalid
                NativeMethods.bytesswap_ip_32(height * strideSrc / sizeof(uint), usrc, 0);

                if (strideDst == strideSrc)
                {
                    NativeMethods.copy_u2m(height * strideSrc, usrc, 0, dst, 0);
                }
                else
                {
                    for (int i = 0, offsrc = 0, offdst = 0; i < height; i++, offsrc += strideSrc, offdst += strideDst)
                    {
                        NativeMethods.copy_u2m(strideSrc, usrc, offsrc, dst, offdst);
                    }
                }
            }

            ////BitUtils64.BiteSwap(dst.Length, dst, 0);

            if (swapBits)
            {
                BitUtils64.BitSwap(dst.Length, dst, 0);
            }
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName, EntryPoint = "copy_s8")]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void copy_u2m(int n, [In] uint* x, int offx, [Out] ulong[] dst, int offy);

            [DllImport(NativeMethods.DllName, EntryPoint = "copy_s8")]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void copy_m2u(int n, [In] ulong[] x, int offx, [Out] uint* dst, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void bytesswap_ip_32(int n, [In, Out] uint* xy, int offxy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void bits_reverse_ip_32(int length, [In, Out] uint* xy, int offxy);
        }
    }
}
#endif