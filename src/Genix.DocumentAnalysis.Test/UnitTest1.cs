namespace Genix.DocumentAnalysis.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.Imaging;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Genix.Imaging.Leptonica;
    using Genix.Core;
    using System.Threading;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Stopwatch stopwatch = new Stopwatch();
            ImageSegmentation segmentation = new ImageSegmentation();

            ////foreach ((Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"D:\source.bmp"))
            ////foreach ((Image image, _, _) in Imaging.Image.FromFile(@"C:\Users\Alexander\Desktop\hcfa.png"))
            ////foreach ((Image image, _, _) in Imaging.Image.FromFile(@"C:\Users\Alexander\Desktop\07227200002.tif"))
            foreach ((Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
            ////foreach ((Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"C:\Users\avolgunin\Desktop\hcfa.png"))
            ////foreach ((Image image, _, _) in Image.FromFile(@"C:\DNN\dnn\test4.jpg"))
            {
                Image xxx = image.ConvertTo(null, 1).CleanOverscan(0.5f, 0.5f).Deskew(null).Despeckle(null);
                Image yyy = xxx.CreateTemplate(null, xxx.BitsPerPixel);

                ////LineDetector.FindLines(xxx, new LineDetectionOptions());

                /*Pix pix = Pix.FromImage(xxx);*/

                ////int[] a = new int[10000];

                ////for (int kw = 49; kw <= 49; kw++)
                {
                    ////xxx.Dilate(yyy, StructuringElement.Brick(1, kw), 1, BorderType.BorderConst, 0);
                    ////pix.DilateGray(1, kw);

                    ////Pix pix = Pix.FromImage(image);
                    stopwatch.Restart();

                    for (int i = 0; i < 1; i++)
                    {
                        segmentation.Segment(xxx, null, CancellationToken.None);
                        ////xxx.Binarize(yyy, 0, 0, 0, 0, true, 0, 0);
                        ////Statistic.Smooth(a.Length, a, 0);
                        ////pix.DilateGray(1, kw);
                        ////xxx.Convert8To1(yyy, 128);
                        ////xxx.Dilate(yyy, StructuringElement.Brick(1, kw), 1, BorderType.BorderConst, 0);
                        ////xxx.Dilate3x3(yyy, BorderType.BorderConst, 0);
                        ////xxx.Dilate(yyy, StructuringElement.Brick(3, 3), 1, BorderType.BorderConst, 0);
                    }


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

                    /*workImage = workImage.CleanOverscan(0.5f, 0.5f).Deskew(null).Despeckle(null);
                    workImage.SetResolution(300, 300);
                    ISet<LineShape> components = LineDetector.FindLines(workImage, new LineDetectionOptions());*/

                    stopwatch.Stop();
                    ////Console.WriteLine("{0}: {1:F4} ms", kw, stopwatch.ElapsedMilliseconds/* / Count*/);
                    Console.WriteLine("{0:F4} ms", stopwatch.ElapsedMilliseconds/* / Count*/);
                }

                Console.WriteLine(segmentation.PrintPerformanceReport(1));
            }
        }
    }
}
