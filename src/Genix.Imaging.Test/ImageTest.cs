namespace Genix.Imaging.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Imaging;

    [TestClass]
    public class ImageTest
    {
        /// <summary>
        /// Calculates intensity of binary image (sum of black pixels).
        /// </summary>
        [TestMethod]
        public void PowerTest1()
        {
            Image image = new Image((64 * 2) + 23, 43, 1, 200, 200);
            image.SetWhiteIP();

            image.SetPixel(5, 9, 1);
            image.SetPixel(64 + 37, 19, 1);
            image.SetPixel((2 * 64) + 11, 29, 1);

            Assert.AreEqual(3, image.Power());
            Assert.AreEqual(2, image.Power(5, 9, 134, 20));
            Assert.AreEqual(3, image.Power(5, 9, 135, 21));
        }

        /// <summary>
        /// Calculates intensity of binary image (sum of pixel values).
        /// </summary>
        [TestMethod]
        public void PowerTest2()
        {
            Image image = new Image((64 * 2) + 23, 43, 8, 200, 200);

            image.SetPixel(5, 9, 255);
            image.SetPixel(64 + 37, 19, 128);
            image.SetPixel((2 * 64) + 11, 29, 64);

            Assert.AreEqual(255 + 128 + 64, image.Power());
            Assert.AreEqual(255 + 128, image.Power(5, 9, 134, 20));
            Assert.AreEqual(255 + 128 + 64, image.Power(5, 9, 135, 21));
        }

        [TestMethod]
        public void BlackAreaTest()
        {
            Image image = new Image(10, 20, 1, 200, 200);
            image.SetPixel(5, 9, 1);
            image.SetPixel(8, 15, 1);

            System.Drawing.Rectangle blackArea = image.BlackArea();
            Assert.AreEqual(new System.Drawing.Rectangle(5, 9, 4, 7), blackArea);
        }

        [TestMethod]
        public void CropTest()
        {
            Image image = new Image(20, 35, 1, 200, 200);
            image.SetPixel(1, 1, 1);
            image.SetPixel(18, 33, 1);

            image = image.CropBlackArea(0, 0);

            Assert.AreEqual(new System.Drawing.Rectangle(0, 0, 18, 33), image.Bounds);
            Assert.AreEqual(1u, image.GetPixel(0, 0));
            Assert.AreEqual(1u, image.GetPixel(17, 32));
            Assert.AreEqual(2, image.Power());
        }
    }
}
