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
    public static class CheckboxDetector
    {
        /// <summary>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MinBoxSize = 15;

        /// <summary>
        /// The minimum check box size, in pixels, for images with resolution 200 dpi.
        /// </summary>
        private const int MaxBoxSize = 50;

        /// <summary>
        /// Finds check boxes on the <see cref="Image"/>.
        /// The type of check boxes to find is determined by the <paramref name="options"/> parameter.
        /// </summary>
        /// <param name="image">The source <see cref="Image"/>.</param>
        /// <param name="options">The parameters of this method.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="PictureDetector"/> that operation should be canceled.</param>
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
        public static ISet<CheckboxShape> FindCheckboxes(Image image, CheckboxDetectorOptions options, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            HashSet<CheckboxShape> result = new HashSet<CheckboxShape>();
            int minHBoxSize = CheckboxDetector.MinBoxSize.MulDiv(image.HorizontalResolution, 200);
            int maxHBoxSize = CheckboxDetector.MaxBoxSize.MulDiv(image.HorizontalResolution, 200);
            int minVBoxSize = CheckboxDetector.MinBoxSize.MulDiv(image.VerticalResolution, 200);
            int maxVBoxSize = CheckboxDetector.MaxBoxSize.MulDiv(image.VerticalResolution, 200);

            // open up in both directions to find lines
            Image hlines = image.MorphOpen(null, StructuringElement.Brick(minHBoxSize, 1), 1, BorderType.BorderConst, image.WhiteColor);
            if (hlines.IsAllWhite())
            {
                hlines = null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            Image vlines = image.MorphOpen(null, StructuringElement.Brick(1, minVBoxSize), 1, BorderType.BorderConst, image.WhiteColor);
            if (vlines.IsAllWhite())
            {
                vlines = null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            ISet<ConnectedComponent> hcomps = hlines.FindConnectedComponents(8);
            ISet<ConnectedComponent> vcomps = vlines.FindConnectedComponents(8);

            cancellationToken.ThrowIfCancellationRequested();

            BoundedObjectGrid<ConnectedComponent> hgrid = new BoundedObjectGrid<ConnectedComponent>(
                image.Bounds,
                (image.Width / 10).Clip(1, image.Width),
                (image.Height / 20).Clip(1, image.Height));
            hgrid.Add(hcomps.Where(x => x.Bounds.Width <= maxHBoxSize), true, true);

            BoundedObjectGrid<ConnectedComponent> vgrid = new BoundedObjectGrid<ConnectedComponent>(
                image.Bounds,
                (image.Width / 10).Clip(1, image.Width),
                (image.Height / 20).Clip(1, image.Height));
            vgrid.Add(vcomps.Where(x => x.Bounds.Height <= maxVBoxSize), true, true);

            Image draft = image.Clone(false);

            foreach (ConnectedComponent hcomp in hgrid.EnumObjects())
            {
                Rectangle hcbounds = hcomp.Bounds;
                int hdelta = hcbounds.Width / 5;
                Rectangle hsearchArea = Rectangle.Inflate(hcbounds, 0, hcbounds.Width.MulDiv(3, 2));

                foreach (ConnectedComponent hcandidate in hgrid.EnumObjects(hsearchArea))
                {
                    if (hcandidate != hcomp)
                    {
                        Rectangle hcandbounds = hcandidate.Bounds;

                        int vdist = Math.Abs(hcbounds.CenterY - hcandbounds.CenterY);
                        if (vdist.Between(minVBoxSize, maxVBoxSize) &&
                            hcandbounds.X.AreEqual(hcbounds.X, hdelta) &&
                            hcandbounds.Right.AreEqual(hcbounds.Right, hdelta))
                        {
                            Rectangle hunion = Rectangle.Union(hcbounds, hcandbounds);
                            Rectangle vsearchArea = Rectangle.Inflate(hunion, hdelta, 0);

                            ConnectedComponent vcandidatel = null;
                            ConnectedComponent vcandidater = null;
                            foreach (ConnectedComponent vcandidate in vgrid.EnumObjects(vsearchArea))
                            {
                                Rectangle vcandbounds = vcandidate.Bounds;

                                if (vcandbounds.Height.AreEqual(hunion.Height, hdelta) &&
                                    vcandbounds.Top.AreEqual(hunion.Top, hdelta) &&
                                    vcandbounds.Bottom.AreEqual(hunion.Bottom, hdelta))
                                {
                                    if (vcandbounds.Left.AreEqual(hunion.Left, hdelta))
                                    {
                                        vcandidatel = vcandidate;
                                    }
                                    else if (vcandbounds.Right.AreEqual(hunion.Right, hdelta))
                                    {
                                        vcandidater = vcandidate;
                                    }
                                }
                            }

                            if (vcandidatel != null && vcandidater != null)
                            {
                                Rectangle vunion = Rectangle.Union(vcandidatel.Bounds, vcandidater.Bounds);
                                result.Add(new CheckboxShape(Rectangle.Union(hunion, vunion)));

                                draft.AddConnectedComponent(hcomp);
                                draft.AddConnectedComponent(hcandidate);
                                draft.AddConnectedComponent(vcandidatel);
                                draft.AddConnectedComponent(vcandidater);
                            }
                        }
                    }
                }
            }

            // delete check boxes from the image
            foreach (CheckboxShape shape in result)
            {
                image.SetWhite(Rectangle.Inflate(shape.Bounds, 1, 1));
            }

            return result;
        }
    }
}