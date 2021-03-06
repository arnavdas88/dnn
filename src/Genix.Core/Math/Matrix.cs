﻿// -----------------------------------------------------------------------
// <copyright file="Matrix.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides matrix operations.
    /// </summary>
    public static class Matrix
    {
        /// <summary>
        /// Calculates a dot product between values from one array of single-precision floating point numbers
        /// starting at the specified index
        /// and values from another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which calculation begins.</param>
        /// <returns>
        /// The calculated dot product value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(int length, float[] a, int offa, float[] b, int offb)
        {
            return NativeMethods.dot_f32(length, a, offa, 1, b, offb, 1);
        }

        /// <summary>
        /// Calculates a dot product between values from one array of double-precision floating point numbers
        /// starting at the specified index
        /// and values from another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which calculation begins.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which calculation begins.</param>
        /// <returns>
        /// The calculated dot product value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DotProduct(int length, double[] a, int offa, double[] b, int offb)
        {
            return NativeMethods.dot_f64(length, a, offa, 1, b, offb, 1);
        }

        /// <summary>
        /// Calculates a dot product between values from one array starting at the specified index
        /// and values from another array starting at the specified index.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="a">The first array that contains the data.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which calculation begins.</param>
        /// <param name="inca">the increment for the elements of <paramref name="a"/>.</param>
        /// <param name="b">The second array that contains the data.</param>
        /// <param name="offb">The index in the <paramref name="b"/> at which calculation begins.</param>
        /// <param name="incb">the increment for the elements of <paramref name="b"/>.</param>
        /// <returns>
        /// The calculated dot product value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(int length, float[] a, int offa, int inca, float[] b, int offb, int incb)
        {
            return NativeMethods.dot_f32(length, a, offa, inca, b, offb, incb);
        }

        /// <summary>
        /// Performs a rank-1 update of a general matrix.
        /// The operation is defined as A := x*y'+ A or as A = x*y' depending on value of <paramref name="cleara"/> parameter.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrix A is row-major or column-major.</param>
        /// <param name="m">Specifies the number of rows of the matrix A.</param>
        /// <param name="n">Specifies the number of columns of the matrix A.</param>
        /// <param name="x">The array that contains the vector x.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which the vector x begins.</param>
        /// <param name="y">The array that contains the vector y.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which the vector y begins.</param>
        /// <param name="a">The array that contains the destination matrix A.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which the matrix A begins.</param>
        /// <param name="cleara">Specifies whether the <paramref name="a"/> should be cleared before operation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VxV(MatrixLayout matrixLayout, int m, int n, float[] x, int offx, float[] y, int offy, float[] a, int offa, bool cleara)
        {
            NativeMethods.matrix_vv(matrixLayout == MatrixLayout.RowMajor, m, n, x, offx, y, offy, a, offa, cleara);
        }

        /// <summary>
        /// Computes a matrix-vector product.
        /// The operation is defined as y := op(A)*x or as y := op(A)*x + y depending on value of <paramref name="cleary"/> parameter.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrix A is row-major or column-major.</param>
        /// <param name="m">Specifies the number of rows of the matrix A.</param>
        /// <param name="n">Specifies the number of columns of the matrix A.</param>
        /// <param name="a">The array that contains the matrix A.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which the matrix A begins.</param>
        /// <param name="transa">Specifies whether the matrix A should be transposed before computation.</param>
        /// <param name="x">The array that contains the vector x.</param>
        /// <param name="offx">The index in the <paramref name="x"/> at which the vector x begins.</param>
        /// <param name="y">The array that receives the vector y.</param>
        /// <param name="offy">The index in the <paramref name="y"/> at which the vector y begins.</param>
        /// <param name="cleary">Specifies whether the <paramref name="y"/> should be cleared before operation.</param>
        /// <remarks>
        /// <para>
        /// The size of input vector <paramref name="x"/> should be at least <paramref name="n"/> when <paramref name="transa"/> is <b>false</b> and at least <paramref name="m"/> otherwise.
        /// </para>
        /// <para>
        /// The size of output vector <paramref name="y"/> should be at least <paramref name="m"/> when <paramref name="transa"/> is <b>false</b> and at least <paramref name="n"/> otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MxV(MatrixLayout matrixLayout, int m, int n, float[] a, int offa, bool transa, float[] x, int offx, float[] y, int offy, bool cleary)
        {
            NativeMethods.matrix_mv(matrixLayout == MatrixLayout.RowMajor, m, n, a, offa, transa, x, offx, y, offy, cleary);
        }

        /// <summary>
        /// Computes a matrix-matrix product.
        /// The operation is defined as C := op(A)*op(B) + C or as C := op(A)*op(B) depending on value of <paramref name="clearc"/> parameter.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrices A, B, and C are row-major or column-major.</param>
        /// <param name="m">Specifies the number of rows of the matrix op(A) and of the matrix C.</param>
        /// <param name="k">Specifies the number of columns of the matrix op(A) and the number of rows of the matrix op(B).</param>
        /// <param name="n">Specifies the number of columns of the matrix op(B) and of the matrix C.</param>
        /// <param name="a">The array that contains the matrix A.</param>
        /// <param name="offa">The index in the <paramref name="a"/> at which the matrix A begins.</param>
        /// <param name="transa">Specifies whether the matrix A should be transposed before computation.</param>
        /// <param name="b">The array that contains the matrix B.</param>
        /// <param name="offb">The index in the <paramref name="a"/> at which the matrix B begins.</param>
        /// <param name="transb">Specifies whether the matrix B should be transposed before computation.</param>
        /// <param name="c">The array that contains the destination matrix C.</param>
        /// <param name="offc">The index in the <paramref name="a"/> at which the matrix C begins.</param>
        /// <param name="clearc">Specifies whether the matrix C should be cleared before operation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MxM(MatrixLayout matrixLayout, int m, int k, int n, float[] a, int offa, bool transa, float[] b, int offb, bool transb, float[] c, int offc, bool clearc)
        {
            NativeMethods.matrix_mm(matrixLayout == MatrixLayout.RowMajor, m, k, n, a, offa, transa, b, offb, transb, c, offc, clearc);
        }

        /// <summary>
        /// Transposes a matrix.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrices AB is row-major or column-major.</param>
        /// <param name="m">The number of rows in matrix AB before the transpose operation.</param>
        /// <param name="n">The number of columns in matrix AB before the transpose operation.</param>
        /// <param name="ab">The array that contains the matrix AB.</param>
        /// <param name="offab">The index in the <paramref name="ab"/> at which the matrix AB begins.</param>
        public static void Transpose(MatrixLayout matrixLayout, int m, int n, float[] ab, int offab)
        {
            NativeMethods.matrix_transpose(matrixLayout == MatrixLayout.RowMajor, m, n, ab, offab);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern float dot_f32(int n, [In] float[] x, int offx, int incx, [In] float[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern double dot_f64(int n, [In] double[] x, int offx, int incx, [In] double[] y, int offy, int incy);

            [DllImport(NativeMethods.DllName)]
            public static extern void matrix_vv(
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor,
                int m,
                int n,
                [In] float[] x,
                int offx,
                [In] float[] y,
                int offy,
                [Out] float[] a,
                int offa,
                [MarshalAs(UnmanagedType.Bool)] bool cleara);

            [DllImport(NativeMethods.DllName)]
            public static extern void matrix_mv(
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor,
                int m,
                int n,
                [In] float[] a,
                int offa,
                [MarshalAs(UnmanagedType.Bool)] bool transa,
                [In] float[] x,
                int offx,
                [Out] float[] y,
                int offy,
                [MarshalAs(UnmanagedType.Bool)] bool cleary);

            [DllImport(NativeMethods.DllName)]
            public static extern void matrix_mm(
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor,
                int m,
                int k,
                int n,
                [In] float[] a,
                int offa,
                [MarshalAs(UnmanagedType.Bool)] bool transa,
                [In] float[] b,
                int offb,
                [MarshalAs(UnmanagedType.Bool)] bool transb,
                [In, Out] float[] c,
                int offc,
                [MarshalAs(UnmanagedType.Bool)] bool clearc);

            [DllImport(NativeMethods.DllName)]
            public static extern void matrix_transpose(
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor,
                int m,
                int n,
                [In] float[] ab,
                int offab);
        }
    }
}
