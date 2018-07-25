// -----------------------------------------------------------------------
// <copyright file="Convert.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Leptonica;

    /// <summary>
    /// Provides conversion extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to binarize.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Binarize(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }

            try
            {
                using (Pix pixs = image.CreatePix())
                {
                    using (Pix pixd = pixs.pixOtsu(false))
                    {
                        return pixd.CreateImage(image.HorizontalResolution, image.VerticalResolution);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot binarize the image.", e);
            }
        }

        /// <summary>
        /// Converts this <see cref="Image"/> from black-and-white to gray scale.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to convert.</param>
        /// <param name="value0">8-bit value to be used for 0s pixels.</param>
        /// <param name="value1">8-bit value to be used for 1s pixels.</param>
        /// <returns>
        /// A new gray scale <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Convert1To8(this Image image, byte value0, byte value1)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                8,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert1to8(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
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
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to convert.</param>
        /// <param name="threshold">The threshold level.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        /// <remarks>
        /// <para>
        /// If the input pixel is more than, or equal to the <paramref name="threshold"/> value, the corresponding output bit is set to 0.
        /// </para>
        /// <para>
        /// If the input pixel is less than the <paramref name="threshold"/> value, the corresponding output bit is set to 1.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image Convert8To1(this Image image, byte threshold)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            Image dst = new Image(
                image.Width,
                image.Height,
                1,
                image.HorizontalResolution,
                image.VerticalResolution);

            if (NativeMethods._convert8to1(
                image.Width,
                image.Height,
                image.Bits,
                image.Stride,
                dst.Bits,
                dst.Stride,
                threshold) != 0)
            {
                throw new OutOfMemoryException();
            }

            return dst;
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Imaging.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int _convert1to8(
                int width,
                int height,
                [In] ulong[] src,
                int srcstride,
                [Out] ulong[] dst,
                int dststride,
                byte value0,
                byte value1);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int _convert8to1(
                int width,
                int height,
                [In] ulong[] src,
                int srcstride,
                [Out] ulong[] dst,
                int dststride,
                byte threshold);
        }
    }
}
