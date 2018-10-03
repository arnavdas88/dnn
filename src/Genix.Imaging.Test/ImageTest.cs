namespace Genix.Imaging.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Genix.Drawing;
    using Genix.Imaging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImageTest
    {
        public object ImagePreprocessing { get; private set; }

        /// <summary>
        /// Calculates intensity of binary image (sum of black pixels).
        /// </summary>
        [TestMethod]
        public void PowerTest1()
        {
            Image image = new Image((64 * 2) + 23, 43, 1, 200, 200);
            image.SetWhite();

            image.SetPixel(5, 9, 1);
            image.SetPixel(64 + 37, 19, 1);
            image.SetPixel((2 * 64) + 11, 29, 1);

            Assert.AreEqual(3, image.Power());
            Assert.AreEqual(2, image.Power(5, 9, 134, 20));
            Assert.AreEqual(3, image.Power(5, 9, 135, 21));
        }

        /// <summary>
        /// Calculates intensity of binary image (sum of pixel values).
        /// </summary>
        [TestMethod]
        public void PowerTest2()
        {
            Image image = new Image((64 * 2) + 23, 43, 8, 200, 200);

            image.SetPixel(5, 9, 255);
            image.SetPixel(64 + 37, 19, 128);
            image.SetPixel((2 * 64) + 11, 29, 64);

            Assert.AreEqual(255 + 128 + 64, image.Power());
            Assert.AreEqual(255 + 128, image.Power(5, 9, 134, 20));
            Assert.AreEqual(255 + 128 + 64, image.Power(5, 9, 135, 21));
        }

        [TestMethod]
        public void BlackAreaTest()
        {
            Image image = new Image(10, 20, 1, 200, 200);
            image.SetPixel(5, 9, 1);
            image.SetPixel(8, 15, 1);

            Rectangle blackArea = image.BlackArea();
            Assert.AreEqual(new Rectangle(5, 9, 4, 7), blackArea);
        }

        [TestMethod]
        public void CropTest()
        {
            Image image = new Image(20, 35, 1, 200, 200);
            image.SetPixel(1, 1, 1);
            image.SetPixel(18, 33, 1);

            image = image.CropBlackArea(0, 0);

            Assert.AreEqual(new Rectangle(0, 0, 18, 33), image.Bounds);
            Assert.AreEqual(1u, image.GetPixel(0, 0));
            Assert.AreEqual(1u, image.GetPixel(17, 32));
            Assert.AreEqual(2, image.Power());
        }

        [TestMethod]
        public void XXXTest()
        {
#if false
            const int Count = 10000000;
            Stopwatch stopwatch = new Stopwatch();

            RandomGenerator random = new RandomGenerator();

            int length = 128;
            float[] dense1 = random.Generate(length);
            float[] dense2 = new float[length];
            for (int i = 0; i < 16; i++)
            {
                dense2[(int)random.Generate(0, length)] = random.Generate();
            }

            SparseVectorF sparse = SparseVectorF.FromDense(length, dense2, 0);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                Math32f.EuclideanDistance(length, dense1, 0, dense2, 0);
            }

            stopwatch.Stop();

            Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                sparse.EuclideanDistance(dense1, 0);
            }

            stopwatch.Stop();

            Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);
#else
            const int Count = 5;
            Stopwatch stopwatch = new Stopwatch();

            foreach ((Image image, _, _) in Image.FromFile(@"C:\DNN\dnn\363978.tif"))
            {
                stopwatch.Restart();

                for (int i = 0; i < Count; i++)
                {
                    ////long power = image.Power();
                    ////Histogram hyst = image.GrayHistogram();
                    Image workImage = image
                        .ConvertTo(null, 8)
                        .Scale(null, 100.0 / image.HorizontalResolution, 100.0 / image.VerticalResolution, ScalingOptions.None)
                        ////.Binarize(null)
                        .Convert8To1(null, 128)
                        .CleanOverscan(0.5f, 0.5f)
                        .Deskew(null)
                        .Despeckle(null);

                    ISet<ConnectedComponent> components = workImage.FindConnectedComponents(8);
                    workImage.RemoveConnectedComponents(components);

                    /*workImage = workImage.Scale(100.0 / image.HorizontalResolution, 100.0 / image.VerticalResolution, Imaging.ScalingOptions.None);
                    workImage = workImage.Binarize();
                    ////workImage = workImage.Convert8To1(128);
                    workImage = workImage.Dilate(StructuringElement.Rectangle(5, 1), 1);
                    workImage = workImage.Dilate(StructuringElement.Rectangle(1, 5), 1);
                    workImage = workImage.Convert1To8();

                    DenseVectorPackF vectors = workImage.HOG(8, 2, 1, 9, 0.2f);

                    vectors = DenseVectorPackF.Pack(vectors.Unpack().Where(x => x.Sum() != 0.0f).ToList());*/
                    ////Image temp = image.Binarize();
                }

                stopwatch.Stop();
            }

            Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds / Count);
#endif
        }
    }
}
