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
    [JsonObject(MemberSerialization.OptIn)]
    public struct SparseVectorF
        : IEquatable<SparseVectorF>, IVector<float>
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
        /// The vector length.
        /// </summary>
        [JsonProperty("length")]
        private int length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseVectorF"/> struct.
        /// </summary>
        /// <param name="length">The vector length.</param>
        /// <param name="idx">The indexes of the elements.</param>
        /// <param name="x">The values of the elements.</param>
        public SparseVectorF(int length, int[] idx, float[] x)
        {
            this.length = length;
            this.Idx = idx;
            this.X = x;
        }

        /// <inheritdoc />
        public int Length => this.length;

        /// <summary>
        /// Compares two <see cref="SparseVectorF"/> objects. The result specifies whether the properties specified by the two <see cref="SparseVectorF"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="SparseVectorF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="SparseVectorF"/> structure that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="SparseVectorF"/> structures have equal properties; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator ==(SparseVectorF left, SparseVectorF right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="SparseVectorF"/> objects. The result specifies whether the properties specified by the two <see cref="SparseVectorF"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="SparseVectorF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="SparseVectorF"/> structure that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="SparseVectorF"/> structures have unequal properties; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator !=(SparseVectorF left, SparseVectorF right) => !left.Equals(right);

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

            return new SparseVectorF(length, idx, vals);
        }

        /// <inheritdoc />
        public bool Equals(SparseVectorF other)
        {
            return this.length == other.length &&
                this.Idx.Length == other.Idx.Length &&
                (this.Idx.Length == 0 ||
                    (Vectors.Equals(this.Idx.Length, this.Idx, 0, other.Idx, 0) &&
                     Vectors.Equals(this.Idx.Length, this.X, 0, other.X, 0)));
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is SparseVectorF))
            {
                return false;
            }

            return this.Equals((SparseVectorF)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)(CRC.Calculate(this.Idx.Length, this.Idx, 0) & CRC.Calculate(this.Idx.Length, this.X, 0));
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
        public void AddC(float alpha) => Mathematics.AddC(this.Idx.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha) => Mathematics.SubC(this.Idx.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha) => Mathematics.MulC(this.Idx.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DivC(float alpha) => Mathematics.DivC(this.Idx.Length, alpha, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProductC(float alpha, float[] y, int offy) => NativeMethods.sparse_addproductc_f32(this.Idx.Length, this.Idx, this.X, alpha, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sum() => Vectors.Sum(this.Idx.Length, this.X, 0);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ManhattanDistance(float[] y, int offy) => NativeMethods.sparse_manhattan_distance_f32(this.Idx.Length, this.Idx, this.X, y, offy);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EuclideanDistance(float[] y, int offy) => NativeMethods.sparse_euclidean_distance_f32(this.Idx.Length, this.Idx, this.X, y, offy);

        /// <summary>
        /// Converts this <see cref="SparseVectorF"/> vector into a dense vector.
        /// </summary>
        /// <returns>
        /// The dense vector this method creates.
        /// </returns>
        public float[] ToDense()
        {
            float[] dense = new float[this.length];
            this.Copy(dense, 0);
            return dense;
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void sparse_addproductc_f32(int n, [In] int[] xidx, [In] float[] x, float a, [Out] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float sparse_manhattan_distance_f32(int n, [In] int[] xidx, [In] float[] x, [In] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float sparse_euclidean_distance_f32(int n, [In] int[] xidx, [In] float[] x, [In] float[] y, int offy);
        }
    }
}
