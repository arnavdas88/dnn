namespace Genix.Imaging.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BitmapExtensionsTest
    {
        [TestMethod]
        public void ToBitmapTest()
        {
            Image image = new Image(20, 35, 1, 200, 200);
            image.SetPixel(1, 1, 1);
            image.SetPixel(18, 33, 1);

            System.Drawing.Bitmap bitmap = image.ToBitmap();

            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), bitmap.GetPixel(1, 1).ToArgb());
            Assert.AreEqual(System.Drawing.Color.Black.ToArgb(), bitmap.GetPixel(18, 33).ToArgb());
        }

        [TestMethod]
        public void FromBitmapTest()
        {
            foreach ((Imaging.Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
            {
                ////Image deskew = image.Deskew();

                ///Image dst = image.Deskew().Open(StructuringElement.Rectangle(1, 100), 1).Dilate(StructuringElement.Square(2), 1);
                Image dst = image.Despeckle();
            }

            foreach (bool whiteOnBlack in new bool[] { true, false })
            {
                using (System.Drawing.Bitmap bitmap = BitmapExtensionsTest.Create1bppIndexed(20, 35, whiteOnBlack))
                {
                    bitmap.SetResolution(252, 345);
                    BitmapExtensionsTest.SetPixelIndexed(bitmap, 1, 1, whiteOnBlack);
                    BitmapExtensionsTest.SetPixelIndexed(bitmap, 18, 33, whiteOnBlack);

                    Image image = BitmapExtensions.FromBitmap(bitmap);
                    Assert.AreEqual(20, image.Width);
                    Assert.AreEqual(35, image.Height);
                    Assert.AreEqual(1, image.BitsPerPixel);
                    Assert.AreEqual(252, image.HorizontalResolution);
                    Assert.AreEqual(345, image.VerticalResolution);

                    Assert.AreEqual(1u, image.GetPixel(1, 1));
                    Assert.AreEqual(1u, image.GetPixel(18, 33));
                }
            }
        }

#if false
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
#endif

        private static System.Drawing.Bitmap Create1bppIndexed(int width, int height, bool whiteOnBlack)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            try
            {
                if (whiteOnBlack)
                {
                    System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
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
                    System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
                    palette.Entries[0] = System.Drawing.Color.White;
                    palette.Entries[1] = System.Drawing.Color.Black;
                    bitmap.Palette = palette;
                }

                return bitmap;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }

        private static void SetPixelIndexed(System.Drawing.Bitmap bitmap, int x, int y, bool whiteOnBlack)
        {
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
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
