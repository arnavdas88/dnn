// -----------------------------------------------------------------------
// <copyright file="SparseVectorF.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
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
    public struct SparseVectorF
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

            int[] idx = this.Idx;
            float[] x = this.X;

            for (int i = 0, ii = idx.Length; i < ii; i++)
            {
                dense[idx[i]] = x[i];
            }

            return dense;
        }

        /// <summary>
        /// Adds a constant value to each element of the <see cref="SparseVectorF"/>.
        /// </summary>
        /// <param name="alpha">The scalar to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddC(float alpha)
        {
            Math32f.AddC(this.X.Length, alpha, this.X, 0);
        }

        /// <summary>
        /// Subtracts a constant value from each element of the <see cref="SparseVectorF"/>.
        /// </summary>
        /// <param name="alpha">The scalar to subtract.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha)
        {
            Math32f.SubC(this.X.Length, alpha, this.X, 0);
        }

        /// <summary>
        /// Multiplies each element of the <see cref="SparseVectorF"/> by a constant value in-place.
        /// </summary>
        /// <param name="alpha">The scalar to multiply.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha)
        {
            Math32f.MulC(this.X.Length, alpha, this.X, 0);
        }

        /// <summary>
        /// Divides each element of the <see cref="SparseVectorF"/> by a constant value in-place.
        /// </summary>
        /// <param name="alpha">The scalar to divide.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DivC(float alpha)
        {
            Math32f.DivC(this.X.Length, alpha, this.X, 0);
        }

        /// <summary>
        /// Computes the Manhattan distance between elements of this <see cref="SparseVectorF"/> and the dense vector.
        /// </summary>
        /// <param name="x">The dense vector <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The Manhattan distance between elements of two vectors.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanDistance(float[] x, int offx)
        {
            return NativeMethods.sparse_manhattan_distance_f32(this.X.Length, this.Idx, this.X, x, offx);
        }

        /// <summary>
        /// Computes the Euclidean distance between elements of this <see cref="SparseVectorF"/> and the dense vector.
        /// </summary>
        /// <param name="x">The dense vector <paramref name="x"/>.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <returns>
        /// The Euclidean distance between elements of two vectors.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] x, int offx)
        {
            return NativeMethods.sparse_euclidean_distance_f32(this.X.Length, this.Idx, this.X, x, offx);
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
