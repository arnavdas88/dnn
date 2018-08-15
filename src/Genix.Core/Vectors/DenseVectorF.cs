// -----------------------------------------------------------------------
// <copyright file="DenseVectorF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a dense vector of single-precision floating point numbers.
    /// </summary>
    public struct DenseVectorF : IVector<float>
    {
        /// <summary>
        /// The values of the elements.
        /// </summary>
        [JsonProperty("x")]
        public float[] X;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(float[] y, int offy) => Array32f.Copy(this.X.Length, this.X, 0, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(float[] y, int offy) => Arrays.Equals(this.X.Length, this.X, 0, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddC(float alpha) => Math32f.AddC(this.X.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha) => Math32f.SubC(this.X.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha) => Math32f.MulC(this.X.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DivC(float alpha) => Math32f.DivC(this.X.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanDistance(float[] y, int offy) => Math32f.ManhattanDistance(this.X.Length, this.X, 0, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] y, int offy) => Math32f.EuclideanDistance(this.X.Length, this.X, 0, y, offy);
    }
}
