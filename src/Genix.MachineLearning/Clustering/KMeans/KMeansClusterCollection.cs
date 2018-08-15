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
    /// <typeparam name="T">The type of vector this algorithm supports.</typeparam>
    public class KMeansClusterCollection<T> : List<KMeansCluster>
        where T : IVector<float>
    {
        /// <summary>
        /// The distance function.
        /// </summary>
        [JsonProperty("distance", TypeNameHandling = TypeNameHandling.Objects)]
        private readonly IVectorDistance<float, T, float> distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansClusterCollection{T}"/> class.
        /// </summary>
        /// <param name="k">The number of clusters.</param>
        /// <param name="dimension">The vector length.</param>
        /// <param name="distance">The distance function.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="distance"/> is <b>null</b>.
        /// </exception>
        public KMeansClusterCollection(int k, int dimension, IVectorDistance<float, T, float> distance)
            : base(k)
        {
            this.distance = distance ?? throw new ArgumentNullException(nameof(distance));

            for (int i = 0; i < k; i++)
            {
                this.Add(new KMeansCluster(dimension));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansClusterCollection{T}"/> class.
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
        /// <returns>
        /// The zero-based index of the cluster closest to the <paramref name="x"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Assign(T x) => this.Assign(0, this.Count, x, out float score);

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
        public int Assign(T x, out float score) => this.Assign(0, this.Count, x, out score);

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
        public int Assign(int startingIndex, int count, T x, out float score)
        {
            // TODO: use KDTree
            int win = startingIndex;
            score = this.distance.Distance(x, this[startingIndex].Centroid, 0);

            for (int i = startingIndex + 1, ii = startingIndex + count; i < ii; i++)
            {
                float dist = this.distance.Distance(x, this[i].Centroid, 0);
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
        public float[] Assign(IList<T> x, float[] result) => this.Assign(0, this.Count, x, result);

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
        public float[] Assign(int startingIndex, int count, IList<T> x, float[] result)
        {
            if (result == null)
            {
                result = new float[x.Count];
            }

            for (int i = 0, ii = x.Count; i < ii; i++)
            {
                this.Assign(startingIndex, count, x[i], out result[i]);
            }

            return result;
        }

        /// <summary>
        /// Performs initial cluster seeding by choosing clusters randomly from data points.
        /// </summary>
        /// <param name="x">The data points <paramref name="x"/> to clusterize.</param>
        /// <param name="weights">The <c>weight</c> of importance for each data point.</param>
        internal void RandomSeeding(IList<T> x, IList<float> weights)
        {
            Random random = new Random(0);

            int k = this.Count;
            int dimension = this.Dimension;
            int samples = x.Count;
            ProbabilityDistribution distribution = weights != null ? new ProbabilityDistribution(weights, random) : null;

            // 1. Choose one center uniformly at random from among the data points.
            int idx = Next();
            x[idx].Copy(this[0].Centroid, 0);

            // 2. Choose other centers uniformly at random from among the data points
            // make sure data points are different
            for (int centroid = 1; centroid < k; centroid++)
            {
                idx = Next();
                while (!this.Take(centroid).Any(c => x[idx].Equals(c.Centroid, 0)))
                {
                    idx = Next();
                }

                x[idx].Copy(this[centroid].Centroid, 0);
            }

            int Next()
            {
                return distribution != null ? distribution.Next() : random.Next(0, samples);
            }
        }

        /// <summary>
        /// Performs initial cluster seeding according to K-Means++ algorithm.
        /// </summary>
        /// <param name="x">The data points <paramref name="x"/> to clusterize.</param>
        /// <param name="weights">The <c>weight</c> of importance for each data point.</param>
        /// <see cref="https://en.wikipedia.org/wiki/K-means++"/>
        internal void KMeansPlusPlusSeeding(IList<T> x, IList<float> weights)
        {
            Random random = new Random(0);

            int k = this.Count;
            int dimension = this.Dimension;
            int samples = x.Count;

            // 1. Choose one center uniformly at random from among the data points.
            int idx = random.Next(0, samples);
            x[idx].Copy(this[0].Centroid, 0);

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
                        Array32f.Copy(dimension, this[0].Centroid, 0, this[centroid].Centroid, 0);
                    }
                }
                else
                {
                    // Choose a point from a weighted probability distribution
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

                    x[idx].Copy(this[centroid].Centroid, 0);
                }

                // 4. Repeat Steps 2 and 3 until k centers have been chosen.
            }
        }
    }
}
