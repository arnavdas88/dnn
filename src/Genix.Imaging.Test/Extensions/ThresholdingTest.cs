namespace Genix.Imaging.Test.Extensions
{
    using System;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ThresholdingTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void ThresholdLTTest()
        {
            foreach (int bitsPerPixel in new[] { 8/*, 24, 32*/ })
            {
                Image src = new Image(231, 50, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                uint threshold = Color.Divide(Color.FromArgb(src.MaxColor), Color.FromArgb(3, 3, 3), 1, MidpointRounding.AwayFromZero).Argb;
                uint value = src.MaxColor;
                Image dst = src.ThresholdLT(null, threshold, value);

                Assert.IsTrue(dst.Min() >= threshold);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        uint srccolor = src.GetPixel(x, y);
                        uint dstcolor = dst.GetPixel(x, y);
                        Assert.AreEqual(srccolor < threshold ? value : srccolor, dstcolor);
                    }
                }
            }
        }
    }
}
