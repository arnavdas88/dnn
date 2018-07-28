// -----------------------------------------------------------------------
// <copyright file="ImageF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Runtime.CompilerServices;

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
        /// <para><c>width</c> is less than or equal to zero.</para>
        /// <para>-or-</para>
        /// <para><c>height</c> is less than or equal to zero.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImageF(int width, int height, int horizontalResolution, int verticalResolution)
            : base(width, height, 32, horizontalResolution, verticalResolution)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ImageF(ImageF image)
            : this(
            image.Width,
            image.Height,
            image.HorizontalResolution,
            image.VerticalResolution)
        {
        }
    }
}
