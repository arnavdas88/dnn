namespace Genix.DocumentAnalysis.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.Imaging;
    using System.Collections.Generic;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            foreach ((Image image, _, _) in Image.FromFile(@"C:\DNN\dnn\test.jpg"))
            {
                Image workImage = Image.Binarize(image);
                workImage = Image.Deskew(workImage);
                ISet<ConnectedComponent> components = LineDetector.FindLines(workImage, new LineDetectionOptions());
            }
        }
    }
}
