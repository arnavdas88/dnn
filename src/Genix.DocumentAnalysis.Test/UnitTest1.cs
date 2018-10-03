namespace Genix.DocumentAnalysis.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.Imaging;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Genix.Imaging.Leptonica;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Stopwatch stopwatch = new Stopwatch();

            ////foreach ((Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"D:\source.bmp"))
            foreach ((Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
            ////foreach ((Image image, _, _) in Image.FromFile(@"C:\DNN\dnn\test.jpg"))
            {
                Image workImage = image.BitsPerPixel > 1 ? image.Binarize(null) : image;

#if false
                stopwatch.Restart();

                for (int i = 0; i < 10; i++)
                workImage.FindConnectedComponents(8);
                ////workImage.DistanceToBackground(4, 8);

                stopwatch.Stop();
                Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);

                using (Pix pixComp = Pix.FromImage(workImage))
                {
                    stopwatch.Restart();

                    using (Pix pixDist = pixComp.DistanceFunction(4, 8, 1))
                    {
                    }

                    stopwatch.Stop();
                    Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);
                }
#endif

                workImage = workImage.CleanOverscan(0.5f, 0.5f).Deskew(null).Despeckle(null);
                workImage.SetResolution(300, 300);
                stopwatch.Restart();
                ISet<LineShape> components = LineDetector.FindLines(workImage, new LineDetectionOptions());
                stopwatch.Stop();
                Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);
            }
        }
    }
}
