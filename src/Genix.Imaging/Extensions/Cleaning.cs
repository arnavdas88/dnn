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
        /// Cleans scan border noise from this <see cref="Image"/>.
        /// </summary>
        /// <param name="maxNoiseWidth">The maximum width of the noise, in inches.</param>
        /// <param name="maxNoiseHeight">The maximum height of the noise, in inches.</param>
        /// <returns>
        /// A new cleaned <see cref="Image"/>.
        /// </returns>
        public Image CleanBorderNoise(float maxNoiseWidth, float maxNoiseHeight)
        {
            int width = this.Width;
            int height = this.Height;
            int stride1 = this.Stride1;
            int stride = this.Stride;

            Image dst = this.Clone(true);
            ulong[] bits = dst.Bits;

            int maxwidth = (int)((maxNoiseWidth * this.HorizontalResolution) + 0.5f);
            if (maxwidth > 0)
            {
                ClearLeft(FindLeft(maxwidth));
                ClearRight(FindRight(maxwidth));
            }

            int maxheight = (int)((maxNoiseHeight * this.VerticalResolution) + 0.5f);
            if (maxheight > 0)
            {
                ClearTop(FindTop(maxheight));
                ClearBottom(FindBottom(maxheight));
            }

            return dst;

            int[] FindLeft(int maxlen)
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

            int[] FindRight(int maxlen)
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

            int[] FindTop(int maxlen)
            {
                int scanlen = Maximum.Min(height, maxlen);
                int[] lengths = Arrays.Create(width, scanlen);

                for (int ix = 0; ix < width; ix++)
                {
                    ulong mask = BitUtils64.SetBit(0, ix & 63);
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
                int scanlen = Maximum.Min(height, maxlen);
                int[] lengths = Arrays.Create(width, scanlen);

                int size = stride * height;
                for (int ix = 0; ix < width; ix++)
                {
                    ulong mask = BitUtils64.SetBit(0, ix & 63);
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
                        BitUtils64.ResetBits(len, bits, off);
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
                        BitUtils64.ResetBits(len, bits, off - len);
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
                        ulong mask = ~BitUtils64.SetBit(0, ix & 63);
                        BitUtils64.AND(len, mask, bits, ix >> 6, stride);
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
                        ulong mask = ~BitUtils64.SetBit(0, ix & 63);
                        BitUtils64.AND(len, mask, bits, size - (len * stride) + (ix >> 6), stride);
                    }
                }
            }
        }
    }
}
