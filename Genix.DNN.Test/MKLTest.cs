namespace Genix.DNN.Test
{
    using System;
    using System.Linq;
    using Accord.DNN;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MKLTest
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
                MKL.Add(length, x, 0, y, 0);
                Helpers.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                int count = length - Math.Max(offx, offy);
                MKL.Add(count, x, offx, y, offy);
                Helpers.AreArraysEqual(offy, y0, 0, y, 0);
                Helpers.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy), (a, b) => a + b).ToArray(), 0, y, offy);

                y = y0.ToArray();
                MKL.Add(length, x, 0, y0, 0, y, 0);
                Helpers.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                count = length - Math.Max(offx, Math.Max(offy, offy0));
                MKL.Add(count, x, offx, y0, offy0, y, offy);
                Helpers.AreArraysEqual(offy, y0, 0, y, 0);
                Helpers.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy0), (a, b) => a + b).ToArray(), 0, y, offy);
            }
        }

        [TestMethod]
        public void AddScalarTest()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                SetCopy.Set(length, 0.0f, a, 0);
                MKL.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == 1.0f));

                SetCopy.Set(length, float.NegativeInfinity, a, 0);
                MKL.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                SetCopy.Set(length, float.PositiveInfinity, a, 0);
                MKL.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                SetCopy.Set(length, float.MinValue, a, 0);
                MKL.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                SetCopy.Set(length, float.MaxValue, a, 0);
                MKL.Add(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                SetCopy.Set(length, float.NaN, a, 0);
                MKL.Add(length, a, 0, 1.0f, y, 0);
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

                SetCopy.Set(length, 0.0f, a, 0);
                MKL.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == -1.0f));

                SetCopy.Set(length, float.NegativeInfinity, a, 0);
                MKL.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                SetCopy.Set(length, float.PositiveInfinity, a, 0);
                MKL.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                SetCopy.Set(length, float.MinValue, a, 0);
                MKL.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                SetCopy.Set(length, float.MaxValue, a, 0);
                MKL.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                SetCopy.Set(length, float.NaN, a, 0);
                MKL.Subtract(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }

        [TestMethod]
        public void DotProductTest()
        {
            float res = MKL.DotProduct(3, new float[] { 1, 2, 3, 4 }, 0, 1, new float[] { 5, 6, 7, 8 }, 1, 1);
            Assert.AreEqual((1 * 6) + (2 * 7) + (3 * 8), res);
        }
    }
}
