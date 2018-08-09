// -----------------------------------------------------------------------
// <copyright file="KMeans.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
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
        private readonly KMeansClusterCollection clusters = new KMeansClusterCollection();

        /// <summary>
        /// The distance function.
        /// </summary>
        [JsonProperty("distance", TypeNameHandling = TypeNameHandling.Objects)]
        private readonly IDistance<float[], float> distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeans"/> class.
        /// </summary>
        /// <param name="distance">The distance function.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="distance"/> is <b>null</b>.
        /// </exception>
        public KMeans(IDistance<float[], float> distance)
        {
            this.distance = distance ?? throw new ArgumentNullException(nameof(distance));

            NativeMethods.kmeans(3, 5);
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
        public static KMeans Learn(int k, IEnumerable<(float[] x, float weight)> samples)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            return null;
        }

        /// <summary>
        /// Assigns the vector to one of the clusters.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        public int Assign(float[] x)
        {
            return this.Assign(x, out float distance);
        }

        /// <summary>
        /// Assigns the vector to one of the clusters and returns the distance to that cluster.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <param name="distance">The distance to the cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        public int Assign(float[] x, out float distance)
        {
            // TODO: use KDTree
            int win = 0;
            distance = this.distance.Distance(x, this.clusters[0].Centroid);

            for (int i = 1, ii = this.clusters.Count; i < ii; i++)
            {
                float dist = this.distance.Distance(x, this.clusters[i].Centroid);
                if (dist < distance)
                {
                    win = i;
                    distance = dist;
                }
            }

            return win;
        }

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
            public static extern void kmeans(int k, int iter);
        }
    }
}
