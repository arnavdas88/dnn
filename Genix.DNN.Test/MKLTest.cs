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
        [TestMethod]
        public void DotProductTest()
        {
            float res = MKL.DotProduct(3, new float[] { 1, 2, 3, 4 }, 0, 1, new float[] { 5, 6, 7, 8 }, 1, 1);
            Assert.AreEqual((1 * 6) + (2 * 7) + (3 * 8), res);
        }
    }
}
