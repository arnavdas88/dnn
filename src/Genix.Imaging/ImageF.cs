// -----------------------------------------------------------------------
// <copyright file="ImageF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Represents an image with each pixel described by <see cref="float"/> value.
    /// </summary>
    public class ImageF : Image<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageF"/> class.
        /// </summary>
        /// <param name="width">The image width, in pixels.</param>
        /// <param name="height">The image height, in pixels.</param>
        /// <param name="horizontalResolution">The image horizontal resolution, in pixels per inch.</param>
        /// <param name="verticalResolution">The image vertical resolution, in pixels per inch.</param>
        /// <exception cref="System.ArgumentException">
        /// <para><paramref name="width"/> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImageF(int width, int height, int horizontalResolution, int verticalResolution)
            : base(width, height, 32, horizontalResolution, verticalResolution, null)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageF(ImageF image)
            : base(
            image.Width,
            image.Height,
            image.BitsPerPixel,
            image.HorizontalResolution,
            image.VerticalResolution,
            image.Transform)
        {
        }

        /// <summary>
        /// Computes the minimum of <see cref="ImageF"/> values.
        /// </summary>
        /// <returns>
        /// The minimum of <see cref="ImageF"/> values.
        /// </returns>
        public float Min()
        {
            int width = this.Width;
            int height = this.Height;
            int stride = this.Stride;
            float[] bits = this.Bits;

            if (width == stride)
            {
                return Vectors.Min(stride * height, bits, 0);
            }
            else
            {
                float result = Vectors.Min(width, bits, 0);
                for (int i = 1, off = stride; i < height; i++, off += stride)
                {
                    result = MinMax.Min(result, Vectors.Min(width, bits, off));
                }

                return result;
            }
        }

        /// <summary>
        /// Computes the maximum of <see cref="ImageF"/> values.
        /// </summary>
        /// <returns>
        /// The maximum of <see cref="ImageF"/> values.
        /// </returns>
        public float Max()
        {
            int width = this.Width;
            int height = this.Height;
            int stride = this.Stride;
            float[] bits = this.Bits;

            if (width == stride)
            {
                return Vectors.Max(stride * height, bits, 0);
            }
            else
            {
                float result = Vectors.Max(width, bits, 0);
                for (int i = 1, off = stride; i < height; i++, off += stride)
                {
                    result = MinMax.Max(result, Vectors.Max(width, bits, off));
                }

                return result;
            }
        }

        /// <summary>
        /// Sets the border outside specified area of interest.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        internal void SetBorder(int x, int y, int width, int height, BorderType borderType, float borderValue)
        {
            int stride = this.Stride;
            float[] bits = this.Bits;

            int right = x + width;
            int bottom = y + height;
            int offy = y * stride;

            switch (borderType)
            {
                case BorderType.BorderRepl:
                    SetBorderRepl();
                    break;

                case BorderType.BorderConst:
                    SetBorderConst();
                    break;
            }

            void SetBorderRepl()
            {
                // set top
                if (y > 0)
                {
                    Vectors.Tile(this.Width, y, bits, offy + stride, bits, 0);
                }

                // set left
                if (x > 0)
                {
                    for (int i = y, off = offy; i < height; i++, off += stride)
                    {
                        Vectors.Set(x, bits[off + x], bits, off);
                    }
                }

                // set right
                int len = this.Width - right;
                if (len > 0)
                {
                    for (int i = y, off = offy + right; i < height; i++, off += stride)
                    {
                        Vectors.Set(len, bits[off - 1], bits, off);
                    }
                }

                // set bottom
                len = this.Height - bottom;
                if (len > 0)
                {
                    Vectors.Tile(this.Width, len, bits, (bottom - 1) * stride, bits, bottom * stride);
                }
            }

            void SetBorderConst()
            {
                // set top
                if (y > 0)
                {
                    Vectors.Set(offy, borderValue, bits, 0);
                }

                // set left
                if (x > 0)
                {
                    for (int i = y, off = offy; i < height; i++, off += stride)
                    {
                        Vectors.Set(x, borderValue, bits, off);
                    }
                }

                // set right
                int len = this.Width - right;
                if (len > 0)
                {
                    for (int i = y, off = offy + right; i < height; i++, off += stride)
                    {
                        Vectors.Set(len, borderValue, bits, off);
                    }
                }

                // set bottom
                len = this.Height - bottom;
                if (len > 0)
                {
                    Vectors.Set(len * stride, borderValue, bits, bottom * stride);
                }
            }
        }
    }
}
