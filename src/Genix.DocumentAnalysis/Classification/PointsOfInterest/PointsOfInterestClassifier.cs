// -----------------------------------------------------------------------
// <copyright file="PointsOfInterestClassifier.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Genix.Core;
    using Genix.MachineLearning.Clustering;
    using Genix.MachineLearning.Distances;
    using Genix.MachineLearning.Kernels;
    using Genix.MachineLearning.VectorMachines;
    using Genix.MachineLearning.VectorMachines.Learning;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a classifier that analyzes features (points of interest) extracted from the <see cref="Imaging.Image"/>.
    /// </summary>
    public class PointsOfInterestClassifier
        : Classifier<ImageSource, PointsOfInterestFeatures, PointsOfInterestFeatureBuilder>
    {
        [JsonProperty("isLearned")]
        private bool isLearned;

        [JsonProperty("classes")]
        private List<string> classes = new List<string>();

        [JsonProperty("kmeans")]
        private KMeans kmeans;

        [JsonProperty("svm")]
        private OneVsAllSupportVectorMachine svm;

        private Learner learner;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsOfInterestClassifier"/> class.
        /// </summary>
        public PointsOfInterestClassifier()
        {
        }

        /// <inheritdoc />
        public override bool IsLearned => this.isLearned;

        /// <inheritdoc />
        public override IReadOnlyCollection<string> Classes => this.classes;

        /// <summary>
        /// Gets or sets the length of the feature vector used for classification.
        /// </summary>
        /// <value>
        /// The length of a feature vector. Default value is 32.
        /// </value>
        public int VectorLength { get; set; } = 192;

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

            if (!this.isLearned)
            {
                throw new InvalidOperationException(Properties.Resources.E_Classifier_NotLearned);
            }

            if (features.Features.Count > 0)
            {
                // build a feature vector
                float[] featureVector = PointsOfInterestClassifier.PrepareVector(this.kmeans, features.Features, cancellationToken);

                // classify feature vector
                float[] w = this.svm.Classify(featureVector, null, cancellationToken);
                Maximum.SoftMax(w.Length, w, 0);

                // find best class
                int[] indices = Arrays.Indexes(w.Length);
                Arrays.Sort(w.Length, w, 0, indices, 0, false);

                /*float confidence = w[0];
                float diff = w[0] - w[1];
                if (diff < 0.15f)
                {
                    // penalize first answer which score is close to second's
                    confidence *= 1.0f - (1.0f / (float)Math.Exp(100.0 * diff / Math.PI));
                }*/

                float confidence = (float)(-Math.Log(w[1] / w[0], 2.0));

                return new Answer(
                    features.Id,
                    this.classes[indices[0]],
                    confidence.Clip(0, 1),
                    w.Take(5).Select((x, i) => (this.classes[indices[i]], w[i])));
            }
            else
            {
                return new Answer(features.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float[] PrepareVector(KMeans kmeans, IVectorPack<float> x, CancellationToken cancellationToken)
        {
            return kmeans.Transform(x, null, false, null, cancellationToken);
        }

        /// <inheritdoc />
        private protected override void BeginTraining(CancellationToken cancellationToken)
        {
            this.learner = new Learner();
        }

        /// <inheritdoc />
        private protected override void FinishTraining(CancellationToken cancellationToken)
        {
            (this.classes, this.kmeans, this.svm) = this.learner.FinishLearning(this.VectorLength, cancellationToken);
            this.isLearned = true;
        }

        /// <inheritdoc />
        private protected override bool Train(PointsOfInterestFeatures features, string truth, CancellationToken cancellationToken)
        {
            ////features.SaveToFile(@"d:\dnn\temp3\" + Path.GetFileNameWithoutExtension(features.Id.Id) + ".json");

            this.learner.AddFeatures(features, truth);
            return true;
        }

        private class Learner
        {
            private readonly List<(FeatureDetectors.Features features, string truth)> features =
                new List<(FeatureDetectors.Features features, string truth)>();

            public void AddFeatures(PointsOfInterestFeatures features, string truth)
            {
                this.features.Add((features.Features, truth));
            }

            public (List<string> classes, KMeans kmeans, OneVsAllSupportVectorMachine svm) FinishLearning(
                int vectorLength,
                CancellationToken cancellationToken)
            {
                // count classes
                List<string> classes = new List<string>(this.features.Select(x => x.truth).ToLookup(x => x).Select(x => x.Key));
                if (classes.Count < 2)
                {
                    throw new ArgumentException();
                }

                classes.Sort();

                // count vectors
                int numberOfVectors = this.features.Sum(x => x.features.Count);

                // copy vectors
                Dictionary<IVector<float>, float> vectors = new Dictionary<IVector<float>, float>(numberOfVectors);
                for (int i = 0, ii = this.features.Count; i < ii; i++)
                {
                    FeatureDetectors.Features f = this.features[i].features;
                    for (int j = 0, jj = f.Count, len = f.Length, off = 0; j < jj; j++, off += len)
                    {
                        ////DenseVectorF vector = new DenseVectorF(len, f.X, off);
                        SparseVectorF vector = SparseVectorF.FromDense(len, f.X, off);
                        vectors[vector] = vectors.TryGetValue(vector, out float weight) ? weight + 1.0f : 1.0f;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                // learn k-means
                KMeans kmeans = KMeans.Learn(
                    vectorLength,
                    KMeansSeeding.Random,
                    2,
                    default(EuclideanDistance),
                    vectors.Keys.ToList(),
                    vectors.Values.ToList(),
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                // learn svm
                Dictionary<string, int> classesLookup = classes.ToDictionary((x, i) => x, (x, i) => i);

                SequentualMinimalOptimization smo = new SequentualMinimalOptimization(new ChiSquare())
                {
                    Algorithm = SMOAlgorithm.LibSVM,
                    Tolerance = 0.01f,
                };

                List<float[]> svmx = new List<float[]>(this.features.Count);
                List<int> svmy = new List<int>(this.features.Count);

                for (int i = 0, ii = this.features.Count; i < ii; i++)
                {
                    (FeatureDetectors.Features features, string truth) = this.features[i];
                    svmx.Add(PointsOfInterestClassifier.PrepareVector(kmeans, features, cancellationToken));
                    svmy.Add(classesLookup[truth]);
                }

                cancellationToken.ThrowIfCancellationRequested();

                OneVsAllSupportVectorMachine svm = OneVsAllSupportVectorMachine.Learn(
                    smo,
                    classes.Count,
                    svmx,
                    svmy,
                    null,
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                return (classes, kmeans, svm);
            }
        }
    }
}
