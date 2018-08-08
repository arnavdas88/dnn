// -----------------------------------------------------------------------
// <copyright file="HistogramsOfOrientedGradients.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.FeatureDetectors
{
    using System;
    using System.Drawing;
    using System.Threading;
    using Genix.Imaging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Histograms of Oriented Gradients (HOG) feature detector.
    /// </summary>
    public class HistogramsOfOrientedGradients : IFeatureDetector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramsOfOrientedGradients"/> class,
        /// using default parameters.
        /// </summary>
        [JsonConstructor]
        public HistogramsOfOrientedGradients()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramsOfOrientedGradients"/> class,
        /// using custom parameters.
        /// </summary>
        /// <param name="cellSize">The cell size, in pixels.</param>
        /// <param name="blockSize">The block size, in number of <paramref name="cellSize"/>.</param>
        /// <param name="numberOfBins">The number of bins (orientations) in the histogram.</param>
        public HistogramsOfOrientedGradients(int cellSize, int blockSize, int numberOfBins)
        {
            this.CellSize = cellSize;
            this.BlockSize = blockSize;
            this.NumberOfBins = numberOfBins;
        }

        /// <summary>
        /// Gets the cell size, in pixels.
        /// </summary>
        /// <value>
        /// The cell size, in pixels. The default value is 8.
        /// </value>
        [JsonProperty("cellSize")]
        public int CellSize { get; } = 8;

        /// <summary>
        /// Gets the block size, in number of <see cref="CellSize"/>.
        /// </summary>
        /// <value>
        /// The block size, in number of <see cref="CellSize"/>. The default value is 2.
        /// </value>
        [JsonProperty("blockSize")]
        public int BlockSize { get; } = 2;

        /// <summary>
        /// Gets the number of bins (orientations) in the histogram.
        /// </summary>
        /// <value>
        /// The number of bins (orientations) in the histogram. The default value is 9.
        /// </value>
        [JsonProperty("numberOfBins")]
        public int NumberOfBins { get; } = 9;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public Features Detect(Imaging.Image image, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            (float[] vectors, int length) = image.HOG(this.CellSize, this.BlockSize, this.NumberOfBins);
            return new Features(vectors.Length / length, length, vectors);
        }
    }
}