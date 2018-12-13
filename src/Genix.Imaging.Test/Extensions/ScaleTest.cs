namespace Genix.Imaging.Test
{
    using System;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScaleTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void ScaleToSizeTest()
        {
            Image image = new Image(118, 133, 1, 200, 200);
            image.SetWhite();

            image.ScaleToSize(image, 113, 124, new ScalingOptions());
        }

        [TestMethod]
        public void ScaleByDownsampling2Test()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image src = new Image(231, 51, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                // not-in-place
                Image dst = src.ScaleByDownsampling2(null);
                Assert.AreEqual(src.Width / 2, dst.Width);
                Assert.AreEqual(src.Height / 2, dst.Height);

                for (int x = 0; x < dst.Width; x++)
                {
                    for (int y = 0; y < dst.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(2 * x, 2 * y), dst.GetPixel(x, y));
                    }
                }

                // in-place
                Image copy = src.Clone(true);
                dst = src.ScaleByDownsampling2(src);
                Assert.AreEqual(copy.Width / 2, dst.Width);
                Assert.AreEqual(copy.Height / 2, dst.Height);

                for (int x = 0; x < dst.Width; x++)
                {
                    for (int y = 0; y < dst.Height; y++)
                    {
                        Assert.AreEqual(copy.GetPixel(2 * x, 2 * y), dst.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void ScaleByDownsampling4Test()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image src = new Image(231, 51, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                // not-in-place
                Image dst = src.ScaleByDownsampling4(null);
                Assert.AreEqual(src.Width / 4, dst.Width);
                Assert.AreEqual(src.Height / 4, dst.Height);

                for (int x = 0; x < dst.Width; x++)
                {
                    for (int y = 0; y < dst.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(4 * x, 4 * y), dst.GetPixel(x, y));
                    }
                }

                // in-place
                Image copy = src.Clone(true);
                dst = src.ScaleByDownsampling4(src);
                Assert.AreEqual(copy.Width / 4, dst.Width);
                Assert.AreEqual(copy.Height / 4, dst.Height);

                for (int x = 0; x < dst.Width; x++)
                {
                    for (int y = 0; y < dst.Height; y++)
                    {
                        Assert.AreEqual(copy.GetPixel(4 * x, 4 * y), dst.GetPixel(x, y));
                    }
                }
            }
        }
    }
}
