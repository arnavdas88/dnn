// -----------------------------------------------------------------------
// <copyright file="PictureLocator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;
    using System.Threading;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Locates machine-printed text the <see cref="Image"/>.
    /// </summary>
    public class PictureLocator : LocatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureLocator"/> class.
        /// </summary>
        public PictureLocator()
        {
        }

        /// <inheritdoc />
        public override void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            ISet<PictureShape> pictures = PictureDetector.FindPictures(image, new PictureDetectionOptions(), cancellationToken);

            // add found lines to the image
            page.AddShapes(pictures);
        }
    }
}
