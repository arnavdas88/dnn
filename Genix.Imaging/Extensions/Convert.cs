// -----------------------------------------------------------------------
// <copyright file="Convert.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Leptonica;

    /// <summary>
    /// Provides conversion extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to binarize.</param>
        /// <returns>
        /// A new binarized <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Binarize(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }

            try
            {
                using (Pix pixs = image.CreatePix())
                {
                    using (Pix pixd = pixs.pixOtsu(false))
                    {
                        return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot binarize the image.", e);
            }
        }

        /// <summary>
        /// Converts this <see cref="Image"/> from black-and-white to gray scale.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to convert.</param>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Convert1To8(this Image image, byte value0, byte value1)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            int width = image.Width;
            int height = image.Height;

            ulong[] bitssrc = image.Bits;
            ulong[] bitsdst = dst.Bits;

            int stridesrc = image.Stride;
            int stridedst = dst.Stride;

            // build conversion table
            uint[] values = { value0, value1 };
            ulong[] map = new ulong[256];
            for (int i = 0; i < 256; i++)
            {
                map[i] = (values[(i >> 7) & 1] << 56) |
                         (values[(i >> 6) & 1] << 48) |
                         (values[(i >> 5) & 1] << 40) |
                         (values[(i >> 4) & 1] << 32) |
                         (values[(i >> 3) & 1] << 24) |
                         (values[(i >> 2) & 1] << 16) |
                         (values[(i >> 1) & 1] << 8) |
                         values[i & 1];
            }

            for (int y = 0, offysrc = 0, offydst = 0; y < height; y++, offysrc += stridesrc, offydst += stridedst)
            {
                for (int x = 0, offxsrc = offysrc, offxdst = offydst; x < width;)
                {
                    ulong b = bitssrc[offxsrc++];

                    // convert 8 bits at a time
                    for (int i = 64 - 8; i >= 0 && x < width; i -= 8, x += 8)
                    {
                        bitsdst[offxdst++] = map[(b >> i) & 0xff];
                    }
                }
            }

            return dst;
        }
    }
}
