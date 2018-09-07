// -----------------------------------------------------------------------
// <copyright file="LineDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Detects and removes vertical and horizontal lines.
    /// </summary>
    public static class LineDetector
    {
        /// <summary>
        /// Finds and removes lines from the <see cref="Image"/>.
        /// The type of lines to find is determined by the <c>options</c> parameter.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to find the lines on.</param>
        /// <param name="options">The parameters of this method.</param>
        /// <param name="lines">The detected lines.</param>
        /// <returns>
        /// The <see cref="Image"/> with lines removed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not one.
        /// </exception>
        /// <remarks>
        /// <para>This method works with binary (1bpp) images only.</para>
        /// </remarks>
        public static Image FindAndRemoveLines(Image image, LineDetectionOptions options, out IList<LineShape> lines)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotImplementedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            Image cleanedImage = null;
            lines = new List<LineShape>();

            // find horizontal lines
            if (options.Types.HasFlag(LineTypes.Horizontal))
            {
                // morphology open leaves only pixels that have required number of horizontal neighbors
                Image workImage = image.Open(StructuringElement.Rectangle(30, 1), 1);

                // dilate lines to merge small gaps and include neighboring isolated pixels
                const int DilationSize = 2;
                workImage.DilateIP(StructuringElement.Square(1 + (2 * DilationSize)), 1);

                // find line components and filter out non-line components
                int minLineLength = (int)((options.MinLineLength * image.HorizontalResolution) + 0.5f);
                List<ConnectedComponent> components = workImage.FindConnectedComponents()
                                                               .Where(x => x.Bounds.Width >= minLineLength)
                                                               .ToList();

                // add components to found lines
                foreach (ConnectedComponent component in components)
                {
                    int y = (component.Bounds.Top + component.Bounds.Bottom) / 2;
                    lines.Add(new LineShape(
                        new Point(component.Bounds.Left, y),
                        new Point(component.Bounds.Right, y),
                        Math.Max(1, component.Bounds.Height - (2 * DilationSize)),
                        LineTypes.Horizontal));
                }

                // create mask image
                Image mask = new Image(
                    workImage.Width,
                    workImage.Height,
                    1,
                    workImage.HorizontalResolution,
                    workImage.VerticalResolution);
                mask.AddConnectedComponents(components);
                mask.AndIP(image);

                // apply mask
                cleanedImage = image ^ mask;

                // dilate vertically to close gaps in vertical lines opened by lines removal
                cleanedImage.DilateIP(StructuringElement.Rectangle(1, 7), 1);
                cleanedImage = cleanedImage & image;
            }

            return cleanedImage ?? image.Copy();
        }
    }
}