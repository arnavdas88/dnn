// -----------------------------------------------------------------------
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
        /// Performs a rank-1 update of a general matrix.
        /// The operation is defined as A := x*y'+ A.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrix A is row-major or column-major.</param>
        /// <param name="m">Specifies the number of rows of the matrix A.</param>
        /// <param name="n">Specifies the number of columns of the matrix A.</param>
        /// <param name="x">The array that contains the vector x.</param>
        /// <param name="offx">The index in the <c>x</c> at which the vector x begins.</param>
        /// <param name="y">The array that contains the vector y.</param>
        /// <param name="offy">The index in the <c>y</c> at which the vector y begins.</param>
        /// <param name="a">The array that contains the destination matrix A.</param>
        /// <param name="offa">The index in the <c>a</c> at which the matrix A begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VxV(MatrixLayout matrixLayout, int m, int n, float[] x, int offx, float[] y, int offy, float[] a, int offa)
        {
            NativeMethods.matrix_vv(matrixLayout == MatrixLayout.RowMajor, m, n, x, offx, y, offy, a, offa);
        }

        /// <summary>
        /// Computes a matrix-vector product.
        /// The operation is defined as y := op(A)*x or as y := op(A)*x + y depending on value of <c>cleary</c> parameter.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrix A is row-major or column-major.</param>
        /// <param name="m">Specifies the number of rows of the matrix A.</param>
        /// <param name="n">Specifies the number of columns of the matrix A.</param>
        /// <param name="a">The array that contains the matrix A.</param>
        /// <param name="offa">The index in the <c>a</c> at which the matrix A begins.</param>
        /// <param name="transa">Specifies whether the matrix A should be transposed before computation.</param>
        /// <param name="x">The array that contains the vector x.</param>
        /// <param name="offx">The index in the <c>x</c> at which the vector x begins.</param>
        /// <param name="y">The array that receives the vector y.</param>
        /// <param name="offy">The index in the <c>y</c> at which the vector y begins.</param>
        /// <param name="cleary">Specifies whether the <c>y</c> should be cleared before operation.</param>
        /// <remarks>
        /// <para>
        /// The size of input vector <c>x</c> should be at least <c>n</c> when <c>transa</c> is <b>false</b> and at least <c>m</c> otherwise.
        /// </para>
        /// <para>
        /// The size of output vector <c>y</c> should be at least <c>m</c> when <c>transa</c> is <b>false</b> and at least <c>n</c> otherwise.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MxV(MatrixLayout matrixLayout, int m, int n, float[] a, int offa, bool transa, float[] x, int offx, float[] y, int offy, bool cleary)
        {
            NativeMethods.matrix_mv(matrixLayout == MatrixLayout.RowMajor, m, n, a, offa, transa, x, offx, y, offy, cleary);
        }

        /// <summary>
        /// Computes a matrix-matrix product.
        /// The operation is defined as C := op(A)*op(B) + C or as C := op(A)*op(B) depending on value of <c>clearc</c> parameter.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrices A, B, and C are row-major or column-major.</param>
        /// <param name="m">Specifies the number of rows of the matrix op(A) and of the matrix C.</param>
        /// <param name="k">Specifies the number of columns of the matrix op(A) and the number of rows of the matrix op(B).</param>
        /// <param name="n">Specifies the number of columns of the matrix op(B) and of the matrix C.</param>
        /// <param name="a">The array that contains the matrix A.</param>
        /// <param name="offa">The index in the <c>a</c> at which the matrix A begins.</param>
        /// <param name="transa">Specifies whether the matrix A should be transposed before computation.</param>
        /// <param name="b">The array that contains the matrix B.</param>
        /// <param name="offb">The index in the <c>a</c> at which the matrix B begins.</param>
        /// <param name="transb">Specifies whether the matrix B should be transposed before computation.</param>
        /// <param name="c">The array that contains the destination matrix C.</param>
        /// <param name="offc">The index in the <c>a</c> at which the matrix C begins.</param>
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
        /// <param name="offab">The index in the <c>ab</c> at which the matrix AB begins.</param>
        public static void Transpose(MatrixLayout matrixLayout, int m, int n, float[] ab, int offab)
        {
            NativeMethods.matrix_transpose(matrixLayout == MatrixLayout.RowMajor, m, n, ab, offab);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void matrix_vv(
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor,
                int m,
                int n,
                [In] float[] x,
                int offx,
                [In] float[] y,
                int offy,
                [Out] float[] a,
                int offa);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
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
            [SuppressUnmanagedCodeSecurity]
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
            [SuppressUnmanagedCodeSecurity]
            public static extern void matrix_transpose(
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor,
                int m,
                int n,
                [In] float[] ab,
                int offab);
        }
    }
}
