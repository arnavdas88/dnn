namespace Genix.Imaging.Test
{
    using System;
    using Genix.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EditTest
    {
        [TestMethod]
        public void GetPixelSetPixelTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                uint whiteColor = bitsPerPixel == 1 ? 0u : (uint)~(ulong.MaxValue << bitsPerPixel);
                uint blackColor = bitsPerPixel == 1 ? 1u : 0u;

                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.SetBlackIP();

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        image.SetPixel(x, y, whiteColor);
                        Assert.AreEqual(whiteColor, image.GetPixel(x, y));
                    }
                }

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        image.SetPixel(x, y, blackColor);
                        Assert.AreEqual(blackColor, image.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void SetWhiteTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                uint whiteColor = bitsPerPixel == 1 ? 0u : (uint)(ulong.MaxValue >> (64 - bitsPerPixel));

                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image whiteImage = image.SetWhite();

                for (int x = 0; x < whiteImage.Width; x++)
                {
                    for (int y = 0; y < whiteImage.Height; y++)
                    {
                        Assert.AreEqual(whiteColor, whiteImage.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void SetWhiteTest2()
        {
            Rectangle[] areas = new Rectangle[]
            {
                new Rectangle(5, 12, (32 * 0) + 17, 20),
                new Rectangle(5, 12, (32 * 1) + 17, 20),
                new Rectangle(5, 12, (32 * 2) + 17, 20),
            };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    uint whiteColor = bitsPerPixel == 1 ? 0u : (uint)(ulong.MaxValue >> (64 - bitsPerPixel));

                    Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image whiteImage = image.SetWhite(area);

                    for (int x = 0; x < whiteImage.Width; x++)
                    {
                        for (int y = 0; y < whiteImage.Height; y++)
                        {
                            uint color = area.Contains(x, y) ? whiteColor : image.GetPixel(x, y);
                            Assert.AreEqual(color, whiteImage.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetBlackTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                uint blackColor = bitsPerPixel == 1 ? 1u : 0u;

                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image blackImage = image.SetBlack();

                for (int x = 0; x < blackImage.Width; x++)
                {
                    for (int y = 0; y < blackImage.Height; y++)
                    {
                        Assert.AreEqual(blackColor, blackImage.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void SetBlackTest2()
        {
            Rectangle[] areas = new Rectangle[]
            {
                new Rectangle(5, 12, (32 * 0) + 17, 20),
                new Rectangle(5, 12, (32 * 1) + 17, 20),
                new Rectangle(5, 12, (32 * 2) + 17, 20),
            };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    uint blackColor = bitsPerPixel == 1 ? 1u : 0u;

                    Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image blackImage = image.SetBlack(area);

                    for (int x = 0; x < blackImage.Width; x++)
                    {
                        for (int y = 0; y < blackImage.Height; y++)
                        {
                            uint color = area.Contains(x, y) ? blackColor : image.GetPixel(x, y);
                            Assert.AreEqual(color, blackImage.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void InvertTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                uint maxColor = (uint)(ulong.MaxValue >> (64 - bitsPerPixel));

                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image invertedImage = image.NOT();

                for (int x = 0; x < invertedImage.Width; x++)
                {
                    for (int y = 0; y < invertedImage.Height; y++)
                    {
                        Assert.AreEqual(maxColor - image.GetPixel(x, y), invertedImage.GetPixel(x, y));
                    }
                }
            }
        }
    }
}
