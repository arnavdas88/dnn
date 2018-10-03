// -----------------------------------------------------------------------
// <copyright file="Image.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    public partial class Image : Image<ulong>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="width">The image width, in pixels.</param>
        /// <param name="height">The image height, in pixels.</param>
        /// <param name="bitsPerPixel">The image color depth, in number of bits per pixel.</param>
        /// <param name="horizontalResolution">The image horizontal resolution, in pixels per inch.</param>
        /// <param name="verticalResolution">The image vertical resolution, in pixels per inch.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="width"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image(int width, int height, int bitsPerPixel, int horizontalResolution, int verticalResolution)
            : base(width, height, bitsPerPixel, horizontalResolution, verticalResolution, null)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, int bitsPerPixel, Image image)
            : base(width, height, bitsPerPixel, image)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, Image image)
            : base(width, height, image)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(Size size, Image image)
            : base(size, image)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(Image image)
            : base(image)
        {
        }

        /// <summary>
        /// Gets the offset, in bits, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset, in bits, between the beginning of one scan line and the next.
        /// </value>
        public int Stride1 => this.Stride << 6;

        /// <summary>
        /// Gets the offset, in bytes, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset, in bytes, between the beginning of one scan line and the next.
        /// </value>
        public int Stride8 => this.Stride << 3;

        /// <summary>
        /// Gets the pixel value that represents black color for this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// 1 for binary images; otherwise, 0.
        /// </value>
        [CLSCompliant(false)]
        public uint BlackColor => this.BitsPerPixel == 1 ? 1u : 0u;

        /// <summary>
        /// Gets the pixel value that represents white color for this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// 0 for binary images; otherwise, ~(0xffffffff &lt;&lt; bpp).
        /// </value>
        [CLSCompliant(false)]
        public uint WhiteColor => this.BitsPerPixel == 1 ? 0u : this.MaxColor;

        /// <summary>
        /// Gets the maximum pixel value for this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// (1 &lt;&lt; bpp) - 1.
        /// </value>
        [CLSCompliant(false)]
        public uint MaxColor => ~(uint)(ulong.MaxValue << this.BitsPerPixel);

        /// <summary>
        /// Gets the mask that clears ending unused bits in the stride.
        /// </summary>
        /// <value>
        /// The mask that clears ending unused bits in the stride.
        /// </value>
        internal ulong EndMask => ulong.MaxValue >> (64 - (this.WidthBits & 63));

        /// <summary>
        /// Create a gray palette used by the <see cref="Image"/>.
        /// </summary>
        /// <param name="bitsPerPixel">The image color depth, in number of bits per pixel.</param>
        /// <returns>
        /// The array of <see cref="Color"/> objects that contains the palette.
        /// </returns>
        public static Color[] CreatePalette(int bitsPerPixel)
        {
            switch (bitsPerPixel)
            {
                case 1:
                    return new Color[2]
                    {
                        Color.FromArgb(0xff, 0xff, 0xff, 0xff),
                        Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                    };

                case 2:
                    return new Color[4]
                    {
                        Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                        Color.FromArgb(0xff, 0x55, 0x55, 0x55),
                        Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa),
                        Color.FromArgb(0xff, 0xff, 0xff, 0xff),
                    };

                case 4:
                    return new Color[16]
                    {
                        Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                        Color.FromArgb(0xff, 0x14, 0x14, 0x14),
                        Color.FromArgb(0xff, 0x20, 0x20, 0x20),
                        Color.FromArgb(0xff, 0x2c, 0x2c, 0x2c),
                        Color.FromArgb(0xff, 0x38, 0x38, 0x38),
                        Color.FromArgb(0xff, 0x45, 0x45, 0x45),
                        Color.FromArgb(0xff, 0x51, 0x51, 0x51),
                        Color.FromArgb(0xff, 0x61, 0x61, 0x61),
                        Color.FromArgb(0xff, 0x71, 0x71, 0x71),
                        Color.FromArgb(0xff, 0x82, 0x82, 0x82),
                        Color.FromArgb(0xff, 0x92, 0x92, 0x92),
                        Color.FromArgb(0xff, 0xa2, 0xa2, 0xa2),
                        Color.FromArgb(0xff, 0xb6, 0xb6, 0xb6),
                        Color.FromArgb(0xff, 0xcb, 0xcb, 0xcb),
                        Color.FromArgb(0xff, 0xe3, 0xe3, 0xe3),
                        Color.FromArgb(0xff, 0xff, 0xff, 0xff),
                    };

                case 8:
                    Color[] colors = new Color[256];
                    for (int i = 0; i < 256; i++)
                    {
                        byte c = (byte)i;
                        colors[i] = Color.FromArgb(0xff, c, c, c);
                    }

                    return colors;

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is Image other)
            {
                return this.Width == other.Width &&
                    this.Height == other.Height &&
                    this.BitsPerPixel == other.BitsPerPixel &&
                    this.HorizontalResolution == other.HorizontalResolution &&
                    this.VerticalResolution == other.VerticalResolution &&
                    CompareBits(this.Bits, other.Bits);

                bool CompareBits(ulong[] bits1, ulong[] bits2)
                {
                    if (this.Width * this.BitsPerPixel == this.Stride1)
                    {
                        return Vectors.Equals(this.Height * this.Stride, bits1, 0, bits2, 0);
                    }

                    ulong endMask = this.EndMask;
                    for (int i = 0, ii = this.Height, stride = this.Stride, off = 0; i < ii; i++, off += stride)
                    {
                        if (!Vectors.Equals(stride - 1, bits1, off, bits2, off) ||
                            (bits1[off + stride - 1] & endMask) != (bits2[off + stride - 1] & endMask))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.Width ^ this.Height ^ this.BitsPerPixel;

        /// <summary>
        /// Creates a new <see cref="Image"/> that is a copy of the current instance.
        /// </summary>
        /// <param name="copyBits">The value indicating whether the <see cref="Image{T}.Bits"/> should be copied to the new <see cref="Image"/>.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public Image Clone(bool copyBits)
        {
            Image dst = new Image(this.Width, this.Height, this);

            if (copyBits)
            {
                Vectors.Copy(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Randomizes all colors in the <see cref="Image"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Randomize()
        {
            new UlongRandomGenerator().Generate(this.Bits.Length, this.Bits);
            this.ZeroTail();
        }

        /// <summary>
        /// Returns a critical sum for this <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A value that specifies a critical sum for this <see cref="Image"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public uint GetCRC()
        {
            return CRC.Calculate(this.Bits);
        }

        /// <summary>
        /// Creates a scan line filled with pixels of the specified color.
        /// </summary>
        /// <param name="length">The scan line length.</param>
        /// <param name="color">The color to fill the scan line.</param>
        /// <returns>The array that contains the created scan line.</returns>
        private ulong[] ColorScanline(int length, uint color)
        {
            // fill one line with specified color
            uint maxcolor = this.MaxColor;
            color &= maxcolor;

            ulong[] buf = new ulong[length];
            if (color == maxcolor)
            {
                Vectors.Set(length, ulong.MaxValue, buf, 0);
            }
            else if (color != 0)
            {
                if (this.BitsPerPixel == 24)
                {
                    ulong ucolor = (ulong)color | ((ulong)color << 24);
                    buf[0] = ucolor | (ucolor << 48);
                    if (length > 1)
                    {
                        buf[1] = ((ucolor >> 16) & 0x0000_0000_ffff_fffful) | (ucolor << 32);
                    }

                    if (length > 2)
                    {
                        buf[2] = ((ucolor >> 32) & 0x0000_0000_0000_fffful) | (ucolor << 16);
                    }

                    if (length > 3)
                    {
                        for (int yoff = 3, count = 3; yoff < length; count *= 2)
                        {
                            Vectors.Copy(Math.Min(count, length - yoff), buf, 0, buf, yoff);
                            yoff += count;
                        }
                    }
                }
                else
                {
                    // create 64-bit value with each position filled with given color
                    Vectors.Set(length, this.ColorBits(color), buf, 0);
                }
            }

            return buf;
        }

        /// <summary>
        /// Creates a single 64-bit element filled with pixels of the specified color.
        /// </summary>
        /// <param name="color">The color to fill the scan line.</param>
        /// <returns>The 64-bit integer filled with pixels of the specified color.</returns>
        private ulong ColorBits(uint color)
        {
            if (this.BitsPerPixel == 24)
            {
                throw new NotImplementedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            if (color == 0)
            {
                return 0;
            }

            uint maxcolor = this.MaxColor;
            color &= maxcolor;
            if (color == maxcolor)
            {
                return ulong.MaxValue;
            }

            // create 64-bit value with each position filled with given color
            ulong value = color;
            for (int step = this.BitsPerPixel; step < 64; step *= 2)
            {
                value |= value << step;
            }

            return value;
        }

        /// <summary>
        /// Sets unused bits on the right side of the image to zero.
        /// </summary>
        private void ZeroTail()
        {
            ulong mask = this.EndMask;
            ulong[] bits = this.Bits;
            int stride = this.Stride;
            for (int i = 0, ii = this.Height, off = stride - 1; i < ii; i++, off += stride)
            {
                bits[off] &= mask;
            }
        }

        private static partial class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";
        }
    }
}
