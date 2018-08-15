// <copyright file="DenseVectorPackF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents packed dense vectors of single-precision floating point numbers.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DenseVectorPackF
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVectorPackF"/> class.
        /// </summary>
        /// <param name="count">The number of vectors.</param>
        /// <param name="length">The length of each vector.</param>
        /// <param name="x">The feature vectors.</param>
        public DenseVectorPackF(int count, int length, float[] x)
        {
            this.Count = count;
            this.Length = length;
            this.X = x;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVectorPackF"/> class.
        /// </summary>
        /// <param name="other">The <see cref="DenseVectorPackF"/> to copy the data from.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is <b>null</b>.
        /// </exception>
        public DenseVectorPackF(DenseVectorPackF other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Count = other.Count;
            this.Length = other.Length;
            this.X = other.X;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVectorPackF"/> class.
        /// </summary>
        [JsonConstructor]
        protected DenseVectorPackF()
        {
        }

        /// <summary>
        /// Gets the number of vectors in <see cref="X"/>.
        /// </summary>
        /// <value>
        /// The number of vectors.
        /// </value>
        [JsonProperty("count")]
        public int Count { get; private set; }

        /// <summary>
        /// Gets the length of each vector in <see cref="X"/>.
        /// </summary>
        /// <value>
        /// The length of each vector.
        /// </value>
        [JsonProperty("length")]
        public int Length { get; private set; }

        /// <summary>
        /// Gets the packed vectors.
        /// </summary>
        /// <value>
        /// The packed vectors.
        /// </value>
        /// <remarks>
        /// The total number of vectors contained in this property is <see cref="Count"/> * <see cref="Length"/>.
        /// </remarks>
        [JsonProperty("x")]
        public float[] X { get; private set; }
    }
}