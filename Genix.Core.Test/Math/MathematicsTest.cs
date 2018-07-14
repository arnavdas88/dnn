﻿namespace Genix.Core.Test.Math
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MathematicsTest
    {
        private readonly RandomNumberGenerator random = new RandomGenerator();

        [TestMethod]
        public void AddTest()
        {
            const int offx = 5;
            const int offy0 = 8;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                float[] x = random.Generate(length, null);
                float[] y0 = random.Generate(length, null);

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
        public void AddScalarTest()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                Arrays.Set(length, 0.0f, a, 0);
                Mathematics.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == 1.0f));

                Arrays.Set(length, float.NegativeInfinity, a, 0);
                Mathematics.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                Arrays.Set(length, float.PositiveInfinity, a, 0);
                Mathematics.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                Arrays.Set(length, float.MinValue, a, 0);
                Mathematics.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                Arrays.Set(length, float.MaxValue, a, 0);
                Mathematics.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                Arrays.Set(length, float.NaN, a, 0);
                Mathematics.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }

        [TestMethod]
        public void SubtractScalarTest()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                Arrays.Set(length, 0.0f, a, 0);
                Mathematics.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == -1.0f));

                Arrays.Set(length, float.NegativeInfinity, a, 0);
                Mathematics.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                Arrays.Set(length, float.PositiveInfinity, a, 0);
                Mathematics.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                Arrays.Set(length, float.MinValue, a, 0);
                Mathematics.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                Arrays.Set(length, float.MaxValue, a, 0);
                Mathematics.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                Arrays.Set(length, float.NaN, a, 0);
                Mathematics.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }
    }
}