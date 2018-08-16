// -----------------------------------------------------------------------
// <copyright file="DenseVectorProxyF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a wrapper around dense vector of single-precision floating point numbers.
    /// </summary>
    public struct DenseVectorProxyF
        : IDenseVector<float>
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

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(float[] y, int offy) => Array32f.Copy(this.length, this.x, this.offset, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(float[] y, int offy) => Arrays.Equals(this.length, this.x, this.offset, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddC(float alpha) => Math32f.AddC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha) => Math32f.SubC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha) => Math32f.MulC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DivC(float alpha) => Math32f.DivC(this.length, alpha, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProductC(float alpha, float[] y, int offy) => Math32f.AddProductC(this.length, this.x, this.offset, alpha, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sum() => Math32f.Sum(this.length, this.x, this.offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanDistance(float[] y, int offy) => Math32f.ManhattanDistance(this.length, this.x, this.offset, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] y, int offy) => Math32f.EuclideanDistance(this.length, this.x, this.offset, y, offy);
    }
}
