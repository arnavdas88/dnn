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
        /// Adds pixel values of two images not-in-place.
        /// </summary>
        /// <param name="src">The <see cref="Image"/> to add.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// A new destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method adds corresponding pixel values of <paramref name="src"/> to this <see cref="Image"/> and places the results in a destination image.
        /// </para>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="src"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image Add(Image src, int scaleFactor)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (src.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (src.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = src.Clone(false);
                    NativeMethods._add(
                        this.BitsPerPixel,
                        Math.Min(src.Width, this.Width),
                        Math.Min(src.Height, this.Height),
                        this.Bits,
                        this.Stride8,
                        src.Bits,
                        src.Stride8,
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
        /// Adds pixel values of two images in-place.
        /// </summary>
        /// <param name="src">The <see cref="Image"/> to add.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <remarks>
        /// <para>
        /// This method adds corresponding pixel values of <paramref name="src"/> to this <see cref="Image"/>.
        /// </para>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="src"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIP(Image src, int scaleFactor)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (src.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (this.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    NativeMethods._add(
                        this.BitsPerPixel,
                        Math.Min(src.Width, this.Width),
                        Math.Min(src.Height, this.Height),
                        src.Bits,
                        src.Stride8,
                        null,
                        0,
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
        /// Subtracts pixel values of two images not-in-place.
        /// </summary>
        /// <param name="src">The <see cref="Image"/> to subtract.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// A new destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method subtracts corresponding pixel values of <paramref name="src"/> to this <see cref="Image"/> and places the results in a destination image.
        /// </para>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="src"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image Sub(Image src, int scaleFactor)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (src.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (src.BitsPerPixel)
            {
                case 1:
                    return this.Xand(src);

                case 8:
                case 24:
                case 32:
                    Image dst = src.Clone(false);
                    NativeMethods._sub(
                        this.BitsPerPixel,
                        Math.Min(src.Width, this.Width),
                        Math.Min(src.Height, this.Height),
                        this.Bits,
                        this.Stride8,
                        src.Bits,
                        src.Stride8,
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
        /// Subtracts pixel values of two images in-place.
        /// </summary>
        /// <param name="src">The source <see cref="Image"/>.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <remarks>
        /// <para>
        /// This method subtracts corresponding pixel values of the source image of equal depth to this image.
        /// </para>
        /// <para>
        /// <paramref name="src"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="src"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="src"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubIP(Image src, int scaleFactor)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (src.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (this.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    NativeMethods._sub(
                        src.BitsPerPixel,
                        Math.Min(src.Width, this.Width),
                        Math.Min(src.Height, this.Height),
                        src.Bits,
                        src.Stride8,
                        null,
                        0,
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
            public static extern int _add(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src1,
                int src1step,
                [In] ulong[] src2,
                int src2step,
                [Out] ulong[] dst,
                int dststep,
                int scaleFactor);

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
            public static extern int _sub(
                int bitsPerPixel,
                int width,
                int height,
                [In] ulong[] src1,
                int src1step,
                [In] ulong[] src2,
                int src2step,
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
