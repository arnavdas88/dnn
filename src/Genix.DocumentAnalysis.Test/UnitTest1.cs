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

            ////foreach ((Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
            foreach ((Image image, _, _) in Image.FromFile(@"C:\DNN\dnn\test.jpg"))
            {
                Image workImage = Image.Binarize(image);

                stopwatch.Restart();

                workImage.DistanceToBackground(4, 8);

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

                workImage = Image.Deskew(workImage);
                ISet<ConnectedComponent> components = LineDetector.FindLines(workImage, new LineDetectionOptions());
            }
        }
    }
}
