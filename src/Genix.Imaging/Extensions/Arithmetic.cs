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
        /// <param name="a">The first source <see cref="Image"/>.</param>
        /// <param name="b">The second source <see cref="Image"/>.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// A new destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method adds corresponding pixel values of two source images with equal depth and places the results in a destination image.
        /// </para>
        /// <para>
        /// <paramref name="a"/> and <paramref name="b"/> do not have to have the same width and height.
        /// If image sizes are different, the destination image has the size of <paramref name="a"/> and the operation is performed in its upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="a"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="b"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="a"/> and <paramref name="b"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image Add(Image a, Image b, int scaleFactor)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (a.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = a.Clone(false);
                    NativeMethods._add(
                        a.BitsPerPixel,
                        Math.Min(a.Width, b.Width),
                        Math.Min(a.Height, b.Height),
                        a.Bits,
                        a.Stride8,
                        b.Bits,
                        b.Stride8,
                        dst.Bits,
                        dst.Stride8,
                        scaleFactor);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        a.BitsPerPixel));
            }
        }

        /// <summary>
        /// Adds pixel values of two images in-place.
        /// </summary>
        /// <param name="a">The source <see cref="Image"/>.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <remarks>
        /// <para>
        /// This method adds corresponding pixel values of the source image of equal depth to this image.
        /// </para>
        /// <para>
        /// <paramref name="a"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="a"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="a"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIP(Image a, int scaleFactor)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a.BitsPerPixel != this.BitsPerPixel)
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
                        Math.Min(a.Width, this.Width),
                        Math.Min(a.Height, this.Height),
                        a.Bits,
                        a.Stride8,
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
        /// <param name="a">The source <see cref="Image"/>.</param>
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
        /// <para><paramref name="a"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image AddC(Image a, int value, int scaleFactor)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            switch (a.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = a.Clone(false);
                    NativeMethods._addc(
                        a.BitsPerPixel,
                        a.Width,
                        a.Height,
                        a.Bits,
                        a.Stride8,
                        value,
                        dst.Bits,
                        dst.Stride8,
                        scaleFactor);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        a.BitsPerPixel));
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
        /// <param name="a">The first source <see cref="Image"/>.</param>
        /// <param name="b">The second source <see cref="Image"/>.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>
        /// A new destination <see cref="Image"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method subtracts corresponding pixel values of two source images with equal depth and places the results in a destination image.
        /// </para>
        /// <para>
        /// <paramref name="a"/> and <paramref name="b"/> do not have to have the same width and height.
        /// If image sizes are different, the destination image has the size of <paramref name="a"/> and the operation is performed in its upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="a"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="b"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="a"/> and <paramref name="b"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image Sub(Image a, Image b, int scaleFactor)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (a.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = a.Clone(false);
                    NativeMethods._sub(
                        a.BitsPerPixel,
                        Math.Min(a.Width, b.Width),
                        Math.Min(a.Height, b.Height),
                        a.Bits,
                        a.Stride8,
                        b.Bits,
                        b.Stride8,
                        dst.Bits,
                        dst.Stride8,
                        scaleFactor);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        a.BitsPerPixel));
            }
        }

        /// <summary>
        /// Subtracts pixel values of two images in-place.
        /// </summary>
        /// <param name="a">The source <see cref="Image"/>.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <remarks>
        /// <para>
        /// This method subtracts corresponding pixel values of the source image of equal depth to this image.
        /// </para>
        /// <para>
        /// <paramref name="a"/> and this <see cref="Image"/> do not have to have the same width and height.
        /// If image sizes are different, the operation is performed in this <see cref="Image"/> upper-left corner.
        /// </para>
        /// <para>
        /// The scaling of a result is done by multiplying the output pixel values by 2^-<paramref name="scaleFactor"/> before the method returns.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="a"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="a"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubIP(Image a, int scaleFactor)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (a.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            switch (this.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    NativeMethods._sub(
                        a.BitsPerPixel,
                        Math.Min(a.Width, this.Width),
                        Math.Min(a.Height, this.Height),
                        a.Bits,
                        a.Stride8,
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
        /// <param name="a">The source <see cref="Image"/>.</param>
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
        /// <para><paramref name="a"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 8, 24, or 32.
        /// </exception>
        public Image SubC(Image a, int value, int scaleFactor)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            switch (a.BitsPerPixel)
            {
                case 8:
                case 24:
                case 32:
                    Image dst = a.Clone(false);
                    NativeMethods._subc(
                        a.BitsPerPixel,
                        a.Width,
                        a.Height,
                        a.Bits,
                        a.Stride8,
                        value,
                        dst.Bits,
                        dst.Stride8,
                        scaleFactor);
                    return dst;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_UnsupportedDepth,
                        a.BitsPerPixel));
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
