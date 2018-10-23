// -----------------------------------------------------------------------
// <copyright file="TextLocator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Locates machine-printed text the <see cref="Image"/>.
    /// </summary>
    public class TextLocator : LocatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLocator"/> class.
        /// </summary>
        public TextLocator()
        {
        }

        /// <inheritdoc />
        public override void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            ISet<TextShape> texts = TextDetector.FindText(image, new TextDetectionOptions(), cancellationToken);

            // add found lines to the image
            page.AddShapes(texts);
        }
    }
}
