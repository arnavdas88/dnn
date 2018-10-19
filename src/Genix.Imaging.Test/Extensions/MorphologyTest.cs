namespace Genix.Imaging.Test
{
    using System.Globalization;
    using Genix.Core;
    using Genix.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MorphologyTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod, TestCategory("Image")]
        public void DilateTest1()
        {
            const int Width = 150;
            const int Height = 20;
            for (int ix = 0; ix < Width; ix++)
            {
                for (int iy = 0; iy < Height; iy++)
                {
                    Image image = new Image(Width, Height, 1, 200, 200);
                    image.SetPixel(ix, iy, 1);

                    Image dilatedImage = image.Dilate(null, StructuringElement.Square(3), 1, BorderType.BorderConst, image.WhiteColor);

                    if ((ix == 0 || ix == Width - 1) && (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            4ul,
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else if ((ix == 0 || ix == Width - 1) || (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            6ul,
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else
                    {
                        Assert.AreEqual(
                            9ul,
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                }
            }
        }

        [TestMethod, TestCategory("Image")]
        public void DilateTest2()
        {
            StructuringElement[] ses = new[]
            {
                StructuringElement.Brick(3, 1),
                /*StructuringElement.Brick(1, 3),
                StructuringElement.Brick(1, 4),
                StructuringElement.Brick(1, 4, new Point(0, 1)),
                StructuringElement.Square(7),
                StructuringElement.Brick(8, 6),
                StructuringElement.Brick(8, 6, new Point(6, 5)),
                StructuringElement.Brick(8, 6, new Point(10, 8)),
                StructuringElement.Cross(10, 5),
                StructuringElement.Cross(10, 5, new Point(-3, -2)),*/
            };

            foreach (int bitsPerPixel in new[] { 1, /*2, 4,*/ 8, 16, 24, 32 })
            {
                foreach (int width in new[] { 64 * 2, 131 })
                {
                    Image src = new Image(width, 50, bitsPerPixel, 200, 200);

                    foreach (StructuringElement se in ses)
                    {
                        // constant border
                        foreach (uint borderValue in new[] { src.BlackColor, src.WhiteColor, (uint)(((ulong)src.BlackColor + (ulong)src.WhiteColor) / 2) })
                        {
                            src.Randomize();
                            Image dst = src.Dilate(null, se, 1, BorderType.BorderConst, borderValue);

                            for (int x = 0; x < src.Width; x++)
                            {
                                for (int y = 0; y < src.Height; y++)
                                {
                                    Assert.AreEqual(ComputePixelBorder(x, y, borderValue), dst.GetPixel(x, y));
                                }
                            }
                        }

                        // replica border
                        {
                            src.Randomize();
                            Image dst = src.Dilate(null, se, 1, BorderType.BorderRepl, 0);

                            for (int x = 0; x < src.Width; x++)
                            {
                                for (int y = 0; y < src.Height; y++)
                                {
                                    Assert.AreEqual(ComputePixel(x, y), dst.GetPixel(x, y));
                                }
                            }
                        }

                        uint ComputePixel(int x, int y)
                        {
                            uint maxcolor = uint.MinValue;
                            foreach (Point point in se.GetElements())
                            {
                                Point pt = new Point(x + point.X, y + point.Y);
                                if (src.Bounds.Contains(pt))
                                {
                                    maxcolor = Color.Max(maxcolor, src.GetPixel(pt), bitsPerPixel);
                                }
                            }

                            return maxcolor;
                        }

                        uint ComputePixelBorder(int x, int y, uint borderValue)
                        {
                            uint maxcolor = uint.MinValue;
                            foreach (Point point in se.GetElements())
                            {
                                Point pt = new Point(x + point.X, y + point.Y);
                                uint color = src.Bounds.Contains(pt) ? src.GetPixel(pt) : borderValue;
                                maxcolor = Color.Max(maxcolor, color, bitsPerPixel);
                            }

                            return maxcolor;
                        }
                    }
                }
            }
        }

        [TestMethod, TestCategory("Image")]
        public void ErodeTest1()
        {
            const int Width = 150;
            const int Height = 20;
            for (int ix = 0; ix < Width; ix++)
            {
                for (int iy = 0; iy < Height; iy++)
                {
                    Image image = new Image(Width, Height, 1, 200, 200);
                    image.SetBlack();
                    image.SetPixel(ix, iy, 0);

                    Image dilatedImage = image.Erode(null, StructuringElement.Square(3), 1, BorderType.BorderConst, uint.MaxValue);

                    if ((ix == 0 || ix == Width - 1) && (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            (ulong)((Width * Height) - 4),
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else if ((ix == 0 || ix == Width - 1) || (iy == 0 || iy == Height - 1))
                    {
                        Assert.AreEqual(
                            (ulong)((Width * Height) - 6),
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                    else
                    {
                        Assert.AreEqual(
                            (ulong)((Width * Height) - 9),
                            dilatedImage.Power(),
                            string.Format(CultureInfo.InvariantCulture, "{0} {1}", ix, iy));
                    }
                }
            }
        }

        [TestMethod, TestCategory("Image")]
        public void ErodeTest2()
        {
            StructuringElement[] ses = new[]
            {
                StructuringElement.Square(7),
                StructuringElement.Brick(8, 6),
                StructuringElement.Brick(8, 6, new Point(6, 5)),
                StructuringElement.Brick(8, 6, new Point(10, 8)),
                StructuringElement.Cross(10, 5),
                StructuringElement.Cross(10, 5, new Point(-3, -2)),
            };

            foreach (int bitsPerPixel in new[] { 1, /*2, 4,*/ 8, 16, 24, 32 })
            {
                foreach (int width in new[] { 64 * 2, 131 })
                {
                    Image src = new Image(width, 50, bitsPerPixel, 200, 200);

                    foreach (StructuringElement se in ses)
                    {
                        // constant border
                        foreach (uint borderValue in new[] { src.BlackColor, src.WhiteColor, (uint)(((ulong)src.BlackColor + (ulong)src.WhiteColor) / 2) })
                        {
                            src.Randomize();
                            Image dst = src.Erode(null, se, 1, BorderType.BorderConst, borderValue);

                            for (int x = 0; x < src.Width; x++)
                            {
                                for (int y = 0; y < src.Height; y++)
                                {
                                    Assert.AreEqual(ComputePixelBorder(x, y, borderValue), dst.GetPixel(x, y));
                                }
                            }
                        }

                        // replica border
                        {
                            src.Randomize();
                            Image dst = src.Erode(null, se, 1, BorderType.BorderRepl, 0);

                            for (int x = 0; x < src.Width; x++)
                            {
                                for (int y = 0; y < src.Height; y++)
                                {
                                    Assert.AreEqual(ComputePixel(x, y), dst.GetPixel(x, y));
                                }
                            }
                        }

                        uint ComputePixel(int x, int y)
                        {
                            uint maxcolor = src.MaxColor;
                            foreach (Point point in se.GetElements())
                            {
                                Point pt = new Point(x + point.X, y + point.Y);
                                if (src.Bounds.Contains(pt))
                                {
                                    maxcolor = Color.Min(maxcolor, src.GetPixel(pt), bitsPerPixel);
                                }
                            }

                            return maxcolor;
                        }

                        uint ComputePixelBorder(int x, int y, uint borderValue)
                        {
                            uint maxcolor = src.MaxColor;
                            foreach (Point point in se.GetElements())
                            {
                                Point pt = new Point(x + point.X, y + point.Y);
                                uint color = src.Bounds.Contains(pt) ? src.GetPixel(pt) : borderValue;
                                maxcolor = Color.Min(maxcolor, color, bitsPerPixel);
                            }

                            return maxcolor;
                        }
                    }
                }
            }
        }
    }
}
