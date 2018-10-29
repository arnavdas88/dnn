// -----------------------------------------------------------------------
// <copyright file="CheckboxDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Finds check boxes on the <see cref="Image"/>.
    /// </summary>
    public class CheckboxDetector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxDetector"/> class.
        /// </summary>
        public CheckboxDetector()
        {
        }

        /// <summary>
        /// Gets or sets the minimum check box size.
        /// </summary>
        /// <value>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </value>
        private int MinBoxSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the minimum check box size.
        /// </summary>
        /// <value>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </value>
        private int MaxBoxSize { get; set; } = 50;

        /// <summary>
        /// Gets or sets a value indicating whether the found check boxes should be removed from the image.
        /// </summary>
        /// <value>
        /// <b>true</b> to remove found check boxes from the image; otherwise, <b>false</b>.
        /// </value>
        private bool RemoveCheckboxes { get; set; } = true;

        /// <summary>
        /// Finds check boxes on the <see cref="Image"/>.
        /// The type of check boxes to find is determined by the class parameters.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="CheckboxDetector"/> that operation should be canceled.</param>
        /// <returns>
        /// The set of detected check boxes.
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
        public ISet<CheckboxShape> FindCheckboxes(Image image, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            HashSet<CheckboxShape> result = new HashSet<CheckboxShape>(32);

            // compute image-dependent parameters
            int minBoxSizeH = this.MinBoxSize.MulDiv(image.HorizontalResolution, 200);
            int maxBoxSizeH = this.MaxBoxSize.MulDiv(image.HorizontalResolution, 200);
            int minBoxSizeV = this.MinBoxSize.MulDiv(image.VerticalResolution, 200);
            int maxBoxSizeV = this.MaxBoxSize.MulDiv(image.VerticalResolution, 200);

            // create a draft that would show found checkboxes
#if DEBUG
            Image draft = image.Clone(false);
#endif

            // keep track of tested horizontal components that did not yield results
            HashSet<Rectangle> testedHBounds = new HashSet<Rectangle>();

            // the algorith proceeds in two steps
            // first, we find a pair of parallel horizontal lines that have similar length and horizontal position
            // second, we find a pair of parallel vertical lines that would connect horizontal lines on both sides to form a box
            BoundedObjectGrid<ConnectedComponent> hgrid = FindHorizontalLines();
            if (hgrid != null)
            {
                BoundedObjectGrid<ConnectedComponent> vgrid = FindVerticalLines();
                if (vgrid != null)
                {
                    foreach (ConnectedComponent hcomp1 in hgrid.EnumObjects())
                    {
                        if (hcomp1.HorizontalAlignment == HorizontalAlignment.None)
                        {
                            Rectangle hbounds1 = hcomp1.Bounds;
                            int hdelta = hbounds1.Width / 5;

                            foreach (ConnectedComponent hcomp2 in hgrid.EnumObjects(Rectangle.Inflate(hbounds1, 0, hbounds1.Width.MulDiv(3, 2))))
                            {
                                if (hcomp2 != hcomp1)
                                {
                                    if (hcomp2.HorizontalAlignment == HorizontalAlignment.None)
                                    {
                                        Rectangle hbounds2 = hcomp2.Bounds;
                                        Rectangle hbounds = Rectangle.Union(hbounds1, hbounds2);
                                        hdelta = hbounds.Width / 5;

                                        if (!testedHBounds.Contains(hbounds))
                                        {
                                            testedHBounds.Add(hbounds);

                                            if (TestHorizontalComponents(hbounds1, hbounds2, hdelta))
                                            {
                                                // after we found a pair of matching horizontal lines
                                                // start looking for a pair of vertical lines that connect them
                                                ConnectedComponent vcomp1 = null;
                                                ConnectedComponent vcomp2 = null;
                                                foreach (ConnectedComponent vcomp in vgrid.EnumObjects(Rectangle.Inflate(hbounds, hdelta, 0)))
                                                {
                                                    if (vcomp.VerticalAlignment == VerticalAlignment.None)
                                                    {
                                                        Rectangle vbounds = vcomp.Bounds;

                                                        if (TestVerticalComponent(hbounds, vbounds, hdelta))
                                                        {
                                                            if (vbounds.Left.AreEqual(hbounds.Left, hdelta))
                                                            {
                                                                vcomp1 = vcomp;
                                                            }
                                                            else if (vbounds.Right.AreEqual(hbounds.Right, hdelta))
                                                            {
                                                                vcomp2 = vcomp;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (vcomp1 != null && vcomp2 != null)
                                                {
                                                    Rectangle vunion = Rectangle.Union(vcomp1.Bounds, vcomp2.Bounds);
                                                    result.Add(new CheckboxShape(Rectangle.Union(hbounds, vunion)));
#if DEBUG
                                                    draft.AddConnectedComponent(hcomp1);
                                                    draft.AddConnectedComponent(hcomp2);
                                                    draft.AddConnectedComponent(vcomp1);
                                                    draft.AddConnectedComponent(vcomp2);
#endif

                                                    // mark used components, so we do not test them twice
                                                    hcomp1.HorizontalAlignment = HorizontalAlignment.Left;
                                                    hcomp2.HorizontalAlignment = HorizontalAlignment.Left;
                                                    vcomp1.VerticalAlignment = VerticalAlignment.Top;
                                                    vcomp2.VerticalAlignment = VerticalAlignment.Top;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // delete check boxes from the image
            if (this.RemoveCheckboxes)
            {
                /*foreach (CheckboxShape shape in result)
                {
                    image.SetWhite(Rectangle.Inflate(shape.Bounds, 1, 1));
                }*/
#if DEBUG
                image.Sub(image, draft, 0);
#endif
            }

            return result;

            BoundedObjectGrid<ConnectedComponent> FindHorizontalLines()
            {
                Image lines = image.MorphOpen(null, StructuringElement.Brick(minBoxSizeH, 1), 1, BorderType.BorderConst, image.WhiteColor);
                if (lines.IsAllWhite())
                {
                    return null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);
                ////comps.Re

                cancellationToken.ThrowIfCancellationRequested();

                BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(
                    image.Bounds,
                    (image.Width / 10).Clip(1, image.Width),
                    (image.Height / 20).Clip(1, image.Height));
                grid.Add(comps.Where(x => x.Bounds.Width <= maxBoxSizeH), true, true);

                cancellationToken.ThrowIfCancellationRequested();

                return grid;
            }

            BoundedObjectGrid<ConnectedComponent> FindVerticalLines()
            {
                Image lines = image.MorphOpen(null, StructuringElement.Brick(1, minBoxSizeV), 1, BorderType.BorderConst, image.WhiteColor);
                if (lines.IsAllWhite())
                {
                    return null;
                }

                ISet<ConnectedComponent> comps = lines.FindConnectedComponents(8);

                cancellationToken.ThrowIfCancellationRequested();

                BoundedObjectGrid<ConnectedComponent> grid = new BoundedObjectGrid<ConnectedComponent>(
                    image.Bounds,
                    (image.Width / 10).Clip(1, image.Width),
                    (image.Height / 20).Clip(1, image.Height));
                grid.Add(comps.Where(x => x.Bounds.Height <= maxBoxSizeV), true, true);

                return grid;
            }

            bool TestHorizontalComponents(Rectangle bounds1, Rectangle bounds2, int delta)
            {
                int dist = Math.Abs(bounds1.CenterY - bounds2.CenterY);
                return dist.Between(minBoxSizeV, maxBoxSizeV) &&
                    (dist.AreEqual(bounds1.Width, delta) || dist.AreEqual(bounds2.Width, delta)) &&
                    bounds2.X.AreEqual(bounds1.X, delta) &&
                    bounds2.Right.AreEqual(bounds1.Right, delta);
            }

            bool TestVerticalComponent(Rectangle hbounds, Rectangle vbounds, int delta)
            {
                return vbounds.Height.AreEqual(hbounds.Height, delta) &&
                    vbounds.Top.AreEqual(hbounds.Top, delta) &&
                    vbounds.Bottom.AreEqual(hbounds.Bottom, delta);
            }
        }
    }
}