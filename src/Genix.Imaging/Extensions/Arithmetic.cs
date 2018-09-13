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
        /// Adds a constant to pixel values of an image not-in-place.
        /// </summary>
        /// <param name="src">The source <see cref="Image"/>.</param>
        /// <param name="value">The constant value to add to image pixel values.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// A new destination <see cref="Image"/>.
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
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="src"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image AddC(Image src, int value, int scaleFactor)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            switch (src.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = src.Clone(false);
                    NativeMethods._addc(
                        src.BitsPerPixel,
                        src.Width,
                        src.Height,
                        src.Bits,
                        src.Stride8,
                        value,
                        dst.Bits,
                        dst.Stride8,
                        scaleFactor);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        src.BitsPerPixel));
            }
        }

        /// <summary>
        /// Adds a constant to pixel values of an image in-place.
        /// </summary>
        /// <param name="value">The constant value to add to image pixel values.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
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
        /// </para>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCIP(int value, int scaleFactor)
        {
            switch (this.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    NativeMethods._addc(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        null,
                        0,
                        value,
                        this.Bits,
                        this.Stride8,
                        scaleFactor);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
        }

        /// <summary>
        /// Subtracts a constant from pixel values of an image not-in-place.
        /// </summary>
        /// <param name="src">The source <see cref="Image"/>.</param>
        /// <param name="value">The constant value to subtract from image pixel values.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// A new destination <see cref="Image"/>.
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
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="src"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image SubC(Image src, int value, int scaleFactor)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            switch (src.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = src.Clone(false);
                    NativeMethods._subc(
                        src.BitsPerPixel,
                        src.Width,
                        src.Height,
                        src.Bits,
                        src.Stride8,
                        value,
                        dst.Bits,
                        dst.Stride8,
                        scaleFactor);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        src.BitsPerPixel));
            }
        }

        /// <summary>
        /// Subtracts a constant from pixel values of an image in-place.
        /// </summary>
        /// <param name="value">The constant value to subtract from image pixel values.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
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
        /// </para>
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubCIP(int value, int scaleFactor)
        {
            switch (this.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    NativeMethods._subc(
                        this.BitsPerPixel,
                        this.Width,
                        this.Height,
                        null,
                        0,
                        value,
                        this.Bits,
                        this.Stride8,
                        scaleFactor);
                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        this.BitsPerPixel));
            }
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
                int value,
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
                int value,
                [Out] ulong[] dst,
                int dststep,
                int scaleFactor);
        }
    }
}
