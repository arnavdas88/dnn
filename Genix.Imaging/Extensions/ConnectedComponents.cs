// -----------------------------------------------------------------------
// <copyright file="ConnectedComponents.cs" company="Noname, Inc.">
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
    /// Provides connected components extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class ConnectedComponents
    {
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
            List<ConnectedComponent> last = new List<ConnectedComponent>();
            HashSet<ConnectedComponent> current = new HashSet<ConnectedComponent>();

            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            ulong[] bits = image.Bits;

            for (int y = 0, ypos = 0; y < height; y++, ypos += stride1)
            {
                // prepare new line
                last.Clear();

                if (current.Count > 0)
                {
                    last.AddRange(current);
                    last.Sort(ConnectedComponentComparer.Default);
                    current.Clear();
                }

                // find intervals
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
                    mergeComponent(start - ypos, end - start);
                    xpos = end + 1;

                    void mergeComponent(int x, int length)
                    {
                        ConnectedComponent component = null;

                        for (int i = 0, ii = last.Count; i < ii; i++)
                        {
                            ConnectedComponent lastComponent = last[i];
                            if (lastComponent.TouchesBottom(y, x, length))
                            {
                                if (component == null)
                                {
                                    lastComponent.AddStroke(y, x, length);
                                    component = lastComponent;
                                }
                                else
                                {
                                    component.MergeWith(lastComponent, false);
                                    all.Remove(lastComponent);
                                    current.Remove(lastComponent);
                                    last.RemoveAt(i--);
                                    ii--;
                                }
                            }
                            else if (x + length < lastComponent.Bounds.Left)
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

                        current.Add(component);
                    }
                }
            }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private sealed class ConnectedComponentComparer : IComparer<ConnectedComponent>
        {
            public static ConnectedComponentComparer Default = new ConnectedComponentComparer();

            public int Compare(ConnectedComponent x, ConnectedComponent y)
            {
                if (x == y)
                {
                    return 0;
                }

                int res = x.Bounds.X - y.Bounds.X;
                if (res == 0)
                {
                    res = x.Bounds.Y - y.Bounds.Y;
                }

                if (res == 0)
                {
                    res = x.GetHashCode() - y.GetHashCode();
                }

                Debug.Assert(res != 0, "Method cannot return zero.");
                return res;
            }
        }
    }
}