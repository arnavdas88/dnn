namespace Genix.Imaging.Test
{
    using System;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ArithmeticTest
    {
        [TestMethod]
        public void AddCTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src.Randomize();

                    Image copy = src.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 8:
                            {
                                Image dst1 = src.AddC(null, 23, scaleFactor);
                                Image dst2 = src.AddC(src, 23, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        uint expected = (uint)Math.Min(255, (int)Math.Round(((int)copy.GetPixel(x, y) + 23) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven));
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Color addcolor = Color.FromArgb(12, 34, 56);
                                Image dst1 = src.AddC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.AddC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color expected = Color.Add(Color.FromArgb(copy.GetPixel(x, y)), addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Color addcolor = Color.FromArgb(23, 12, 34, 56);
                                Image dst1 = src.AddC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.AddC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Add(source, addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src.AddC(null, 23, scaleFactor);
                                Assert.Fail();
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void SubCTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src.Randomize();

                    Image copy = src.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 8:
                            {
                                Image dst1 = src.SubC(null, 23, scaleFactor);
                                Image dst2 = src.SubC(src, 23, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        uint expected = (uint)((int)Math.Round(((int)copy.GetPixel(x, y) - 23) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven)).Clip(0, 255);
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Color addcolor = Color.FromArgb(12, 34, 56);
                                Image dst1 = src.SubC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.SubC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color expected = Color.Subtract(Color.FromArgb(copy.GetPixel(x, y)), addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Color addcolor = Color.FromArgb(23, 12, 34, 56);
                                Image dst1 = src.SubC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.SubC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Subtract(source, addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src.SubC(null, 23, scaleFactor);
                                Assert.Fail();
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void MulCTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src.Randomize();

                    Image copy = src.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 8:
                            {
                                Image dst1 = src.MulC(null, 2, scaleFactor);
                                Image dst2 = src.MulC(src, 2, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        uint expected = (uint)Math.Min(255, (int)Math.Round(((int)copy.GetPixel(x, y) * 2) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven));
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Color addcolor = Color.FromArgb(2, 3, 4);
                                Image dst1 = src.MulC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.MulC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color expected = Color.Multiply(Color.FromArgb(copy.GetPixel(x, y)), addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Color addcolor = Color.FromArgb(1, 2, 3, 4);
                                Image dst1 = src.MulC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.MulC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Multiply(source, addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src.MulC(null, 2, scaleFactor);
                                Assert.Fail();
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void DivCTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src.Randomize();

                    Image copy = src.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 8:
                            {
                                Image dst1 = src.DivC(null, 2, scaleFactor);
                                Image dst2 = src.DivC(src, 2, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        uint expected = (uint)Math.Min(255, (int)Math.Round(((double)copy.GetPixel(x, y) / 2) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven));
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Color addcolor = Color.FromArgb(2, 3, 4);
                                Image dst1 = src.DivC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.DivC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color expected = Color.Divide(Color.FromArgb(copy.GetPixel(x, y)), addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Color addcolor = Color.FromArgb(1, 2, 3, 4);
                                Image dst1 = src.DivC(null, addcolor.Argb, scaleFactor);
                                Image dst2 = src.DivC(src, addcolor.Argb, scaleFactor);
                                Assert.IsTrue(src == dst2);

                                for (int y = 0; y < src.Height; y++)
                                {
                                    for (int x = 0; x < src.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Divide(source, addcolor, Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src.MulC(null, 2, scaleFactor);
                                Assert.Fail();
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void AddTest()
        {
            UlongRandomGenerator random = new UlongRandomGenerator();

            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src1 = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src1.Randomize(random);

                    Image src2 = new Image(src1.Width, src1.Height, src1.BitsPerPixel, src1.HorizontalResolution, src1.VerticalResolution);
                    src2.Randomize(random);

                    Image copy = src1.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 1:
                        case 8:
                            {
                                Image dst1 = src1.Add(null, src2, scaleFactor);
                                Image dst2 = src1.Add(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        uint expected = bitsPerPixel == 1 ?
                                            (uint)Math.Min(copy.MaxColor, copy.GetPixel(x, y) + (int)src2.GetPixel(x, y)) :
                                            (uint)Math.Min(copy.MaxColor, (int)Math.Round(((int)copy.GetPixel(x, y) + (int)src2.GetPixel(x, y)) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven));
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Image dst1 = src1.Add(null, src2, scaleFactor);
                                Image dst2 = src1.Add(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color expected = Color.Add(Color.FromArgb(copy.GetPixel(x, y)), Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Image dst1 = src1.Add(null, src2, scaleFactor);
                                Image dst2 = src1.Add(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Add(source, Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src1.Add(null, src2, scaleFactor);
                                Assert.Fail($"bpp={bitsPerPixel}");
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void SubTest()
        {
            UlongRandomGenerator random = new UlongRandomGenerator();

            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src1 = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src1.Randomize(random);

                    Image src2 = new Image(src1.Width, src1.Height, src1.BitsPerPixel, src1.HorizontalResolution, src1.VerticalResolution);
                    src2.Randomize(random);

                    Image copy = src1.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 1:
                        case 8:
                            {
                                Image dst1 = src1.Sub(null, src2, scaleFactor);
                                Image dst2 = src1.Sub(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        uint expected = bitsPerPixel == 1 ?
                                            (uint)((int)copy.GetPixel(x, y) - (int)src2.GetPixel(x, y)).Clip(0, 255) :
                                            (uint)((int)Math.Round(((int)copy.GetPixel(x, y) - (int)src2.GetPixel(x, y)) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven)).Clip(0, 255);
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Image dst1 = src1.Sub(null, src2, scaleFactor);
                                Image dst2 = src1.Sub(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color expected = Color.Subtract(Color.FromArgb(copy.GetPixel(x, y)), Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Image dst1 = src1.Sub(null, src2, scaleFactor);
                                Image dst2 = src1.Sub(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Subtract(source, Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src1.Sub(null, src2, scaleFactor);
                                Assert.Fail($"bpp={bitsPerPixel}");
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void MulTest()
        {
            UlongRandomGenerator random = new UlongRandomGenerator();

            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src1 = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src1.Randomize(random);

                    Image src2 = new Image(src1.Width, src1.Height, src1.BitsPerPixel, src1.HorizontalResolution, src1.VerticalResolution);
                    src2.Randomize(random);

                    Image copy = src1.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 8:
                            {
                                Image dst1 = src1.Mul(null, src2, scaleFactor);
                                Image dst2 = src1.Mul(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        uint expected = (uint)Math.Min(copy.MaxColor, (int)Math.Round(((int)copy.GetPixel(x, y) * (int)src2.GetPixel(x, y)) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven));
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Image dst1 = src1.Mul(null, src2, scaleFactor);
                                Image dst2 = src1.Mul(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color expected = Color.Multiply(Color.FromArgb(copy.GetPixel(x, y)), Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Image dst1 = src1.Mul(null, src2, scaleFactor);
                                Image dst2 = src1.Mul(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Multiply(source, Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src1.Mul(null, src2, scaleFactor);
                                Assert.Fail($"bpp={bitsPerPixel}");
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void DivTest()
        {
            UlongRandomGenerator random = new UlongRandomGenerator();

            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int scaleFactor in new[] { 0, 1, -2 })
                {
                    Image src1 = new Image((32 * 2) + 23, 43, bitsPerPixel, 200, 200);
                    src1.Randomize(random);

                    Image src2 = new Image(src1.Width, src1.Height, src1.BitsPerPixel, src1.HorizontalResolution, src1.VerticalResolution);
                    src2.Randomize(random);

                    Image copy = src1.Clone(true);

                    switch (bitsPerPixel)
                    {
                        case 8:
                            {
                                Image dst1 = src1.Div(null, src2, scaleFactor);
                                Image dst2 = src1.Div(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        uint expected = src2.GetPixel(x, y) == 0 ?
                                            (copy.GetPixel(x, y) == 0 ? 0u : 255u) :
                                            (uint)Math.Min(copy.MaxColor, (int)Math.Round((double)copy.GetPixel(x, y) / src2.GetPixel(x, y) * Math.Pow(2, -scaleFactor), MidpointRounding.ToEven));
                                        Assert.AreEqual(expected, dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected, dst2.GetPixel(x, y));
                                    }
                                }
                            }

                            break;

                        case 24:
                            {
                                Image dst1 = src1.Div(null, src2, scaleFactor);
                                Image dst2 = src1.Div(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color expected = Color.Divide(Color.FromArgb(copy.GetPixel(x, y)), Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        case 32:
                            {
                                Image dst1 = src1.Div(null, src2, scaleFactor);
                                Image dst2 = src1.Div(src1, src2, scaleFactor);
                                Assert.IsTrue(src1 == dst2);

                                for (int y = 0; y < src1.Height; y++)
                                {
                                    for (int x = 0; x < src1.Width; x++)
                                    {
                                        Color source = Color.FromArgb(copy.GetPixel(x, y));
                                        Color expected = Color.Divide(source, Color.FromArgb(src2.GetPixel(x, y)), Math.Pow(2, -scaleFactor), MidpointRounding.ToEven);

                                        Color actual1 = Color.FromArgb(dst1.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual1.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual1.R);
                                        Assert.AreEqual(expected.G, actual1.G);
                                        Assert.AreEqual(expected.B, actual1.B);

                                        Color actual2 = Color.FromArgb(dst2.GetPixel(x, y));
                                        ////Assert.AreEqual(source.A, actual2.A);  // alpha channel should not be affected
                                        Assert.AreEqual(expected.R, actual2.R);
                                        Assert.AreEqual(expected.G, actual2.G);
                                        Assert.AreEqual(expected.B, actual2.B);
                                    }
                                }
                            }

                            break;

                        default:
                            try
                            {
                                src1.Div(null, src2, scaleFactor);
                                Assert.Fail($"bpp={bitsPerPixel}");
                            }
                            catch (NotSupportedException)
                            {
                            }

                            break;
                    }
                }
            }
        }
    }
}
