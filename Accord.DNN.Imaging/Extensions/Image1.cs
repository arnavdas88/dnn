// -----------------------------------------------------------------------
// <copyright file="Image1.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Image1
    {
        /// <summary>
        /// Returns the number of black pixels on this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <returns>
        /// The number of black pixels on the bitmap.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>
        /// The <see cref="Image.BitsPerPixel"/> is not 1.
        /// </para>
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ulong Power(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            ulong[] bits = image.Bits;
            int stride1 = image.Stride1;
            int width = image.WidthBits;

            ulong sum = 0;
            for (int i = 0, ii = image.Height, pos = 0; i < ii; i++, pos += stride1)
            {
                sum += BitUtils64.CountOneBits(width, bits, pos);
            }

            return sum;
        }

        /// <summary>
        /// Returns the area on this <see cref="Image"/> that contains black pixels.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analyze.</param>
        /// <returns>
        /// The <see cref="Rectangle"/> that describes the area of the image that contains black pixels;
        /// <b>Rectangle.Empty</b> if the image does not have black pixels.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The image is not black-and-white.
        /// </exception>
        /// <remarks>
        /// This method supports black-and-white images only and will throw an exception if called on gray-scale or color images.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle BlackArea(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;
            ulong[] bits = image.Bits;

            // calculate top
            int top = findTop();
            if (top < 0)
            {
                return Rectangle.Empty;
            }

            // calculate bottom
            int bottom = findBottom();
            if (bottom < top)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            // calculate left
            ulong endMask = ulong.MaxValue << (64 - (image.WidthBits & 63));
            int left = findLeft(out int leftColumn, out ulong leftMask);
            if (left == -1)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            // calculate right
            int right = findRight();
            if (right < left)
            {
                throw new InvalidOperationException("Something went wrong.");
            }

            return new Rectangle(left, top, right - left + 1, bottom - top + 1);

            int findTop()
            {
                for (int i = 0, off = 0; i < height; i++, off += stride1)
                {
                    if (BitUtils64.BitScanOneForward(width, bits, off) != -1)
                    {
                        return i;
                    }
                }

                return -1;
            }

            int findBottom()
            {
                for (int i = height - 1, off = i * stride1; i >= 0; i--, off -= stride1)
                {
                    if (BitUtils64.BitScanOneForward(width, bits, off) != -1)
                    {
                        return i;
                    }
                }

                return -1;
            }

            int findLeft(out int resultColumn, out ulong resultMask)
            {
                resultColumn = 0;
                resultMask = 0;

                for (int i = 0; i < stride; i++)
                {
                    ulong mask = columnBlackMask(i);
                    if (i == stride - 1)
                    {
                        mask &= endMask;
                    }

                    if (mask != 0ul)
                    {
                        resultColumn = i;
                        resultMask = mask;
                        return (i * 64) + BitUtils64.BitScanOneForward(mask);
                    }
                }

                return -1;
            }

            int findRight()
            {
                for (int i = stride - 1; i >= 0; i--)
                {
                    ulong mask = i == leftColumn ? leftMask : columnBlackMask(i);
                    if (i == stride - 1)
                    {
                        mask &= endMask;
                    }

                    if (mask != 0ul)
                    {
                        return (i * 64) + BitUtils64.BitScanOneReverse(mask);
                    }
                }

                return -1;
            }

            ulong columnBlackMask(int column)
            {
                ulong mask = 0;

                for (int i = top, off = (i * stride) + column; i <= bottom; i++, off += stride)
                {
                    mask |= bits[off];
                }

                return mask;
            }
        }
    }
}
