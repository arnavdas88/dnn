// -----------------------------------------------------------------------
// <copyright file="DenseVectorProxyF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a wrapper around dense vector of single-precision floating point numbers.
    /// </summary>
    public struct DenseVectorProxyF
        : IEquatable<DenseVectorProxyF>, IDenseVector<float>
    {
        /// <summary>
        /// The vector length.
        /// </summary>
        private readonly int length;

        /// <summary>
        /// The vector elements.
        /// </summary>
        private readonly float[] x;

        /// <summary>
        /// The staring position of vector elements in <see cref="X"/>.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVectorProxyF"/> struct.
        /// </summary>
        /// <param name="length">The vector length.</param>
        /// <param name="x">The vector elements.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DenseVectorProxyF(int length, float[] x, int offx)
        {
            this.length = length;
            this.x = x;
            this.offset = offx;
        }

        /// <inheritdoc />
        public int Length => this.length;

        /// <inheritdoc />
        public float[] X => this.x;

        /// <inheritdoc />
        public int Offset => this.offset;

        /// <summary>
        /// Compares two <see cref="DenseVectorProxyF"/> objects. The result specifies whether the properties specified by the two <see cref="DenseVectorProxyF"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="DenseVectorProxyF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="DenseVectorProxyF"/> structure that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="DenseVectorProxyF"/> structures have equal properties; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator ==(DenseVectorProxyF left, DenseVectorProxyF right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="DenseVectorProxyF"/> objects. The result specifies whether the properties specified by the two <see cref="DenseVectorProxyF"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="DenseVectorProxyF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="DenseVectorProxyF"/> structure that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="DenseVectorProxyF"/> structures have unequal properties; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator !=(DenseVectorProxyF left, DenseVectorProxyF right) => !left.Equals(right);

        /// <inheritdoc />
        public bool Equals(DenseVectorProxyF other)
        {
            return this.length == other.length &&
                (this.length == 0 || Array32f.Equals(this.length, this.x, this.offset, other.x, other.offset));
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is DenseVectorProxyF))
            {
                return false;
            }

            return this.Equals((DenseVectorProxyF)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)CRC.Calculate(this.length, this.x, this.offset);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(float[] y, int offy) => Vectors.Copy(this.length, this.x, this.offset, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddC(float alpha) => Vectors.AddC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha) => Vectors.SubC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha) => Vectors.MulC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DivC(float alpha) => Vectors.DivC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProductC(float alpha, float[] y, int offy) => Vectors.AddProductC(this.length, this.x, this.offset, alpha, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sum() => Math32f.Sum(this.length, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanDistance(float[] y, int offy) => Vectors.ManhattanDistance(this.length, this.x, this.offset, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] y, int offy) => Vectors.EuclideanDistance(this.length, this.x, this.offset, y, offy);
    }
}
