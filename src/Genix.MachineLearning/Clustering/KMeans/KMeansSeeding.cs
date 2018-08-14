// -----------------------------------------------------------------------
// <copyright file="KMeansSeeding.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Clustering
{
    /// <summary>
    /// Defines the cluster initialization algorithm for K-Means clustering.
    /// </summary>
    public enum KMeansSeeding
    {
        /// <summary>
        /// Choose centroids randomly from data points.
        /// </summary>
        Random = 0,

        /// <summary>
        /// The kmeans++ algorithm.
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/K-means++"/>
        KMeansPlusPlus,
    }
}
