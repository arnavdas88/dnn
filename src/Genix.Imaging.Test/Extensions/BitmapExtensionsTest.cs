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
                image.SetWhite();

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
            foreach (int bitsPerPixel in new int[] { 1, 4, 8, 24, 32 })
            {
                foreach (bool whiteOnBlack in new bool[] { true, false })
                {
                    using (System.Drawing.Bitmap bitmap = BitmapHelpers.CreateBitmap(20, 35, bitsPerPixel, whiteOnBlack))
                    {
                        bitmap.SetResolution(252, 345);
                        BitmapHelpers.SetPixel(bitmap, 1, 1, whiteOnBlack);
                        BitmapHelpers.SetPixel(bitmap, 18, 33, whiteOnBlack);

                        Image image = BitmapExtensions.FromBitmap(bitmap);
                        Assert.AreEqual(20, image.Width);
                        Assert.AreEqual(35, image.Height);
                        Assert.AreEqual(bitsPerPixel, image.BitsPerPixel);
                        Assert.AreEqual(252, image.HorizontalResolution);
                        Assert.AreEqual(345, image.VerticalResolution);

                        uint color = bitsPerPixel == 1 ? 1u : 0u;
                        Assert.AreEqual(color, image.GetPixel(1, 1));
                        Assert.AreEqual(color, image.GetPixel(18, 33));
                    }
                }
            }
        }
    }
}
