// -----------------------------------------------------------------------
// <copyright file="IntegralImage.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

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
    public class IntegralImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegralImage"/> class.
        /// </summary>
        /// <param name="width">The image width, in pixels.</param>
        /// <param name="height">The image height, in pixels.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="width"/> is less than or equal to one.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is less than or equal to one.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IntegralImage(int width, int height)
        {
            if (width <= 1)
            {
                throw new ArgumentException(Properties.Resources.E_IntegralImage_InvalidWidth, nameof(width));
            }

            if (height <= 1)
            {
                throw new ArgumentException(Properties.Resources.E_IntegralImage_InvalidHeight, nameof(height));
            }

            this.Width = width;
            this.Height = height;
            this.Bits = new int[width * height];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegralImage"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IntegralImage()
        {
        }

        /// <summary>
        /// Gets the width, in pixels, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The width, in pixels, of this <see cref="Image{T}"/>.
        /// </value>
        public int Width { get; }

        /// <summary>
        /// Gets the height, in pixels, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The height, in pixels, of this <see cref="Image{T}"/>.
        /// </value>
        public int Height { get; }

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <value>
        /// The array that contains the image bits.
        /// </value>
        public int[] Bits { get; }

        /// <summary>
        /// Gets the value at the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>
        /// The value at the specified coordinate.
        /// </returns>
        public int this[int x, int y] => this.Bits[(y * this.Width) + x];

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
        /// <remarks>
        /// <para>The size of resulting integral image is (<paramref name="image"/>.width + 1) * (<paramref name="image"/>.height + 1).</para>
        /// </remarks>
        public static IntegralImage FromImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int bitsPerPixel = image.BitsPerPixel;
            if (image.BitsPerPixel > 16)
            {
                throw new NotSupportedException();
            }

            int width = image.Width;
            int height = image.Height;

            IntegralImage dst = new IntegralImage(width + 1, height + 1);

#if false
            // IPP version is approximately twice faster
            unsafe
            {
                fixed (ulong* bitssrc = image.Bits)
                {
                    fixed (int* bitsdst = dst.Bits)
                    {
                        byte* ptrsrc = (byte*)bitssrc;
                        uint* ptrdst = (uint*)bitsdst;

                        int stridesrc = image.Stride8;
                        int stridedst = dst.Width;

                        ptrdst += stridedst;
                        for (int iy = 0; iy < height; iy++, ptrsrc += stridesrc, ptrdst += stridedst)
                        {
                            // sum current line
                            Vectors.CumulativeSum(width, ptrsrc, ptrdst + 1);

                            // sum with previous line
                            if (iy > 0)
                            {
                                Vectors.Add(stridedst, ptrdst - stridedst, ptrdst);
                            }
                        }
                    }
                }
            }
#else
            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = image.Bits)
                    {
                        fixed (int* bitsdst = dst.Bits)
                        {
                            return NativeMethods.integral32s(
                                width,
                                height,
                                (byte*)bitssrc,
                                image.Stride8,
                                bitsdst,
                                dst.Width,
                                0);
                        }
                    }
                }
            });
#endif

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
        public int GetSum(int x1, int y1, int x2, int y2)
        {
            ////this.ValidateArea(x, y, width, height);

            x2++;
            y2++;

            int[] bits = this.Bits;
            int off1 = y1 * this.Width;
            int off2 = y2 * this.Width;

            return bits[off2 + x2] + bits[off1 + x1] - bits[off2 + x1] - bits[off1 + x2];
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int integral32s(
                int width,
                int height,
                byte* src,
                int stridesrc,
                int* dst,
                int stridedst,
                int value);
        }
    }
}
