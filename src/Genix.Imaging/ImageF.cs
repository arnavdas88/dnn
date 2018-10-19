// -----------------------------------------------------------------------
// <copyright file="ImageF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
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
        /// Converts this <see cref="ImageF"/> to a gray 8-bit <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="bitsPerPixel">The image color depth, in number of bits per pixel. Could be 8, 24, or 32.</param>
        /// <param name="rounding">The rounding mode.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <paramref name="bitsPerPixel"/> is neither 8 not 24 nor 32.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image ConvertTo(Image dst, int bitsPerPixel, MidpointRounding rounding)
        {
            // create destination
            if (dst == null)
            {
                dst = new Image(this.Width, this.Height, bitsPerPixel, this.HorizontalResolution, this.VerticalResolution, this.Transform);
            }
            else
            {
                dst.Reallocate(this.Width, this.Height, bitsPerPixel, this.HorizontalResolution, this.VerticalResolution, this.Transform);
            }

            // do the conversion
            unsafe
            {
                fixed (ulong* bitsdst = dst.Bits)
                {
                    switch (bitsPerPixel)
                    {
                        case 8:
                            if (NativeMethods._convert32fto8(
                                0,
                                0,
                                this.Width,
                                this.Height,
                                this.Bits,
                                this.Stride,
                                (byte*)bitsdst,
                                dst.Stride8,
                                (int)rounding) != 0)
                            {
                                throw new OutOfMemoryException();
                            }

                            break;

                        case 24:
                            if (NativeMethods._convert32fto24(
                                0,
                                0,
                                this.Width,
                                this.Height,
                                this.Bits,
                                this.Stride,
                                (byte*)bitsdst,
                                dst.Stride8,
                                (int)rounding) != 0)
                            {
                                throw new OutOfMemoryException();
                            }

                            break;

                        case 32:
                            if (NativeMethods._convert32fto32(
                                0,
                                0,
                                this.Width,
                                this.Height,
                                this.Bits,
                                this.Stride,
                                (byte*)bitsdst,
                                dst.Stride8,
                                (int)rounding) != 0)
                            {
                                throw new OutOfMemoryException();
                            }

                            break;

                        default:
                            throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
                    }
                }
            }

            return dst;
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

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert32fto8(
                int x,
                int y,
                int width,
                int height,
                [In] float[] src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int roundMode);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert32fto24(
                int x,
                int y,
                int width,
                int height,
                [In] float[] src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int roundMode);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert32fto32(
                int x,
                int y,
                int width,
                int height,
                [In] float[] src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int roundMode);
        }
    }
}
