// -----------------------------------------------------------------------
// <copyright file="CheckboxLocator.cs" company="Noname, Inc.">
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
    public class CheckboxLocator : LocatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxLocator"/> class.
        /// </summary>
        public CheckboxLocator()
        {
        }

        /// <inheritdoc />
        public override void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            ISet<CheckboxShape> checkboxes = CheckboxDetector.FindCheckboxes(image, new CheckboxDetectorOptions(), cancellationToken);

            // add found check boxes to the image
            page.AddShapes(checkboxes);
        }
    }
}
