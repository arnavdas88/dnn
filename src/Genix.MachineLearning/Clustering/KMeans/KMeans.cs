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
        /// <param name="distance">The distance function.</param>
        /// <param name="x">The data points <paramref name="x"/> to clusterize.</param>
        /// <param name="weights">The <c>weight</c> of importance for each data point.</param>
        /// <returns>
        /// The <see cref="KMeans"/> clusterizer learned by this metthod.
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
            IVectorDistance<float, IVector<float>, float> distance,
            IList<IVector<float>> x,
            IList<float> weights)
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

#if false
            NativeMethods.kmeans(k, 1, dimension, sampleCount, x);
#else
            int maxiter = 20;

            KMeansClusterCollection clusters = new KMeansClusterCollection(k, dimension, distance);
            switch (seeding)
            {
                case KMeansSeeding.KMeansPlusPlus:
                    clusters.KMeansPlusPlusSeeding(x, weights);
                    break;

                default:
                    clusters.RandomSeeding(x, weights);
                    break;
            }

            float[] countsmeans = new float[k * (dimension + 1)];
            object sync = new object();

            for (int iter = 0; iter < maxiter; iter++)
            {
                // reset means and counts
                if (iter > 0)
                {
                    Array32f.Set(countsmeans.Length, 0.0f, countsmeans, 0);
                }

                // assign vectors to new clusters
                CommonParallel.For(
                    0,
                    sampleCount,
                    (a, b) =>
                    {
                        float[] local = new float[countsmeans.Length];

                        for (int i = a; i < b; i++)
                        {
                            int index = clusters.Assign(x[i]);
                            float weight = weights?[i] ?? 1.0f;

                            int off = index * (dimension + 1);
                            local[off] += weight;
                            x[i].AddProductC(weight, local, off + 1);
                        }

                        lock (sync)
                        {
                            Math32f.Add(countsmeans.Length, local, 0, countsmeans, 0);
                        }
                    },
                    new ParallelOptions());

                // calculate new centroids
                for (int i = 0, off = 0; i < k; i++, off += dimension + 1)
                {
                    if (countsmeans[off] != 0)
                    {
                        Math32f.DivC(dimension, countsmeans, off + 1, countsmeans[off], clusters[i].Centroid, 0);
                    }
                }
            }

            return new KMeans(clusters)
            {
                Seeding = seeding,
            };
#endif
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
        /// <returns>
        /// A vector containing the distance between each point and its assigned cluster.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Assign(IList<IVector<float>> x) => this.clusters.Assign(x, null);

        /// <summary>
        /// Creates a feature vector by assigning each data point in <paramref name="x"/> to one of the clusters.
        /// </summary>
        /// <param name="x">The data points to assign.</param>
        /// <returns>
        /// The feature vector of length <see cref="K"/>.
        /// Each element of the feature vector contains the number of data points assigned to corresponding cluster.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Transform(IList<IVector<float>> x)
        {
            float[] result = new float[this.K];
            for (int i = 0, ii = result.Length; i < ii; i++)
            {
                int cluster = this.Assign(x[i]);
                result[cluster] += 1.0f;
            }

            return result;
        }

        /*private static class NativeMethods
        {
            private const string DllName = "Genix.MachineLearning.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void kmeans(int k, int iter, int dimension, int samples, float[] x);
        }*/
    }
}
