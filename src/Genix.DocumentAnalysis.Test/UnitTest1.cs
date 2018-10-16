namespace Genix.DocumentAnalysis.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.Imaging;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Genix.Imaging.Leptonica;
    using Genix.Core;

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
                Image xxx = image.Convert1To8(null);
                Image yyy = xxx.CreateTemplate(null, xxx.BitsPerPixel);

                for (int kw = 49; kw <= 49; kw++)
                {
                    xxx.Dilate(yyy, StructuringElement.Brick(kw, 1), 1, BorderType.BorderConst, 0);

                    ////Pix pix = Pix.FromImage(image);
                    stopwatch.Restart();

                    for (int i = 0; i < 100; i++)
                    {
                        ////xxx.Convert8To1(yyy, 128);
                        xxx.Dilate(yyy, StructuringElement.Brick(kw, 1), 1, BorderType.BorderConst, 0);
                    }

                    ////pix = pix.BackgroundNorm(null, null, 64, 128, 128, (64 * 128) / 3, 255, 2, 1);

                    ////image.NormalizeBackground(image, 0, 0, 0, 0, 255);

                    ////Image xxx = image.FilterBox(null, 30, 30, BorderType.BorderRepl, 0);
                    ////Image xxx = image.MorphClose(null, StructuringElement.Square(25), 1, BorderType.BorderRepl, 0);
                    ////xxx.FilterBox(xxx, 30, 30, BorderType.BorderRepl, 0);
                    ////xxx.Not(xxx);
                    ////xxx.AddC(xxx, 1, 0);

                    ////Image yyy = image.Mul(null, xxx, 1);

                    ////Image yyy = image.Div(null, xxx, 8);

                    ////xxx.FilterBox(xxx, 30, 30, BorderType.BorderRepl, 0);

                    ////xxx.Not(xxx);
                    ////Image yyy = image.Add(null, xxx, 0);
                    ////yyy = yyy.Binarize(null).Deskew(null).Despeckle(null);
                    ///

                    ////Image yyy = image.MorphBlackHat(null, StructuringElement.Square(30), 1, BorderType.BorderRepl, 0).Not(null);

                    ////LineDetector.FindLines(yyy, new LineDetectionOptions());


                    ////Image workImage = image.BitsPerPixel > 1 ? image.Binarize(null) : image;

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
                    Console.WriteLine("{0}: {1:F4} ms", kw, stopwatch.ElapsedMilliseconds/* / Count*/);
                }
            }
        }
    }
}
