namespace Genix.Core.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public partial class VectorsTest
    {
        private readonly RandomNumberGenerator<float> random = new RandomGeneratorF();

        [TestMethod]
        public void AddTest()
        {
            const int offx = 5;
            const int offy0 = 8;
            const int offy = 10;

            foreach (int length in new[] { 24, 128 })
            {
                float[] x = this.random.Generate(length, null);
                float[] y0 = this.random.Generate(length, null);

                float[] y = y0.ToArray();
                Vectors.Add(length, x, 0, y, 0);
                GenixAssert.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                int count = length - Math.Max(offx, offy);
                Vectors.Add(count, x, offx, y, offy);
                GenixAssert.AreArraysEqual(offy, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy), (a, b) => a + b).ToArray(), 0, y, offy);

                y = y0.ToArray();
                Vectors.Add(length, x, 0, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(x.Zip(y0, (a, b) => a + b).ToArray(), y);

                y = y0.ToArray();
                count = length - Math.Max(offx, Math.Max(offy, offy0));
                Vectors.Add(count, x, offx, y0, offy0, y, offy);
                GenixAssert.AreArraysEqual(offy, y0, 0, y, 0);
                GenixAssert.AreArraysEqual(count, x.Skip(offx).Zip(y0.Skip(offy0), (a, b) => a + b).ToArray(), 0, y, offy);
            }
        }

        [TestMethod]
        public void AddCTest()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                Vectors.Set(length, 0.0f, a, 0);
                Vectors.AddC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == 1.0f));

                Vectors.Set(length, float.NegativeInfinity, a, 0);
                Vectors.AddC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                Vectors.Set(length, float.PositiveInfinity, a, 0);
                Vectors.AddC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                Vectors.Set(length, float.MinValue, a, 0);
                Vectors.AddC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                Vectors.Set(length, float.MaxValue, a, 0);
                Vectors.AddC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                Vectors.Set(length, float.NaN, a, 0);
                Vectors.AddC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }

        [TestMethod]
        public void SubCTest()
        {
            foreach (int length in new[] { 24, 128 })
            {
                float[] a = new float[length];
                float[] y = new float[length];

                Vectors.Set(length, 0.0f, a, 0);
                Vectors.SubC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == -1.0f));

                Vectors.Set(length, float.NegativeInfinity, a, 0);
                Vectors.SubC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNegativeInfinity(x)));

                Vectors.Set(length, float.PositiveInfinity, a, 0);
                Vectors.SubC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsPositiveInfinity(x)));

                Vectors.Set(length, float.MinValue, a, 0);
                Vectors.SubC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MinValue));

                Vectors.Set(length, float.MaxValue, a, 0);
                Vectors.SubC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => x == float.MaxValue));

                Vectors.Set(length, float.NaN, a, 0);
                Vectors.SubC(length, a, 0, 1.0f, y, 0);
                Assert.IsTrue(y.All(x => float.IsNaN(x)));
            }
        }

        [TestMethod]
        public void ClipTest()
        {
            float[] oldValues = new float[] { -10.0f, 10.0f, -10.0f, 0.0f, 10.0f };

            float[] values = oldValues.ToArray();
            Vectors.Clip(values.Length - 2, -2.0f, 2.0f, values, 2);
            CollectionAssert.AreEqual(
                new float[] { -10.0f, 10.0f, -2.0f, 0.0f, 2.0f },
                values);

            values = oldValues.ToArray();
            Vectors.Clip(values.Length - 2, float.NaN, 2.0f, values, 2);
            CollectionAssert.AreEqual(
                new float[] { -10.0f, 10.0f, -10.0f, 0.0f, 2.0f },
                values);

            values = oldValues.ToArray();
            Vectors.Clip(values.Length - 2, -2.0f, float.NaN, values, 2);
            CollectionAssert.AreEqual(
                new float[] { -10.0f, 10.0f, -2.0f, 0.0f, 10.0f },
                values);
        }

        [TestMethod]
        public void ReplaceTest()
        {
            float[] oldValues = new float[]
            {
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, float.MinValue, float.MaxValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity,
            };
            float[] newValues = new float[oldValues.Length - 3];

            Arrays.Replace(6, oldValues, 6, 1.0f, 2.0f, newValues, 3);
            Console.WriteLine(newValues[6]);
            CollectionAssert.AreEqual(
                new float[]
                {
                    2.0f, float.MinValue, float.MaxValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity,
                },
                newValues.Skip(3).ToArray(),
                "value");

            Arrays.Replace(6, oldValues, 6, float.MinValue, 2.0f, newValues, 3);
            CollectionAssert.AreEqual(
                new float[]
                {
                    1.0f, 2.0f, float.MaxValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity,
                },
                newValues.Skip(3).ToArray(),
                "MinValue");

            Arrays.Replace(6, oldValues, 6, float.MaxValue, 2.0f, newValues, 3);
            CollectionAssert.AreEqual(
                new float[]
                {
                    1.0f, float.MinValue, 2.0f, float.NaN, float.NegativeInfinity, float.PositiveInfinity,
                },
                newValues.Skip(3).ToArray(),
                "MaxValue");

            Arrays.Replace(6, oldValues, 6, float.NaN, 2.0f, newValues, 3);
            CollectionAssert.AreEqual(
                new float[]
                {
                    1.0f, float.MinValue, float.MaxValue, 2.0f, float.NegativeInfinity, float.PositiveInfinity,
                },
                newValues.Skip(3).ToArray(),
                "NaN");

            Arrays.Replace(6, oldValues, 6, float.NegativeInfinity, 2.0f, newValues, 3);
            CollectionAssert.AreEqual(
                new float[]
                {
                    1.0f, float.MinValue, float.MaxValue, float.NaN, 2.0f, float.PositiveInfinity,
                },
                newValues.Skip(3).ToArray(),
                "NegativeInfinity");

            Arrays.Replace(6, oldValues, 6, float.PositiveInfinity, 2.0f, newValues, 3);
            CollectionAssert.AreEqual(
                new float[]
                {
                    1.0f, float.MinValue, float.MaxValue, float.NaN, float.NegativeInfinity, 2.0f,
                },
                newValues.Skip(3).ToArray(),
                "PositiveInfinity");
        }

        [TestMethod]
        public void SwapTest()
        {
            const int Length = 64 + 47;
            Random random = new Random(0);

            // initialize data
            int[] array1 = new int[Length];
            int[] array2 = new int[Length];
            for (int i = 0; i < Length; i++)
            {
                array1[i] = random.Next();
                array2[i] = random.Next();
            }

            // create test set
            int[] workarray1 = array1.ToArray();
            int[] workarray2 = array2.ToArray();

            // test
            for (int i = 0; i < Length; i++)
            {
                Assert.AreEqual(array1[i], workarray1[i]);
                Assert.AreEqual(array2[i], workarray2[i]);
            }

            Vectors.Swap(Length, workarray1, 0, workarray2, 0);
            for (int i = 0; i < Length; i++)
            {
                Assert.AreEqual(array2[i], workarray1[i]);
                Assert.AreEqual(array1[i], workarray2[i]);
            }

            Assert.IsTrue(Vectors.Equals(Length, workarray1, 0, array2, 0));
            Assert.IsTrue(Vectors.Equals(Length, workarray2, 0, array1, 0));
        }
    }
}
