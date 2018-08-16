// -----------------------------------------------------------------------
// <copyright file="HistogramsOfOrientedGradients.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.FeatureDetectors
{
    using System;
    using System.Linq;
    using System.Threading;
    using Genix.Core;
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
        /// <param name="blockStride">The block stride size, in number of <paramref name="cellSize"/>.</param>
        /// <param name="numberOfBins">The number of bins (orientations) in the histogram.</param>
        /// <param name="threshold">
        /// The threshold value to apply after normalization.
        /// Bins that are less than the threshold, are set to zero.
        /// </param>
        public HistogramsOfOrientedGradients(
            int cellSize,
            int blockSize,
            int blockStride,
            int numberOfBins,
            float threshold)
        {
            this.CellSize = cellSize;
            this.BlockSize = blockSize;
            this.BlockStride = blockStride;
            this.NumberOfBins = numberOfBins;
            this.Threshold = threshold;
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
        /// Gets the block stride size, in number of <see cref="CellSize"/>.
        /// </summary>
        /// <value>
        /// The block stride size, in number of <see cref="CellSize"/>. The default value is 1.
        /// </value>
        [JsonProperty("blockSize")]
        public int BlockStride { get; } = 1;

        /// <summary>
        /// Gets the number of bins (orientations) in the histogram.
        /// </summary>
        /// <value>
        /// The number of bins (orientations) in the histogram. The default value is 9.
        /// </value>
        [JsonProperty("numberOfBins")]
        public int NumberOfBins { get; } = 9;

        /// <summary>
        /// Gets the threshold to apply to bin values after normalization.
        /// </summary>
        /// <value>
        /// The threshold to apply. The default value is 0.2f.
        /// </value>
        /// <remarks>
        /// Bins that are less than the threshold, are set to zero.
        /// </remarks>
        [JsonProperty("threshold")]
        public float Threshold { get; } = 0.2f;

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

            DenseVectorPackF vectors = image.HOG(
                this.CellSize,
                this.BlockSize,
                this.BlockStride,
                this.NumberOfBins,
                this.Threshold);

            vectors = DenseVectorPackF.Pack(vectors.Unpack().Where(x => x.Sum() != 0.0f).ToList());

            return new Features(vectors);
        }
    }
}