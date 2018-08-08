// -----------------------------------------------------------------------
// <copyright file="PointsOfInterestClassifier.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Genix.MachineLearning.Clustering;
    using Genix.MachineLearning.VectorMachines;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a classifier that analyzes features (points of interest) extracted from the <see cref="Imaging.Image"/>.
    /// </summary>
    public class PointsOfInterestClassifier
        : Classifier<ImageSource, PointsOfInterestFeatures, PointsOfInterestFeatureBuilder>
    {
        [JsonProperty("kmeans")]
        private readonly KMeans kmeans = null;

        [JsonProperty("svm")]
        private readonly SupportVectorMachine svm = null;

        private List<(FeatureDetectors.Features features, string truth)> features = null;

        /// <inheritdoc />
        public override bool IsTrained => false;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="features"/> is <b>null</b>.
        /// </exception>
        public override Answer Classify(PointsOfInterestFeatures features, CancellationToken cancellationToken)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            // build a feature vector
            float[] featureVector = null; //// this.kmeans.Transform(features.Features);

            // classify feature vector
            float w = this.svm.Classify(featureVector);

            return null;
        }

        /// <inheritdoc />
        private protected override void BeginTraining(CancellationToken cancellationToken)
        {
            this.features = new List<(FeatureDetectors.Features features, string truth)>();
        }

        /// <inheritdoc />
        private protected override void FinishTraining(CancellationToken cancellationToken)
        {
            // learn k-means
        }

        /// <inheritdoc />
        private protected override bool Train(PointsOfInterestFeatures features, string truth, CancellationToken cancellationToken)
        {
            this.features.Add((features.Features, truth));
            return true;
        }
    }
}
