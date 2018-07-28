namespace Genix.Core.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ArraysTest
    {
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

            Arrays.Swap(Length, workarray1, 0, workarray2, 0);
            for (int i = 0; i < Length; i++)
            {
                Assert.AreEqual(array2[i], workarray1[i]);
                Assert.AreEqual(array1[i], workarray2[i]);
            }

            Assert.IsTrue(Arrays.Equals(Length, workarray1, 0, array2, 0));
            Assert.IsTrue(Arrays.Equals(Length, workarray2, 0, array1, 0));
        }
    }
}
