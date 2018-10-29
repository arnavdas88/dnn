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
    public class PictureDetector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureDetector"/> class.
        /// </summary>
        public PictureDetector()
        {
        }

        /// <summary>
        /// Finds pictures on the <see cref="Image"/>.
        /// The type of pictures to find is determined by the class parameters.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
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
        public ISet<PictureShape> FindPictures(Image image, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return new HashSet<PictureShape>();
        }
    }
}