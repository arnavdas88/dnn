// -----------------------------------------------------------------------
// <copyright file="KMeans.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// <param name="k">The number of clusters.</param>
        /// <param name="dimension">The vector length.</param>
        /// <param name="distance">The distance function.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="distance"/> is <b>null</b>.
        /// </exception>
        public KMeans(int k, int dimension, IVectorDistance<float, float> distance)
        {
            this.clusters = new KMeansClusterCollection(k, dimension, distance);
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
        /// Gets or sets the cluster initialization algorithm.
        /// </summary>
        /// <value>
        /// The <see cref="KMeansSeeding"/> enumeration. The default valus is <see cref="KMeansSeeding.KMeansPlusPlus"/>.
        /// </value>
        [JsonProperty("seeding")]
        public KMeansSeeding Seeding { get; set; } = KMeansSeeding.KMeansPlusPlus;

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
        /// <param name="samples">
        /// The samples to clusterize.
        /// Each sample consist of vector <c>x</c> and its <c>weight</c>.
        /// </param>
        public void Learn(NumericTable<float> x)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            int k = this.K;
            int dimension = this.clusters.Dimension;
            int sampleCount = /*x.Count; ////*/ x.Count / dimension;

#if false
            NativeMethods.kmeans(k, 1, dimension, sampleCount, x);
#else
            int maxiter = 20;

            KMeansClusterCollection clusters = this.clusters;
            switch (this.Seeding)
            {
                case KMeansSeeding.KMeansPlusPlus:
                    clusters.KMeansPlusPlusSeeding(x);
                    break;

                default:
                    clusters.RandomSeeding(x);
                    break;
            }

            float[][] means = JaggedArray.Create<float>(k, dimension);
            int[] counts = new int[k];
            object[] sync = new object[k];
            for (int i = 0; i < k; i++)
            {
                sync[i] = new object();
            }

            for (int iter = 0; iter < maxiter; iter++)
            {
                // reset means and counts
                if (iter > 0)
                {
                    for (int i = 0; i < k; i++)
                    {
                        Array32f.Set(dimension, 0.0f, means[i], 0);
                    }

                    Arrays.Set(counts.Length, 0, counts, 0);
                }

                // assign vectors to new clusters
                CommonParallel.For(
                    0,
                    sampleCount,
                    (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            int index = this.clusters.Assign(x[i], 0);

                            lock (sync[index])
                            {
                                Math32f.AddProductC(dimension, x[i], 0, 1.0f, means[index], 0);
                                counts[index]++;
                            }
                        }
                    },
                    new ParallelOptions());

                // calculate new centroids
                for (int i = 0; i < k; i++)
                {
                    if (counts[i] != 0)
                    {
                        Math32f.DivC(dimension, means[i], 0, counts[i], clusters[i].Centroid, 0);
                    }
                }
            }
#endif
        }

        /*        public static KMeans Learn(int k, IEnumerable<(float[] x, float weight)> samples)
                {
                    if (samples == null)
                    {
                        throw new ArgumentNullException(nameof(samples));
                    }

                    return null;
                }*/

        /// <summary>
        /// Assigns the vector to one of the clusters.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(float[] x) => this.clusters.Assign(x, 0);

        /// <summary>
        /// Assigns the vector to one of the clusters and returns the distance to that cluster.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <param name="score">The distance to the cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(float[] x, out float score) => this.clusters.Assign(x, 0, out score);

        /// <summary>
        /// Assigns the range of data points to feature vector containing the distance between each point and its assigned cluster.
        /// </summary>
        /// <param name="x">The range of data points to assign.</param>
        /// <returns>
        /// A vector containing the distance between each point and its assigned cluster.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Assign(IList<float[]> x) => this.clusters.Assign(x, null);

        /// <summary>
        /// Creates a feature vector by assigning each data point in <paramref name="xs"/> to one of the clusters.
        /// </summary>
        /// <param name="xs">The range of data points to assign.</param>
        /// <returns>
        /// The feature vector of length <see cref="K"/>.
        /// Each element of the feature vector contains the number of data points assigned to corresponding cluster.
        /// </returns>
        public float[] Transform(IList<float[]> xs)
        {
            float[] result = new float[this.K];
            for (int i = 0, ii = result.Length; i < ii; i++)
            {
                int cluster = this.Assign(xs[i]);
                result[cluster] += 1.0f;
            }

            return result;
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.MachineLearning.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void kmeans(int k, int iter, int dimension, int samples, float[] x);
        }
    }
}
