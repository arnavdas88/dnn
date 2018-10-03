// -----------------------------------------------------------------------
// <copyright file="IntegralImage.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Imaging;

    /// <summary>
    /// Represents a summed-area table.
    /// The value at any point (x, y) in the <see cref="IntegralImage"/> is the sum of all the pixels in the <see cref="Image"/> above and to the left of (x, y), inclusive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class uses 32-bit unsigned integers to represent the sum of pixels at any point of the image.
    /// </para>
    /// <para>
    /// For sum calculation optimization purposes,
    /// the <see cref="IntegralImage"/> is one pixel larger in each dimension compared to the <see cref="Image"/> it was created from.
    /// </para>
    /// </remarks>
    [CLSCompliant(false)]
    public class IntegralImage : Image<uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegralImage"/> class.
        /// </summary>
        /// <param name="width">The image width, in pixels.</param>
        /// <param name="height">The image height, in pixels.</param>
        /// <param name="horizontalResolution">The image horizontal resolution, in pixels per inch.</param>
        /// <param name="verticalResolution">The image vertical resolution, in pixels per inch.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="width"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IntegralImage(int width, int height, int horizontalResolution, int verticalResolution)
            : base(width, height, 32, horizontalResolution, verticalResolution, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegralImage"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntegralImage()
        {
        }

        /// <summary>
        /// Gets the value at the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>
        /// The value at the specified coordinate.
        /// </returns>
        internal uint this[int x, int y] => this.Bits[(y * this.Stride) + x];

        /// <summary>
        /// Create a <see cref="IntegralImage"/> from the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <returns>
        /// The <see cref="IntegralImage"/> this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public static IntegralImage FromImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int width = image.Width;
            int height = image.Height;
            int bitsPerPixel = image.BitsPerPixel;

            if (image.BitsPerPixel > 16)
            {
                throw new NotSupportedException();
            }

            IntegralImage dst = new IntegralImage(
                width + 1,
                height + 1,
                image.HorizontalResolution,
                image.VerticalResolution);

            ulong[] bitssrc = image.Bits;
            int stridesrc = image.Stride;
            uint[] bitsdst = dst.Bits;
            int stridedst = dst.Stride;
            ulong mask = ~(ulong.MaxValue << bitsPerPixel);

            for (int iy = 0, offysrc = 0, offydst = stridedst; iy < height; iy++, offysrc += stridesrc, offydst += stridedst)
            {
                // sum current line
                ulong sum = 0;
                for (int x = 0, offxsrc = offysrc, offxdst = offydst + 1; x < width;)
                {
                    ulong b = bitssrc[offxsrc++];

                    for (int shift = 0; shift < 64 && x < width; x++, shift += bitsPerPixel)
                    {
                        sum += (b >> shift) & mask;

                        bitsdst[offxdst++] = (uint)sum;
                    }
                }

                // sum with previous line
                if (iy > 0)
                {
                    Vectors.Add(stridedst, bitsdst, offydst - stridedst, bitsdst, offydst);
                }
            }

            return dst;
        }

        /// <summary>
        /// Returns the sum of pixels withing a rectangular area
        /// specified by coordinates of its upper-left and bottom-right corners.
        /// </summary>
        /// <param name="x1">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y1">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="x2">The x-coordinate of the bottom-right corner of the area, inclusive.</param>
        /// <param name="y2">The y-coordinate of the bottom-right corner of the area, inclusive.</param>
        /// <returns>
        /// The sum of pixels in the specified rectangular area.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The coordinates are out of image bounds.
        /// </exception>
        [CLSCompliant(false)]
        public uint GetSum(int x1, int y1, int x2, int y2)
        {
            ////this.ValidateArea(x, y, width, height);

            x2++;
            y2++;

            uint[] bits = this.Bits;
            int off1 = y1 * this.Stride;
            int off2 = y2 * this.Stride;

            return bits[off2 + x2] + bits[off1 + x1] - bits[off2 + x1] - bits[off1 + x2];
        }
    }
}
