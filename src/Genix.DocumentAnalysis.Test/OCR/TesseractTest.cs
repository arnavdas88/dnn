
namespace Genix.DocumentAnalysis.OCR.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.DocumentAnalysis.OCR.Tesseract;
    using Genix.Imaging;

    [TestClass]
    public class TesseractTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (Tesseract tess = Tesseract.Create(null))
            {
                foreach ((Image image, int? frameIndex, _) in Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
                {
                    tess.SetImage(image.Convert1To8());
                }
            }
        }
    }
}
