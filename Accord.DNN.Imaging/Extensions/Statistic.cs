// -----------------------------------------------------------------------
// <copyright file="Convert.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Leptonica;

    /// <summary>
    /// Provides statistical extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Statistic
    {
        /// <summary>
        /// Calculates a horizontal histogram for the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to analize.</param>
        /// <returns>
        /// A collection of histogram bins.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] HistogramY(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            ulong[] bits = image.Bits;

            int[] histogram = new int[height];

            for (int i = 0, pos = 0; i < height; i++, pos += stride1)
            {
                histogram[i] = (int)BitUtils.CountOneBits(width, bits, pos);
            }

            return histogram;
        }
    }
}
