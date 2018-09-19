namespace Genix.Imaging.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScaleTest
    {
        [TestMethod]
        public void ScaleToSizeTest()
        {
            Image image = new Image(118, 133, 1, 200, 200);
            image = Image.SetWhite(image);

            image = image.ScaleToSize(113, 124, ScalingOptions.None);
        }
    }
}
