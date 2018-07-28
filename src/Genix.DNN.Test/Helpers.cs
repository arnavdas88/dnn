namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class Helpers
    {
        /// <summary>
        /// Creates a class using private constructor.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <returns>The object this method creates.</returns>
        public static T CreateClassWithPrivateConstructor<T>()
            where T : class
        {
            var constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            return (T)constructor.Invoke(null);
        }

        public static void AreTensorsEqual(Tensor expected, Tensor actual)
        {
            CollectionAssert.AreEqual(expected.Axes, actual.Axes);

            Assert.AreEqual(expected.Length, actual.Length);

            Helpers.AreArraysEqual(expected.Weights, actual.Weights);
        }

        public static void AreGradientsEqual(Tensor expected, Tensor actual)
        {
            CollectionAssert.AreEqual(expected.Axes, actual.Axes);

            Assert.AreEqual(expected.Length, actual.Length);

            Helpers.AreArraysEqual(expected.Gradient, actual.Gradient);
        }

        public static void AreArraysEqual(float[] expected, float[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0, ii = expected.Length; i < ii; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-5f, i.ToString(CultureInfo.InvariantCulture));
            }
        }

        /*public static void AreArraysEqual(float[] expected, float[] actual, Func<float, float, Tuple<float, float>> selector)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0, ii = expected.Length; i < ii; i++)
            {
                Tuple<float, float> values = selector(expected[i], actual[i]);
                Assert.AreEqual(values.Item1, values.Item2, 1e-6f);
            }
        }*/

        public static void AreArraysEqual(int length, float[] expected, int expectedIndex, float[] actual, int actualIndex)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected[expectedIndex + i], actual[actualIndex + i], 1e-6f);
            }
        }

        /*public static void AreArraysEqual(int length, float[] expected, int expectedIndex, int expectedIncrement, float[] actual, int actualIndex, int actualIncrement)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected[expectedIndex + (expectedIncrement * i)], actual[actualIndex + (actualIncrement * i)], 1e-6f);
            }
        }*/

        public static void IsArrayEqual(int length, float expected, float[] actual, int actualIndex)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected, actual[actualIndex + i], 1e-6f);
            }
        }
    }
}
