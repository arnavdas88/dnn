namespace Genix.Imaging.Test
{
    using System;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IntegralImageTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void FromImageTest()
        {
            foreach (int bitsPerPixel in new[] { 8/*, 16*/ })
            {
                Image src = new Image(23, 20, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                IntegralImage dst = IntegralImage.FromImage(src);
                Assert.AreEqual(src.Width + 1, dst.Width);
                Assert.AreEqual(src.Height + 1, dst.Height);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        int actual = dst[x + 1, y + 1];

                        int expected = 0;
                        for (int ix = 0; ix <= x; ix++)
                        {
                            for (int iy = 0; iy <= y; iy++)
                            {
                                expected += (int)src.GetPixel(ix, iy);
                            }
                        }

                        Assert.AreEqual(expected, actual);
                    }
                }
            }
        }
    }
}
