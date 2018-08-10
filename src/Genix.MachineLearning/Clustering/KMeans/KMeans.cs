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
        /// The distance function.
        /// </summary>
        [JsonProperty("distance", TypeNameHandling = TypeNameHandling.Objects)]
        private readonly IVectorDistance<float, float> distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeans"/> class.
        /// </summary>
        /// <param name="distance">The distance function.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="distance"/> is <b>null</b>.
        /// </exception>
        public KMeans(IVectorDistance<float, float> distance)
        {
            this.distance = distance ?? throw new ArgumentNullException(nameof(distance));

            ////NativeMethods.kmeans(3, 5);
        }

        /// <summary>
        /// Gets the number of clusters.
        /// </summary>
        /// <value>
        /// The number of clusters.
        /// </value>
        [JsonProperty("K")]
        public int K { get; private set; }

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
        /// <param name="samples">
        /// The samples to clusterize.
        /// Each sample consist of vector <c>x</c> and its <c>weight</c>.
        /// </param>
        /// <returns>
        /// The learned <see cref="KMeans"/> model.
        /// </returns>
        public static KMeans Learn(int k, int dimension, /*IList<float[]>*/ float[] x)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            int sampleCount = /*x.Count; ////*/ x.Length / dimension;

#if false
            NativeMethods.kmeans(k, 1, dimension, sampleCount, x);
#else
            int maxiter = 20;
            IVectorDistance<float, float> distance = default(EuclideanDistance);

            KMeansClusterCollection clusters = Seed();
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
                        for (int i = a, off = a * dimension; i < b; i++, off += dimension)
                        {
                            int index = clusters.Assign(distance, x, off);

                            lock (sync[index])
                            {
                                Math32f.AddProductC(dimension, x, off, 1.0f, means[index], 0);
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
            return null;

            KMeansClusterCollection Seed()
            {
                Random random = new Random(0);

                KMeansClusterCollection result = new KMeansClusterCollection(k, dimension);
                for (int i = 0; i < k; i++)
                {
                    int index = random.Next(0, sampleCount);
                    int off = index * dimension;
                    if (i > 0 && result.Take(i).Any(c => Arrays.Equals(dimension, c.Centroid, 0, x, off)))
                    {
                        i--;
                        continue;
                    }

                    Arrays.Copy(dimension, x, off, result[i].Centroid, 0);
                }

                Console.WriteLine("seeded.");

                return result;
            }
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
        public int Assign(float[] x) => this.clusters.Assign(this.distance, x, 0);

        /// <summary>
        /// Assigns the vector to one of the clusters and returns the distance to that cluster.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <param name="score">The distance to the cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(float[] x, out float score) => this.clusters.Assign(this.distance, x, 0, out score);

        /// <summary>
        /// Assigns the range of data points to feature vector containing the distance between each point and its assigned cluster.
        /// </summary>
        /// <param name="xs">The range of data points to assign.</param>
        /// <returns>
        /// A vector containing the distance between each point and its assigned cluster.
        /// </returns>
        public float[] Assign(IList<float[]> xs)
        {
            float[] result = new float[xs.Count];
            for (int i = 0, ii = result.Length; i < ii; i++)
            {
                this.Assign(xs[i], out float distance);
                result[i] = distance;
            }

            return result;
        }

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
