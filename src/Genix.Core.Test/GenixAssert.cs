namespace Genix.Core.Test
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class GenixAssert
    {
        public static void AreArraysEqual(float[] expected, float[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0, ii = expected.Length; i < ii; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-5f, i.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static void AreArraysEqual(int length, float[] expected, int expectedIndex, float[] actual, int actualIndex)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected[expectedIndex + i], actual[actualIndex + i], 1e-6f);
            }
        }
    }
}
