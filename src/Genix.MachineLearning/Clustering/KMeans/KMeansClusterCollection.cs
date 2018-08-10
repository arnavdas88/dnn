// -----------------------------------------------------------------------
// <copyright file="KMeansClusterCollection.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Genix.MachineLearning.Distances;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a collection of clusters used by the K-Means clustering algorithm.
    /// </summary>
    public class KMeansClusterCollection : List<KMeansCluster>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansClusterCollection"/> class.
        /// </summary>
        /// <param name="k">The number of clusters.</param>
        /// <param name="dimension">The vector length.</param>
        public KMeansClusterCollection(int k, int dimension)
            : base(k)
        {
            for (int i = 0; i < k; i++)
            {
                this.Add(new KMeansCluster(dimension));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansClusterCollection"/> class.
        /// </summary>
        [JsonConstructor]
        private KMeansClusterCollection()
        {
        }

        /// <summary>
        /// Assigns the vector to one of the clusters.
        /// </summary>
        /// <param name="distance">The distance function.</param>
        /// <param name="x">The data point to assign.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(IVectorDistance<float, float> distance, float[] x, int offx) =>
            this.Assign(distance, x, offx, out float score);

        /// <summary>
        /// Assigns the vector to one of the clusters and returns the distance to that cluster.
        /// </summary>
        /// <param name="distance">The distance function.</param>
        /// <param name="x">The data point to assign.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="score">The distance to the cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        public int Assign(IVectorDistance<float, float> distance, float[] x, int offx, out float score)
        {
            // TODO: use KDTree
            int dimension = this[0].Centroid.Length;
            int win = 0;
            score = distance.Distance(dimension, x, offx, this[0].Centroid, 0);

            for (int i = 1, ii = this.Count; i < ii; i++)
            {
                float dist = distance.Distance(dimension, x, offx, this[i].Centroid, 0);
                if (dist < score)
                {
                    win = i;
                    score = dist;
                }
            }

            return win;
        }
    }
}
