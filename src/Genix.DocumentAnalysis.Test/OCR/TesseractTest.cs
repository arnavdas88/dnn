
namespace Genix.DocumentAnalysis.OCR.Test
{
    using System;
    using System.Drawing;
    using System.Windows;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.DocumentAnalysis.OCR.Tesseract;
    using Genix.Imaging;
    using Genix.Imaging.Leptonica;
    using Genix.Drawing;
    using System.Collections.Generic;

    [TestClass]
    public class TesseractTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (Canvas canvas = new Canvas(100, 50))
            {
                using (Tesseract tess = Tesseract.Create(null))
                {
                    canvas.DrawText("TEST", new System.Drawing.Rectangle(0, 0, 100, 50), HorizontalAlignment.Left);

                    ////Imaging.Image image = canvas.ToImage(Rectangle.Empty);
                    ////Pix pix = Pix.FromImage(image/*.Convert1To8()*/);
                    ////Bitmap bitmap = pix.ToBitmap();

                    ////Imaging.Image image = canvas.ToImage(Rectangle.Empty/*new Rectangle(0, 0, 100, 50)*/);
                    ////tess.SetImage(image.Convert1To8());

                    foreach ((Imaging.Image image, _, _) in Imaging.Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
                    {
                        PageShape answer = tess.Recognize(image.Convert1To8(null), PageSegmentationMode.PSM_AUTO_OSD);
                    }
                }
            }
        }
    }
}
