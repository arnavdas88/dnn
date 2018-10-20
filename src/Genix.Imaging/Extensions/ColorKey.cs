// -----------------------------------------------------------------------
// <copyright file="ColorKey.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <content>
    /// The color keying lets you replace all pixels in the image equal to the specified color
    /// with the corresponding pixels of the another background image.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Performs color keying of two images.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="background">The <see cref="Image"/> that contains background pixels.</param>
        /// <param name="color">The value of the key color.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="background"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is neither 8 nor 24 nor 32 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method replaces all areas of this <see cref="Image"/> containing the specified <paramref name="color"/>
        /// with the corresponding pixels of the background image <paramref name="background"/> and stores the result in the destination image <paramref name="dst"/>.
        /// </para>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image ColorKey(Image dst, Image background, uint color)
        {
            if (background == null)
            {
                throw new ArgumentNullException(nameof(background));
            }

            if (this.BitsPerPixel != 8 && this.BitsPerPixel != 24 && this.BitsPerPixel != 32)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Properties.Resources.E_UnsupportedDepth,
                    this.BitsPerPixel));
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            Image.ExecuteIPPMethod(() =>
            {
                return NativeMethods.colorkey(
                    this.BitsPerPixel,
                    0,
                    0,
                    this.Width,
                    this.Height,
                    this.Bits,
                    this.Stride8,
                    background.Bits,
                    background.Stride8,
                    dst.Bits,
                    dst.Stride8,
                    color);
            });

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int colorkey(
                int bitsPerPixel,
                int x,
                int y,
                int width,
                int height,
                ulong[] src1,
                int stridesrc1,
                ulong[] src2,
                int stridesrc2,
                ulong[] dst,
                int stridedst,
                uint color);
        }
    }
}
