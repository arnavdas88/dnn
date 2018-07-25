namespace Genix.Imaging.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConvertTest
    {
        [TestMethod]
        public void Convert1to8Test()
        {
            // image is:
            // 1 0 0 0 0 1 0 0   0 1
            Image image = new Image(10, 1, 1, 200, 200);
            image.SetPixel(0, 0, 1);
            image.SetPixel(5, 0, 1);
            image.SetPixel(9, 0, 1);

            Image result = image.Convert1To8(255, 0);
            result.ToBitmap();
            Assert.AreEqual(0u, result.GetPixel(0, 0));
            Assert.AreEqual(255u, result.GetPixel(1, 0));
            Assert.AreEqual(255u, result.GetPixel(2, 0));
            Assert.AreEqual(255u, result.GetPixel(3, 0));
            Assert.AreEqual(255u, result.GetPixel(4, 0));
            Assert.AreEqual(0u, result.GetPixel(5, 0));
            Assert.AreEqual(255u, result.GetPixel(6, 0));
            Assert.AreEqual(255u, result.GetPixel(7, 0));
            Assert.AreEqual(255u, result.GetPixel(8, 0));
            Assert.AreEqual(0u, result.GetPixel(9, 0));
        }

        [TestMethod]
        public void Convert8to1Test()
        {
            // image is:
            // 1 0 0 0 0 1 0 0   0 1
            Image image = new Image(10, 1, 8, 200, 200).SetWhite();
            image.SetPixel(0, 0, 0);
            image.SetPixel(5, 0, 0);
            image.SetPixel(9, 0, 0);

            Image result = image.Convert8To1(128);
            result.ToBitmap();
            Assert.AreEqual(1u, result.GetPixel(0, 0));
            Assert.AreEqual(0u, result.GetPixel(1, 0));
            Assert.AreEqual(0u, result.GetPixel(2, 0));
            Assert.AreEqual(0u, result.GetPixel(3, 0));
            Assert.AreEqual(0u, result.GetPixel(4, 0));
            Assert.AreEqual(1u, result.GetPixel(5, 0));
            Assert.AreEqual(0u, result.GetPixel(6, 0));
            Assert.AreEqual(0u, result.GetPixel(7, 0));
            Assert.AreEqual(0u, result.GetPixel(8, 0));
            Assert.AreEqual(1u, result.GetPixel(9, 0));
        }
    }
}
