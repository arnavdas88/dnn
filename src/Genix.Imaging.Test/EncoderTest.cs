namespace Genix.Imaging.Test
{
    using System;
    using System.IO;
    using Genix.Imaging.Encoders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EncoderTest
    {
        [TestMethod]
        public void BitmapEncoderTest()
        {
            foreach ((Image image, _, _) in Image.FromFile(@"C:\DNN\dnn\TestData\2bpp.bmp"))
            {

            }

            foreach (int bitCount in new int[] { /*1, 2,*/ 4, 8, 16, 24, 32 })
            {
                Image image1 = new Image(65, 1, bitCount, 168, 204);
                image1.Randomize();

                BitmapEncoder encoder = new BitmapEncoder();

                byte[] ba1;
                using (MemoryStream stream1 = new MemoryStream())
                {
                    image1.Save(stream1, ImageFormat.Bmp);
                    ba1 = stream1.ToArray();
                }

                byte[] ba2;
                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream, image1);
                    ba2 = stream.ToArray();
                }

                using (MemoryStream stream2 = new MemoryStream(ba1))
                {
                    foreach ((Image image2, _, _) in Image.FromStream(stream2))
                    {
                        Assert.AreEqual(image1.Width, image2.Width);
                        Assert.AreEqual(image1.Height, image2.Height);
                        Assert.AreEqual(image1.BitsPerPixel, image2.BitsPerPixel);
                        Assert.AreEqual(image1.HorizontalResolution, image2.HorizontalResolution);
                        Assert.AreEqual(image1.VerticalResolution, image2.VerticalResolution);
                        CollectionAssert.AreEqual(image1.Bits, image2.Bits);
                    }
                }
            }
        }
    }
}
