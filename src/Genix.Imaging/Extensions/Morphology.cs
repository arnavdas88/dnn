﻿// -----------------------------------------------------------------------
// <copyright file="Morphology.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    /// <content>
    /// Provides morphology extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        public void FloodFill(Image mask)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            if (mask.BitsPerPixel != this.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int bitsPerPixel = this.BitsPerPixel;
            int stridesrc = this.Stride;
            int stridemask = mask.Stride;

            int width = Math.Min(this.Width, mask.Width);
            int bitwidth = width * bitsPerPixel;
            int height = Math.Min(this.Height, mask.Height);
            int stride = Math.Min(stridesrc, stridemask);

            ulong[] bits = this.Bits;
            ulong[] bitsmask = mask.Bits;
            ulong[] buffer = new ulong[stride];

            int top = 0;
            int bottom = height - 1;
            while (top <= bottom)
            {
                // scan from upper-left to bottom-right corner
                Vectors.Set(stride, 0, buffer, 0);
                for (int i = top, off = i * stridesrc, offmask = i * stridemask; i < height; i++, off += stridesrc, offmask += stridemask)
                {
                    // or from above
                    if (i > 0)
                    {
                        Vectors.Copy(stride, bits, off - stridesrc, buffer, 0);
                    }

                    // or from left
                    BitUtils.Or(bitwidth - bitsPerPixel, bits, off * 64, buffer, bitsPerPixel);

                    // mask pixels
                    Vectors.And(stride, bitsmask, offmask, buffer, 0);

                    // copy back to image
                    int oldcount = BitUtils.CountOneBits(bitwidth, bits, off * 64);
                    BitUtils.Or(bitwidth, buffer, 0, bits, off * 64);
                    int newcount = BitUtils.CountOneBits(bitwidth, bits, off * 64);

                    if (oldcount == newcount)
                    {
                        // pixel count did not change - shrink upper boundary if we are at the top
                        if (i == top)
                        {
                            top++;
                        }

                        if (i >= bottom)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // pixel count did change - expand lower boundary
                        bottom = Core.MinMax.Max(i, bottom);
                    }
                }

                // scan from bottom-right to upper-left corner
                Vectors.Set(stride, 0, buffer, 0);
                for (int i = bottom, off = i * stridesrc, offmask = i * stridemask; i >= 0; i--, off -= stridesrc, offmask -= stridemask)
                {
                    // or from below
                    if (i < height - 1)
                    {
                        Vectors.Copy(stride, bits, off + stridesrc, buffer, 0);
                    }

                    // or from right
                    BitUtils.Or(bitwidth - bitsPerPixel, bits, (off * 64) + bitsPerPixel, buffer, 0);

                    // mask pixels
                    Vectors.And(stride, bitsmask, offmask, buffer, 0);

                    // copy back to image
                    int oldcount = BitUtils.CountOneBits(bitwidth, bits, off * 64);
                    BitUtils.Or(bitwidth, buffer, 0, bits, off * 64);
                    int newcount = BitUtils.CountOneBits(bitwidth, bits, off * 64);

                    if (oldcount == newcount)
                    {
                        // pixel count did not change - shrink lower boundary if we are at the bottom
                        if (i == bottom)
                        {
                            bottom--;
                        }

                        if (i <= top)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // pixel count did change - expand upper boundary
                        top = Core.MinMax.Min(i, top);
                    }
                }
            }
        }

        /// <summary>
        /// Dilates this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public void Dilate(StructuringElement kernel, int iterations)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            Image mask = this.Clone(false);
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // create mask
                if (iteration > 0)
                {
                    mask.SetToMin();
                }

                // special case for rectangular kernel
                // instead of applying m x n mask
                // we sequentially apply n x 1 and 1 x n masks
                if (kernel is RectangleStructuringElement rectangularKernel)
                {
                    // create vertical mask
                    foreach (Point point in rectangularKernel.GetVerticalElements(new Point(-1, -1)))
                    {
                        MakeMask(point);
                    }

                    // apply mask
                    this.MaximumIP(0, 0, this.Width, this.Height, mask, 0, 0);

                    // create horizontal mask
                    mask.SetToMin();

                    foreach (Point point in rectangularKernel.GetHorizontalElements(new Point(-1, -1)))
                    {
                        MakeMask(point);
                    }
                }
                else
                {
                    foreach (Point point in kernel.GetElements())
                    {
                        MakeMask(point);
                    }
                }

                // apply mask
                this.MaximumIP(0, 0, this.Width, this.Height, mask, 0, 0);
            }

            void MakeMask(Point point)
            {
                mask.MaximumIP(
                    Core.MinMax.Max(-point.X, 0),
                    Core.MinMax.Max(-point.Y, 0),
                    this.Width - Math.Abs(point.X),
                    this.Height - Math.Abs(point.Y),
                    this,
                    Core.MinMax.Max(point.X, 0),
                    Core.MinMax.Max(point.Y, 0));
            }
        }

        /// <summary>
        /// Erodes this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public void Erode(StructuringElement kernel, int iterations)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            Image mask = this.Clone(false);
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // create mask
                mask.SetToMax();

                // special case for rectangular kernel
                // instead of applying m x n mask
                // we sequentially apply n x 1 and 1 x n masks
                if (kernel is RectangleStructuringElement rectangularKernel)
                {
                    // create vertical mask
                    foreach (Point point in rectangularKernel.GetVerticalElements(new Point(-1, -1)))
                    {
                        MakeMask(point);
                    }

                    // apply mask
                    this.MinimumIP(0, 0, this.Width, this.Height, mask, 0, 0);

                    // create horizontal mask
                    mask.SetToMax();

                    foreach (Point point in rectangularKernel.GetHorizontalElements(new Point(-1, -1)))
                    {
                        MakeMask(point);
                    }
                }
                else
                {
                    foreach (Point point in kernel.GetElements())
                    {
                        MakeMask(point);
                    }
                }

                // apply mask
                this.MinimumIP(0, 0, this.Width, this.Height, mask, 0, 0);
            }

            void MakeMask(Point point)
            {
                mask.MinimumIP(
                    Core.MinMax.Max(-point.X, 0),
                    Core.MinMax.Max(-point.Y, 0),
                    this.Width - Math.Abs(point.X),
                    this.Height - Math.Abs(point.Y),
                    this,
                    Core.MinMax.Max(point.X, 0),
                    Core.MinMax.Max(point.Y, 0));
            }
        }

        /// <summary>
        /// Perform morphological opening operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public void MorphOpen(StructuringElement kernel, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                this.Erode(kernel, 1);
                this.Dilate(kernel, 1);
            }
        }

        /// <summary>
        /// Perform morphological closing operation this <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public void MorphClose(StructuringElement kernel, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                this.Dilate(kernel, 1);
                this.Erode(kernel, 1);
            }
        }

        /// <summary>
        /// Removes small isolated pixels from this <see cref="Image"/>.
        /// </summary>
        public void Despeckle()
        {
            // create masks
            ulong[] mask = new ulong[this.Bits.Length];
            ulong[] notbits = new ulong[this.Bits.Length];
            BitUtils64.WordsNOT(this.Bits.Length, this.Bits, 0, notbits, 0);

            // remove isolated pixels
            Image.BuildORMask(this, StructuringElement.Square(3), null, mask, false);
            Vectors.And(mask.Length, mask, 0, this.Bits, 0);

            // 0 0 0
            // 0 x 0
            // x x x
            Image.BuildORMask(this, StructuringElement.Rectangle(3, 2, new Point(1, 1)), null, mask, true);
            Image.BuildORMask(this, StructuringElement.Rectangle(3, 1, new Point(1, -1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, this.Bits, 0);

            // x x x
            // 0 x 0
            // 0 0 0
            Image.BuildORMask(this, StructuringElement.Rectangle(3, 2, new Point(1, 0)), null, mask, true);
            Image.BuildORMask(this, StructuringElement.Rectangle(3, 1, new Point(1, 1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, this.Bits, 0);

            // x 0 0
            // x x 0
            // x 0 0
            Image.BuildORMask(this, StructuringElement.Rectangle(2, 3, new Point(0, 1)), null, mask, true);
            Image.BuildORMask(this, StructuringElement.Rectangle(1, 3, new Point(1, 1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, this.Bits, 0);

            // 0 0 x
            // 0 x x
            // 0 0 x
            Image.BuildORMask(this, StructuringElement.Rectangle(2, 3, new Point(1, 1)), null, mask, true);
            Image.BuildORMask(this, StructuringElement.Rectangle(1, 3, new Point(-1, 1)), notbits, mask, false);
            Vectors.And(mask.Length, mask, 0, this.Bits, 0);

            // fill isolated gaps
            Image.BuildANDMask(this, StructuringElement.Cross(3, 3), null, mask, true);
            Vectors.Or(mask.Length, mask, 0, this.Bits, 0);

            // x x x
            // x 0 x
            // 0 0 0
            Image.BuildANDMask(this, StructuringElement.Rectangle(3, 2, new Point(1, 1)), null, mask, true);
            Image.BuildANDMask(this, StructuringElement.Rectangle(3, 1, new Point(1, -1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, this.Bits, 0);

            // 0 0 0
            // x 0 x
            // x x x
            Image.BuildANDMask(this, StructuringElement.Rectangle(3, 2, new Point(1, 0)), null, mask, true);
            Image.BuildANDMask(this, StructuringElement.Rectangle(3, 1, new Point(1, 1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, this.Bits, 0);

            // 0 x x
            // 0 0 x
            // 0 x x
            Image.BuildANDMask(this, StructuringElement.Rectangle(2, 3, new Point(0, 1)), null, mask, true);
            Image.BuildANDMask(this, StructuringElement.Rectangle(1, 3, new Point(1, 1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, this.Bits, 0);

            // x x 0
            // x 0 0
            // x x 0
            Image.BuildANDMask(this, StructuringElement.Rectangle(2, 3, new Point(1, 1)), null, mask, true);
            Image.BuildANDMask(this, StructuringElement.Rectangle(1, 3, new Point(-1, 1)), notbits, mask, false);
            Vectors.Or(mask.Length, mask, 0, this.Bits, 0);
        }

        /// <summary>
        /// Finds connected components on this <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A set of <see cref="ConnectedComponent"/> objects found.
        /// </returns>
        public ISet<ConnectedComponent> FindConnectedComponents()
        {
            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            HashSet<ConnectedComponent> all = new HashSet<ConnectedComponent>();
            List<Stroke> last = new List<Stroke>();
            List<Stroke> current = new List<Stroke>();

            int width = this.Width;
            int height = this.Height;
            int stride1 = this.Stride1;
            ulong[] bits = this.Bits;

            for (int y = 0, ypos = 0; y < height; y++, ypos += stride1)
            {
                // find intervals
                int lastIndex = 0;
                for (int xpos = ypos, xposend = ypos + width; xpos < xposend;)
                {
                    int start = BitUtils.BitScanOneForward(xposend - xpos, bits, xpos);
                    if (start == -1)
                    {
                        break;
                    }

                    int end = BitUtils.BitScanZeroForward(xposend - (start + 1), bits, start + 1);
                    if (end == -1)
                    {
                        end = xposend;
                    }

                    // merge interval
                    MergeStroke(start - ypos, end - start);
                    xpos = end + 1;

                    void MergeStroke(int x, int length)
                    {
                        // the component we will attach the stroke to
                        ConnectedComponent component = null;

                        // start matching from the position we stopped at last time
                        for (int i = lastIndex, x2 = x + length, ii = last.Count; i < ii; i++)
                        {
                            Stroke lastStroke = last[i];
                            if (ConnectedComponent.StrokesIntersect(lastStroke.X, lastStroke.Length, x, length))
                            {
                                if (component == null)
                                {
                                    component = lastStroke.Component;
                                    component.AddStroke(y, x, length);
                                }
                                else
                                {
                                    ConnectedComponent anotherComponent = lastStroke.Component;
                                    if (anotherComponent != component)
                                    {
                                        // merge components if strokes touches more than one components
                                        component.MergeWith(anotherComponent);

                                        // remove merged component from the set
                                        all.Remove(anotherComponent);

                                        // replace merged component in previous line
                                        ReplaceComponent(last, i);

                                        // replace merged component in this line
                                        ReplaceComponent(current, current.Count);

                                        void ReplaceComponent(List<Stroke> strokes, int anchorPosition)
                                        {
                                            Rectangle anotherBounds = anotherComponent.Bounds;
                                            for (int j = anchorPosition, jj = strokes.Count; j < jj; j++)
                                            {
                                                if (strokes[j].Component == anotherComponent)
                                                {
                                                    strokes[j].Component = component;
                                                }
                                                else if (strokes[j].Component != component && strokes[j].X > anotherBounds.Right)
                                                {
                                                    Debug.Assert(strokes.Skip(anchorPosition).All(s => s.Component != anotherComponent), "Component must be removed.");
                                                    break;
                                                }
                                            }

                                            for (int j = anchorPosition - 1; j >= 0; j--)
                                            {
                                                if (strokes[j].Component == anotherComponent)
                                                {
                                                    strokes[j].Component = component;
                                                }
                                                else if (strokes[j].Component != component && strokes[j].X < anotherBounds.X)
                                                {
                                                    Debug.Assert(strokes.Take(anchorPosition - 1).All(s => s.Component != anotherComponent), "Component must be removed.");
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                lastIndex = i;
                            }
                            else if (x2 < lastStroke.X)
                            {
                                // all remaining strokes from previous line are to the left of this one
                                // we can stop matching
                                break;
                            }
                        }

                        if (component == null)
                        {
                            component = new ConnectedComponent(y, x, length);
                            all.Add(component);
                        }

                        current.Add(new Stroke(x, length, component));
                    }
                }

                Swapping.Swap(ref current, ref last);
                current.Clear();
            }

            Debug.Assert(this.Power() == all.Sum(x => x.Power), "The number of pixels on image and in components must match.");
            return all;
        }

        /// <summary>
        /// Adds black pixels contained in the <see cref="ConnectedComponent"/> to this <see cref="Image"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="component"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public void AddConnectedComponent(ConnectedComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ulong[] bits = this.Bits;
            int stride1 = this.Stride1;

            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils.SetBits(length, bits, (y * stride1) + x);
            }
        }

        /// <summary>
        /// Adds black pixels contained in the collection of <see cref="ConnectedComponent"/> objects to this <see cref="Image"/>.
        /// </summary>
        /// <param name="components">The collection if <see cref="ConnectedComponent"/> objects to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="components"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public void AddConnectedComponents(IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            foreach (ConnectedComponent component in components)
            {
                this.AddConnectedComponent(component);
            }
        }

        /// <summary>
        /// Removes black pixels contained in the <see cref="ConnectedComponent"/> from this <see cref="Image"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="component"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public void RemoveConnectedComponent(ConnectedComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            ulong[] bits = this.Bits;
            int stride1 = this.Stride1;

            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils.ResetBits(length, bits, (y * stride1) + x);
            }
        }

        /// <summary>
        /// Removes black pixels contained in the collection of <see cref="ConnectedComponent"/> objects from this <see cref="Image"/>.
        /// </summary>
        /// <param name="components">The collection if <see cref="ConnectedComponent"/> objects to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="components"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public void RemoveConnectedComponents(IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            foreach (ConnectedComponent component in components)
            {
                this.RemoveConnectedComponent(component);
            }
        }

        /// <summary>
        /// Crops the black pixels contained in the <see cref="ConnectedComponent"/> from this <see cref="Image"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to crop.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="component"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public Image CropConnectedComponent(ConnectedComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            // calculate area to crop
            Rectangle bounds = component.Bounds;

            // allocate new image
            Image dst = new Image(bounds.Width, bounds.Height, this);

            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc = this.Stride;
            int stridedst = dst.Stride;

            // copy bits
            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils.CopyBits(length, bitssrc, (y * stridesrc) + x, bitsdst, ((y - bounds.Y) * stridedst) + x - bounds.X);
            }

            return dst;
        }

        /// <summary>
        /// Crops the black pixels contained in the collection of <see cref="ConnectedComponent"/> objects from this <see cref="Image"/>.
        /// </summary>
        /// <param name="components">The collection of <see cref="ConnectedComponent"/> objects to crop.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="components"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image{T}.BitsPerPixel"/> is not 1.
        /// </exception>
        public Image CropConnectedComponents(IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (this.BitsPerPixel != 1)
            {
                throw new NotSupportedException(Properties.Resources.E_UnsupportedDepth_1bpp);
            }

            // calculate area to crop
            Rectangle bounds = Rectangle.Union(components.Select(x => x.Bounds));
            if (bounds.IsEmpty)
            {
                return new Image(1, 1, this);
            }

            // allocate new image
            Image dst = new Image(bounds.Width, bounds.Height, this);

            ulong[] bitssrc = this.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc1 = this.Stride1;
            int stridedst1 = dst.Stride1;

            // copy bits
            foreach (ConnectedComponent component in components)
            {
                foreach ((int y, int x, int length) in component.EnumStrokes())
                {
                    BitUtils.CopyBits(length, bitssrc, (y * stridesrc1) + x, bitsdst, ((y - bounds.Y) * stridedst1) + x - bounds.X);
                }
            }

            return dst;
        }

        private static void BuildORMask(Image image, StructuringElement kernel, ulong[] bits, ulong[] mask, bool cleanMask)
        {
            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            if (cleanMask)
            {
                Vectors.Set(mask.Length, 0, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point.X == 0)
                {
                    Vectors.Or(
                        (height - Math.Abs(point.Y)) * stride,
                        bits,
                        Core.MinMax.Max(point.Y, 0) * stride,
                        mask,
                        Core.MinMax.Max(-point.Y, 0) * stride);
                }
                else
                {
                    int count = width - Math.Abs(point.X);
                    int offx = (Core.MinMax.Max(point.Y, 0) * stride1) + Core.MinMax.Max(point.X, 0);
                    int offy = (Core.MinMax.Max(-point.Y, 0) * stride1) + Core.MinMax.Max(-point.X, 0);
                    for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                    {
                        BitUtils.Or(count, bits, offx, mask, offy);
                    }
                }
            }
        }

        private static void BuildANDMask(Image image, StructuringElement kernel, ulong[] bits, ulong[] mask, bool cleanMask)
        {
            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            if (cleanMask)
            {
                Vectors.Set(mask.Length, ulong.MaxValue, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point.X == 0)
                {
                    Vectors.And(
                        (height - Math.Abs(point.Y)) * stride,
                        bits,
                        Core.MinMax.Max(point.Y, 0) * stride,
                        mask,
                        Core.MinMax.Max(-point.Y, 0) * stride);
                }
                else
                {
                    int count = width - Math.Abs(point.X);
                    int offx = (Core.MinMax.Max(point.Y, 0) * stride1) + Core.MinMax.Max(point.X, 0);
                    int offy = (Core.MinMax.Max(-point.Y, 0) * stride1) + Core.MinMax.Max(-point.X, 0);
                    for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                    {
                        BitUtils.And(count, bits, offx, mask, offy);
                    }
                }
            }
        }

        [DebuggerDisplay("{X} {Length} {Component}")]
        private sealed class Stroke
        {
            private readonly int x;
            private readonly int length;
            private ConnectedComponent component;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Stroke(int x, int length, ConnectedComponent component)
            {
                this.x = x;
                this.length = length;
                this.component = component;
            }

            public int X => this.x;

            public int Length => this.length;

            public int X2 => this.x + this.length;

            public ConnectedComponent Component
            {
                get => this.component;
                set => this.component = value;
            }
        }
    }
}
