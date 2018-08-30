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

#if false
        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToTest()
        {
            Rectangle rect1 = new Rectangle(10, 20, 5, 15);
            Rectangle rect2 = new Rectangle(5, 18);
            Assert.AreEqual((float)Math.Sqrt((5 * 5) + (2 * 2)), rect1.DistanceTo(rect2));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToSquaredTest()
        {
            Rectangle rect1 = new Rectangle(10, 20, 5, 15);
            Rectangle rect2 = new Rectangle(5, 18);
            Assert.AreEqual((5 * 5) + (2 * 2), rect1.DistanceToSquared(rect2));
        }
#endif

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
