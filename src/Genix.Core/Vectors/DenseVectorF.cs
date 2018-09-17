// -----------------------------------------------------------------------
// <copyright file="DenseVectorF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a dense vector of single-precision floating point numbers.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public struct DenseVectorF
        : IEquatable<DenseVectorF>, IDenseVector<float>
    {
        /// <summary>
        /// The values of the elements.
        /// </summary>
        [JsonProperty("x")]
        private readonly float[] x;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVectorF"/> struct.
        /// </summary>
        /// <param name="length">The number of elements in the vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DenseVectorF(int length)
        {
            this.x = new float[length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVectorF"/> struct.
        /// </summary>
        /// <param name="length">The number of elements in the vector.</param>
        /// <param name="x">The vector elements.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DenseVectorF(int length, float[] x, int offx)
            : this(length)
        {
            Vectors.Copy(length, x, offx, this.x, 0);
        }

        /// <inheritdoc />
        public int Length => this.x.Length;

        /// <inheritdoc />
        public float[] X => this.x;

        /// <inheritdoc />
        public int Offset => 0;

        /// <summary>
        /// Compares two <see cref="DenseVectorF"/> objects. The result specifies whether the properties specified by the two <see cref="DenseVectorF"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="DenseVectorF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="DenseVectorF"/> structure that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="DenseVectorF"/> structures have equal properties; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator ==(DenseVectorF left, DenseVectorF right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="DenseVectorF"/> objects. The result specifies whether the properties specified by the two <see cref="DenseVectorF"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="DenseVectorF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="DenseVectorF"/> structure that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="DenseVectorF"/> structures have unequal properties; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator !=(DenseVectorF left, DenseVectorF right) => !left.Equals(right);

        /// <inheritdoc />
        public bool Equals(DenseVectorF other)
        {
            return this.Length == other.Length &&
                (this.Length == 0 || Vectors.Equals(this.x.Length, this.x, 0, other.X, 0));
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is DenseVectorF))
            {
                return false;
            }

            return this.Equals((DenseVectorF)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)CRC.Calculate(this.x);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(float[] y, int offy) => Vectors.Copy(this.x.Length, this.x, 0, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddC(float alpha) => Vectors.AddC(this.x.Length, alpha, this.x, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha) => Vectors.SubC(this.x.Length, alpha, this.x, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha) => Vectors.MulC(this.x.Length, alpha, this.x, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DivC(float alpha) => Vectors.DivC(this.x.Length, alpha, this.x, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProductC(float alpha, float[] y, int offy) => Vectors.AddProductC(this.x.Length, this.x, 0, alpha, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sum() => Vectors.Sum(this.x.Length, this.x, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanDistance(float[] y, int offy) => Vectors.ManhattanDistance(this.x.Length, this.x, 0, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] y, int offy) => Vectors.EuclideanDistance(this.x.Length, this.x, 0, y, offy);
    }
}
