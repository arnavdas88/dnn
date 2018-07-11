// -----------------------------------------------------------------------
// <copyright file="Image{T}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    /// <typeparam name="T">The type of elements that store image bits.</typeparam>
    public class Image<T> where T : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Image{T}"/> class.
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

            int sizeInBytes = Marshal.SizeOf(default(T));

            this.Width = width;
            this.Height = height;
            this.BitsPerPixel = bitsPerPixel;
            this.Stride = CalculateStride();
            this.ImageSize = this.Stride * height * sizeInBytes;
            this.HorizontalResolution = horizontalResolution;
            this.VerticalResolution = verticalResolution;
            this.Bits = new T[this.Stride * height];

            int CalculateStride()
            {
                int sizeInBits = sizeInBytes * 8;
                return ((width * bitsPerPixel) + sizeInBits - 1) / sizeInBits;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, Image<T> image) : this(
            width,
            height,
            image.BitsPerPixel,
            image.HorizontalResolution,
            image.VerticalResolution)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(Image<T> image) : this(
            image.Width,
            image.Height,
            image.BitsPerPixel,
            image.HorizontalResolution,
            image.VerticalResolution)
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
        /// Gets the color depth, in number of bits per pixel, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The color depth, in number of bits per pixel, of this <see cref="Image{T}"/>.
        /// </value>
        public int BitsPerPixel { get; }

        /// <summary>
        /// Gets the offset, in <see cref="sizeof(T)"/>, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset between the beginning of one scan line and the next.
        /// </value>
        public int Stride { get; }

        /// <summary>
        /// Gets the number of bytes occupied in memory by this <see cref="Image{T}"/> bits.
        /// </summary>
        /// <value>
        /// The number of bytes occupied in memory by this <see cref="Image{T}"/> bits.
        /// </value>
        public int ImageSize { get; }

        /// <summary>
        /// Gets the horizontal resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The horizontal resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </value>
        public int HorizontalResolution { get; }

        /// <summary>
        /// Gets the vertical resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The vertical resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </value>
        public int VerticalResolution { get; }

        /// <summary>
        /// Gets the bounds, in pixels, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Drawing.Rectangle"/> structure that contains the bounds, in pixels, of this <see cref="Image{T}"/>.
        /// </value>
        public System.Drawing.Rectangle Bounds => new System.Drawing.Rectangle(0, 0, this.Width, this.Height);

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <value>
        /// The array that contains the image bits.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Provide direct access to image data.")]
        public T[] Bits { get; }

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
