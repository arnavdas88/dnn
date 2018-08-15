// -----------------------------------------------------------------------
// <copyright file="SparseVectorF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a sparse vector of single-precision floating point numbers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The sparse vector can be represented in compressed form by two arrays, <see cref="X"/> (values) and <see cref="Idx"/> (Indexes).
    /// Each array has <see cref="Length"/> elements.
    /// </para>
    /// </remarks>
    public struct SparseVectorF : IVector<float>
    {
        /// <summary>
        /// The indexes of the elements.
        /// </summary>
        [JsonProperty("idx")]
        public int[] Idx;

        /// <summary>
        /// The values of the elements.
        /// </summary>
        [JsonProperty("x")]
        public float[] X;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVectorF"/> struct.
        /// </summary>
        /// <param name="idx">The indexes of the elements.</param>
        /// <param name="x">The values of the elements.</param>
        private SparseVectorF(int[] idx, float[] x)
        {
            this.Idx = idx;
            this.X = x;
        }

        /// <summary>
        /// Gets the number of elements in the vector.
        /// </summary>
        /// <value>
        /// The number of elements in the vector.
        /// </value>
        public int Length => this.Idx.Length;

        /// <summary>
        /// Create a <see cref="SparseVectorF"/> vector from a dense vector's non-zero elements.
        /// </summary>
        /// <param name="length">The number of elements in the vector.</param>
        /// <param name="x">The source vector.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The <see cref="SparseVectorF"/> object this method creates.
        /// </returns>
        public static SparseVectorF FromDense(int length, float[] x, int offx)
        {
            // count non-zero elements
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                if (x[offx + i] != 0.0f)
                {
                    count++;
                }
            }

            int[] idx = new int[count];
            float[] vals = new float[count];
            if (count > 0)
            {
                for (int i = 0, j = 0; i < length; i++)
                {
                    float value = x[offx + i];
                    if (value != 0.0f)
                    {
                        idx[j] = i;
                        vals[j] = value;
                        j++;
                    }
                }
            }

            return new SparseVectorF(/*length, count,*/ idx, vals);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(float[] y, int offy)
        {
            int[] idx = this.Idx;
            float[] x = this.X;

            for (int i = 0, ii = idx.Length; i < ii; i++)
            {
                y[offy + idx[i]] = x[i];
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(float[] y, int offy)
        {
            throw new NotImplementedException();
        }

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
        public float ManhattanDistance(float[] y, int offy) => NativeMethods.sparse_manhattan_distance_f32(this.X.Length, this.Idx, this.X, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] y, int offy) => NativeMethods.sparse_euclidean_distance_f32(this.X.Length, this.Idx, this.X, y, offy);

        /// <summary>
        /// Converts this <see cref="SparseVectorF"/> vector into a dense vector.
        /// </summary>
        /// <param name="length">The length of a dense vector.</param>
        /// <returns>
        /// The dense vector this method creates.
        /// </returns>
        public float[] ToDense(int length)
        {
            float[] dense = new float[length];
            this.Copy(dense, 0);
            return dense;
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float sparse_manhattan_distance_f32(int n, [In] int[] xidx, [In] float[] x, [In] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern float sparse_euclidean_distance_f32(int n, [In] int[] xidx, [In] float[] x, [In] float[] y, int offy);
        }
    }
}
