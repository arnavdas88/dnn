// -----------------------------------------------------------------------
// <copyright file="PictureLocator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;
    using System.Threading;
    using Genix.Geometry;
    using Genix.Imaging;

    /// <summary>
    /// Locates pictures on the <see cref="Image"/>.
    /// </summary>
    public class PictureLocator : LocatorBase
    {
        private readonly PictureDetector detector = new PictureDetector();

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureLocator"/> class.
        /// </summary>
        public PictureLocator()
        {
        }

        /// <inheritdoc />
        public override void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            ISet<PictureShape> pictures = this.detector.FindPictures(image, cancellationToken);

            // add found pictures to the image
            page.AddShapes(pictures);
        }
    }
}
