// -----------------------------------------------------------------------
// <copyright file="Morphology.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides morphology extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Morphology
    {
        /// <summary>
        /// Dilates an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to dilate.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The dilated <see cref="Image"/>.
        /// </returns>
        public static Image Dilate(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.DilateIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Dilates an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to dilate.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void DilateIP(this Image image, StructuringElement kernel, int iterations)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            // create mask
            ulong[] mask = new ulong[image.Bits.Length];
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.BuildORMask(image, kernel, null, mask, iteration > 0);

                // process image
                BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);
            }
        }

        /// <summary>
        /// Erodes an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to erode.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The eroded <see cref="Image"/>.
        /// </returns>
        public static Image Erode(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.ErodeIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Erodes an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to erode.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void ErodeIP(this Image image, StructuringElement kernel, int iterations)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            // create mask
            ulong[] mask = new ulong[image.Bits.Length];
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.BuildANDMask(image, kernel, null, mask, true);

                // process image
                BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);
            }
        }

        /// <summary>
        /// Perform morphological opening operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The opened <see cref="Image"/>.
        /// </returns>
        public static Image Open(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.OpenIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Perform morphological opening operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void OpenIP(this Image image, StructuringElement kernel, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.ErodeIP(image, kernel, 1);
                Morphology.DilateIP(image, kernel, 1);
            }
        }

        /// <summary>
        /// Perform morphological closing operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The closed <see cref="Image"/>.
        /// </returns>
        public static Image Close(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.CloseIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Perform morphological closing operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void CloseIP(this Image image, StructuringElement kernel, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.DilateIP(image, kernel, 1);
                Morphology.ErodeIP(image, kernel, 1);
            }
        }

        /// <summary>
        /// Removes small isolated pixels from this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to cleaned.</param>
        /// <returns>
        /// The cleaned <see cref="Image"/>.
        /// </returns>
        public static Image Despeckle(this Image image)
        {
            Image dst = CopyCrop.Copy(image);
            dst.DespeckleIP();
            return dst;
        }

        /// <summary>
        /// Removes small isolated pixels from this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to cleaned.</param>
        public static void DespeckleIP(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // create masks
            ulong[] mask = new ulong[image.Bits.Length];
            ulong[] notbits = new ulong[image.Bits.Length];
            BitUtils64.WordsNOT(image.Bits.Length, image.Bits, 0, notbits, 0);

            // remove isolated pixels
            Morphology.BuildORMask(image, StructuringElement.Square(3), null, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // 0 0 0
            // 0 x 0
            // x x x
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 1)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 1, new Point(1, -1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // x x x
            // 0 x 0
            // 0 0 0
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 0)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 1, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // x 0 0
            // x x 0
            // x 0 0
            Morphology.BuildORMask(image, StructuringElement.Rectangle(2, 3, new Point(0, 1)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(1, 3, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // 0 0 x
            // 0 x x
            // 0 0 x
            Morphology.BuildORMask(image, StructuringElement.Rectangle(2, 3, new Point(1, 1)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(1, 3, new Point(-1, 1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // fill isolated gaps
            Morphology.BuildANDMask(image, StructuringElement.Cross(3, 3), null, mask, true);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // x x x
            // x 0 x
            // 0 0 0
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 1)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 1, new Point(1, -1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // 0 0 0
            // x 0 x
            // x x x
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 0)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 1, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // 0 x x
            // 0 0 x
            // 0 x x
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(2, 3, new Point(0, 1)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(1, 3, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // x x 0
            // x 0 0
            // x x 0
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(2, 3, new Point(1, 1)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(1, 3, new Point(-1, 1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);
        }

        /// <summary>
        /// Finds connected components on the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to find the connected components on.</param>
        /// <returns>
        /// A set of <see cref="ConnectedComponent"/> objects found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        public static ISet<ConnectedComponent> FindConnectedComponents(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            HashSet<ConnectedComponent> all = new HashSet<ConnectedComponent>();
            List<Stroke> last = new List<Stroke>();
            List<Stroke> current = new List<Stroke>();

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            ulong[] bits = image.Bits;

            for (int y = 0, ypos = 0; y < height; y++, ypos += stride1)
            {
                // find intervals
                int lastIndex = 0;
                for (int xpos = ypos, xposend = ypos + width; xpos < xposend;)
                {
                    int start = BitUtils64.BitScanOneForward(xposend - xpos, bits, xpos);
                    if (start == -1)
                    {
                        break;
                    }

                    int end = BitUtils64.BitScanZeroForward(xposend - (start + 1), bits, start + 1);
                    if (end == -1)
                    {
                        end = xposend;
                    }

                    // merge interval
                    mergeStroke(start - ypos, end - start);
                    xpos = end + 1;

                    void mergeStroke(int x, int length)
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
                                        component.MergeWith(anotherComponent, false);

                                        // remove merged component from the set
                                        all.Remove(anotherComponent);

                                        // replace merged component in previous line
                                        replaceComponent(last, i);

                                        // replace merged component in this line
                                        replaceComponent(current, current.Count);

                                        void replaceComponent(List<Stroke> strokes, int anchorPosition)
                                        {
                                            Rectangle anotherBounds = anotherComponent.Bounds;
                                            for (int j = anchorPosition, jj = strokes.Count; j < jj && strokes[j].X <= anotherBounds.Right; j++)
                                            {
                                                if (strokes[j].Component == anotherComponent)
                                                {
                                                    strokes[j].Component = component;
                                                }
                                            }

                                            for (int j = anchorPosition - 1; j >= 0 && strokes[j].X2 >= anotherBounds.X; j--)
                                            {
                                                if (strokes[j].Component == anotherComponent)
                                                {
                                                    strokes[j].Component = component;
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

            Debug.Assert(image.Power() == all.Sum(x => x.Power), "The number of pixels on image and in components must match.");
            return all;
        }

        /// <summary>
        /// Adds black pixels contained in the <see cref="ConnectedComponent"/> to the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to edit.</param>
        /// <param name="component">The <see cref="ConnectedComponent"/> to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>
        /// <c>image</c> is <b>null</b>
        /// </para>
        /// <para>
        /// -or
        /// </para>
        /// <para>
        /// <c>component</c> is <b>null</b>
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image.BitsPerPixel"/> is not 1.
        /// </exception>
        public static void AddConnectedComponent(this Image image, ConnectedComponent component)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            ulong[] bits = image.Bits;
            int stride1 = image.Stride1;

            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils64.SetBits(length, bits, (y * stride1) + x);
            }
        }

        /// <summary>
        /// Adds black pixels contained in the collection of <see cref="ConnectedComponent"/> objects to the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to edit.</param>
        /// <param name="components">The collection if <see cref="ConnectedComponent"/> objects to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>
        /// <c>image</c> is <b>null</b>
        /// </para>
        /// <para>
        /// -or
        /// </para>
        /// <para>
        /// <c>components</c> is <b>null</b>
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image.BitsPerPixel"/> is not 1.
        /// </exception>
        public static void AddConnectedComponents(this Image image, IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            foreach (ConnectedComponent component in components)
            {
                image.AddConnectedComponent(component);
            }
        }

        /// <summary>
        /// Removes black pixels contained in the <see cref="ConnectedComponent"/> from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to edit.</param>
        /// <param name="component">The <see cref="ConnectedComponent"/> to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>
        /// <c>image</c> is <b>null</b>
        /// </para>
        /// <para>
        /// -or
        /// </para>
        /// <para>
        /// <c>component</c> is <b>null</b>
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image.BitsPerPixel"/> is not 1.
        /// </exception>
        public static void RemoveConnectedComponent(this Image image, ConnectedComponent component)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            ulong[] bits = image.Bits;
            int stride1 = image.Stride1;

            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils64.ResetBits(length, bits, (y * stride1) + x);
            }
        }

        /// <summary>
        /// Removes black pixels contained in the collection of <see cref="ConnectedComponent"/> objects from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to edit.</param>
        /// <param name="components">The collection if <see cref="ConnectedComponent"/> objects to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>
        /// <c>image</c> is <b>null</b>
        /// </para>
        /// <para>
        /// -or
        /// </para>
        /// <para>
        /// <c>components</c> is <b>null</b>
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image.BitsPerPixel"/> is not 1.
        /// </exception>
        public static void RemoveConnectedComponents(this Image image, IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            foreach (ConnectedComponent component in components)
            {
                image.RemoveConnectedComponent(component);
            }
        }

        /// <summary>
        /// Crops the black pixels contained in the <see cref="ConnectedComponent"/> from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to modify.</param>
        /// <param name="component">The <see cref="ConnectedComponent"/> to crop.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para>
        /// <c>image</c> is <b>null</b>
        /// </para>
        /// <para>
        /// -or
        /// </para>
        /// <para>
        /// <c>component</c> is <b>null</b>
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image.BitsPerPixel"/> is not 1.
        /// </exception>
        public static Image CropConnectedComponent(this Image image, ConnectedComponent component)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            // calculate area to crop
            Rectangle bounds = component.Bounds;

            // allocate new image
            Image dst = new Image(bounds.Width, bounds.Height, image);

            ulong[] bitssrc = image.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc = image.Stride;
            int stridedst = dst.Stride;

            // copy bits
            foreach ((int y, int x, int length) in component.EnumStrokes())
            {
                BitUtils64.CopyBits(length, bitssrc, (y * stridesrc) + x, bitsdst, ((y - bounds.Y) * stridedst) + x - bounds.X);
            }

            return dst;
        }

        /// <summary>
        /// Crops the black pixels contained in the collection of <see cref="ConnectedComponent"/> objects from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to modify.</param>
        /// <param name="components">The collection of <see cref="ConnectedComponent"/> objects to crop.</param>
        /// <returns>
        /// A new cropped <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para>
        /// <c>image</c> is <b>null</b>
        /// </para>
        /// <para>
        /// -or
        /// </para>
        /// <para>
        /// <c>component</c> is <b>null</b>
        /// </para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <see cref="Image.BitsPerPixel"/> is not 1.
        /// </exception>
        public static Image CropConnectedComponents(this Image image, IEnumerable<ConnectedComponent> components)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            // calculate area to crop
            Rectangle bounds = RectangleExtensions.Union(components.Select(x => x.Bounds));
            if (bounds.IsEmpty)
            {
                return new Image(1, 1, image);
            }

            // allocate new image
            Image dst = new Image(bounds.Width, bounds.Height, image);

            ulong[] bitssrc = image.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc1 = image.Stride1;
            int stridedst1 = dst.Stride1;

            // copy bits
            foreach (ConnectedComponent component in components)
            {
                foreach ((int y, int x, int length) in component.EnumStrokes())
                {
                    BitUtils64.CopyBits(length, bitssrc, (y * stridesrc1) + x, bitsdst, ((y - bounds.Y) * stridedst1) + x - bounds.X);
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
                Arrays.Set(mask.Length, 0, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point.X == 0)
                {
                    BitUtils64.WordsOR(
                        (height - Math.Abs(point.Y)) * stride,
                        bits,
                        Math.Max(point.Y, 0) * stride,
                        mask,
                        Math.Max(-point.Y, 0) * stride);
                }
                else
                {
                    int count = width - Math.Abs(point.X);
                    int offx = (Math.Max(point.Y, 0) * stride1) + Math.Max(point.X, 0);
                    int offy = (Math.Max(-point.Y, 0) * stride1) + Math.Max(-point.X, 0);
                    for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                    {
                        BitUtils64.BitsOR(count, bits, offx, mask, offy);
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
                Arrays.Set(mask.Length, ulong.MaxValue, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point.X == 0)
                {
                    BitUtils64.WordsAND(
                        (height - Math.Abs(point.Y)) * stride,
                        bits,
                        Math.Max(point.Y, 0) * stride,
                        mask,
                        Math.Max(-point.Y, 0) * stride);
                }
                else
                {
                    int count = width - Math.Abs(point.X);
                    int offx = (Math.Max(point.Y, 0) * stride1) + Math.Max(point.X, 0);
                    int offy = (Math.Max(-point.Y, 0) * stride1) + Math.Max(-point.X, 0);
                    for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                    {
                        BitUtils64.BitsAND(count, bits, offx, mask, offy);
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

            public ConnectedComponent Component {
                get => this.component;
                set => this.component = value;
            }
        }
    }
}
