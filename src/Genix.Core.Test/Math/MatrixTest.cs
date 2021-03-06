﻿namespace Genix.Core.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MatrixTest
    {
        [TestMethod]
        public void DotProductTest()
        {
            float res = Matrix.DotProduct(3, new float[] { 1, 2, 3, 4 }, 0, 1, new float[] { 5, 6, 7, 8 }, 1, 1);
            Assert.AreEqual((1 * 6) + (2 * 7) + (3 * 8), res);
        }

        [TestMethod]
        public void VxVTest()
        {
            const int m = 3;
            const int n = 2;

            float[] x = new float[m] { 1, 2, 3 };
            float[] y = new float[n] { 4, 5 };

            // column-major
            // [1]
            // [2] x [4, 5]
            // [3]
            float[] a = new float[m * n];
            Matrix.VxV(MatrixLayout.ColumnMajor, m, n, x, 0, y, 0, a, 0, true);

            float[] expected = new float[m * n]
            {
                (1 * 4),
                (2 * 4),
                (3 * 4),
                (1 * 5),
                (2 * 5),
                (3 * 5)
            };
            GenixAssert.AreArraysEqual(expected, a);

            // row-major
            // [1]
            // [2] x [4, 5]
            // [3]
            a = new float[m * n];
            Matrix.VxV(MatrixLayout.RowMajor, m, n, x, 0, y, 0, a, 0, true);

            expected = new float[m * n]
            {
                (1 * 4),
                (1 * 5),
                (2 * 4),
                (2 * 5),
                (3 * 4),
                (3 * 5)
            };
            GenixAssert.AreArraysEqual(expected, a);
        }

        [TestMethod]
        public void MxVTest()
        {
            const int m = 3;
            const int n = 2;

            float[] a = new float[m * n] { 1, 2, 3, 4, 5, 6 };

            // column-major, no transpose
            // [1, 4]
            // [2, 5] x [7]
            // [3, 6]   [8]
            float[] x = new float[n] { 7, 8 };
            float[] y = new float[m];
            Matrix.MxV(MatrixLayout.ColumnMajor, m, n, a, 0, false, x, 0, y, 0, true);

            float[] expected = new float[m]
            {
                (1 * 7) + (4 * 8),
                (2 * 7) + (5 * 8),
                (3 * 7) + (6 * 8)
            };
            GenixAssert.AreArraysEqual(expected, y);

            // row-major, no transpose
            // [1, 2]
            // [3, 4] x [7]
            // [5, 6]   [8]
            Matrix.MxV(MatrixLayout.RowMajor, m, n, a, 0, false, x, 0, y, 0, true);

            expected = new float[m]
            {
                (1 * 7) + (2 * 8),
                (3 * 7) + (4 * 8),
                (5 * 7) + (6 * 8)
            };
            GenixAssert.AreArraysEqual(expected, y);

            // column-major, transpose
            // [1, 4]'   [7]
            // [2, 5]  x [8]
            // [3, 6]    [9]
            x = new float[m] { 7, 8, 9 };
            y = new float[n];
            Matrix.MxV(MatrixLayout.ColumnMajor, m, n, a, 0, true, x, 0, y, 0, true);

            expected = new float[n]
            {
                (1 * 7) + (2 * 8) + (3 * 9),
                (4 * 7) + (5 * 8) + (6 * 9)
            };
            GenixAssert.AreArraysEqual(expected, y);

            // row-major, transpose
            // [1, 2]'   [7]
            // [3, 4]  x [8]
            // [5, 6]    [9]
            Matrix.MxV(MatrixLayout.RowMajor, m, n, a, 0, true, x, 0, y, 0, true);

            expected = new float[n]
            {
                (1 * 7) + (3 * 8) + (5 * 9),
                (2 * 7) + (4 * 8) + (6 * 9)
            };
            GenixAssert.AreArraysEqual(expected, y);
        }

        [TestMethod]
        public void MxMTest()
        {
            const int m = 3;
            const int k = 2;
            const int n = 4;

            float[] a = new float[m * k] { 1, 2, 3, 4, 5, 6 };
            float[] b = new float[k * n] { 1, 2, 3, 4, 5, 6, 7, 8 };
            float[] c = new float[m * n];

            // column-major, no transpose
            // [1, 4]
            // [2, 5] x [1, 3, 5, 7]
            // [3, 6]   [2, 4, 6, 8]
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a, 0, false, b, 0, false, c, 0, true);

            float[] expected = new float[m * n]
            {
                (1 * 1) + (4 * 2),
                (2 * 1) + (5 * 2),
                (3 * 1) + (6 * 2),
                (1 * 3) + (4 * 4),
                (2 * 3) + (5 * 4),
                (3 * 3) + (6 * 4),
                (1 * 5) + (4 * 6),
                (2 * 5) + (5 * 6),
                (3 * 5) + (6 * 6),
                (1 * 7) + (4 * 8),
                (2 * 7) + (5 * 8),
                (3 * 7) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // column-major, transpose A
            // [1, 3, 5]' x [1, 3, 5, 7]
            // [2, 4, 6]    [2, 4, 6, 8]
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a, 0, true, b, 0, false, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (2 * 2),
                (3 * 1) + (4 * 2),
                (5 * 1) + (6 * 2),
                (1 * 3) + (2 * 4),
                (3 * 3) + (4 * 4),
                (5 * 3) + (6 * 4),
                (1 * 5) + (2 * 6),
                (3 * 5) + (4 * 6),
                (5 * 5) + (6 * 6),
                (1 * 7) + (2 * 8),
                (3 * 7) + (4 * 8),
                (5 * 7) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // column-major, transpose B
            // [1, 4]   [1, 5]'
            // [2, 5] x [2, 6]
            // [3, 6]   [3, 7]
            //          [4, 8]
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a, 0, false, b, 0, true, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (4 * 5),
                (2 * 1) + (5 * 5),
                (3 * 1) + (6 * 5),
                (1 * 2) + (4 * 6),
                (2 * 2) + (5 * 6),
                (3 * 2) + (6 * 6),
                (1 * 3) + (4 * 7),
                (2 * 3) + (5 * 7),
                (3 * 3) + (6 * 7),
                (1 * 4) + (4 * 8),
                (2 * 4) + (5 * 8),
                (3 * 4) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // column-major, transpose A and B
            // [1, 3, 5]'   [1, 5]'
            // [2, 4, 6]  x [2, 6]
            //              [3, 7]
            //              [4, 8]
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a, 0, true, b, 0, true, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (2 * 5),
                (3 * 1) + (4 * 5),
                (5 * 1) + (6 * 5),
                (1 * 2) + (2 * 6),
                (3 * 2) + (4 * 6),
                (5 * 2) + (6 * 6),
                (1 * 3) + (2 * 7),
                (3 * 3) + (4 * 7),
                (5 * 3) + (6 * 7),
                (1 * 4) + (2 * 8),
                (3 * 4) + (4 * 8),
                (5 * 4) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // row-major, no transpose
            // [1, 2]
            // [3, 4] x [1, 2, 3, 4]
            // [5, 6]   [5, 6, 7, 8]
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a, 0, false, b, 0, false, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (2 * 5),
                (1 * 2) + (2 * 6),
                (1 * 3) + (2 * 7),
                (1 * 4) + (2 * 8),
                (3 * 1) + (4 * 5),
                (3 * 2) + (4 * 6),
                (3 * 3) + (4 * 7),
                (3 * 4) + (4 * 8),
                (5 * 1) + (6 * 5),
                (5 * 2) + (6 * 6),
                (5 * 3) + (6 * 7),
                (5 * 4) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // row-major, transpose A
            // [1, 2, 3]' x [1, 2, 3, 4]
            // [4, 5, 6]    [5, 6, 7, 8]
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a, 0, true, b, 0, false, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (4 * 5),
                (1 * 2) + (4 * 6),
                (1 * 3) + (4 * 7),
                (1 * 4) + (4 * 8),
                (2 * 1) + (5 * 5),
                (2 * 2) + (5 * 6),
                (2 * 3) + (5 * 7),
                (2 * 4) + (5 * 8),
                (3 * 1) + (6 * 5),
                (3 * 2) + (6 * 6),
                (3 * 3) + (6 * 7),
                (3 * 4) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // row-major, transpose B
            // [1, 2]   [1, 2]'
            // [3, 4] x [3, 4]
            // [5, 6]   [5, 6]
            //          [7, 8]
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a, 0, false, b, 0, true, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (2 * 2),
                (1 * 3) + (2 * 4),
                (1 * 5) + (2 * 6),
                (1 * 7) + (2 * 8),
                (3 * 1) + (4 * 2),
                (3 * 3) + (4 * 4),
                (3 * 5) + (4 * 6),
                (3 * 7) + (4 * 8),
                (5 * 1) + (6 * 2),
                (5 * 3) + (6 * 4),
                (5 * 5) + (6 * 6),
                (5 * 7) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);

            // row-major, transpose A and B
            // [1, 2, 3]'   [1, 2]'
            // [4, 5, 6]  x [3, 4]
            //              [5, 6]
            //              [7, 8]
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a, 0, true, b, 0, true, c, 0, true);

            expected = new float[m * n]
            {
                (1 * 1) + (4 * 2),
                (1 * 3) + (4 * 4),
                (1 * 5) + (4 * 6),
                (1 * 7) + (4 * 8),
                (2 * 1) + (5 * 2),
                (2 * 3) + (5 * 4),
                (2 * 5) + (5 * 6),
                (2 * 7) + (5 * 8),
                (3 * 1) + (6 * 2),
                (3 * 3) + (6 * 4),
                (3 * 5) + (6 * 6),
                (3 * 7) + (6 * 8),
            };
            GenixAssert.AreArraysEqual(expected, c);
        }
    }
}
