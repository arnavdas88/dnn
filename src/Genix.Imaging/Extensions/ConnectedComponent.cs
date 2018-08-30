﻿// -----------------------------------------------------------------------
// <copyright file="ConnectedComponent.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    /// <summary>
    /// Encapsulates a bitmap, which consists of the pixel data for a graphics image and its attributes.
    /// </summary>
    [DebuggerDisplay("{Bounds}")]
    public class ConnectedComponent : IBoundedObject
    {
        private static readonly Stroke[][] EmptyStrokes = new Stroke[0][];
        [DebuggerDisplay("{FormattedStrokes}")]
        private Stroke[][] strokes = ConnectedComponent.EmptyStrokes;

        private Rectangle bounds = Rectangle.Empty;
        private int power = -1;

        internal ConnectedComponent(int y, int x, int length)
        {
            this.AddStroke(y, x, length);
        }

        private ConnectedComponent()
        {
        }

        /// <summary>
        /// Gets the number of black pixels on this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// The number of black pixels on this <see cref="ConnectedComponent"/>.
        /// </value>
        public int Power
        {
            get
            {
                if (this.power == -1)
                {
                    int sum = 0;

                    Stroke[][] lines = this.strokes;
                    for (int i = 0, ii = lines.Length; i < ii; i++)
                    {
                        sum += ConnectedComponent.LinePower(lines[i]);
                    }

                    this.power = sum;
                }

                return this.power;
            }
        }

        /// <summary>
        /// Gets the bounds, in pixels, of this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Drawing.Rectangle"/> structure that contains the bounds, in pixels, of this <see cref="ConnectedComponent"/>.
        /// </value>
        public Rectangle Bounds => this.bounds;

        private string FormattedStrokes =>
            string.Join(
                Environment.NewLine,
                this.strokes.Select((x, i) => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", this.bounds.Y + i, string.Join(" ", x))));

        /// <summary>
        /// Calculates a power histogram for the collection of <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="maxPower">The maximum power of components to put into histogram. -1 to use all components.</param>
        /// <param name="components">The collection of <see cref="ConnectedComponent"/> objects to calculate the histogram for.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="components"/> is <b>null</b>.
        /// </exception>
        public static Histogram PowerHistogram(int maxPower, IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (maxPower == -1)
            {
                maxPower = components.Max(x => x.Power);
                return new Histogram(maxPower + 1, components.Select(x => x.Power));
            }
            else
            {
                return new Histogram(maxPower + 1, components.Select(x => x.Power).Where(x => x <= maxPower));
            }
        }

        /// <summary>
        /// Calculates a width histogram for the collection of <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="maxWidth">The maximum width of components to put into histogram. -1 to use all components.</param>
        /// <param name="components">The collection of <see cref="ConnectedComponent"/> objects to calculate the histogram for.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="components"/> is <b>null</b>.
        /// </exception>
        public static Histogram WidthHistogram(int maxWidth, IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (maxWidth == -1)
            {
                maxWidth = components.Max(x => x.Bounds.Width);
                return new Histogram(maxWidth + 1, components.Select(x => x.Bounds.Width));
            }
            else
            {
                return new Histogram(maxWidth + 1, components.Select(x => x.Bounds.Width).Where(x => x <= maxWidth));
            }
        }

        /// <summary>
        /// Calculates a height histogram for the collection of <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="maxHeight">The maximum height of components to put into histogram. -1 to use all components.</param>
        /// <param name="components">The collection of <see cref="ConnectedComponent"/> objects to calculate the histogram for.</param>
        /// <returns>
        /// The <see cref="Histogram"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="components"/> is <b>null</b>.
        /// </exception>
        public static Histogram HeightHistogram(int maxHeight, IEnumerable<ConnectedComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (maxHeight == -1)
            {
                maxHeight = components.Max(x => x.Bounds.Height);
                return new Histogram(maxHeight + 1, components.Select(x => x.Bounds.Height));
            }
            else
            {
                return new Histogram(maxHeight + 1, components.Select(x => x.Bounds.Height).Where(x => x <= maxHeight));
            }
        }

        /// <inheritdoc />
        public override string ToString() => this.Bounds.ToString();

        /// <summary>
        /// Adds a stroke to this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="y">The y-coordinate of the stroke.</param>
        /// <param name="x">The x-coordinate of the stroke.</param>
        /// <param name="length">The length of the stroke.</param>
        public void AddStroke(int y, int x, int length)
        {
            // insert new horizontal line
            if (this.strokes.Length == 0)
            {
                this.strokes = new Stroke[1][];
            }
            else if (y < this.bounds.Y)
            {
                this.strokes = this.strokes.Expand(0, this.bounds.Y - y);
            }
            else if (y >= this.bounds.Bottom)
            {
                this.strokes = this.strokes.Expand(this.bounds.Height, y - this.bounds.Bottom + 1);
            }

            // update position
            this.bounds.Union(x, y, length, 1);

            // insert stroke into the line
            ref Stroke[] line = ref this.strokes[y - this.bounds.Y];
            line = line.Add(new Stroke() { X = x, Length = length });
        }

        /// <summary>
        /// Returns a collection of strokes in this <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <returns>
        /// The collection of strokes in this <see cref="ConnectedComponent"/>.
        /// </returns>
        public IEnumerable<(int y, int x, int length)> EnumStrokes()
        {
            Stroke[][] lines = this.strokes;
            for (int i = 0, ii = lines.Length, y = this.bounds.Y; i < ii; i++, y++)
            {
                Stroke[] line = lines[i];
                for (int j = 0, jj = line.Length; j < jj; j++)
                {
                    yield return (y, line[j].X, line[j].Length);
                }
            }
        }

        /// <summary>
        /// Merges this <see cref="ConnectedComponent"/> with the specified <see cref="ConnectedComponent"/>.
        /// </summary>
        /// <param name="component">The <see cref="ConnectedComponent"/> to merge with.</param>
        public void MergeWith(ConnectedComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            // pre-allocate stroke holders
            if (component.bounds.Y < this.bounds.Y || component.bounds.Bottom > this.bounds.Bottom)
            {
                int y1 = Maximum.Min(component.bounds.Y, this.bounds.Y);
                int y2 = Maximum.Max(component.bounds.Bottom, this.bounds.Bottom);

                Stroke[][] newstrokes = new Stroke[y2 - y1][];
                Array.Copy(this.strokes, 0, newstrokes, this.bounds.Y - y1, this.bounds.Height);
                this.strokes = newstrokes;
            }

            // update position
            this.bounds.Union(component.bounds);

            Stroke[][] lines = component.strokes;
            for (int i = 0, ii = lines.Length, y = component.bounds.Y; i < ii; i++, y++)
            {
                ref Stroke[] thisline = ref this.strokes[y - this.bounds.Y];
                thisline = ConnectedComponent.MergeLines(thisline, lines[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool StrokesIntersect(int x1, int width1, int x2, int width2)
        {
            return x2.Between(x1, x1 + width1) || x1.Between(x2, x2 + width2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Stroke[] MergeLines(Stroke[] line1, Stroke[] line2)
        {
            int ii1 = line1 != null ? line1.Length : 0;
            int ii2 = line2 != null ? line2.Length : 0;
            Stroke[] newline = new Stroke[ii1 + ii2];

            int i1 = 0;
            int i2 = 0;
            int i = 0;
            while (i1 < ii1 && i2 < ii2)
            {
                if (line1[i1].X <= line2[i2].X)
                {
                    newline[i++] = line1[i1++];
                }
                else
                {
                    newline[i++] = line2[i2++];
                }
            }

            while (i1 < ii1)
            {
                newline[i++] = line1[i1++];
            }

            while (i2 < ii2)
            {
                newline[i++] = line2[i2++];
            }

            /*for (int j1 = 0, jj = newline.Length; j1 < jj - 1; j1++)
            {
                for (int j2 = j1 + 1; j2 < jj; j2++)
                {
                    Debug.Assert(!StrokesIntersect(newline[j1].X, newline[j1].Length, newline[j2].X, newline[j2].Length));
                }
            }*/

            return newline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LinePower(Stroke[] line)
        {
            int sum = 0;

            for (int i = 0, ii = line.Length; i < ii; i++)
            {
                sum += line[i].Length;
            }

            return sum;
        }

        [DebuggerDisplay("{X} {Length}")]
        private struct Stroke
        {
            public int X;

            public int Length;

            /// <inheritdoc />
            public override string ToString() =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1}",
                    this.X,
                    this.Length);
        }
    }
}
