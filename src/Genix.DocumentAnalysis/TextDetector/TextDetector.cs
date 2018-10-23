// -----------------------------------------------------------------------
// <copyright file="TextDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging;
    using Genix.Imaging.Leptonica;

    /// <summary>
    /// Finds machine-printed text on the <see cref="Image"/>.
    /// </summary>
    public static class TextDetector
    {
        /// <summary>
        /// Finds machine-printed text on the <see cref="Image"/>.
        /// The type of text to find is determined by the <paramref name="options"/> parameter.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="options">The parameters of this method.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="TextDetector"/> that operation should be canceled.</param>
        /// <returns>
        /// The detected text.
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
        public static ISet<TextShape> FindText(Image image, TextDetectionOptions options, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image closing = image.MorphClose(null, StructuringElement.Brick(9, 1), 1, BorderType.BorderConst, 0);

            ISet<ConnectedComponent> components = closing.FindConnectedComponents(4);

            BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(
                image.Bounds,
                200.MulDiv(image.HorizontalResolution, 200),
                100.MulDiv(image.VerticalResolution, 200),
                new RectangleLTRBComparer());
            grid.Add(components, false, false);

            return new HashSet<TextShape>();
        }
    }
}