// <copyright file="DenseVectorPackF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents packed dense vectors of single-precision floating point numbers.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DenseVectorPackF : IVectorPack<float>
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
        /// Initializes a new instance of the <see cref="DenseVectorPackF"/> class.
        /// </summary>
        /// <param name="count">The number of vectors.</param>
        /// <param name="length">The length of each vector.</param>
        private DenseVectorPackF(int count, int length)
        {
            this.Count = count;
            this.Length = length;
            this.X = new float[count * length];
        }

        /// <inheritdoc />
        [JsonProperty("count")]
        public int Count { get; private set; }

        /// <inheritdoc />
        [JsonProperty("length")]
        public int Length { get; private set; }

        /// <inheritdoc />
        [JsonProperty("x")]
        public float[] X { get; private set; }

        /// <summary>
        /// Packs a collection of dense vectors.
        /// </summary>
        /// <param name="vectors">The dense vectors to pack.</param>
        /// <returns>
        /// The <see cref="DenseVectorPackF"/> object that contains packed dense vectors.
        /// </returns>
        public static DenseVectorPackF Pack(IList<IDenseVector<float>> vectors)
        {
            DenseVectorPackF result = new DenseVectorPackF(vectors.Count, vectors[0].Length);

            float[] x = result.X;
            for (int i = 0, ii = result.Count, len = result.Length, off = 0; i < ii; i++, off += len)
            {
                Array32f.Copy(len, vectors[i].X, vectors[i].Offset, x, off);
            }

            return result;
        }

        /// <summary>
        /// Unpacks the dense vectors.
        /// </summary>
        /// <returns>
        /// The collection of <see cref="IDenseVector{T}"/> objects.
        /// </returns>
        public IList<IDenseVector<float>> Unpack()
        {
            List<IDenseVector<float>> result = new List<IDenseVector<float>>(this.Count);

            float[] x = this.X;
            for (int i = 0, ii = this.Count, len = this.Length, off = 0; i < ii; i++, off += len)
            {
                result.Add(new DenseVectorProxyF(len, x, off));
            }

            return result;
        }
    }
}