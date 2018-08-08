// <copyright file="Features.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.FeatureDetectors
{
    using Genix.Imaging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents features (points of interest) extracted from an <see cref="Image"/>.
    /// </summary>
    public class Features
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class.
        /// </summary>
        /// <param name="count">The number of vectors.</param>
        /// <param name="length">The length of each vector.</param>
        /// <param name="vectors">The feature vectors.</param>
        public Features(int count, int length, float[] vectors)
        {
            this.Count = count;
            this.Length = length;
            this.Vectors = vectors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class.
        /// </summary>
        [JsonConstructor]
        private Features()
        {
        }

        /// <summary>
        /// Gets the number of vectors in <see cref="Vectors"/>.
        /// </summary>
        /// <value>
        /// The number of vectors.
        /// </value>
        [JsonProperty("count")]
        public int Count { get; private set; }

        /// <summary>
        /// Gets the length of each vector in <see cref="Vectors"/>.
        /// </summary>
        /// <value>
        /// The length of each vector.
        /// </value>
        [JsonProperty("length")]
        public int Length { get; private set; }

        /// <summary>
        /// Gets the feature vectors.
        /// </summary>
        /// <value>
        /// The feature vectors.
        /// </value>
        /// <remarks>
        /// The total number of vectors contained in this property is <see cref="Count"/> * <see cref="Length"/>.
        /// </remarks>
        [JsonProperty("vectors")]
        public float[] Vectors { get; private set; }
    }
}
