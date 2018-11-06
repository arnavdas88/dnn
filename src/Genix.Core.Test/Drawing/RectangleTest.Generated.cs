﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 11/5/2018 12:05:56 AM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Drawing.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class RectangleTest
    {

        [TestMethod]
        public void CenterXTest_int()
        {
            Rectangle rect = new Rectangle(0, 2, 7, 9);
            Assert.AreEqual((int)3, rect.CenterX);

            rect = new Rectangle(0, 2, 8, 10);
            Assert.AreEqual((int)3, rect.CenterX);
        }

        [TestMethod]
        public void CenterYTest_int()
        {
            Rectangle rect = new Rectangle(0, 2, 7, 9);
            Assert.AreEqual((int)6, rect.CenterY);

            rect = new Rectangle(0, 2, 8, 10);
            Assert.AreEqual((int)6, rect.CenterY);
        }

        [TestMethod]
        public void CenterTest_int()
        {
            Rectangle rect = new Rectangle(0, 2, 7, 9);
            Assert.AreEqual((int)3, rect.CenterX);
            Assert.AreEqual((int)6, rect.CenterY);

            rect = new Rectangle(0, 2, 8, 10);
            Assert.AreEqual((int)3, rect.CenterX);
            Assert.AreEqual((int)6, rect.CenterY);
        }

        [TestMethod]
        public void CenterXTest_float()
        {
            RectangleF rect = new RectangleF(0, 2, 7, 9);
            Assert.AreEqual((float)3, rect.CenterX);

            rect = new RectangleF(0, 2, 8, 10);
            Assert.AreEqual((float)3.5, rect.CenterX);
        }

        [TestMethod]
        public void CenterYTest_float()
        {
            RectangleF rect = new RectangleF(0, 2, 7, 9);
            Assert.AreEqual((float)6, rect.CenterY);

            rect = new RectangleF(0, 2, 8, 10);
            Assert.AreEqual((float)6.5, rect.CenterY);
        }

        [TestMethod]
        public void CenterTest_float()
        {
            RectangleF rect = new RectangleF(0, 2, 7, 9);
            Assert.AreEqual((float)3, rect.CenterX);
            Assert.AreEqual((float)6, rect.CenterY);

            rect = new RectangleF(0, 2, 8, 10);
            Assert.AreEqual((float)3.5, rect.CenterX);
            Assert.AreEqual((float)6.5, rect.CenterY);
        }

        [TestMethod]
        public void CenterXTest_double()
        {
            RectangleD rect = new RectangleD(0, 2, 7, 9);
            Assert.AreEqual((double)3, rect.CenterX);

            rect = new RectangleD(0, 2, 8, 10);
            Assert.AreEqual((double)3.5, rect.CenterX);
        }

        [TestMethod]
        public void CenterYTest_double()
        {
            RectangleD rect = new RectangleD(0, 2, 7, 9);
            Assert.AreEqual((double)6, rect.CenterY);

            rect = new RectangleD(0, 2, 8, 10);
            Assert.AreEqual((double)6.5, rect.CenterY);
        }

        [TestMethod]
        public void CenterTest_double()
        {
            RectangleD rect = new RectangleD(0, 2, 7, 9);
            Assert.AreEqual((double)3, rect.CenterX);
            Assert.AreEqual((double)6, rect.CenterY);

            rect = new RectangleD(0, 2, 8, 10);
            Assert.AreEqual((double)3.5, rect.CenterX);
            Assert.AreEqual((double)6.5, rect.CenterY);
        }
    }
}