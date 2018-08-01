// -----------------------------------------------------------------------
// <copyright file="Image.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

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
            : base(width, height, bitsPerPixel, horizontalResolution, verticalResolution)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, Image image)
            : base(width, height, image)
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
        /// Gets the mask that clears ending unused bits in the stride.
        /// </summary>
        /// <value>
        /// The mask that clears ending unused bits in the stride.
        /// </value>
        internal ulong EndMask => ulong.MaxValue >> (64 - (this.WidthBits & 63));

        /// <summary>
        /// Creates a new <see cref="Image"/> that is a copy of the current instance.
        /// </summary>
        /// <param name="copyBits">The value indicating whether the <see cref="Image{T}.Bits"/> should be copied to the new <see cref="Image"/>.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public Image Clone(bool copyBits)
        {
            Image dst = new Image(
                 this.Width,
                 this.Height,
                 this.BitsPerPixel,
                 this.HorizontalResolution,
                 this.VerticalResolution);

            if (copyBits)
            {
                Arrays.Copy(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Randomizes all colors in the <see cref="Image"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Randomize()
        {
            Random random = new Random(0);

            ulong[] bits = this.Bits;
            for (int i = 0, ii = bits.Length; i < ii; i++)
            {
                bits[i] = (ulong)(uint)random.Next() |
                          (random.Next(0, 2) == 0 ? 0x8000_0000ul : 0ul) |
                          (ulong)(uint)random.Next() << 32 |
                          (random.Next(0, 2) == 0 ? 0x8000_0000_0000_0000ul : 0ul);
            }
        }

        private static partial class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";
        }
    }
}
