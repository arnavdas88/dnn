namespace Genix.Imaging.Test.Extensions
{
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransformTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void Rotate90270Test()
        {
            foreach (int bitsPerPixel in new[] { 1, /*2, 4,*/ 8, 24, 32 })
            {
                Image src = new Image(231, 51, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Image dst = src.Rotate90(null);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(x, y), dst.GetPixel(y, src.Width - 1 - x));
                    }
                }

                dst = dst.Rotate270(null);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(x, y), dst.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void FlipTest_XAxis()
        {
            foreach (int bitsPerPixel in new[] { /*1, 2, 4,*/ 8, 24, 32 })
            {
                Image src = new Image(231, 51, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Image dst = src.Flip(null, FlipAxis.X);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(x, src.Height - 1 - y), dst.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void FlipTest_YAxis()
        {
            foreach (int bitsPerPixel in new[] { /*1, 2, 4,*/ 8, 24, 32 })
            {
                Image src = new Image(231, 51, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Image dst = src.Flip(null, FlipAxis.Y);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(src.Width - 1 - x, y), dst.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void FlipTest_XYAxis()
        {
            foreach (int bitsPerPixel in new[] { /*1, 2, 4,*/ 8, 24, 32 })
            {
                Image src = new Image(231, 51, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Image dst = src.Flip(null, FlipAxis.Both);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Assert.AreEqual(src.GetPixel(src.Width - 1 - x, src.Height - 1 - y), dst.GetPixel(x, y));
                    }
                }
            }
        }
    }
}
