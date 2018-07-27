// -----------------------------------------------------------------------
// <copyright file="Histogram.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a distribution of numerical data.
    /// </summary>
    [DebuggerDisplay("Count: {bins.Length}")]
    public class Histogram
    {
        private readonly int[] bins;
        private readonly int minValue;
        private readonly int maxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram" /> class, using the number of bins.
        /// </summary>
        /// <param name="bins">The number of bins.</param>
        /// <exception cref="ArgumentException">
        /// <c>bins</c> is less or equal to zero.
        /// </exception>
        public Histogram(int bins)
        {
            if (bins <= 0)
            {
                throw new ArgumentException("The number of bins must be greater than zero.");
            }

            this.minValue = 0;
            this.maxValue = bins;
            this.bins = new int[bins];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram" /> class, using the number of bins and bin counts.
        /// </summary>
        /// <param name="bins">The number of bins.</param>
        /// <param name="counts">The collection of zero-based indexes of the bins to initialize the histogram with.</param>
        /// <exception cref="ArgumentException">
        /// <c>bins</c> is less or equal to zero.
        /// </exception>
        public Histogram(int bins, IEnumerable<int> counts) : this(bins)
        {
            foreach (int bin in counts)
            {
                this.bins[bin]++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram" /> class, using the number of bins and bin counts.
        /// </summary>
        /// <param name="bins">The number of bins.</param>
        /// <param name="counts">The collection of (bin, count) pairs to initialize the histogram with.</param>
        /// <exception cref="ArgumentException">
        /// <c>bins</c> is less or equal to zero.
        /// </exception>
        public Histogram(int bins, IEnumerable<(int bin, int count)> counts) : this(bins)
        {
            foreach ((int bin, int count) in counts)
            {
                this.bins[bin] += count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram" /> class, using the values bounds.
        /// </summary>
        /// <param name="minValue">The lower inclusive bound of values in the histogram.</param>
        /// <param name="maxValue">The upper exclusive bound of values in the histogram.</param>
        public Histogram(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentException();
            }

            this.minValue = minValue;
            this.maxValue = maxValue;
            this.bins = new int[maxValue - minValue];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram" /> class, using the values bounds and bin counts.
        /// </summary>
        /// <param name="minValue">The lower inclusive bound of values in the histogram.</param>
        /// <param name="maxValue">The upper exclusive bound of values in the histogram.</param>
        /// <param name="values">The collection of values to initialize the histogram with.</param>
        public Histogram(int minValue, int maxValue, IEnumerable<int> values) : this(minValue, maxValue)
        {
            foreach (int value in values)
            {
                this.bins[this.BinIndex(value)]++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram" /> class, using the values bounds and bin counts.
        /// </summary>
        /// <param name="minValue">The lower inclusive bound of values in the histogram.</param>
        /// <param name="maxValue">The upper exclusive bound of values in the histogram.</param>
        /// <param name="counts">The collection of (value, count) pairs to initialize the histogram with.</param>
        public Histogram(int minValue, int maxValue, IEnumerable<(int value, int count)> counts) : this(minValue, maxValue)
        {
            foreach ((int value, int count) in counts)
            {
                this.bins[this.BinIndex(value)] += count;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Histogram" /> class from being created.
        /// </summary>
        private Histogram()
        {
        }

        /// <summary>
        /// Gets the bins for this <see cref="Histogram"/>.
        /// </summary>
        /// <value>
        /// The collection of bins.
        /// </value>
        public int[] Bins => this.bins;

        /// <summary>
        /// Gets the number of bins in this <see cref="Histogram"/>.
        /// </summary>
        /// <value>
        /// The number of bins in this <see cref="Histogram"/>.
        /// </value>
        public int Count => this.bins.Length;

        /// <summary>
        /// Gets or sets the value of the specified bin in this <see cref="Histogram"/>.
        /// </summary>
        /// <param name="bin">The zero-based index of the bin.</param>
        /// <returns>The number of cases in the bin.</returns>
        public int this[int bin]
        {
            get => this.bins[bin];
            set => this.bins[bin] = value;
        }

        /// <summary>
        /// Increments the specified bin by one.
        /// </summary>
        /// <param name="bin">The zero-based index of the bin to increment.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Increment(int bin) => this.bins[bin]++;

        /// <summary>
        /// Increments the specified bin by the specified count.
        /// </summary>
        /// <param name="bin">The zero-based index of the bin to increment.</param>
        /// <param name="count">The value of increment.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Increment(int bin, int count) => this.bins[bin] += count;

        /// <summary>
        /// Increments the bin that corresponds to the specified value by one.
        /// </summary>
        /// <param name="value">The value which bin to increment.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int value) => this.bins[this.BinIndex(value)]++;

        /// <summary>
        /// Increments the bin that corresponds to the specified value by the specified count.
        /// </summary>
        /// <param name="value">The value which bin to increment.</param>
        /// <param name="count">The value of increment.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int value, int count) => this.bins[this.BinIndex(value)] += count;

        /// <summary>
        /// Returns the zero-based index of the first bin that contains minimum value.
        /// </summary>
        /// <returns>
        /// The zero-based index of the first bin that contains minimum value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgMin()
        {
            return Maximum.ArgMin(this.bins.Length, this.bins, 0);
        }

        /// <summary>
        /// Returns the zero-based index of the first bin that contains maximum value.
        /// </summary>
        /// <returns>
        /// The zero-based index of the first bin that contains maximum value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgMax()
        {
            return Maximum.ArgMax(this.bins.Length, this.bins, 0);
        }

        private int BinIndex(int value)
        {
            if (value < this.minValue)
            {
                value = this.minValue;
            }

            if (value >= this.maxValue)
            {
                value = this.maxValue - 1;
            }

            return value / this.Count;
        }
    }
}
