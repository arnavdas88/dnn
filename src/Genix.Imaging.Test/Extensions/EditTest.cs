namespace Genix.Imaging.Test
{
    using System;
    using System.Collections.Generic;
    using Genix.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EditTest
    {
        private readonly Dictionary<int, uint[]> colors = new Dictionary<int, uint[]>()
        {
            { 1, new uint[] { 0, uint.MaxValue } },
            { 2, new uint[] { 0, 1, 2, uint.MaxValue } },
            { 4, new uint[] { 0, 5, 12, uint.MaxValue } },
            { 8, new uint[] { 0, 43, 157, uint.MaxValue } },
            { 16, new uint[] { 0, 1234, 6789, uint.MaxValue } },
            { 24, new uint[] { 0, 0x124578, 0x454545, 0x875421, uint.MaxValue } },
            { 32, new uint[] { 0, 0x124578ab, 0x45454545, 0xba875421, uint.MaxValue } },
        };

        private readonly (int left, int top, int right, int bottom)[] borders = new (int, int, int, int)[]
        {
            (2, 3, 4, 5),
            (0, 3, 4, 5),
            (2, 0, 4, 5),
            (2, 3, 0, 5),
            (2, 3, 4, 0),
            (0, 3, 0, 5),
            (0, 0, 0, 0),
        };

        [TestMethod]
        public void GetPixelSetPixelTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                uint whiteColor = bitsPerPixel == 1 ? 0u : (uint)~(ulong.MaxValue << bitsPerPixel);
                uint blackColor = bitsPerPixel == 1 ? 1u : 0u;

                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.SetBlack();

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
        public void SetToZeroTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image zeroImage = image.Clone(true);
                zeroImage.SetToZero();

                for (int x = 0; x < zeroImage.Width; x++)
                {
                    for (int y = 0; y < zeroImage.Height; y++)
                    {
                        Assert.AreEqual(0u, zeroImage.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void SetToZeroTest2()
        {
            Rectangle[] areas = new Rectangle[]
            {
                new Rectangle(5, 12, (32 * 0) + 17, 20),
                new Rectangle(5, 12, (32 * 1) + 17, 20),
                new Rectangle(5, 12, (32 * 2) + 17, 20),
                new Rectangle(0, 12, (32 * 2) + 23, 20),
            };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image zeroImage = image.Clone(true);
                    zeroImage.SetToZero(area);

                    for (int x = 0; x < zeroImage.Width; x++)
                    {
                        for (int y = 0; y < zeroImage.Height; y++)
                        {
                            uint color = area.Contains(x, y) ? 0u : image.GetPixel(x, y);
                            Assert.AreEqual(color, zeroImage.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetToOneTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image oneImage = image.Clone(true);
                oneImage.SetToOne();

                for (int x = 0; x < oneImage.Width; x++)
                {
                    for (int y = 0; y < oneImage.Height; y++)
                    {
                        Assert.AreEqual(uint.MaxValue & oneImage.MaxColor, oneImage.GetPixel(x, y));
                    }
                }
            }
        }

        [TestMethod]
        public void SetToOneTest2()
        {
            Rectangle[] areas = new Rectangle[]
            {
                new Rectangle(5, 12, (32 * 0) + 17, 20),
                new Rectangle(5, 12, (32 * 1) + 17, 20),
                new Rectangle(5, 12, (32 * 2) + 17, 20),
                new Rectangle(0, 12, (32 * 2) + 23, 20),
            };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image oneImage = image.Clone(true);
                    oneImage.SetToOne(area);

                    for (int x = 0; x < oneImage.Width; x++)
                    {
                        for (int y = 0; y < oneImage.Height; y++)
                        {
                            uint color = area.Contains(x, y) ? (uint.MaxValue & oneImage.MaxColor) : image.GetPixel(x, y);
                            Assert.AreEqual(color, oneImage.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetWhiteTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image whiteImage = image.Clone(true);
                whiteImage.SetWhite();

                for (int x = 0; x < whiteImage.Width; x++)
                {
                    for (int y = 0; y < whiteImage.Height; y++)
                    {
                        Assert.AreEqual(image.WhiteColor, whiteImage.GetPixel(x, y));
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
                new Rectangle(0, 12, (32 * 2) + 23, 20),
            };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image whiteImage = image.Clone(true);
                    whiteImage.SetWhite(area);

                    for (int x = 0; x < whiteImage.Width; x++)
                    {
                        for (int y = 0; y < whiteImage.Height; y++)
                        {
                            uint color = area.Contains(x, y) ? image.WhiteColor : image.GetPixel(x, y);
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
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image blackImage = image.Clone(true);
                blackImage.SetBlack();

                for (int x = 0; x < blackImage.Width; x++)
                {
                    for (int y = 0; y < blackImage.Height; y++)
                    {
                        Assert.AreEqual(image.BlackColor, blackImage.GetPixel(x, y));
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
                new Rectangle(0, 12, (32 * 2) + 23, 20),
           };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image blackImage = image.Clone(true);
                    blackImage.SetBlack(area);

                    for (int x = 0; x < blackImage.Width; x++)
                    {
                        for (int y = 0; y < blackImage.Height; y++)
                        {
                            uint color = area.Contains(x, y) ? image.BlackColor : image.GetPixel(x, y);
                            Assert.AreEqual(color, blackImage.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetColorTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (uint color in this.colors[bitsPerPixel])
                {
                    Image image = new Image((64 * 4) + 23, 43, bitsPerPixel, 200, 200);
                    image.Randomize();

                    Image coloredImage = image.Clone(true);
                    coloredImage.SetColor(color);

                    for (int x = 0; x < coloredImage.Width; x++)
                    {
                        for (int y = 0; y < coloredImage.Height; y++)
                        {
                            Assert.AreEqual(color & image.MaxColor, coloredImage.GetPixel(x, y), $"bpp={bitsPerPixel}");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetColorTest2()
        {
            Rectangle[] areas = new Rectangle[]
            {
                new Rectangle(5, 12, (32 * 0) + 17, 20),
                new Rectangle(5, 12, (32 * 1) + 17, 20),
                new Rectangle(5, 12, (32 * 2) + 17, 20),
                new Rectangle(0, 12, (32 * 2) + 23, 20),
            };

            foreach (Rectangle area in areas)
            {
                foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
                {
                    foreach (uint color in this.colors[bitsPerPixel])
                    {
                        Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                        image.Randomize();

                        Image coloredImage = image.Clone(true);
                        coloredImage.SetColor(area, color);

                        for (int x = 0; x < coloredImage.Width; x++)
                        {
                            for (int y = 0; y < coloredImage.Height; y++)
                            {
                                uint c = area.Contains(x, y) ? color & image.MaxColor : image.GetPixel(x, y);
                                Assert.AreEqual(c, coloredImage.GetPixel(x, y), $"bpp={bitsPerPixel}");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetBlackBorderTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);

                foreach ((int left, int top, int right, int bottom) in this.borders)
                {
                    Rectangle area = Rectangle.Inflate(image.Bounds, -left, -top, -right, -bottom);

                    image.Randomize();

                    Image borderImage = image.Clone(true);
                    borderImage.SetBlackBorder(area);

                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            uint c = area.Contains(x, y) ? image.GetPixel(x, y) : borderImage.BlackColor;
                            Assert.AreEqual(c, borderImage.GetPixel(x, y), $"bpp={bitsPerPixel}");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetWhiteBorderTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);

                foreach ((int left, int top, int right, int bottom) in this.borders)
                {
                    Rectangle area = Rectangle.Inflate(image.Bounds, -left, -top, -right, -bottom);

                    image.Randomize();

                    Image borderImage = image.Clone(true);
                    borderImage.SetWhiteBorder(area);

                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            uint c = area.Contains(x, y) ? image.GetPixel(x, y) : borderImage.WhiteColor;
                            Assert.AreEqual(c, borderImage.GetPixel(x, y), $"bpp={bitsPerPixel}");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetBorderTest_BorderConst()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);

                foreach ((int left, int top, int right, int bottom) in this.borders)
                {
                    Rectangle area = Rectangle.Inflate(image.Bounds, -left, -top, -right, -bottom);

                    foreach (uint color in this.colors[bitsPerPixel])
                    {
                        image.Randomize();

                        Image borderImage = image.Clone(true);
                        borderImage.SetBorder(area, BorderType.BorderConst, color);

                        for (int x = 0; x < image.Width; x++)
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                uint c = area.Contains(x, y) ? image.GetPixel(x, y) : color & image.MaxColor;
                                Assert.AreEqual(c, borderImage.GetPixel(x, y), $"bpp={bitsPerPixel}");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SetBorderTest_BorderRepl()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                Image image = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);

                foreach ((int left, int top, int right, int bottom) in this.borders)
                {
                    Rectangle area = Rectangle.Inflate(image.Bounds, -left, -top, -right, -bottom);

                    image.Randomize();

                    Image borderImage = image.Clone(true);
                    borderImage.SetBorder(area, BorderType.BorderRepl, 0);

                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            uint c = area.Contains(x, y) ? image.GetPixel(x, y) : image.GetPixel(x.Clip(area.X, area.Right - 1), y.Clip(area.Y, area.Bottom - 1));
                            Assert.AreEqual(c, borderImage.GetPixel(x, y), $"bpp={bitsPerPixel}");
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

                Image invertedImage = image.Not(null);

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
