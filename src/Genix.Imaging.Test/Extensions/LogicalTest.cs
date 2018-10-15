namespace Genix.Imaging.Test
{
    using System;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LogicalTest
    {
        private readonly UlongRandomGenerator random = new UlongRandomGenerator();

        [TestMethod]
        public void OrTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int width in new[] { 64 * 4, 231 })
                {
                    Image src1 = new Image(width, 100, bitsPerPixel, 200, 200);
                    src1.Randomize(this.random);

                    // simple operation
                    Image src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    Image dst = src1.Or(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) | src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a larger image
                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Or(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) | src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a smaller image
                    // pixels outside src2 bounds should remain unchanged
                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Or(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? src1.GetPixel(x, y) | src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation
                    Image copy = src1.Clone(true);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = src1.Or(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) | src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a larger image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Or(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) | src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a smaller image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Or(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? copy.GetPixel(x, y) | src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // operation with existing destination
                    src1.Randomize(this.random);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = new Image(src1.Width + 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    dst.Randomize(this.random);

                    dst = src1.Or(dst, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) | src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void AndTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int width in new[] { 64 * 4, 231 })
                {
                    Image src1 = new Image(width, 100, bitsPerPixel, 200, 200);
                    src1.Randomize(this.random);

                    // simple operation
                    Image src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    Image dst = src1.And(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) & src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a larger image
                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.And(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) & src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a smaller image
                    // pixels outside src2 bounds should remain unchanged
                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.And(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? src1.GetPixel(x, y) & src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation
                    Image copy = src1.Clone(true);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = src1.And(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) & src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a larger image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.And(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) & src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a smaller image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.And(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? copy.GetPixel(x, y) & src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // operation with existing destination
                    src1.Randomize(this.random);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = new Image(src1.Width + 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    dst.Randomize(this.random);

                    dst = src1.And(dst, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) & src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void XorTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int width in new[] { 64 * 4, 231 })
                {
                    Image src1 = new Image(width, 100, bitsPerPixel, 200, 200);
                    src1.Randomize(this.random);

                    // simple operation
                    Image src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    Image dst = src1.Xor(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) ^ src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a larger image
                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xor(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) ^ src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a smaller image
                    // pixels outside src2 bounds should remain unchanged
                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xor(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? src1.GetPixel(x, y) ^ src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation
                    Image copy = src1.Clone(true);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = src1.Xor(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) ^ src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a larger image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xor(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) ^ src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a smaller image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xor(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? copy.GetPixel(x, y) ^ src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // operation with existing destination
                    src1.Randomize(this.random);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = new Image(src1.Width + 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    dst.Randomize(this.random);

                    dst = src1.Xor(dst, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) ^ src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void XandTest()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 24, 32 })
            {
                foreach (int width in new[] { 64 * 4, 231 })
                {
                    Image src1 = new Image(width, 100, bitsPerPixel, 200, 200);
                    src1.Randomize(this.random);

                    // simple operation
                    Image src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    Image dst = src1.Xand(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) & ~src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a larger image
                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xand(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) & ~src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // operation with a smaller image
                    // pixels outside src2 bounds should remain unchanged
                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xand(null, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? src1.GetPixel(x, y) & ~src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation
                    Image copy = src1.Clone(true);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = src1.Xand(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) & ~src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a larger image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width + 10, src1.Height + 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xand(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(copy.GetPixel(x, y) & ~src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }

                    // in-place operation with a smaller image
                    src1.Randomize(this.random);
                    copy = src1.Clone(true);

                    src2 = new Image(src1.Width - 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    src2.Randomize(this.random);

                    dst = src1.Xand(src1, src2);
                    Assert.IsTrue(src1 == dst);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(
                                src2.Bounds.Contains(x, y) ? copy.GetPixel(x, y) & ~src2.GetPixel(x, y) : src1.GetPixel(x, y),
                                dst.GetPixel(x, y));
                        }
                    }

                    // operation with existing destination
                    src1.Randomize(this.random);

                    src2 = src1.Clone(false);
                    src2.Randomize(this.random);

                    dst = new Image(src1.Width + 10, src1.Height - 10, bitsPerPixel, 200, 200);
                    dst.Randomize(this.random);

                    dst = src1.Xand(dst, src2);
                    Assert.AreEqual(dst.Width, src1.Width);
                    Assert.AreEqual(dst.Height, src1.Height);
                    Assert.AreEqual(dst.BitsPerPixel, src1.BitsPerPixel);
                    Assert.AreEqual(dst.HorizontalResolution, src1.HorizontalResolution);
                    Assert.AreEqual(dst.VerticalResolution, src1.VerticalResolution);

                    for (int y = 0; y < src1.Height; y++)
                    {
                        for (int x = 0; x < src1.Width; x++)
                        {
                            Assert.AreEqual(src1.GetPixel(x, y) & ~src2.GetPixel(x, y), dst.GetPixel(x, y));
                        }
                    }
                }
            }
        }
    }
}
