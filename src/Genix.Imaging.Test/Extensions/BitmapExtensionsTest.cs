namespace Genix.Imaging.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BitmapExtensionsTest
    {
        [TestMethod]
        public void ToBitmapTest()
        {
            foreach (int bitsPerPixel in new[] { 1, /*4,*/ 8, /*16,*/ 32 })
            {
                Image image = new Image(64 + 47, 35, bitsPerPixel, 200, 200);
                image.SetWhiteIP();

                image.SetPixel(11, 1, 1);
                image.SetPixel(67, 15, 1);
                image.SetPixel(93, 33, 1);

                System.Drawing.Bitmap bitmap = image.ToBitmap();

                int color = bitsPerPixel == 1 ?
                    System.Drawing.Color.Black.ToArgb() :
                    bitsPerPixel == 8 ?
                        System.Drawing.Color.FromArgb(1, 1, 1).ToArgb() :
                        System.Drawing.Color.FromArgb(0, 0, 1).ToArgb();

                Assert.AreEqual(color, bitmap.GetPixel(11, 1).ToArgb());
                Assert.AreEqual(color, bitmap.GetPixel(67, 15).ToArgb());
                Assert.AreEqual(color, bitmap.GetPixel(93, 33).ToArgb());
            }
        }

        [TestMethod]
        public void FromBitmapTest()
        {
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
