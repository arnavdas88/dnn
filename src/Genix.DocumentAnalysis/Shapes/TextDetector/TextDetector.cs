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

            AlignedObjectGrid<ConnectedComponent> componentgrid = new AlignedObjectGrid<ConnectedComponent>(
                image.Bounds,
                200.MulDiv(image.HorizontalResolution, 200),
                100.MulDiv(image.VerticalResolution, 200));

            componentgrid.Add(
                closing.FindConnectedComponents(4) /*.Where(x => x.Power > 10 && x.Bounds.Height <= 100)*/,
                true,
                true);

            AlignedObjectGrid<TextShape> shapegrid = new AlignedObjectGrid<TextShape>(
                image.Bounds,
                200.MulDiv(image.HorizontalResolution, 200),
                100.MulDiv(image.VerticalResolution, 200));

            foreach (ConnectedComponent component in componentgrid.EnumObjects())
            {
                if (component.VerticalAlignment == VerticalAlignment.None)
                {
                    IList<ConnectedComponent> alignedComponents = componentgrid.FindVerticalAlignment(component, VerticalAlignment.Bottom, 50);
                    if (alignedComponents.Count > 1)
                    {
                        foreach (ConnectedComponent alignedComponent in alignedComponents)
                        {
                            alignedComponent.VerticalAlignment = VerticalAlignment.Bottom;
                        }

                        shapegrid.Add(new TextShape(Rectangle.Union(alignedComponents.Select(x => x.Bounds))), true, true);
                    }
                }
            }

            // assign unassigned components
            foreach (ConnectedComponent component in componentgrid.EnumObjects())
            {
                if (component.VerticalAlignment == VerticalAlignment.None)
                {
                    if (shapegrid.FindContainer(component.Bounds) == null)
                    {
                        shapegrid.Add(new TextShape(component.Bounds), true, true);
                    }
                }
            }

            shapegrid.Compact();

            Image draft = image.ConvertTo(null, 24);
            foreach (TextShape shape in shapegrid.EnumObjects())
            {
                draft.DrawRectangle(shape.Bounds, 0x00800000);
            }

            int count = shapegrid.EnumObjects().Count();

            return new HashSet<TextShape>();
        }
    }
}