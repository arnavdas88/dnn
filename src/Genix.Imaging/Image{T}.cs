// -----------------------------------------------------------------------
// <copyright file="Image{T}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Genix.Drawing;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// This is an abstract class.
    /// </summary>
    /// <typeparam name="T">The type of elements that store image bits.</typeparam>
    public abstract class Image<T>
        where T : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Image{T}"/> class.
        /// </summary>
        /// <param name="width">The image width, in pixels.</param>
        /// <param name="height">The image height, in pixels.</param>
        /// <param name="bitsPerPixel">The image color depth, in number of bits per pixel.</param>
        /// <param name="horizontalResolution">The image horizontal resolution, in pixels per inch.</param>
        /// <param name="verticalResolution">The image vertical resolution, in pixels per inch.</param>
        /// <param name="transform">The image transformation.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="width"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="horizontalResolution"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="verticalResolution"/> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image(int width, int height, int bitsPerPixel, int horizontalResolution, int verticalResolution, Transform transform)
        {
            this.AllocateBits(width, height, bitsPerPixel);
            this.SetResolution(horizontalResolution, verticalResolution);

            if (transform != null)
            {
                this.Transform = transform;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, int bitsPerPixel, Image<T> image)
            : this(
                width,
                height,
                bitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution,
                image.Transform)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(int width, int height, Image<T> image)
            : this(
                width,
                height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution,
                image.Transform)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(Size size, Image<T> image)
            : this(
                size.Width,
                size.Height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution,
                image.Transform)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Image(Image<T> image)
            : this(
                image.Width,
                image.Height,
                image.BitsPerPixel,
                image.HorizontalResolution,
                image.VerticalResolution,
                image.Transform)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image{T}"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Image()
        {
        }

        /// <summary>
        /// Gets the width, in pixels, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The width, in pixels, of this <see cref="Image{T}"/>.
        /// </value>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the width, in bits, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The width, in bits, of this <see cref="Image{T}"/>.
        /// </value>
        public int WidthBits => this.Width * this.BitsPerPixel;

        /// <summary>
        /// Gets the height, in pixels, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The height, in pixels, of this <see cref="Image{T}"/>.
        /// </value>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the color depth, in number of bits per pixel, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The color depth, in number of bits per pixel, of this <see cref="Image{T}"/>.
        /// </value>
        public int BitsPerPixel { get; private set; }

        /// <summary>
        /// Gets the offset, in sizes of <typeparamref name="T"/>, between the beginning of one scan line and the next.
        /// </summary>
        /// <value>
        /// The integer that specifies the offset between the beginning of one scan line and the next.
        /// </value>
        public int Stride { get; private set; }

        /// <summary>
        /// Gets the number of bytes occupied in memory by this <see cref="Image{T}"/> bits.
        /// </summary>
        /// <value>
        /// The number of bytes occupied in memory by this <see cref="Image{T}"/> bits.
        /// </value>
        public int ImageSize => this.Bits.Length * Marshal.SizeOf(default(T));

        /// <summary>
        /// Gets the horizontal resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The horizontal resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </value>
        public int HorizontalResolution { get; private set; }

        /// <summary>
        /// Gets the vertical resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// The vertical resolution, in pixels per inch, of this <see cref="Image{T}"/>.
        /// </value>
        public int VerticalResolution { get; private set; }

        /// <summary>
        /// Gets the bounds, in pixels, of this <see cref="Image{T}"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Rectangle"/> structure that contains the bounds, in pixels, of this <see cref="Image{T}"/>.
        /// </value>
        public Rectangle Bounds => new Rectangle(0, 0, this.Width, this.Height);

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <value>
        /// The array that contains the image bits.
        /// </value>
        public T[] Bits { get; private set; }

        /// <summary>
        /// Gets the transformation performed on the image since it was first created.
        /// </summary>
        /// <value>
        /// The <see cref="Imaging.Transform"/> object that contains the image transformations.
        /// </value>
        public Transform Transform { get; private protected set; } = new IdentityTransform();

        /// <summary>
        /// Changes image resolution without changing its size.
        /// </summary>
        /// <param name="horizontalResolution">The horizontal resolution, in pixels per inch.</param>
        /// <param name="verticalResolution">The vertical resolution, in pixels per inch.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="horizontalResolution"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="verticalResolution"/> is less than or equal to zero.</para>
        /// </exception>
        public void SetResolution(int horizontalResolution, int verticalResolution)
        {
            if (horizontalResolution <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidHorizontalResolution, nameof(horizontalResolution));
            }

            if (verticalResolution <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidVerticalResolution, nameof(verticalResolution));
            }

            this.HorizontalResolution = horizontalResolution;
            this.VerticalResolution = verticalResolution;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ValidateArea(Rectangle area) => this.ValidateArea(area.X, area.Y, area.Width, area.Height);

        private protected static int CalculateStride(int width, int bitsPerPixel)
        {
            int sizeInBytes = Marshal.SizeOf(default(T));
            int sizeInBits = sizeInBytes * 8;
            return ((width * bitsPerPixel) + sizeInBits - 1) / sizeInBits;
        }

        private protected void AllocateBits(int width, int height, int bitsPerPixel)
        {
            if (width <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidWidth, nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidHeight, nameof(height));
            }

            if (this.Width == width && this.Height == height && this.BitsPerPixel == bitsPerPixel)
            {
                // nothing to do
                // already allocated
                return;
            }

            this.Width = width;
            this.Height = height;
            this.BitsPerPixel = bitsPerPixel;
            this.Stride = Image<T>.CalculateStride(width, bitsPerPixel);

            int length = this.Stride * height;
            if (this.Bits?.Length != length)
            {
                this.Bits = new T[length];
            }
        }

        private protected void Attach(Image<T> source)
        {
            this.Width = source.Width;
            this.Height = source.Height;
            this.BitsPerPixel = source.BitsPerPixel;
            this.Stride = source.Stride;
            this.HorizontalResolution = source.HorizontalResolution;
            this.VerticalResolution = source.VerticalResolution;
            this.Bits = source.Bits;
            this.Transform = source.Transform;
        }

        private protected void AppendTransform(Transform transform)
        {
            this.Transform = this.Transform.Append(transform);
        }
    }
}
