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
    }
}
