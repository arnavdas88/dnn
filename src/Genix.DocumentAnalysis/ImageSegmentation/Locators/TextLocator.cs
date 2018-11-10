// -----------------------------------------------------------------------
// <copyright file="TextLocator.cs" company="Noname, Inc.">
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
    /// Locates machine-printed text on the <see cref="Image"/>.
    /// </summary>
    public class TextLocator : LocatorBase
    {
        private readonly TextDetector detector = new TextDetector();

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLocator"/> class.
        /// </summary>
        public TextLocator()
        {
        }

        /// <inheritdoc />
        public override void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            ISet<TextShape> texts = this.detector.FindText(image, page.EnumAllLines(), cancellationToken);

            // add found lines to the image
            page.AddShapes(texts);
        }
    }
}
