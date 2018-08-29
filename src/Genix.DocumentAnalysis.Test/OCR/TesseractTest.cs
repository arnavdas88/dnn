
namespace Genix.DocumentAnalysis.OCR.Test
{
    using System;
    using System.Drawing;
    using System.Windows;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.DocumentAnalysis.OCR.Tesseract;
    using Genix.Imaging;

    [TestClass]
    public class TesseractTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (Canvas canvas = new Canvas(100, 50))
            {
                ////using (Tesseract tess = Tesseract.Create(null))
                {
                    canvas.DrawText("TEST", new Rectangle(0, 0, 100, 50), HorizontalAlignment.Left);

                    ////Bitmap bitmap = canvas.ToBitmap();
                    Imaging.Image image = canvas.ToImage(Rectangle.Empty/*new Rectangle(0, 0, 100, 50)*/);
                    ////tess.SetImage(image.Convert1To8());

                    /*foreach ((Image image, int? frameIndex, _) in Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
                    {
                        tess.SetImage(image.Convert1To8());
                    }*/
                }
            }
        }
    }
}
