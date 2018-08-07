// -----------------------------------------------------------------------
// <copyright file="KMeansCluster.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    /// <summary>
    /// Represents a centroid for the K-Means clustering algorithm.
    /// </summary>
    public class KMeansCluster
    {
        /// <summary>
        /// Gets the centroid vector.
        /// </summary>
        /// <value>
        /// The centroid vector.
        /// </value>
        public float[] Centroid { get; internal set; }
    }
}
