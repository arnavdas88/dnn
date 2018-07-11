// -----------------------------------------------------------------------
// <copyright file="ConnectedComponents.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides connected components extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class ConnectedComponents
    {
        /// <summary>
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <param name="image">The existing <see cref="Image"/> to binarize.</param>
        /// <returns>
        /// A new binary <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>image</c> is <b>null</b>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<ConnectedComponent> FindConnectedComponents(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.BitsPerPixel != 1)
            {
                throw new NotSupportedException();
            }

            List<ConnectedComponent> all = new List<ConnectedComponent>();
            List<ConnectedComponent> last = new List<ConnectedComponent>();
            List<ConnectedComponent> current = new List<ConnectedComponent>();

            ulong[] bits = image.Bits;
            for (int y = 0, startpos = 0; y < image.Height; y++, startpos += image.Stride1)
            {
                current.Clear();

                // find intervals
                for (int pos = startpos, endpos = startpos + image.Width; pos < endpos;)
                {
                    int index1 = BitUtils64.BitScanOneForward(endpos - pos, bits, pos);
                    if (index1 == -1)
                    {
                        break;
                    }

                    int index2 = BitUtils64.BitScanZeroForward(endpos - index1 - 1, bits, index1 + 1);
                    if (index2 == -1)
                    {
                        index2 = endpos;
                    }

                    pos = index2 + 1;

                    // merge interval
                    int start = index1 - startpos;
                    int length = index2 - index1;
                    ConnectedComponent component = ConnectedComponents.MergeComponent(all, last, y, start, length);
                    if (current.IndexOf(component) == -1)
                    {
                        current.Add(component);
                    }
                }

                // rotate lines
                List<ConnectedComponent> temp = last;
                last = current;
                current = temp;
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

            IList<int> intervals = component.Intervals;
            ulong[] bits = image.Bits;
            int stride1 = image.Stride1;

            for (int i = 0, count = intervals.Count; i < count; i += 3)
            {
                int y = intervals[i];
                int x = intervals[i + 1];
                int c = intervals[i + 2];

                BitUtils64.SetBits(c, bits, (y * stride1) + x);
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

            IList<int> intervals = component.Intervals;
            ulong[] bits = image.Bits;
            int stride1 = image.Stride1;

            for (int i = 0, count = intervals.Count; i < count; i += 3)
            {
                int y = intervals[i];
                int x = intervals[i + 1];
                int c = intervals[i + 2];

                BitUtils64.ResetBits(c, bits, (y * stride1) + x);
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

            component.GetBounds(out int left, out int top, out int width, out int height);

            // allocate new image
            Image dst = new Image(width, height, image);

            ulong[] bitssrc = image.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc = image.Stride;
            int stridedst = dst.Stride;

            // copy bits
            IList<int> intervals = component.Intervals;
            for (int i = 0, count = intervals.Count; i < count; i += 3)
            {
                int y = intervals[i];
                int x = intervals[i + 1];
                int c = intervals[i + 2];

                BitUtils64.CopyBits(c, bitssrc, (y * stridesrc) + x, bitsdst, ((y - top) * stridedst) + x - left);
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
            int left = int.MaxValue;
            int top = int.MaxValue;
            int right = 0;
            int bottom = 0;

            foreach (ConnectedComponent component in components)
            {
                if (component.Intervals.Count > 0)
                {
                    component.GetBounds(out int x, out int y, out int width, out int height);

                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x + width);
                    bottom = Math.Max(bottom, y + height);
                }
            }

            if (left == int.MaxValue || top == int.MaxValue)
            {
                return new Image(1, 1, image);
            }

            // allocate new image
            Image dst = new Image(right - left, bottom - top, image);

            ulong[] bitssrc = image.Bits;
            ulong[] bitsdst = dst.Bits;
            int stridesrc1 = image.Stride1;
            int stridedst1 = dst.Stride1;

            // copy bits
            foreach (ConnectedComponent component in components)
            {
                IList<int> intervals = component.Intervals;
                for (int i = 0, count = intervals.Count; i < count; i += 3)
                {
                    int y = intervals[i];
                    int x = intervals[i + 1];
                    int c = intervals[i + 2];

                    BitUtils64.CopyBits(c, bitssrc, (y * stridesrc1) + x, bitsdst, ((y - top) * stridedst1) + x - left);
                }
            }

            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ConnectedComponent MergeComponent(List<ConnectedComponent> all, List<ConnectedComponent> last, int y, int start, int length)
        {
            ConnectedComponent component = null;

            for (int i = 0; i < last.Count; i++)
            {
                ConnectedComponent lastComponent = last[i];
                if (lastComponent.SegmentTouchesBottom(y, start, length))
                {
                    if (component == null)
                    {
                        lastComponent.AddSegment(y, start, length);

                        component = lastComponent;
                    }
                    else
                    {
                        component.Merge(lastComponent);

                        all.Remove(lastComponent);
                        last.RemoveAt(i--);
                    }
                }
            }

            if (component == null)
            {
                component = new ConnectedComponent(y, start, length);
                all.Add(component);
            }

            return component;
        }
    }
}