// -----------------------------------------------------------------------
// <copyright file="LineLocator.cs" company="Noname, Inc.">
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
    /// Locates and removes lines from the <see cref="Image"/>.
    /// </summary>
    public class LineLocator : LocatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineLocator"/> class.
        /// </summary>
        public LineLocator()
        {
        }

        /// <inheritdoc />
        public override void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            ISet<LineShape> lines = LineDetector.FindLines(image, new LineDetectionOptions(), cancellationToken);

            // add found lines to the image
            page.AddShapes(lines);
        }
    }
}
