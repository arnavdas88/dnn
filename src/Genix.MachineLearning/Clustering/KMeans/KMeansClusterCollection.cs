// -----------------------------------------------------------------------
// <copyright file="KMeansClusterCollection.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning.Distances;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a collection of clusters used by the K-Means clustering algorithm.
    /// </summary>
    public class KMeansClusterCollection : List<KMeansCluster>
    {
        /// <summary>
        /// The distance function.
        /// </summary>
        [JsonProperty("distance", TypeNameHandling = TypeNameHandling.Objects)]
        private readonly IVectorDistance<float, float> distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansClusterCollection"/> class.
        /// </summary>
        /// <param name="k">The number of clusters.</param>
        /// <param name="dimension">The vector length.</param>
        /// <param name="distance">The distance function.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="distance"/> is <b>null</b>.
        /// </exception>
        public KMeansClusterCollection(int k, int dimension, IVectorDistance<float, float> distance)
            : base(k)
        {
            this.distance = distance ?? throw new ArgumentNullException(nameof(distance));

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
        /// Gets the number of clusters.
        /// </summary>
        /// <value>
        /// The number of clusters.
        /// </value>
        public int K => this.Count;

        /// <summary>
        /// Gets the vector length.
        /// </summary>
        /// <value>
        /// The vector length.
        /// </value>
        public int Dimension => this[0].Centroid.Length;

        /// <summary>
        /// Assigns a data point to one of the clusters.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(float[] x, int offx) => this.Assign(0, this.Count, x, offx, out float score);

        /// <summary>
        /// Assigns a data point to one of the clusters and returns the distance to that cluster.
        /// </summary>
        /// <param name="x">The data point to assign.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="score">The distance to the assigned cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(float[] x, int offx, out float score) => this.Assign(0, this.Count, x, offx, out score);

        /// <summary>
        /// Assigns a data point to one of the clusters withing specified range and returns the distance to that cluster.
        /// </summary>
        /// <param name="startingIndex">The zero-based index of the first cluster to start the assignment.</param>
        /// <param name="count">The number of clusters to assign.</param>
        /// <param name="x">The data point to assign.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="score">The distance to the assigned cluster.</param>
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(int startingIndex, int count, float[] x, int offx, out float score)
        {
            // TODO: use KDTree
            int dimension = this[startingIndex].Centroid.Length;
            int win = startingIndex;
            score = this.distance.Distance(dimension, x, offx, this[startingIndex].Centroid, 0);

            for (int i = startingIndex + 1, ii = startingIndex + count; i < ii; i++)
            {
                float dist = this.distance.Distance(dimension, x, offx, this[i].Centroid, 0);
                if (dist < score)
                {
                    win = i;
                    score = dist;
                }
            }

            return win;
        }

        /// <summary>
        /// Assigns the range of data points to feature vector containing the distance between each point and its assigned cluster.
        /// </summary>
        /// <param name="x">The range of data points to assign.</param>
        /// <param name="result">The feature vector that receives the result. Can be <b>null</b>.</param>
        /// <returns>
        /// A vector containing the distance between each point and its assigned cluster.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Assign(IList<float[]> x, float[] result) => this.Assign(0, this.Count, x, result);

        /// <summary>
        /// Assigns the range of data points to feature vector containing the distance between each point and its assigned cluster.
        /// </summary>
        /// <param name="startingIndex">The zero-based index of the first cluster to start the assignment.</param>
        /// <param name="count">The number of clusters to assign.</param>
        /// <param name="x">The range of data points to assign.</param>
        /// <param name="result">The feature vector that receives the result. Can be <b>null</b>.</param>
        /// <returns>
        /// A vector containing the distance between each point and its assigned cluster.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Assign(int startingIndex, int count, IList<float[]> x, float[] result)
        {
            if (result == null)
            {
                result = new float[x.Count];
            }

            for (int i = 0, ii = x.Count; i < ii; i++)
            {
                this.Assign(startingIndex, count, x[i], 0, out result[i]);
            }

            return result;
        }

        internal void RandomSeeding(NumericTable<float> x)
        {
            Random random = new Random(0);

            int k = this.Count;
            int dimension = this.Dimension;
            int samples = x.Count;

            for (int centroid = 0; centroid < k; centroid++)
            {
                int idx = random.Next(0, samples);
                while (centroid == 0 || !this.Take(centroid).Any(c => Arrays.Equals(dimension, c.Centroid, 0, x[idx], 0)))
                {
                    idx = random.Next(0, samples);
                }

                Arrays.Copy(dimension, x[idx], 0, this[centroid].Centroid, 0);
            }
        }

        /// <summary>
        /// Performs initial cluster seeding according to K-Means++ algorithm.
        /// </summary>
        /// <param name="x">The data points.</param>
        /// <see cref="https://en.wikipedia.org/wiki/K-means++"/>
        internal void KMeansPlusPlusSeeding(NumericTable<float> x)
        {
            Random random = new Random(0);

            int k = this.Count;
            int dimension = this.Dimension;
            int samples = x.Count;

            // 1. Choose one center uniformly at random from among the data points.
            int idx = random.Next(0, samples);
            Arrays.Copy(dimension, x[idx], 0, this[0].Centroid, 0);

            float[] distances = new float[samples];
            for (int centroid = 1; centroid < k; centroid++)
            {
                // 2. For each data point x, compute D(x), the distance between x and the nearest center that has already been chosen.
                this.Assign(0, centroid, x, distances);

                // 3. Choose one new data point at random as a new center,
                // using a weighted probability distribution where a point x is chosen with probability proportional to D(x)^2.
                Math32f.Square(samples, distances, 0);
                float sum = Math32f.Sum(samples, distances, 0);
                if (sum < 1e-10f)
                {
                    // all points are the same
                    for (; centroid < k; centroid++)
                    {
                        Arrays.Copy(dimension, this[0].Centroid, 0, this[centroid].Centroid, 0);
                    }
                }
                else
                {
                    // Chhose a point from a weighted probability distribution
                    float randomValue = (float)random.NextDouble() * sum;
                    idx = -1;
                    sum = 0.0f;
                    for (int i = 0; i < samples; i++)
                    {
                        sum += distances[i];
                        if (randomValue < sum)
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx == -1)
                    {
                        idx = random.Next(0, samples);
                    }

                    Arrays.Copy(dimension, x[idx], 0, this[centroid].Centroid, 0);
                }

                // 4. Repeat Steps 2 and 3 until k centers have been chosen.
            }
        }
    }
}
