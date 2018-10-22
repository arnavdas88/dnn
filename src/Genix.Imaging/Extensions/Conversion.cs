// -----------------------------------------------------------------------
// <copyright file="Conversion.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <content>
    /// Provides conversion methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Converts this <see cref="Image"/> to a specified depth.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="bitsPerPixel">The requested image depth.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// The method converts image to the specified depth using default conversion method.
        /// If the conversion is not required, the method returns this <see cref="Image"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="NotImplementedException">
        /// The conversion is not supported.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image ConvertTo(Image dst, int bitsPerPixel)
        {
            if (bitsPerPixel == this.BitsPerPixel)
            {
                return this.Copy(dst, true);
            }

            switch (bitsPerPixel)
            {
                case 1: return ConvertTo1bpp();
                case 8: return ConvertTo8bpp();
                case 16: return ConvertTo16bpp();
                case 24: return ConvertTo24bpp();
                case 32: return ConvertTo32bpp();

                default:
                    throw new NotImplementedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, bitsPerPixel));
            }

            Image ConvertTo1bpp()
            {
                dst = ConvertTo8bpp();
                return dst.Binarize(dst, 0, 0, 1, 1, true, 0, 0);
            }

            Image ConvertTo8bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To8(dst);
                    case 2: return this.Convert2To8(dst);
                    case 4: return this.Convert4To8(dst);
                    case 24: return this.Convert24To8(dst);
                    case 32: return this.Convert32To8(dst);

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }

            Image ConvertTo16bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To16(dst);

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }

            Image ConvertTo24bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To8(null).Convert8To24(dst);
                    case 2: return this.Convert2To8(null).Convert8To24(dst);
                    case 4: return this.Convert4To8(null).Convert8To24(dst);
                    case 8: return this.Convert8To24(dst);
                    case 32: return this.Convert32To24(dst);

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }

            Image ConvertTo32bpp()
            {
                switch (this.BitsPerPixel)
                {
                    case 1: return this.Convert1To8(null).Convert8To32(dst, 255);
                    case 2: return this.Convert2To8(null).Convert8To32(dst, 255);
                    case 4: return this.Convert4To8(null).Convert8To32(dst, 255);
                    case 8: return this.Convert8To32(dst, 255);
                    case 24: return this.Convert24To32(dst);

                    default:
                        throw new NotImplementedException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
                }
            }
        }

        /// <summary>
        /// Converts a binary <see cref="Image"/> to 8-bit gray scale image.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>A simple unpacking that uses 255 for zero (white) pixels and 0 for one (black) pixels.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 1 bit per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Convert1To8(Image dst) => this.Convert1To8(dst, 255, 0);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to 8-bit gray scale image.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>
        /// Conversely, the method puts result of the operation in <paramref name="dst"/>.
        /// If sizes of this <see cref="Image"/> and <paramref name="dst"/> do not match, the operation is performed in upper-left corner of image on the area of smallest size.
        /// </para>
        /// <para>A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 1 bit per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert1To8(Image dst, byte value0, byte value1)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 8);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert1to8(
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            value0,
                            value1) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a binary <see cref="Image"/> to 16-bit gray scale image.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>A simple unpacking that uses 65535 for zero (white) pixels and 0 for one (black) pixels.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 1 bit per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Convert1To16(Image dst) => this.Convert1To16(dst, ushort.MaxValue, 0);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to 16-bit gray scale image.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value0">16-bit value to be used for 0s pixels.</param>
        /// <param name="value1">16-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 1 bit per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [CLSCompliant(false)]
        public Image Convert1To16(Image dst, ushort value0, ushort value1)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 16);

            if (NativeMethods._convert1to16(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                value0,
                value1) != 0)
            {
                throw new OutOfMemoryException();
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a binary <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <returns>
        /// The destination <see cref="ImageF"/> this method creates.
        /// </returns>
        /// <remarks>
        /// A simple unpacking that uses 0.0f for zero (white) pixels and 1.0f for one (black) pixels.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImageF Convert1To32f() => this.Convert1To32f(0, 1);

        /// <summary>
        /// Converts a binary <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// The destination <see cref="ImageF"/> this method creates.
        /// </returns>
        /// <remarks>
        /// A simple unpacking might use <paramref name="value0"/> = 255 and <paramref name="value1"/> = 0.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public ImageF Convert1To32f(float value0, float value1)
        {
            if (this.BitsPerPixel != 1)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ImageF dst = new ImageF(
                this.Width,
                this.Height,
                this.HorizontalResolution,
                this.VerticalResolution);

            if (NativeMethods._convert1to32f(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                value0,
                value1) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        /// <summary>
        /// Converts a 2-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>A simple unpacking that uses values 0x00, 0x55, 0xaa, and 0xff.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 2 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image Convert2To8(Image dst) => this.Convert2To8(dst, 0x00, 0x55, 0xaa, 0xff);

        /// <summary>
        /// Converts a 2-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <param name="value2">8-bit value to be used for 2s pixels.</param>
        /// <param name="value3">8-bit value to be used for 3s pixels.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// A simple unpacking might use <paramref name="value0"/> = 0 (0x00), <paramref name="value1"/> = 85 (0x55),
        /// <paramref name="value2"/> = 170 (0xaa), and <paramref name="value3"/> = 255 (0xff).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 2 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert2To8(Image dst, byte value0, byte value1, byte value2, byte value3)
        {
            if (this.BitsPerPixel != 2)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 8);

            if (NativeMethods._convert2to8(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                value0,
                value1,
                value2,
                value3) != 0)
            {
                throw new OutOfMemoryException();
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a 4-bit gray scale <see cref="Image"/> to binary.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// If the input pixel is more than, or equal to the <paramref name="threshold"/> value, the corresponding output bit is set to 0 (white).
        /// </para>
        /// <para>
        /// If the input pixel is less than the <paramref name="threshold"/> value, the corresponding output bit is set to 1 (black).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 4 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert4To1(Image dst, byte threshold)
        {
            if (this.BitsPerPixel != 4)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 1);

            if (NativeMethods._convert4to1(
                0,
                0,
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                threshold) != 0)
            {
                throw new OutOfMemoryException();
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a 4-bit gray scale <see cref="Image"/> to 8-bit gray scale <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>The unpacking uses shift replication, i.e. each pixel is converted using this formula: <code>(val shl 4) | val</code>.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 4 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert4To8(Image dst)
        {
            if (this.BitsPerPixel != 4)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 8);

            if (NativeMethods._convert4to8(
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride) != 0)
            {
                throw new OutOfMemoryException();
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to binary.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// If the input pixel is more than, or equal to the <paramref name="threshold"/> value, the corresponding output bit is set to 0 (white).
        /// </para>
        /// <para>
        /// If the input pixel is less than the <paramref name="threshold"/> value, the corresponding output bit is set to 1 (black).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert8To1(Image dst, byte threshold)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 1);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert8to1(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        threshold) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a color 24-bit <see cref="Image"/> by copying luminance component to color components.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// A new 24-bit <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>This function converts a gray scale image to an RGB/BGR image by copying luminance component to color components.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert8To24(Image dst)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 24);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert8to24(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a color 32-bit <see cref="Image"/> by copying luminance component to color components.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="alpha">Constant value to create the alpha channel.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>This function converts a gray scale image to an RGB/BGR image by copying luminance component to color components.</para>
        /// <para>The alpha channel is filled with the provided value.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert8To32(Image dst, byte alpha)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 32);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert8to32(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8,
                        alpha) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/> of specified width and height.
        /// </summary>
        /// <param name="width">The width of created image.</param>
        /// <param name="height">The height of created image.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts pixel values in this <see cref="Image"/> to a <see cref="float"/> data type and writes them to the destination <see cref="ImageF"/>.
        /// </para>
        /// </remarks>
        public ImageF Convert8To32f(
            int width,
            int height,
            BorderType borderType,
            float borderValue)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            ImageF dst = new ImageF(
                width,
                height,
                this.HorizontalResolution,
                this.VerticalResolution);

            // convert pixels
            int areaWidth = Core.MinMax.Min(width, this.Width);
            int areaHeight = Core.MinMax.Min(height, this.Height);
            unsafe
            {
                fixed (ulong* bitssrc = this.Bits)
                {
                    if (NativeMethods._convert8to32f(
                        0,
                        0,
                        areaWidth,
                        areaHeight,
                        (byte*)bitssrc,
                        this.Stride8,
                        dst.Bits,
                        dst.Stride) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            // set border
            dst.SetBorder(0, 0, areaWidth, areaHeight, borderType, borderValue);

            return dst;
        }

        /// <summary>
        /// Converts a 16-bit gray scale <see cref="Image"/> to binary.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// If the input pixel is more than, or equal to the <paramref name="threshold"/> value, the corresponding output bit is set to 0 (white).
        /// </para>
        /// <para>
        /// If the input pixel is less than the <paramref name="threshold"/> value, the corresponding output bit is set to 1 (black).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 16 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        [CLSCompliant(false)]
        public Image Convert16To1(Image dst, ushort threshold)
        {
            if (this.BitsPerPixel != 16)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 1);

            if (NativeMethods._convert16to1(
                0,
                0,
                this.Width,
                this.Height,
                this.Bits,
                this.Stride,
                dst.Bits,
                dst.Stride,
                threshold) != 0)
            {
                throw new OutOfMemoryException();
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a color 24-bit <see cref="Image"/> to gray scale using fixed transform coefficients.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// Conversion from color image to gray scale uses the following basic equation to compute luma from nonlinear gamma-corrected red, green, and blue values:
        /// Y' = 0.299 * R' + 0.587 * G' + 0.114 * B'.
        /// Note that the transform coefficients conform to the standard for the NTSC red, green, and blue CRT phosphors.
        /// </para>
        /// </remarks>
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 24 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert24To8(Image dst)
        {
            if (this.BitsPerPixel != 24)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_24bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 8);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert24to8(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a color 24-bit <see cref="Image"/> to a color 32-bit <see cref="Image"/> by adding alpha channel.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 24 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert24To32(Image dst)
        {
            if (this.BitsPerPixel != 24)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_24bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 32);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert24to32(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a color 32-bit <see cref="Image"/> to gray scale using fixed transform coefficients.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// <para>
        /// Conversion from color image to gray scale uses the following basic equation to compute luma from nonlinear gamma-corrected red, green, and blue values:
        /// Y' = 0.299 * R' + 0.587 * G' + 0.114 * B'.
        /// Note that the transform coefficients conform to the standard for the NTSC red, green, and blue CRT phosphors.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert32To8(Image dst)
        {
            if (this.BitsPerPixel != 32)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 8);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert32to8(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a color 32-bit <see cref="Image"/> to a color 24-bit <see cref="Image"/> by discarding alpha channel.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        public Image Convert32To24(Image dst)
        {
            if (this.BitsPerPixel != 32)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 24);

            unsafe
            {
                fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                {
                    if (NativeMethods._convert32to24(
                        0,
                        0,
                        this.Width,
                        this.Height,
                        (byte*)bitssrc,
                        this.Stride8,
                        (byte*)bitsdst,
                        dst.Stride8) != 0)
                    {
                        throw new OutOfMemoryException();
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        /// <summary>
        /// Converts a gray scale <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts pixel values in this <see cref="Image"/> to a <see cref="float"/> data type and writes them to the destination <see cref="ImageF"/>.
        /// </para>
        /// </remarks>
        public ImageF Convert8To32f()
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            ImageF dst = new ImageF(
                this.Width,
                this.Height,
                this.HorizontalResolution,
                this.VerticalResolution);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits)
                    {
                        return NativeMethods._convert8to32f(
                            0,
                            0,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            dst.Bits,
                            dst.Stride);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Converts a color 24-bit <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 24 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts pixel values in this <see cref="Image"/> to a <see cref="float"/> data type and writes them to the destination <see cref="ImageF"/>.
        /// </para>
        /// </remarks>
        public ImageF Convert24To32f()
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_24bpp);
            }

            ImageF dst = new ImageF(
                this.Width,
                this.Height,
                this.HorizontalResolution,
                this.VerticalResolution);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits)
                    {
                        return NativeMethods._convert24to32f(
                            0,
                            0,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            dst.Bits,
                            dst.Stride);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Converts a color 32-bit <see cref="Image"/> to a <see cref="float"/> <see cref="ImageF"/>.
        /// </summary>
        /// <param name="convertAlphaChannel">Determines whether the alpha channel should be converted.</param>
        /// <returns>
        /// A new <see cref="float"/> <see cref="ImageF"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 32 bits per pixel.</para>
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Not enough memory to complete this operation.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function converts pixel values in this <see cref="Image"/> to a <see cref="float"/> data type and writes them to the destination <see cref="ImageF"/>.
        /// </para>
        /// </remarks>
        public ImageF Convert32To32f(bool convertAlphaChannel)
        {
            if (this.BitsPerPixel != 32)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_32bpp);
            }

            ImageF dst = new ImageF(
                this.Width,
                this.Height,
                this.HorizontalResolution,
                this.VerticalResolution);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits)
                    {
                        return NativeMethods._convert32to32f(
                            0,
                            0,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            dst.Bits,
                            dst.Stride,
                            convertAlphaChannel);
                    }
                }
            });

            return dst;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert1to8(
               int width,
               int height,
               byte* src,
               int stridesrc,
               byte* dst,
               int stridedst,
               byte value0,
               byte value1);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert1to16(
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] ulong[] dst,
               int stridedst,
               ushort value0,
               ushort value1);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert1to32f(
               int width,
               int height,
               [In] ulong[] src,
               int stridesrc,
               [Out] float[] dst,
               int stridedst,
               float value0,
               float value1);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert2to8(
              int width,
              int height,
              [In] ulong[] src,
              int stridesrc,
              [Out] ulong[] dst,
              int stridedst,
              byte value0,
              byte value1,
              byte value2,
              byte value3);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert4to1(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int threshold);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert4to8(
              int width,
              int height,
              [In] ulong[] src,
              int stridesrc,
              [Out] ulong[] dst,
              int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert8to1(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                int threshold);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert8to24(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert8to32(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                byte alpha);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert8to32f(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                float[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern int _convert16to1(
                int x,
                int y,
                int width,
                int height,
                [In] ulong[] src,
                int stridesrc,
                [Out] ulong[] dst,
                int stridedst,
                int threshold);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert24to8(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert24to32(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert24to32f(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                float[] dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert32to8(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert32to24(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst);

            [DllImport(NativeMethods.DllName)]
            public static extern unsafe int _convert32to32f(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                float[] dst,
                int stridedst,
                [MarshalAs(UnmanagedType.Bool)] bool convertAlphaChannel);
        }
    }
}
