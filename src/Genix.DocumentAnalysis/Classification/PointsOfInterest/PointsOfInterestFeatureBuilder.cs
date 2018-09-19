// -----------------------------------------------------------------------
// <copyright file="PointsOfInterestFeatureBuilder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Genix.DocumentAnalysis.FeatureDetectors;
    using Genix.Imaging;
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
        private readonly IFeatureDetector detector = new HistogramsOfOrientedGradients()
        {
            CellSize = 32,
            BlockSize = 3,
            NumberOfBins = 6,
            Threshold = 0.2f,
        };

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
        /// The <see cref="DocumentAnalysis.ImagePreprocessingOptions"/> enumeration.
        /// </value>
        [JsonProperty("imagePreprocessingOptions")]
        public ImagePreprocessingOptions ImagePreprocessingOptions { get; set; } = ImagePreprocessingOptions.None;

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

            Image image = ImagePreprocessing.Process(source.Image, this.ImagePreprocessingOptions, 8);

            image = image
                .Scale(100.0 / image.HorizontalResolution, 100.0 / image.VerticalResolution, ScalingOptions.None)
                ////.Binarize()
                .Convert8To1(128)
                .CleanOverscan(0.5f, 0.5f)
                .Deskew()
                .Despeckle();

            ISet<ConnectedComponent> components = image.FindConnectedComponents(8);
            image.RemoveConnectedComponents(components.Where(x => x.Power <= 16));

            image = image
                .CropBlackArea(0, 0)
                .Dilate(StructuringElement.Square(3), 1)
                ////.CropBlackArea(0, 0)
                .Convert1To8()
                .FilterLowpass(3);

            FeatureDetectors.Features features = this.detector.Detect(image, cancellationToken);

            return new PointsOfInterestFeatures(source.Id, features);
        }
    }
}
