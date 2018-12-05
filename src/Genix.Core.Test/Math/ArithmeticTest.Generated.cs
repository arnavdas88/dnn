﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 11/29/2018 12:33:40 AM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Core.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ArithmeticTest_float
    {
        private readonly RandomNumberGenerator<float> random = new RandomGeneratorF();

        [TestMethod]
        public void AddTest_float()
        {
            const int offx = 5;
            const int offy0 = 8;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                float[] x = this.random.Generate(length, null);
                float[] y0 = this.random.Generate(length, null);

                float[] y = y0.ToArray();
                Mathematics.Add(length, x, 0, y, 0);
                GenixAssert.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                int count = length - Math.Max(offx, offy);
                Mathematics.Add(count, x, offx, y, offy);
                GenixAssert.AreArraysEqual(offy, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy), (a, b) => a + b).ToArray(), 0, y, offy);

                y = y0.ToArray();
                Mathematics.Add(length, x, 0, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                count = length - Math.Max(offx, Math.Max(offy, offy0));
                Mathematics.Add(count, x, offx, y0, offy0, y, offy);
                GenixAssert.AreArraysEqual(offy, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy0), (a, b) => a + b).ToArray(), 0, y, offy);
            }
        }

        [TestMethod]
        public void AddCTest_float()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                Vectors.Set(length, 0, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == 1));

                Vectors.Set(length, float.NegativeInfinity, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                Vectors.Set(length, float.PositiveInfinity, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                Vectors.Set(length, float.MinValue, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                Vectors.Set(length, float.MaxValue, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                Vectors.Set(length, float.NaN, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }

        [TestMethod]
        public void SubCTest_float()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                Vectors.Set(length, 0, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == -1));

                Vectors.Set(length, float.NegativeInfinity, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                Vectors.Set(length, float.PositiveInfinity, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                Vectors.Set(length, float.MinValue, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                Vectors.Set(length, float.MaxValue, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                Vectors.Set(length, float.NaN, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }
    }

    [TestClass]
    public class ArithmeticTest_double
    {
        private readonly RandomNumberGenerator<double> random = new RandomGeneratorD();

        [TestMethod]
        public void AddTest_double()
        {
            const int offx = 5;
            const int offy0 = 8;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                double[] x = this.random.Generate(length, null);
                double[] y0 = this.random.Generate(length, null);

                double[] y = y0.ToArray();
                Mathematics.Add(length, x, 0, y, 0);
                GenixAssert.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                int count = length - Math.Max(offx, offy);
                Mathematics.Add(count, x, offx, y, offy);
                GenixAssert.AreArraysEqual(offy, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy), (a, b) => a + b).ToArray(), 0, y, offy);

                y = y0.ToArray();
                Mathematics.Add(length, x, 0, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                count = length - Math.Max(offx, Math.Max(offy, offy0));
                Mathematics.Add(count, x, offx, y0, offy0, y, offy);
                GenixAssert.AreArraysEqual(offy, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy0), (a, b) => a + b).ToArray(), 0, y, offy);
            }
        }

        [TestMethod]
        public void AddCTest_double()
        {
            foreach (int length in new[] { 24, 128 })
            {
                double[] a = new double[length];
                double[] y = new double[length];

                Vectors.Set(length, 0, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == 1));

                Vectors.Set(length, double.NegativeInfinity, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => double.IsNegativeInfinity(x)));

                Vectors.Set(length, double.PositiveInfinity, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => double.IsPositiveInfinity(x)));

                Vectors.Set(length, double.MinValue, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == double.MinValue));

                Vectors.Set(length, double.MaxValue, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == double.MaxValue));

                Vectors.Set(length, double.NaN, a, 0);
                Mathematics.AddC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => double.IsNaN(x)));
            }
        }

        [TestMethod]
        public void SubCTest_double()
        {
            foreach (int length in new[] { 24, 128 })
            {
                double[] a = new double[length];
                double[] y = new double[length];

                Vectors.Set(length, 0, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == -1));

                Vectors.Set(length, double.NegativeInfinity, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => double.IsNegativeInfinity(x)));

                Vectors.Set(length, double.PositiveInfinity, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => double.IsPositiveInfinity(x)));

                Vectors.Set(length, double.MinValue, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == double.MinValue));

                Vectors.Set(length, double.MaxValue, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => x == double.MaxValue));

                Vectors.Set(length, double.NaN, a, 0);
                Mathematics.SubC(length, a, 0, 1, y, 0);
                Assert.IsTrue(y.All(x => double.IsNaN(x)));
            }
        }
    }
}