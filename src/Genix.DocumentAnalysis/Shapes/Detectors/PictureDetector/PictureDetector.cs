// -----------------------------------------------------------------------
// <copyright file="PictureDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Genix.Imaging;

    /// <summary>
    /// Finds pictures on the <see cref="Image"/>.
    /// </summary>
    public static class PictureDetector
    {
        /// <summary>
        /// Finds pictures on the <see cref="Image"/>.
        /// The type of pictures to find is determined by the <paramref name="options"/> parameter.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="options">The parameters of this method.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="PictureDetector"/> that operation should be canceled.</param>
        /// <returns>
        /// The set of detected pictures.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not one.
        /// </exception>
        /// <remarks>
        /// <para>This method works with binary (1bpp) images only.</para>
        /// </remarks>
        public static ISet<PictureShape> FindPictures(Image image, PictureDetectorOptions options, CancellationToken cancellationToken)
        {
            return new HashSet<PictureShape>();
        }
    }
}