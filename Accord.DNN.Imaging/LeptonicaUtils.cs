﻿// -----------------------------------------------------------------------
// <copyright file="LeptonicaUtils.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
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
                    image.Stride,
                    pix.pixGetData(),
                    pix.Wpl);
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
                pix.Wpl,
                image.Bits,
                image.Stride);

            return image;
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Use Leptonica.net naming convention.")]
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

            int sx = Math.Max(minSize, Math.Min(64, w / 5));        // tile size in pixels
            int sy = Math.Max(minSize, Math.Min(128, h / 5));
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
        private static void CopyBits(int height, ulong[] src, int strideSrc, IntPtr dst, int strideDst)
        {
            /*if (strideDst == strideSrc)
            {
                NativeMethods.memcpy(bitsDst, bitsSrc, height * strideDst);
            }
            else
            {
                int count = Math.Min(strideDst, strideSrc);
                for (int i = 0; i < height; i++, bitsDst += strideDst, bitsSrc += strideSrc)
                {
                    NativeMethods.memcpy(bitsDst, bitsSrc, count);
                }
            }*/
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, IntPtr src, int strideSrc, ulong[] dst, int strideDst)
        {
            /*if (strideDst == strideSrc)
            {
                NativeMethods.memcpy(bitsDst, bitsSrc, height * strideDst);
            }
            else
            {
                int count = Math.Min(strideDst, strideSrc);
                for (int i = 0; i < height; i++, bitsDst += strideDst, bitsSrc += strideSrc)
                {
                    NativeMethods.memcpy(bitsDst, bitsSrc, count);
                }
            }*/
        }
    }
}
