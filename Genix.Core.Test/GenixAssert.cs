namespace Genix.Core.Test
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class GenixAssert
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "This method is used for testing arguments.")]
        public static void AreArraysEqual(float[] expected, float[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0, ii = expected.Length; i < ii; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-5f, i.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
