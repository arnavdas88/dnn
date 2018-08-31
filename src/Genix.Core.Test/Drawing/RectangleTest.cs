namespace Genix.Drawing.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class RectangleTest
    {
        [TestMethod, TestCategory("Rectangle")]
        public void ConstructorTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.AreEqual(10, rect.X);
            Assert.AreEqual(20, rect.Y);
            Assert.AreEqual(5, rect.Width);
            Assert.AreEqual(15, rect.Height);
            Assert.AreEqual(10, rect.Left);
            Assert.AreEqual(20, rect.Top);
            Assert.AreEqual(15, rect.Right);
            Assert.AreEqual(35, rect.Bottom);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest_NegativeWidth()
        {
            new Rectangle(10, 20, -5, 15);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest_NegativeHeight()
        {
            new Rectangle(10, 20, 5, -15);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ConstructorTest_SystemDrawingRectangle()
        {
            Rectangle rect = new Rectangle(new System.Drawing.Rectangle(10, 20, 5, 15));
            Assert.AreEqual(10, rect.X);
            Assert.AreEqual(20, rect.Y);
            Assert.AreEqual(5, rect.Width);
            Assert.AreEqual(15, rect.Height);
            Assert.AreEqual(10, rect.Left);
            Assert.AreEqual(20, rect.Top);
            Assert.AreEqual(15, rect.Right);
            Assert.AreEqual(35, rect.Bottom);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest_SystemDrawingRectangle_NegativeWidth()
        {
            new Rectangle(new System.Drawing.Rectangle(10, 20, -5, 15));
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest_SystemDrawingRectangle_NegativeHeight()
        {
            new Rectangle(new System.Drawing.Rectangle(10, 20, 5, -15));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void XTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.AreEqual(10, rect.X);
            Assert.AreEqual(15, rect.Right);
            rect.X = 15;
            Assert.AreEqual(15, rect.X);
            Assert.AreEqual(20, rect.Right);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void YTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.AreEqual(20, rect.Y);
            Assert.AreEqual(35, rect.Bottom);
            rect.Y = 25;
            Assert.AreEqual(25, rect.Y);
            Assert.AreEqual(40, rect.Bottom);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void WidthTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Width = 15;
            Assert.AreEqual(15, rect.Width);
            Assert.AreEqual(25, rect.Right);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WidthTest_NegativeWidth()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Width = -15;
        }

        [TestMethod, TestCategory("Rectangle")]
        public void HeightTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Height = 20;
            Assert.AreEqual(20, rect.Height);
            Assert.AreEqual(40, rect.Bottom);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HeightTest_NegativeHeight()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Height = -15;
        }

        [TestMethod, TestCategory("Rectangle")]
        public void IsEmptyTest()
        {
            Assert.IsTrue(Rectangle.Empty.IsEmpty);
            Assert.IsTrue(new Rectangle(0, 0, 0, 0).IsEmpty);
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IsEmpty);
            Assert.IsTrue(new Rectangle(10, 20, 0, 15).IsEmpty);
            Assert.IsTrue(new Rectangle(10, 20, 5, 0).IsEmpty);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void EqualsTest()
        {
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 21, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(11, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 20, 6, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 20, 5, 16)));

            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(0));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 20, 5, 15) as object));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 21, 5, 15) as object));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(11, 20, 5, 15) as object));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 20, 6, 15) as object));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Equals(new Rectangle(10, 20, 5, 16) as object));

            Assert.IsTrue(new Rectangle(10, 20, 5, 15) == new Rectangle(10, 20, 5, 15));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15) == new Rectangle(10, 21, 5, 15));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15) == new Rectangle(11, 20, 5, 15));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15) == new Rectangle(10, 20, 6, 15));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15) == new Rectangle(10, 20, 5, 16));

            Assert.IsFalse(new Rectangle(10, 20, 5, 15) != new Rectangle(10, 20, 5, 15));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15) != new Rectangle(10, 21, 5, 15));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15) != new Rectangle(11, 20, 5, 15));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15) != new Rectangle(10, 20, 6, 15));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15) != new Rectangle(10, 20, 5, 16));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void GetHashCodeTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.AreEqual(10 ^ 20 ^ 5 ^ 15, rect.GetHashCode());
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ToStringTest()
        {
            Assert.AreEqual("10 20 5 15", new Rectangle(10, 20, 5, 15).ToString());
            Assert.AreEqual("-10 -20 5 15", new Rectangle(-10, -20, 5, 15).ToString());
        }

        [TestMethod, TestCategory("Rectangle")]
        public void FromLTRBTest()
        {
            Rectangle rect = Rectangle.FromLTRB(10, 20, 15, 35);
            Assert.AreEqual(10, rect.X);
            Assert.AreEqual(20, rect.Y);
            Assert.AreEqual(5, rect.Width);
            Assert.AreEqual(15, rect.Height);
            Assert.AreEqual(10, rect.Left);
            Assert.AreEqual(20, rect.Top);
            Assert.AreEqual(15, rect.Right);
            Assert.AreEqual(35, rect.Bottom);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FromLTRB_NegativeWidth()
        {
            Rectangle.FromLTRB(10, 20, 5, 25);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FromLTRB_NegativeHeight()
        {
            Rectangle.FromLTRB(10, 20, 15, 15);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void SetTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Set(15, 25, 100, 200);
            Assert.AreEqual(15, rect.X);
            Assert.AreEqual(25, rect.Y);
            Assert.AreEqual(100, rect.Width);
            Assert.AreEqual(200, rect.Height);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetTest_NegativeWidth()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Set(15, 25, -100, 200);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetTest_NegativeHeight()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Set(15, 25, 100, -200);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ClearTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Clear();
            Assert.AreEqual(0, rect.X);
            Assert.AreEqual(0, rect.Y);
            Assert.AreEqual(0, rect.Width);
            Assert.AreEqual(0, rect.Height);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ParseTest()
        {
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Parse("10 20 5 15"));
            Assert.AreEqual(new Rectangle(-10, -20, 5, 15), Rectangle.Parse("-10 -20 5 15"));
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParseTest_NullArgument()
        {
            Rectangle.Parse(null);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument1()
        {
            Rectangle.Parse(string.Empty);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument2()
        {
            Rectangle.Parse("1 2 3");
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument3()
        {
            Rectangle.Parse("1 A 2 3");
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument4()
        {
            Rectangle.Parse("1 2 3 4 5");
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument5()
        {
            Rectangle.Parse("1.0 2 3 4");
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ContainsXTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.IsFalse(rect.ContainsX(9));
            Assert.IsTrue(rect.ContainsX(10));
            Assert.IsTrue(rect.ContainsX(12));
            Assert.IsTrue(rect.ContainsX(14));
            Assert.IsFalse(rect.ContainsX(15));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ContainsYTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.IsFalse(rect.ContainsY(19));
            Assert.IsTrue(rect.ContainsY(20));
            Assert.IsTrue(rect.ContainsY(30));
            Assert.IsTrue(rect.ContainsY(34));
            Assert.IsFalse(rect.ContainsY(35));
        }

#if false
        [TestMethod, TestCategory("Rectangle")]
        public void ScaleTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Scale(2, 3);
            Assert.AreEqual(20, rect.X);
            Assert.AreEqual(60, rect.Y);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ScaleTest_Static()
        {
            Rectangle rect = Rectangle.Scale(new Rectangle(10, 20, 5, 15), 2, 3);
            Assert.AreEqual(20, rect.X);
            Assert.AreEqual(60, rect.Y);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ScaleTest_Float()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Scale(0.45f, 0.31f);
            Assert.AreEqual(5, rect.X);
            Assert.AreEqual(6, rect.Y);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ScaleTest_Float_Static()
        {
            Rectangle rect = Rectangle.Scale(new Rectangle(10, 20, 5, 15), 0.45f, 0.31f);
            Assert.AreEqual(5, rect.X);
            Assert.AreEqual(6, rect.Y);
        }
#endif

        [TestMethod, TestCategory("Rectangle")]
        public void InflateTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Inflate(1, 2);
            Assert.AreEqual(9, rect.X);
            Assert.AreEqual(18, rect.Y);
            Assert.AreEqual(7, rect.Width);
            Assert.AreEqual(19, rect.Height);

            rect.Inflate(-1, -2);
            Assert.AreEqual(10, rect.X);
            Assert.AreEqual(20, rect.Y);
            Assert.AreEqual(5, rect.Width);
            Assert.AreEqual(15, rect.Height);

            rect.Inflate(-3, -8);
            Assert.AreEqual(13, rect.X);
            Assert.AreEqual(28, rect.Y);
            Assert.AreEqual(0, rect.Width);
            Assert.AreEqual(0, rect.Height);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void InflateTest_Static()
        {
            Rectangle rect = Rectangle.Inflate(new Rectangle(10, 20, 5, 15), 1, 2);
            Assert.AreEqual(9, rect.X);
            Assert.AreEqual(18, rect.Y);
            Assert.AreEqual(7, rect.Width);
            Assert.AreEqual(19, rect.Height);

            rect = Rectangle.Inflate(rect, -1, -2);
            Assert.AreEqual(10, rect.X);
            Assert.AreEqual(20, rect.Y);
            Assert.AreEqual(5, rect.Width);
            Assert.AreEqual(15, rect.Height);

            rect = Rectangle.Inflate(rect, -3, -8);
            Assert.AreEqual(13, rect.X);
            Assert.AreEqual(28, rect.Y);
            Assert.AreEqual(0, rect.Width);
            Assert.AreEqual(0, rect.Height);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void OffsetTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            rect.Offset(1, 2);
            Assert.AreEqual(11, rect.X);
            Assert.AreEqual(22, rect.Y);
            rect.Offset(new Point(1, 2));
            Assert.AreEqual(12, rect.X);
            Assert.AreEqual(24, rect.Y);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void OffsetTest_Static()
        {
            Rectangle rect = Rectangle.Offset(new Rectangle(10, 20, 5, 15), 1, 2);
            Assert.AreEqual(11, rect.X);
            Assert.AreEqual(22, rect.Y);

            rect = Rectangle.Offset(rect, new Point(1, 2));
            Assert.AreEqual(12, rect.X);
            Assert.AreEqual(24, rect.Y);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToXTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            // left
            Assert.AreEqual(5, rect.DistanceToX(new Rectangle(0, 20, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToX(new Rectangle(0, 20, 10, 15)));
            // right
            Assert.AreEqual(10, rect.DistanceToX(new Rectangle(25, 20, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToX(new Rectangle(15, 20, 5, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToYTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            // left
            Assert.AreEqual(8, rect.DistanceToY(new Rectangle(10, 0, 5, 12)));
            Assert.AreEqual(0, rect.DistanceToY(new Rectangle(10, 0, 5, 20)));
            // right
            Assert.AreEqual(5, rect.DistanceToY(new Rectangle(10, 40, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToY(new Rectangle(10, 35, 5, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void SerializeTest()
        {
            Rectangle rect1 = new Rectangle(10, 20, 5, 15);
            string s1 = JsonConvert.SerializeObject(rect1);
            Assert.AreEqual("\"10 20 5 15\"", s1);

            Rectangle rect2 = JsonConvert.DeserializeObject<Rectangle>(s1);
            string s2 = JsonConvert.SerializeObject(rect2);
            Assert.AreEqual(s1, s2);
        }
    }
}
