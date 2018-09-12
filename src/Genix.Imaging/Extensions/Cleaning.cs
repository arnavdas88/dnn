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

    /// <content>
    /// Provides various cleaning methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Cleans scan border noise from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="maxNoiseWidth">The maximum width of the noise, in inches.</param>
        /// <param name="maxNoiseHeight">The maximum height of the noise, in inches.</param>
        /// <returns>
        /// A new cleaned <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public static Image CleanOverscan(Image image, float maxNoiseWidth, float maxNoiseHeight)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            Image dst = image.Copy();
            ulong[] bits = dst.Bits;

            int maxwidth = (int)((maxNoiseWidth * image.HorizontalResolution) + 0.5f);
            if (maxwidth > 0)
            {
                ClearLeft(FindLeft(maxwidth));
                ClearRight(FindRight(maxwidth));
            }

            int maxheight = (int)((maxNoiseHeight * image.VerticalResolution) + 0.5f);
            if (maxheight > 0)
            {
                ClearTop(FindTop(maxheight));
                ClearBottom(FindBottom(maxheight));
            }

            return dst;

            int[] FindLeft(int maxlen)
            {
                int scanlen = Core.MinMax.Min(width, maxlen);
                int[] lengths = new int[height];
                for (int iy = 0, pos = 0; iy < height; iy++, pos += stride1)
                {
                    int len = BitUtils.BitScanZeroForward(scanlen, bits, pos);
                    lengths[iy] = len == -1 ? 0 : len - pos + 1;
                }

                return lengths;
            }

            int[] FindRight(int maxlen)
            {
                int scanlen = Core.MinMax.Min(width, maxlen);
                int[] lengths = new int[height];
                for (int iy = 0, pos = width - 1; iy < height; iy++, pos += stride1)
                {
                    int len = BitUtils.BitScanZeroReverse(scanlen, bits, pos);
                    lengths[iy] = len == -1 ? 0 : pos - len + 1;
                }

                return lengths;
            }

            int[] FindTop(int maxlen)
            {
                int scanlen = Core.MinMax.Min(height, maxlen);
                int[] lengths = Vectors.Create(width, scanlen);

                for (int ix = 0; ix < width; ix++)
                {
                    ulong mask = BitUtils.SetBit(0ul, ix & 63);
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

            int[] FindBottom(int maxlen)
            {
                int scanlen = Core.MinMax.Min(height, maxlen);
                int[] lengths = Vectors.Create(width, scanlen);

                int size = stride * height;
                for (int ix = 0; ix < width; ix++)
                {
                    ulong mask = BitUtils.SetBit(0ul, ix & 63);
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

            void ClearLeft(int[] lengths)
            {
                for (int iy = 0, off = 0; iy < height; iy++, off += stride1)
                {
                    int len = lengths[iy];
                    if (len != 0)
                    {
                        BitUtils.ResetBits(len, bits, off);
                    }
                }
            }

            void ClearRight(int[] lengths)
            {
                for (int iy = 0, off = width; iy < height; iy++, off += stride1)
                {
                    int len = lengths[iy];
                    if (len != 0)
                    {
                        BitUtils.ResetBits(len, bits, off - len);
                    }
                }
            }

            void ClearTop(int[] lengths)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    int len = lengths[ix];
                    if (len != 0)
                    {
                        ulong mask = ~BitUtils.SetBit(0ul, ix & 63);
                        BitUtils64.WordsAnd(len, mask, bits, ix >> 6, stride);
                    }
                }
            }

            void ClearBottom(int[] lengths)
            {
                int size = stride * height;
                for (int ix = 0; ix < width; ix++)
                {
                    int len = lengths[ix];
                    if (len != 0)
                    {
                        ulong mask = ~BitUtils.SetBit(0ul, ix & 63);
                        BitUtils64.WordsAnd(len, mask, bits, size - (len * stride) + (ix >> 6), stride);
                    }
                }
            }
        }
    }
}
