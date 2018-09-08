// -----------------------------------------------------------------------
// <copyright file="KMeans.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using Genix.Core;
    using Genix.MachineLearning.Distances;
    using Newtonsoft.Json;

    /// <summary>
    /// The K-Means clustering algorithm.
    /// </summary>
    public class KMeans
    {
        /// <summary>
        /// The collection of clusters.
        /// </summary>
        [JsonProperty("clusters")]
        private readonly KMeansClusterCollection clusters = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeans"/> class.
        /// </summary>
        /// <param name="clusters">The collection of clusters.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="clusters"/> is <b>null</b>.
        /// </exception>
        private KMeans(KMeansClusterCollection clusters)
        {
            this.clusters = clusters ?? throw new ArgumentNullException(nameof(clusters));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeans"/> class.
        /// </summary>
        [JsonConstructor]
        private KMeans()
        {
        }

        /// <summary>
        /// Gets the number of clusters.
        /// </summary>
        /// <value>
        /// The number of clusters.
        /// </value>
        public int K => this.clusters.Count;

        /// <summary>
        /// Gets the cluster initialization algorithm.
        /// </summary>
        /// <value>
        /// The <see cref="KMeansSeeding"/> enumeration. The default value is <see cref="KMeansSeeding.KMeansPlusPlus"/>.
        /// </value>
        [JsonProperty("seeding")]
        public KMeansSeeding Seeding { get; private set; } = KMeansSeeding.KMeansPlusPlus;

        /// <summary>
        /// Gets the collection of clusters.
        /// </summary>
        /// <value>
        /// The <see cref="KMeansClusterCollection"/> object.
        /// </value>
        public KMeansClusterCollection Clusters => this.clusters;

        /// <summary>
        /// Learns a <see cref="KMeans"/> model that can map the given inputs to the desired outputs.
        /// </summary>
        /// <param name="k">The number of clusters.</param>
        /// <param name="seeding">The cluster initialization algorithm.</param>
        /// <param name="maxiter">The maximum number of iterations.</param>
        /// <param name="distance">The distance function.</param>
        /// <param name="x">The data points <paramref name="x"/> to clusterize.</param>
        /// <param name="weights">The <c>weight</c> of importance for each data point.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="KMeans"/> clusterizer learned by this method.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="x"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="distance"/> is <b>null</b>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="weights"/> is not <b>null</b> and the number of elements in <paramref name="weights"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// </exception>
        public static KMeans Learn(
            int k,
            KMeansSeeding seeding,
            int maxiter,
            IVectorDistance<float, IVector<float>, float> distance,
            IList<IVector<float>> x,
            IList<float> weights,
            CancellationToken cancellationToken)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (weights != null && weights.Count != x.Count)
            {
                throw new ArgumentException("The number of weights must match the number of input vectors.", nameof(weights));
            }

            int sampleCount = x.Count;
            int dimension = x[0].Length;

            KMeansClusterCollection clusters = new KMeansClusterCollection(k, dimension, distance);
            switch (seeding)
            {
                case KMeansSeeding.KMeansPlusPlus:
                    clusters.KMeansPlusPlusSeeding(x, weights, cancellationToken);
                    break;

                default:
                    clusters.RandomSeeding(x, weights, cancellationToken);
                    break;
            }

            float[] counts = new float[k];
            float[] means = new float[k * dimension];
            object sync = new object();

            for (int iter = 0; iter < maxiter; iter++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // reset means and counts
                if (iter > 0)
                {
                    Array32f.Set(counts.Length, 0.0f, counts, 0);
                    Array32f.Set(means.Length, 0.0f, means, 0);
                }

                // assign vectors to new clusters
                CommonParallel.For(
                    0,
                    sampleCount,
                    (a, b) =>
                    {
                        float[] lcounts = new float[counts.Length];
                        float[] lmeans = new float[means.Length];

                        for (int i = a; i < b; i++)
                        {
                            int index = clusters.Assign(x[i]);
                            float weight = weights?[i] ?? 1.0f;

                            lcounts[index] += weight;
                            x[i].AddProductC(weight, lmeans, index * dimension);
                        }

                        lock (sync)
                        {
                            Vectors.Add(lcounts.Length, lcounts, 0, counts, 0);
                            Vectors.Add(lmeans.Length, lmeans, 0, means, 0);
                        }
                    },
                    new ParallelOptions());

                // calculate new centroids
                for (int i = 0, off = 0; i < k; i++, off += dimension)
                {
                    if (counts[i] != 0)
                    {
                        Math32f.DivC(dimension, means, off, counts[i], clusters[i].Centroid, 0);
                    }
                }
            }

            return new KMeans(clusters)
            {
                Seeding = seeding,
            };
        }

        /// <summary>
        /// Assigns the vector to one of the clusters.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(IVector<float> x) => this.clusters.Assign(x);

        /// <summary>
        /// Assigns the vector to one of the clusters and returns the distance to that cluster.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <param name="score">The distance to the cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(IVector<float> x, out float score) => this.clusters.Assign(x, out score);

        /// <summary>
        /// Assigns the range of data points to feature vector containing the distance between each point and its assigned cluster.
        /// </summary>
        /// <param name="x">The range of data points to assign.</param>
        /// <param name="result">The feature vector that receives the result. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the clusterizer that the operation should be canceled.</param>
        /// <returns>
        /// A vector containing the distance between each point and its assigned cluster.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Assign(IList<IVector<float>> x, float[] result, CancellationToken cancellationToken) =>
            this.clusters.Assign(x, result, cancellationToken);

        /// <summary>
        /// Creates a feature vector by assigning each data point in <paramref name="x"/> to one of the clusters.
        /// </summary>
        /// <param name="x">The data points to assign.</param>
        /// <param name="weights">The <c>weight</c> of importance for each data point. Can be <b>null</b>.</param>
        /// <param name="normalize">Determines whether the resulting vector should be normalized.</param>
        /// <param name="result">The feature vector that receives the result. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the clusterizer that the operation should be canceled.</param>
        /// <returns>
        /// The feature vector of length <see cref="K"/>.
        /// Each element of the feature vector contains the number of data points assigned to corresponding cluster.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="weights"/> is not <b>null</b> and the number of elements in <paramref name="weights"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Transform(
            IList<IVector<float>> x,
            IList<float> weights,
            bool normalize,
            float[] result,
            CancellationToken cancellationToken)
        {
            if (result == null)
            {
                result = new float[this.K];
            }

            if (weights == null)
            {
                for (int i = 0, ii = x.Count; i < ii; i++)
                {
                    int cluster = this.Assign(x[i]);
                    result[cluster] += 1.0f;
                }
            }
            else
            {
                if (weights.Count != x.Count)
                {
                    throw new ArgumentException("The number of weights must match the number of input vectors.", nameof(weights));
                }

                for (int i = 0, ii = x.Count; i < ii; i++)
                {
                    int cluster = this.Assign(x[i]);
                    result[cluster] += weights[i];
                }
            }

            if (normalize)
            {
                float sum = Math32f.Sum(result.Length, result, 0);
                if (sum != 0.0f)
                {
                    Math32f.DivC(result.Length, sum, result, 0);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a feature vector by assigning each data point in <paramref name="x"/> to one of the clusters.
        /// </summary>
        /// <param name="x">The data points to assign.</param>
        /// <param name="weights">The <c>weight</c> of importance for each data point. Can be <b>null</b>.</param>
        /// <param name="normalize">Determines whether the resulting vector should be normalized.</param>
        /// <param name="result">The feature vector that receives the result. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the clusterizer that the operation should be canceled.</param>
        /// <returns>
        /// The feature vector of length <see cref="K"/>.
        /// Each element of the feature vector contains the number of data points assigned to corresponding cluster.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="weights"/> is not <b>null</b> and the number of elements in <paramref name="weights"/> does not match the number of elements in <paramref name="x"/>.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Transform(
            IVectorPack<float> x,
            IList<float> weights,
            bool normalize,
            float[] result,
            CancellationToken cancellationToken)
        {
            if (result == null)
            {
                result = new float[this.K];
            }

            if (weights == null)
            {
                for (int i = 0, ii = x.Count, len = x.Length, off = 0; i < ii; i++, off += len)
                {
                    int cluster = this.Assign(new DenseVectorProxyF(len, x.X, off));
                    result[cluster] += 1.0f;
                }
            }
            else
            {
                if (weights.Count != x.Count)
                {
                    throw new ArgumentException("The number of weights must match the number of input vectors.", nameof(weights));
                }

                for (int i = 0, ii = x.Count, len = x.Length, off = 0; i < ii; i++, off += len)
                {
                    int cluster = this.Assign(new DenseVectorProxyF(len, x.X, off));
                    result[cluster] += weights[i];
                }
            }

            if (normalize)
            {
                float sum = Math32f.Sum(result.Length, result, 0);
                if (sum != 0.0f)
                {
                    Math32f.DivC(result.Length, sum, result, 0);
                }
            }

            return result;
        }
    }
}
