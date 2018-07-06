#if false
namespace Accord.DNN.Test
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Accord.DNN;

    [TestClass]
    public class BitmapExtensionsTest
    {
        [TestMethod]
        public void CropTest()
        {
            foreach (bool whiteOnBlack in new bool[] { true, false })
            {
                Bitmap src = BitmapExtensionsTest.Create1bppIndexed(10, 20, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 5, 9, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 6, 10, whiteOnBlack);

                Bitmap dst = BitmapExtensions.Crop(src, 2, 3, 6, 15);
                Assert.AreEqual(6, dst.Width);
                Assert.AreEqual(15, dst.Height);
                Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(3, 6).ToArgb());
                Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(4, 7).ToArgb());
            }
        }

        [TestMethod]
        public void InflateTest1()
        {
            foreach (bool whiteOnBlack in new bool[] { true, false })
            {
                Bitmap src = BitmapExtensionsTest.Create1bppIndexed(10, 20, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 5, 9, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 6, 10, whiteOnBlack);

                Bitmap dst = BitmapExtensions.Inflate(src, 1, 2, 3, 4);
                Assert.AreEqual(14, dst.Width);
                Assert.AreEqual(26, dst.Height);
                Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(6, 11).ToArgb());
                Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(7, 12).ToArgb());
            }
        }

        [TestMethod]
        public void InflateTest2()
        {
            Bitmap src = new Bitmap(10, 20);
            src.SetPixel(5, 9, Color.Black);
            src.SetPixel(6, 10, Color.White);

            Bitmap dst = BitmapExtensions.Inflate(src, -1, -2, -3, -4);
            Assert.AreEqual(6, dst.Width);
            Assert.AreEqual(14, dst.Height);
            Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(4, 7).ToArgb());
            Assert.AreEqual(Color.White.ToArgb(), dst.GetPixel(5, 8).ToArgb());
        }

        [TestMethod]
        public void BlackAreaTest()
        {
            foreach (bool whiteOnBlack in new bool[] { true, false })
            {
                Bitmap src = BitmapExtensionsTest.Create1bppIndexed(10, 20, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 5, 9, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 8, 15, whiteOnBlack);

                Rectangle blackArea = BitmapExtensions.BlackArea(src);
                Assert.AreEqual(new Rectangle(5, 9, 4, 7), blackArea);
            }
        }

        [TestMethod]
        public void RemoveWhiteMarginTest()
        {
            foreach (bool whiteOnBlack in new bool[] { true, false })
            {
                Bitmap src = BitmapExtensionsTest.Create1bppIndexed(10, 20, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 5, 9, whiteOnBlack);
                BitmapExtensionsTest.SetPixelIndexed(src, 8, 15, whiteOnBlack);

                Bitmap dst = BitmapExtensions.RemoveWhiteMargin(src, 1, 2);
                Assert.AreEqual(6, dst.Width);
                Assert.AreEqual(11, dst.Height);
                Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(1, 2).ToArgb());
                Assert.AreEqual(Color.Black.ToArgb(), dst.GetPixel(4, 8).ToArgb());
            }
        }

        private static Bitmap Create1bppIndexed(int width, int height, bool whiteOnBlack)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format1bppIndexed);

            if (whiteOnBlack)
            {
                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.WriteOnly,
                    bitmap.PixelFormat);

                unsafe
                {
                    byte* p = (byte*)data.Scan0;
                    for (int i = 0, ii = data.Stride * data.Height; i < ii; i++)
                    {
                        p[i] = (byte)0xff;
                    }
                }

                bitmap.UnlockBits(data);
            }
            else
            {
                ColorPalette palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
            }

            return bitmap;
        }

        private static void SetPixelIndexed(Bitmap bitmap, int x, int y, bool whiteOnBlack)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            unsafe
            {
                byte* p = (byte*)data.Scan0;
                int index = (y * data.Stride) + (x / 8);
                int mask = 0x80 >> (x & 7);

                if (whiteOnBlack)
                {
                    p[index] &= (byte)~mask;
                }
                else
                {
                    p[index] |= (byte)mask;
                }
            }

            bitmap.UnlockBits(data);
        }
    }
}
#endif
