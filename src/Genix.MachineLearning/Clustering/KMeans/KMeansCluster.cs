// -----------------------------------------------------------------------
// <copyright file="KMeansCluster.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a centroid for the K-Means clustering algorithm.
    /// </summary>
    public class KMeansCluster
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansCluster"/> class.
        /// </summary>
        /// <param name="dimension">The vector length.</param>
        public KMeansCluster(int dimension)
        {
            this.Centroid = new float[dimension];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KMeansCluster"/> class.
        /// </summary>
        [JsonConstructor]
        private KMeansCluster()
        {
        }

        /// <summary>
        /// Gets the centroid vector.
        /// </summary>
        /// <value>
        /// The centroid vector.
        /// </value>
        [JsonProperty("centroid")]
        public float[] Centroid { get; internal set; }
    }
}
