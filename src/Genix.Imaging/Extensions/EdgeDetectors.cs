// -----------------------------------------------------------------------
// <copyright file="EdgeDetectors.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <content>
    /// Provides edge detection methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Implements Canny algorithm for edge detection.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="thresholdLow">The lower threshold for edges detection.</param>
        /// <param name="thresholdHigh">The upper threshold for edges detection.</param>
        /// <param name="borderType">The type of border.</param>
        /// <param name="borderValue">The value of border pixels when <paramref name="borderType"/> is <see cref="BorderType.BorderConst"/>.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Image Canny(Image dst, float thresholdLow, float thresholdHigh, BorderType borderType, uint borderValue)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, this.BitsPerPixel);

            IPP.Execute(() =>
            {
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        return NativeMethods.canny(
                            0,
                            0,
                            this.Width,
                            this.Height,
                            (byte*)bitssrc,
                            this.Stride8,
                            (byte*)bitsdst,
                            dst.Stride8,
                            thresholdLow,
                            thresholdHigh,
                            borderType,
                            borderValue);
                    }
                }
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
            public static unsafe extern int canny(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                byte* dst,
                int stridedst,
                float thresholdLow,
                float thresholdHigh,
                BorderType borderType,
                uint borderValue);
        }
    }
}
