// -----------------------------------------------------------------------
// <copyright file="Cleaning.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides various cleaning methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Cleaning
    {
        /// <summary>
        /// Cleans scan border noise from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to clean.</param>
        /// <param name="maxNoiseWidth">The maximum width of the noise, in inches.</param>
        /// <param name="maxNoiseHeight">The maximum height of the noise, in inches.</param>
        /// <returns>
        /// A new cleaned <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image CleanBorderNoise(this Image image, float maxNoiseWidth, float maxNoiseHeight)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            Image dst = new Image(image);
            SetCopy.Copy(image.Bits.Length, image.Bits, 0, dst.Bits, 0);
            ulong[] bits = dst.Bits;

            int maxwidth = (int)((maxNoiseWidth * image.HorizontalResolution) + 0.5f);
            if (maxwidth > 0)
            {
                clearLeft(findLeft(maxwidth));
                clearRight(findRight(maxwidth));
            }

            int maxheight = (int)((maxNoiseHeight * image.VerticalResolution) + 0.5f);
            if (maxheight > 0)
            {
                clearTop(findTop(maxheight));
                clearBottom(findBottom(maxheight));
            }

            return dst;

            int[] findLeft(int maxlen)
            {
                int scanlen = Maximum.Min(width, maxlen);
                int[] lengths = new int[height];
                for (int iy = 0, pos = 0; iy < height; iy++, pos += stride1)
                {
                    int len = BitUtils64.BitScanZeroForward(scanlen, bits, pos);
                    lengths[iy] = len == -1 ? 0 : len - pos + 1;
                }

                return lengths;
            }

            int[] findRight(int maxlen)
            {
                int scanlen = Maximum.Min(width, maxlen);
                int[] lengths = new int[height];
                for (int iy = 0, pos = width - 1; iy < height; iy++, pos += stride1)
                {
                    int len = BitUtils64.BitScanZeroReverse(scanlen, bits, pos);
                    lengths[iy] = len == -1 ? 0 : pos - len + 1;
                }

                return lengths;
            }

            int[] findTop(int maxlen)
            {
                int scanlen = Maximum.Min(height, maxlen);
                int[] lengths = new int[width];
                SetCopy.Set(lengths.Length, scanlen, lengths, 0);

                for (int ix = 0; ix < width; ix++)
                {
                    ulong mask = BitUtils64.LSB >> (ix & 63);
                    for (int iy = 0, off = ix >> 6; iy < scanlen; iy++, off += stride)
                    {
                        if ((bits[off] & mask) == 0)
                        {
                            lengths[ix] = iy;
                            break;
                        }
                    }
                }

                return lengths;
            }

            int[] findBottom(int maxlen)
            {
                int scanlen = Maximum.Min(height, maxlen);
                int[] lengths = new int[width];
                SetCopy.Set(lengths.Length, scanlen, lengths, 0);

                int size = stride * height;
                for (int ix = 0; ix < width; ix++)
                {
                    ulong mask = BitUtils64.LSB >> (ix & 63);
                    for (int iy = 0, off = size - stride + (ix >> 6); iy < scanlen; iy++, off -= stride)
                    {
                        if ((bits[off] & mask) == 0)
                        {
                            lengths[ix] = iy;
                            break;
                        }
                    }
                }

                return lengths;
            }

            void clearLeft(int[] lengths)
            {
                for (int iy = 0, off = 0; iy < height; iy++, off += stride1)
                {
                    int len = lengths[iy];
                    if (len != 0)
                    {
                        BitUtils64.ResetBits(len, bits, off);
                    }
                }
            }

            void clearRight(int[] lengths)
            {
                for (int iy = 0, off = width; iy < height; iy++, off += stride1)
                {
                    int len = lengths[iy];
                    if (len != 0)
                    {
                        BitUtils64.ResetBits(len, bits, off - len);
                    }
                }
            }

            void clearTop(int[] lengths)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    int len = lengths[ix];
                    if (len != 0)
                    {
                        BitUtils64.WordsAND(len, ~(BitUtils64.LSB >> (ix & 63)), bits, ix >> 6, stride);
                    }
                }
            }

            void clearBottom(int[] lengths)
            {
                int size = stride * height;
                for (int ix = 0; ix < width; ix++)
                {
                    int len = lengths[ix];
                    if (len != 0)
                    {
                        BitUtils64.WordsAND(len, ~(BitUtils64.LSB >> (ix & 63)), bits, size - (len * stride) + (ix >> 6), stride);
                    }
                }
            }
        }
    }
}
