namespace Genix.Imaging.Test.Extensions
{
    using System;
    using Genix.Core;
    using Genix.Geometry;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ThresholdingTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void ThresholdLTTest()
        {
            foreach (int bitsPerPixel in new[] { 8, 24, 32 })
            {
                Image src = new Image(231, 50, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Color threshold = Color.Divide(Color.FromArgb(src.MaxColor), Color.FromArgb(3, 3, 3), 1, MidpointRounding.AwayFromZero);
                Color value = Color.FromArgb(src.MaxColor);

                // process whole image
                Image dst = src.ThresholdLT(null, threshold.Argb, value.Argb);

                Color mincolor = Color.FromArgb(dst.Min());
                switch (bitsPerPixel)
                {
                    case 8:
                        Assert.IsTrue(mincolor.Argb >= threshold.Argb);
                        break;

                    case 24:
                    case 32:
                        Assert.IsTrue(mincolor.R >= threshold.R);
                        Assert.IsTrue(mincolor.G >= threshold.G);
                        Assert.IsTrue(mincolor.B >= threshold.B);
                        break;
                }

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(src.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(dst.GetPixel(x, y));
                        switch (bitsPerPixel)
                        {
                            case 8:
                                Assert.AreEqual(srccolor.Argb < threshold.Argb ? value.Argb : srccolor.Argb, dstcolor.Argb);
                                break;

                            case 24:
                                Assert.AreEqual(srccolor.R < threshold.R ? value.R : srccolor.R, dstcolor.R);
                                Assert.AreEqual(srccolor.G < threshold.G ? value.G : srccolor.G, dstcolor.G);
                                Assert.AreEqual(srccolor.B < threshold.B ? value.B : srccolor.B, dstcolor.B);
                                break;

                            case 32:
                                Assert.AreEqual(srccolor.R < threshold.R ? value.R : srccolor.R, dstcolor.R);
                                Assert.AreEqual(srccolor.G < threshold.G ? value.G : srccolor.G, dstcolor.G);
                                Assert.AreEqual(srccolor.B < threshold.B ? value.B : srccolor.B, dstcolor.B);
                                Assert.AreEqual(0, dstcolor.A); // method sets the alpha channel to zero
                                break;
                        }
                    }
                }

                // process area
                src.Randomize(this.random);

                Rectangle area = Rectangle.Inflate(src.Bounds, -1, -2, -3, -4);
                dst = src.ThresholdLT(null, area, threshold.Argb, value.Argb);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(src.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(dst.GetPixel(x, y));
                        if (area.Contains(x, y))
                        {
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    Assert.AreEqual(srccolor.Argb < threshold.Argb ? value.Argb : srccolor.Argb, dstcolor.Argb);
                                    break;

                                case 24:
                                    Assert.AreEqual(srccolor.R < threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G < threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B < threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    break;

                                case 32:
                                    Assert.AreEqual(srccolor.R < threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G < threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B < threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    Assert.AreEqual(0, dstcolor.A); // method sets the alpha channel to zero
                                    break;
                            }
                        }
                        else
                        {
                            Assert.AreEqual(0u, dstcolor.Argb);
                        }
                    }
                }

                // process area in-place
                src.Randomize(this.random);
                Image copy = src.Clone(true);

                src.ThresholdLT(src, area, threshold.Argb, value.Argb);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(copy.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(src.GetPixel(x, y));
                        if (area.Contains(x, y))
                        {
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    Assert.AreEqual(srccolor.Argb < threshold.Argb ? value.Argb : srccolor.Argb, dstcolor.Argb);
                                    break;

                                case 24:
                                    Assert.AreEqual(srccolor.R < threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G < threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B < threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    break;

                                case 32:
                                    Assert.AreEqual(srccolor.R < threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G < threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B < threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    Assert.AreEqual(srccolor.A, dstcolor.A); // method does not change the alpha channel
                                    break;
                            }
                        }
                        else
                        {
                            Assert.AreEqual(srccolor.Argb, dstcolor.Argb);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ThresholdGTTest()
        {
            foreach (int bitsPerPixel in new[] { 8, 24, 32 })
            {
                Image src = new Image(231, 50, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Color threshold = Color.Divide(Color.FromArgb(src.MaxColor), Color.FromArgb(3, 3, 3), 1, MidpointRounding.AwayFromZero);
                Color value = Color.FromArgb(0);

                // process whole image
                Image dst = src.ThresholdGT(null, threshold.Argb, value.Argb);

                Color maxcolor = Color.FromArgb(dst.Max());
                switch (bitsPerPixel)
                {
                    case 8:
                        Assert.IsTrue(maxcolor.Argb <= threshold.Argb);
                        break;

                    case 24:
                    case 32:
                        Assert.IsTrue(maxcolor.R <= threshold.R);
                        Assert.IsTrue(maxcolor.G <= threshold.G);
                        Assert.IsTrue(maxcolor.B <= threshold.B);
                        break;
                }

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(src.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(dst.GetPixel(x, y));
                        switch (bitsPerPixel)
                        {
                            case 8:
                                Assert.AreEqual(srccolor.Argb > threshold.Argb ? value.Argb : srccolor.Argb, dstcolor.Argb);
                                break;

                            case 24:
                                Assert.AreEqual(srccolor.R > threshold.R ? value.R : srccolor.R, dstcolor.R);
                                Assert.AreEqual(srccolor.G > threshold.G ? value.G : srccolor.G, dstcolor.G);
                                Assert.AreEqual(srccolor.B > threshold.B ? value.B : srccolor.B, dstcolor.B);
                                break;

                            case 32:
                                Assert.AreEqual(srccolor.R > threshold.R ? value.R : srccolor.R, dstcolor.R);
                                Assert.AreEqual(srccolor.G > threshold.G ? value.G : srccolor.G, dstcolor.G);
                                Assert.AreEqual(srccolor.B > threshold.B ? value.B : srccolor.B, dstcolor.B);
                                Assert.AreEqual(0, dstcolor.A); // method sets the alpha channel to zero
                                break;
                        }
                    }
                }

                // process area
                src.Randomize(this.random);

                Rectangle area = Rectangle.Inflate(src.Bounds, -1, -2, -3, -4);
                dst = src.ThresholdGT(null, area, threshold.Argb, value.Argb);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(src.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(dst.GetPixel(x, y));
                        if (area.Contains(x, y))
                        {
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    Assert.AreEqual(srccolor.Argb > threshold.Argb ? value.Argb : srccolor.Argb, dstcolor.Argb);
                                    break;

                                case 24:
                                    Assert.AreEqual(srccolor.R > threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G > threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B > threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    break;

                                case 32:
                                    Assert.AreEqual(srccolor.R > threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G > threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B > threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    Assert.AreEqual(0, dstcolor.A); // method sets the alpha channel to zero
                                    break;
                            }
                        }
                        else
                        {
                            Assert.AreEqual(0u, dstcolor.Argb);
                        }
                    }
                }

                // process area in-place
                src.Randomize(this.random);
                Image copy = src.Clone(true);

                src.ThresholdGT(src, area, threshold.Argb, value.Argb);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(copy.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(src.GetPixel(x, y));
                        if (area.Contains(x, y))
                        {
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    Assert.AreEqual(srccolor.Argb > threshold.Argb ? value.Argb : srccolor.Argb, dstcolor.Argb);
                                    break;

                                case 24:
                                    Assert.AreEqual(srccolor.R > threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G > threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B > threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    break;

                                case 32:
                                    Assert.AreEqual(srccolor.R > threshold.R ? value.R : srccolor.R, dstcolor.R);
                                    Assert.AreEqual(srccolor.G > threshold.G ? value.G : srccolor.G, dstcolor.G);
                                    Assert.AreEqual(srccolor.B > threshold.B ? value.B : srccolor.B, dstcolor.B);
                                    Assert.AreEqual(srccolor.A, dstcolor.A); // method does not change the alpha channel
                                    break;
                            }
                        }
                        else
                        {
                            Assert.AreEqual(srccolor.Argb, dstcolor.Argb);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ThresholdLTGTTest()
        {
            foreach (int bitsPerPixel in new[] { 8, 24, 32 })
            {
                Image src = new Image(231, 50, bitsPerPixel, 200, 200);
                src.Randomize(this.random);

                Color thresholdLT = Color.Divide(Color.FromArgb(src.MaxColor), Color.FromArgb(3, 3, 3), 1, MidpointRounding.AwayFromZero);
                Color valueLT = thresholdLT;
                Color thresholdGT = Color.Divide(Color.FromArgb(src.MaxColor), Color.FromArgb(2, 2, 2), 1, MidpointRounding.AwayFromZero);
                Color valueGT = thresholdGT;

                // process whole image
                Image dst = src.ThresholdLTGT(null, thresholdLT.Argb, valueLT.Argb, thresholdGT.Argb, valueGT.Argb);

                Color mincolor = Color.FromArgb(dst.Min());
                Color maxcolor = Color.FromArgb(dst.Max());
                switch (bitsPerPixel)
                {
                    case 8:
                        Assert.IsTrue(mincolor.Argb >= thresholdLT.Argb && maxcolor.Argb <= thresholdGT.Argb);
                        break;

                    case 24:
                    case 32:
                        Assert.IsTrue(mincolor.R >= thresholdLT.R && maxcolor.R <= thresholdGT.R);
                        Assert.IsTrue(mincolor.G >= thresholdLT.G && maxcolor.G <= thresholdGT.G);
                        Assert.IsTrue(mincolor.B >= thresholdLT.B && maxcolor.B <= thresholdGT.B);
                        break;
                }

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(src.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(dst.GetPixel(x, y));
                        switch (bitsPerPixel)
                        {
                            case 8:
                                Assert.AreEqual(srccolor.Argb < thresholdLT.Argb ? valueLT.Argb : (srccolor.Argb > thresholdGT.Argb ? valueGT.Argb : srccolor.Argb), dstcolor.Argb);
                                break;

                            case 24:
                                Assert.AreEqual(srccolor.R < thresholdLT.R ? valueLT.R : (srccolor.R > thresholdGT.R ? valueGT.R : srccolor.R), dstcolor.R);
                                Assert.AreEqual(srccolor.G < thresholdLT.G ? valueLT.G : (srccolor.G > thresholdGT.G ? valueGT.G : srccolor.G), dstcolor.G);
                                Assert.AreEqual(srccolor.B < thresholdLT.B ? valueLT.B : (srccolor.B > thresholdGT.B ? valueGT.B : srccolor.B), dstcolor.B);
                                break;

                            case 32:
                                Assert.AreEqual(srccolor.R < thresholdLT.R ? valueLT.R : (srccolor.R > thresholdGT.R ? valueGT.R : srccolor.R), dstcolor.R);
                                Assert.AreEqual(srccolor.G < thresholdLT.G ? valueLT.G : (srccolor.G > thresholdGT.G ? valueGT.G : srccolor.G), dstcolor.G);
                                Assert.AreEqual(srccolor.B < thresholdLT.B ? valueLT.B : (srccolor.B > thresholdGT.B ? valueGT.B : srccolor.B), dstcolor.B);
                                Assert.AreEqual(0, dstcolor.A); // method sets the alpha channel to zero
                                break;
                        }
                    }
                }

                // process area
                src.Randomize(this.random);

                Rectangle area = Rectangle.Inflate(src.Bounds, -1, -2, -3, -4);
                dst = src.ThresholdLTGT(null, area, thresholdLT.Argb, valueLT.Argb, thresholdGT.Argb, valueGT.Argb);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(src.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(dst.GetPixel(x, y));
                        if (area.Contains(x, y))
                        {
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    Assert.AreEqual(srccolor.Argb < thresholdLT.Argb ? valueLT.Argb : (srccolor.Argb > thresholdGT.Argb ? valueGT.Argb : srccolor.Argb), dstcolor.Argb);
                                    break;

                                case 24:
                                    Assert.AreEqual(srccolor.R < thresholdLT.R ? valueLT.R : (srccolor.R > thresholdGT.R ? valueGT.R : srccolor.R), dstcolor.R);
                                    Assert.AreEqual(srccolor.G < thresholdLT.G ? valueLT.G : (srccolor.G > thresholdGT.G ? valueGT.G : srccolor.G), dstcolor.G);
                                    Assert.AreEqual(srccolor.B < thresholdLT.B ? valueLT.B : (srccolor.B > thresholdGT.B ? valueGT.B : srccolor.B), dstcolor.B);
                                    break;

                                case 32:
                                    Assert.AreEqual(srccolor.R < thresholdLT.R ? valueLT.R : (srccolor.R > thresholdGT.R ? valueGT.R : srccolor.R), dstcolor.R);
                                    Assert.AreEqual(srccolor.G < thresholdLT.G ? valueLT.G : (srccolor.G > thresholdGT.G ? valueGT.G : srccolor.G), dstcolor.G);
                                    Assert.AreEqual(srccolor.B < thresholdLT.B ? valueLT.B : (srccolor.B > thresholdGT.B ? valueGT.B : srccolor.B), dstcolor.B);
                                    Assert.AreEqual(0, dstcolor.A); // method sets the alpha channel to zero
                                    break;
                            }
                        }
                        else
                        {
                            Assert.AreEqual(0u, dstcolor.Argb);
                        }
                    }
                }

                // process area in-place
                src.Randomize(this.random);
                Image copy = src.Clone(true);

                src.ThresholdLTGT(src, area, thresholdLT.Argb, valueLT.Argb, thresholdGT.Argb, valueGT.Argb);

                for (int x = 0; x < src.Width; x++)
                {
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color srccolor = Color.FromArgb(copy.GetPixel(x, y));
                        Color dstcolor = Color.FromArgb(src.GetPixel(x, y));
                        if (area.Contains(x, y))
                        {
                            switch (bitsPerPixel)
                            {
                                case 8:
                                    Assert.AreEqual(srccolor.Argb < thresholdLT.Argb ? valueLT.Argb : (srccolor.Argb > thresholdGT.Argb ? valueGT.Argb : srccolor.Argb), dstcolor.Argb);
                                    break;

                                case 24:
                                    Assert.AreEqual(srccolor.R < thresholdLT.R ? valueLT.R : (srccolor.R > thresholdGT.R ? valueGT.R : srccolor.R), dstcolor.R);
                                    Assert.AreEqual(srccolor.G < thresholdLT.G ? valueLT.G : (srccolor.G > thresholdGT.G ? valueGT.G : srccolor.G), dstcolor.G);
                                    Assert.AreEqual(srccolor.B < thresholdLT.B ? valueLT.B : (srccolor.B > thresholdGT.B ? valueGT.B : srccolor.B), dstcolor.B);
                                    break;

                                case 32:
                                    Assert.AreEqual(srccolor.R < thresholdLT.R ? valueLT.R : (srccolor.R > thresholdGT.R ? valueGT.R : srccolor.R), dstcolor.R);
                                    Assert.AreEqual(srccolor.G < thresholdLT.G ? valueLT.G : (srccolor.G > thresholdGT.G ? valueGT.G : srccolor.G), dstcolor.G);
                                    Assert.AreEqual(srccolor.B < thresholdLT.B ? valueLT.B : (srccolor.B > thresholdGT.B ? valueGT.B : srccolor.B), dstcolor.B);
                                    Assert.AreEqual(srccolor.A, dstcolor.A); // method does not change the alpha channel
                                    break;
                            }
                        }
                        else
                        {
                            Assert.AreEqual(srccolor.Argb, dstcolor.Argb);
                        }
                    }
                }
            }
        }
    }
}
