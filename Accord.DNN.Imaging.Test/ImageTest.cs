namespace Accord.DNN.Imaging.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Imaging;

    [TestClass]
    public class ImageTest
    {
        [TestMethod]
        public void PowerTest()
        {
            ////foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 32 })
            {
                Image image = new Image((64 * 2) + 23, 43, 1, 200, 200);
                image.SetWhite();
                image.SetPixel(5, 9, 1);
                image.SetPixel(64 + 37, 19, 1);
                image.SetPixel((2 * 64) + 11, 29, 1);

                Assert.AreEqual(3ul, image.Power());
            }
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
            Assert.AreEqual(2ul, image.Power());
        }
    }
}
