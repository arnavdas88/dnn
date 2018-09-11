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
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Detects and removes vertical and horizontal lines.
    /// </summary>
    public static class LineDetector
    {
        /// <summary>
        /// The maximum line width, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MaxLineWidth = 10;

        /// <summary>
        /// The minimum line length, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MinLineLength = 50;

        /// <summary>
        /// The maximum size of line residue, in pixels, for images with resolution 200 dpi.
        /// </summary>
        /// <remarks>
        /// These are the pixels that fail the long thin opening,
        /// and therefore don't make it to the candidate line mask,
        /// but are nevertheless part of the line.
        /// </remarks>
        private const int MaxLineResidue = 6;

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

            // close up small holes
            int maxLineWidth = LineDetector.MaxLineWidth.MulDiv(image.HorizontalResolution, 200);
            Image closedImage = image.Close(StructuringElement.Square(maxLineWidth / 3), 1);

            // open up to detect big solid areas
            Image openedImage = image.Open(StructuringElement.Square(maxLineWidth), 1);
            Image hollowImage = closedImage.Sub(openedImage, 0);

            // open up in both directions to find lines
            int minLineLength = LineDetector.MinLineLength.MulDiv(image.HorizontalResolution, 200);
            Image hlinesImage = hollowImage.Open(StructuringElement.Rectangle(minLineLength, 1), 1);
            Image vlinesImage = hollowImage.Open(StructuringElement.Rectangle(1, minLineLength), 1);

            // check for line presence
            bool hasHLines = !hlinesImage.IsAllWhite();
            bool hasVLines = !vlinesImage.IsAllWhite();
            if (!hasHLines && !hasVLines)
            {
                // return the original image
                return image;
            }

            // create image that has no lines
            Image noLinesImage = hasVLines ? image.Sub(vlinesImage, 0) : null;
            if (hasHLines)
            {
                if (noLinesImage != null)
                {
                    noLinesImage.SubIP(hlinesImage, 0);
                }
                else
                {
                    noLinesImage = image.Sub(hlinesImage, 0);
                }
            }

            // vertical lines
            if (hasVLines)
            {
                int maxLineResidue = LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                Image noVLinesImage = noLinesImage.Erode(StructuringElement.Rectangle(maxLineResidue, 1), 1);
            }

            // horizontal lines
            if (hasHLines)
            {
                int maxLineResidue = LineDetector.MaxLineResidue.MulDiv(image.HorizontalResolution, 200);
                Image noHLinesImage = noLinesImage.Erode(StructuringElement.Rectangle(1, maxLineResidue), 1);
            }

            Image res = image.Sub(hlinesImage, 0).Sub(vlinesImage, 0);

            // find horizontal lines
            if (options.Types.HasFlag(LineTypes.Horizontal))
            {
                // morphology open leaves only pixels that have required number of horizontal neighbors
                Image workImage = image.Open(StructuringElement.Rectangle(30, 1), 1);

                // dilate lines to merge small gaps and include neighboring isolated pixels
                const int DilationSize = 2;
                workImage.DilateIP(StructuringElement.Square(1 + (2 * DilationSize)), 1);

                // find line components and filter out non-line components
                /*int*/ minLineLength = (int)((options.MinLineLength * image.HorizontalResolution) + 0.5f);
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
                        MinMax.Max(1, component.Bounds.Height - (2 * DilationSize)),
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