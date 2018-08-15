// -----------------------------------------------------------------------
// <copyright file="PointsOfInterestClassifier.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Genix.Core;
    using Genix.MachineLearning.Clustering;
    using Genix.MachineLearning.Distances;
    using Genix.MachineLearning.VectorMachines;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a classifier that analyzes features (points of interest) extracted from the <see cref="Imaging.Image"/>.
    /// </summary>
    public class PointsOfInterestClassifier
        : Classifier<ImageSource, PointsOfInterestFeatures, PointsOfInterestFeatureBuilder>
    {
        private List<(FeatureDetectors.Features features, string truth)> features = null;

        [JsonProperty("kmeans")]
        private KMeans<IVector<float>> kmeans = null;

        [JsonProperty("svm")]
        private SupportVectorMachine svm = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsOfInterestClassifier"/> class.
        /// </summary>
        public PointsOfInterestClassifier()
        {
        }

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
            this.kmeans = null;
            this.svm = null;
        }

        /// <inheritdoc />
        private protected override void FinishTraining(CancellationToken cancellationToken)
        {
            // learn k-means
            int dimension = this.features[0].features.Length;
            this.kmeans = new KMeans<IVector<float>>(512, this.features[0].features.Length, default(EuclideanDistance));

            IList<float[]> x = this.features.SelectMany(f => f.features.Vectors.Partition(dimension)).ToList();
            ////this.kmeans.Learn(x, null);
            /*this.features.SelectMany(x => x.features.Vectors).ToArray());*/
        }

        /// <inheritdoc />
        private protected override bool Train(PointsOfInterestFeatures features, string truth, CancellationToken cancellationToken)
        {
            this.features.Add((features.Features, truth));
            return true;
        }
    }
}
