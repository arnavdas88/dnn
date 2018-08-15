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
        private KMeans kmeans = null;

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
            // count vectors
            int numberOfVectors = this.features.Sum(x => x.features.Count);

            // copy vectors
            List<IVector<float>> vectors = new List<IVector<float>>(numberOfVectors);
            for (int i = 0, ii = this.features.Count; i < ii; i++)
            {
                FeatureDetectors.Features f = this.features[i].features;
                for (int j = 0, jj = f.Count, len = f.Length, off = 0; j < jj; j++, off += len)
                {
                    vectors.Add(new DenseVectorProxyF(len, f.X, off));
                }
            }

            // learn k-means
            this.kmeans = KMeans.Learn(
                512,
                KMeansSeeding.KMeansPlusPlus,
                default(EuclideanDistance),
                vectors,
                null);
        }

        /// <inheritdoc />
        private protected override bool Train(PointsOfInterestFeatures features, string truth, CancellationToken cancellationToken)
        {
            this.features.Add((features.Features, truth));
            return true;
        }
    }
}
