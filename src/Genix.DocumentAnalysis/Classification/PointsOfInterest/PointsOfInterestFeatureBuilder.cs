// -----------------------------------------------------------------------
// <copyright file="PointsOfInterestFeatureBuilder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Drawing;
    using System.Threading;
    using Genix.DocumentAnalysis.FeatureDetectors;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides features extraction for <see cref="PointsOfInterestClassifier"/>.
    /// </summary>
    public class PointsOfInterestFeatureBuilder
        : IFeatureBuilder<ImageSource, PointsOfInterestFeatures>
    {
        /// <summary>
        /// The feature detector.
        /// </summary>
        [JsonProperty("detector")]
        private readonly IFeatureDetector detector = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsOfInterestFeatureBuilder"/> class.
        /// </summary>
        /// <param name="detector">The feature detector.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="detector"/> is <b>null</b>.
        /// </exception>
        public PointsOfInterestFeatureBuilder(IFeatureDetector detector)
        {
             this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsOfInterestFeatureBuilder"/> class.
        /// </summary>
        [JsonConstructor]
        public PointsOfInterestFeatureBuilder()
        {
        }

        /// <summary>
        /// Gets or sets image enhancing modes that the <see cref="PointsOfInterestFeatureBuilder"/>
        /// should apply to the image before extracting points of interest from it.
        /// </summary>
        /// <value>
        /// The <see cref="ImageEnhancingOptions"/> enumeration.
        /// </value>
        [JsonProperty("imageEnhancingOptions")]
        public ImagePreprocessingOptions ImageEnhancingOptions { get; set; } = ImagePreprocessingOptions.None;

        /// <summary>
        /// Gets the feature detector.
        /// </summary>
        /// <value>
        /// The <see cref="IFeatureDetector"/> that represents the feature detector.
        /// </value>
        public IFeatureDetector Detector => this.detector;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        public PointsOfInterestFeatures BuildFeatures(ImageSource source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Imaging.Image image = ImagePreprocessing.Process(source.Image, this.ImageEnhancingOptions, 8);
            FeatureDetectors.Features features = this.detector.Detect(image, cancellationToken);
            return new PointsOfInterestFeatures(features);
        }
    }
}
