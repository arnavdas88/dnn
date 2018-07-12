﻿namespace Genix.Imaging.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CopyCropTest
    {
        [TestMethod]
        public void CopyTest1()
        {
            foreach (int bitsPerPixel in new[] { 1, 2, 4, 8, 16, 32 })
            {
                Image image = new Image(87, 43, bitsPerPixel, 200, 200);
                image.Randomize();

                Image copyImage = image.Copy();

                Assert.AreEqual(image.Width, copyImage.Width);
                Assert.AreEqual(image.Height, copyImage.Height);
                Assert.AreEqual(image.BitsPerPixel, copyImage.BitsPerPixel);
                Assert.AreEqual(image.HorizontalResolution, copyImage.HorizontalResolution);
                Assert.AreEqual(image.VerticalResolution, copyImage.VerticalResolution);
                CollectionAssert.AreEqual(image.Bits, copyImage.Bits);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void CopyTestNull1()
        {
            Assert.IsNull(CopyCrop.Copy(null));
        }
    }
}