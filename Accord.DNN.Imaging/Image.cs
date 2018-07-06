// -----------------------------------------------------------------------
// <copyright file="Image.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    public partial class Image
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
        /// <para><c>width</c> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><c>height</c> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image(int width, int height, int bitsPerPixel, int horizontalResolution, int verticalResolution)
        {
            if (width <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidWidth, nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidHeight, nameof(height));
            }

            this.Width = width;
            this.Height = height;
            this.BitsPerPixel = bitsPerPixel;
            this.Stride8 = CalculateStride();
            this.ImageSize = this.Stride8 * height;
            this.HorizontalResolution = horizontalResolution;
            this.VerticalResolution = verticalResolution;
            this.Bits = new uint[this.ImageSize / sizeof(uint)];

            int CalculateStride()
            {
                int stride = width * bitsPerPixel;
                return ((stride + 31) & ~31) >> 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, Image image) : this(
            width,
            height,
            image.BitsPerPixel,
            image.HorizontalResolution,
            image.VerticalResolution)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(Image image) : this(
            image.Width,
            image.Height,
            image.BitsPerPixel,
            image.HorizontalResolution,
            image.VerticalResolution)
        {
        }

        /// <summary>
        /// Gets the width, in pixels, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The width, in pixels, of this <see cref="Image"/>.
        /// </value>
        public int Width { get; }

        /// <summary>
        /// Gets the width, in bits, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The width, in bits, of this <see cref="Image"/>.
        /// </value>
        public int WidthBits => this.Width * this.BitsPerPixel;

        /// <summary>
        /// Gets the height, in pixels, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The height, in pixels, of this <see cref="Image"/>.
        /// </value>
        public int Height { get; }

        /// <summary>
        /// Gets the color depth, in number of bits per pixel, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The color depth, in number of bits per pixel, of this <see cref="Image"/>.
        /// </value>
        public int BitsPerPixel { get; }

        /// <summary>
        /// Gets the offset, in bits, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset, in bits, between the beginning of one scan line and the next.
        /// </value>
        public int Stride1 => this.Stride8 * 8;

        /// <summary>
        /// Gets the offset, in bytes, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset, in bytes, between the beginning of one scan line and the next.
        /// </value>
        public int Stride8 { get; }

        /// <summary>
        /// Gets the offset, in 32-bit integers, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset, in 32-bit integers, between the beginning of one scan line and the next.
        /// </value>
        public int Stride32 => this.Stride8 / sizeof(uint);

/*        /// <summary>
        /// Gets the offset, in 64-bit integers, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset, in 64-bit integers, between the beginning of one scan line and the next.
        /// </value>
        public int Stride64 => this.Stride8 / sizeof(ulong);*/

        /// <summary>
        /// Gets the number of bytes occupied in memory by this <see cref="Image"/> bits.
        /// </summary>
        /// <value>
        /// The number of bytes occupied in memory by this <see cref="Image"/> bits.
        /// </value>
        public int ImageSize { get; }

        /// <summary>
        /// Gets the horizontal resolution, in pixels per inch, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The horizontal resolution, in pixels per inch, of this <see cref="Image"/>.
        /// </value>
        public int HorizontalResolution { get; }

        /// <summary>
        /// Gets the vertical resolution, in pixels per inch, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The vertical resolution, in pixels per inch, of this <see cref="Image"/>.
        /// </value>
        public int VerticalResolution { get; }

        /// <summary>
        /// Gets the bounds, in pixels, of this <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Drawing.Rectangle"/> structure that contains the bounds, in pixels, of this <see cref="Image"/>.
        /// </value>
        public System.Drawing.Rectangle Bounds => new System.Drawing.Rectangle(0, 0, this.Width, this.Height);

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <value>
        /// The array that contains the image bits.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Provide direct access to image data.")]
        [CLSCompliant(false)]
        public uint[] Bits { get; }

        /// <summary>
        /// Randomizes all colors in the <see cref="Image"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Randomize()
        {
            Random random = new Random(0);

            uint[] bits = this.Bits;
            for (int i = 0, ii = bits.Length; i < ii; i++)
            {
                bits[i] = (uint)random.Next() | (random.Next(0, 2) == 0 ? 0x8000_0000u : 0u);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ValidatePosition(int x, int y)
        {
            if (x < 0 || x >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, Properties.Resources.E_InvalidCoordinates);
            }

            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, Properties.Resources.E_InvalidCoordinates);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ValidateArea(int x, int y, int width, int height)
        {
            if (x < 0 || x >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, Properties.Resources.E_InvalidCoordinates);
            }

            if (width < 0 || x + width > this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, Properties.Resources.E_InvalidCoordinates);
            }

            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, Properties.Resources.E_InvalidCoordinates);
            }

            if (height < 0 || y + height > this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, Properties.Resources.E_InvalidCoordinates);
            }
        }
    }
}
