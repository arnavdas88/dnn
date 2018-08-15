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
        [JsonProperty("classes")]
        private readonly List<string> classes = new List<string>();

        private List<(FeatureDetectors.Features features, string truth)> features = null;

        [JsonProperty("kmeans")]
        private KMeans kmeans = null;

        [JsonProperty("svm")]
        private OneVsAllSupportVectorMachine svm = null;

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
            float[] featureVector = this.kmeans.Transform(features.Features, null, null, cancellationToken);

            // classify feature vector
            float[] w = this.svm.Classify(featureVector, null, cancellationToken);
            Maximum.SoftMax(w.Length, w, 0);

            int bestClass = Maximum.ArgMax(w.Length, w, 0);

            int[] indices = Arrays.Indexes(w.Length);
            Arrays.Sort(w.Length, w, 0, indices, 0, false);

            return new Answer(
                features.Id,
                this.classes[bestClass],
                w[bestClass],
                w.Take(5).Select((x, i) => (this.classes[indices[i]], w[i])));
        }

        /// <inheritdoc />
        private protected override void BeginTraining(CancellationToken cancellationToken)
        {
            this.classes.Clear();
            this.features = new List<(FeatureDetectors.Features features, string truth)>();
            this.kmeans = null;
            this.svm = null;
        }

        /// <inheritdoc />
        private protected override void FinishTraining(CancellationToken cancellationToken)
        {
            // count classes
            this.classes.AddRange(this.features.Select(x => x.truth).ToLookup(x => x).Select(x => x.Key));
            if (this.classes.Count < 2)
            {
                throw new ArgumentException();
            }

            // count vectors
            int numberOfVectors = this.features.Sum(x => x.features.Count);

            // copy vectors
            Dictionary<IVector<float>, float> vectors = new Dictionary<IVector<float>, float>(numberOfVectors);
            for (int i = 0, ii = this.features.Count; i < ii; i++)
            {
                FeatureDetectors.Features f = this.features[i].features;
                for (int j = 0, jj = f.Count, len = f.Length, off = 0; j < jj; j++, off += len)
                {
                    DenseVectorF vector = new DenseVectorF(len, f.X, off);
                    if (vectors.TryGetValue(vector, out float weight))
                    {
                        vectors[vector] = weight + 1.0f;
                    }
                    else
                    {
                        vectors[vector] = 1.0f;
                    }
                    ////vectors.Add(SparseVectorF.FromDense(len, f.X, off));
                }
            }

            /*List<IVector<float>> vectors = new List<IVector<float>>(numberOfVectors);
            for (int i = 0, ii = this.features.Count; i < ii; i++)
            {
                FeatureDetectors.Features f = this.features[i].features;
                for (int j = 0, jj = f.Count, len = f.Length, off = 0; j < jj; j++, off += len)
                {
                    vectors.Add(new DenseVectorProxyF(len, f.X, off));
                    ////vectors.Add(SparseVectorF.FromDense(len, f.X, off));
                }
            }*/

            // learn k-means
            this.kmeans = KMeans.Learn(
                32,
                KMeansSeeding.Random,
                default(EuclideanDistance),
                vectors.Keys.ToList(),
                vectors.Values.ToList(),
                cancellationToken);

            // learn svm
            Dictionary<string, int> classesLookup = this.classes.ToDictionary((x, i) => x, (x, i) => i);

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
                svmx.Add(this.kmeans.Transform(features, null, null, cancellationToken));
                svmy.Add(classesLookup[truth]);
            }

            this.svm = OneVsAllSupportVectorMachine.Learn(
                smo,
                this.classes.Count,
                svmx,
                svmy,
                null,
                cancellationToken);
        }

        /// <inheritdoc />
        private protected override bool Train(PointsOfInterestFeatures features, string truth, CancellationToken cancellationToken)
        {
            this.features.Add((features.Features, truth));
            return true;
        }
    }
}
