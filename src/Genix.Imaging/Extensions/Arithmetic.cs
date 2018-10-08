// -----------------------------------------------------------------------
// <copyright file="Arithmetic.cs" company="Noname, Inc.">
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
    /// Provides arithmetic extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Adds a constant to pixel values of this <see cref="Image"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value">The constant value to add to image pixel values.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method changes the image intensity by adding value to image pixel values.
        /// </para>
        /// <para>
        /// For gray (8bpp) images, a positive value brightens the image (increases the intensity); a negative value darkens the image (decreases the intensity).
        /// </para>
        /// <para>
        /// For color (24bpp and 32bpp) images, the color components are added to pixel channel values.
        /// In this case, the <paramref name="value"/> should contain three color components (blue, green, and red) ordered from least- to- most-significant byte.
        /// The alpha channel is not affected by this method.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// The result is rounded off to the nearest even integer number.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image AddC(Image dst, uint value, int scaleFactor)
        {
            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            dst = this.Copy(dst);

            NativeMethods._addc(
                dst.BitsPerPixel,
                dst.Width,
                dst.Height,
                dst == this ? null : this.Bits,
                this.Stride8,
                value,
                dst.Bits,
                dst.Stride8,
                scaleFactor);

            return dst;
        }

        /// <summary>
        /// Subtracts a constant from pixel values of an image.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value">The constant value to subtract from image pixel values.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method changes the image intensity by subtracting value from image pixel values.
        /// </para>
        /// <para>
        /// For gray (8bpp) images, a positive value darkens the image (decreases the intensity); a negative value brightens the image (increases the intensity).
        /// </para>
        /// <para>
        /// For color (24bpp and 32bpp) images, the color components are subtracted from pixel channel values.
        /// In this case, the <paramref name="value"/> should contain three color components (blue, green, and red) ordered from least- to- most-significant byte.
        /// The alpha channel is not affected by this method.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// The result is rounded off to the nearest even integer number.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image SubC(Image dst, uint value, int scaleFactor)
        {
            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            dst = this.Copy(dst);

            NativeMethods._subc(
                dst.BitsPerPixel,
                dst.Width,
                dst.Height,
                dst == this ? null : this.Bits,
                this.Stride8,
                value,
                dst.Bits,
                dst.Stride8,
                scaleFactor);

            return dst;
        }

        /// <summary>
        /// Multiplies pixel values of this <see cref="Image"/> by a constant.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value">The constant value to multiply image pixel values by.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method changes the image intensity by multiplying image pixel values by a value.
        /// </para>
        /// <para>
        /// For color (24bpp and 32bpp) images, the pixel channel values are multiplied by color components.
        /// In this case, the <paramref name="value"/> should contain three color components (blue, green, and red) ordered from least- to- most-significant byte.
        /// The alpha channel is not affected by this method.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// The result is rounded off to the nearest even integer number.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image MulC(Image dst, uint value, int scaleFactor)
        {
            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            dst = this.Copy(dst);

            NativeMethods._mulc(
                dst.BitsPerPixel,
                dst.Width,
                dst.Height,
                dst == this ? null : this.Bits,
                this.Stride8,
                value,
                dst.Bits,
                dst.Stride8,
                scaleFactor);

            return dst;
        }

        /// <summary>
        /// Divides pixel values of this <see cref="Image"/> by a constant.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="value">The constant value to multiply image pixel values by.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method changes the image intensity by dividing image pixel values by a value.
        /// </para>
        /// <para>
        /// For color (24bpp and 32bpp) images, the pixel channel values are divided by color components.
        /// In this case, the <paramref name="value"/> should contain three color components (blue, green, and red) ordered from least- to- most-significant byte.
        /// The alpha channel is not affected by this method.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// The result is rounded off to the nearest even integer number.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        [CLSCompliant(false)]
        public Image DivC(Image dst, uint value, int scaleFactor)
        {
            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            dst = this.Copy(dst);

            NativeMethods._divc(
                dst.BitsPerPixel,
                dst.Width,
                dst.Height,
                dst == this ? null : this.Bits,
                this.Stride8,
                value,
                dst.Bits,
                dst.Stride8,
                scaleFactor);

            return dst;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern int _addc(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src,
                int srcstep,
                uint value,
                [Out] ulong[] dst,
                int dststep,
                int scaleFactor);

            [DllImport(NativeMethods.DllName)]
            public static extern int _subc(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src,
                int srcstep,
                uint value,
                [Out] ulong[] dst,
                int dststep,
                int scaleFactor);

            [DllImport(NativeMethods.DllName)]
            public static extern int _mulc(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src,
                int srcstep,
                uint value,
                [Out] ulong[] dst,
                int dststep,
                int scaleFactor);

            [DllImport(NativeMethods.DllName)]
            public static extern int _divc(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src,
                int srcstep,
                uint value,
                [Out] ulong[] dst,
                int dststep,
                int scaleFactor);
        }
    }
}
