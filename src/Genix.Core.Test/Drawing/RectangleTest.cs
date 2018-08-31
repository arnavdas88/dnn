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
            Assert.IsFalse(new Rectangle(10, 20, 0, 15).IsEmpty);
            Assert.IsFalse(new Rectangle(10, 20, 5, 0).IsEmpty);
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
        public void ContainsTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.IsFalse(rect.Contains(9, 19));
            Assert.IsFalse(rect.Contains(9, 20));
            Assert.IsFalse(rect.Contains(9, 30));
            Assert.IsFalse(rect.Contains(9, 34));
            Assert.IsFalse(rect.Contains(9, 35));

            Assert.IsFalse(rect.Contains(10, 19));
            Assert.IsTrue(rect.Contains(10, 20));
            Assert.IsTrue(rect.Contains(10, 30));
            Assert.IsTrue(rect.Contains(10, 34));
            Assert.IsFalse(rect.Contains(10, 35));

            Assert.IsFalse(rect.Contains(12, 19));
            Assert.IsTrue(rect.Contains(12, 20));
            Assert.IsTrue(rect.Contains(12, 30));
            Assert.IsTrue(rect.Contains(12, 34));
            Assert.IsFalse(rect.Contains(12, 35));

            Assert.IsFalse(rect.Contains(14, 19));
            Assert.IsTrue(rect.Contains(14, 20));
            Assert.IsTrue(rect.Contains(14, 30));
            Assert.IsTrue(rect.Contains(14, 34));
            Assert.IsFalse(rect.Contains(14, 35));

            Assert.IsFalse(rect.Contains(15, 19));
            Assert.IsFalse(rect.Contains(15, 20));
            Assert.IsFalse(rect.Contains(15, 30));
            Assert.IsFalse(rect.Contains(15, 34));
            Assert.IsFalse(rect.Contains(15, 35));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ContainsTest_WithRectangle()
        {
            // with empty
            Assert.IsFalse(Rectangle.Empty.Contains(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(Rectangle.Empty));

            // with zero area
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 0, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 5, 0)));

            // with itself
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 5, 15)));

            // inside
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 2, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 5, 10)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(12, 20, 3, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 25, 5, 10)));

            // outside
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 0, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(-5, 0, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(30, 0, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 40, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(-5, 40, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(30, 40, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(-5, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(30, 20, 5, 15)));

            // intersections
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 10, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 10, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 10, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(10, 20, 10, 15)));

            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(5, 15, 10, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(15, 15, 10, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(15, 25, 10, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).Contains(new Rectangle(5, 25, 10, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void ContainsTest_Point()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);
            Assert.IsFalse(rect.Contains(new Point(9, 19)));
            Assert.IsFalse(rect.Contains(new Point(9, 20)));
            Assert.IsFalse(rect.Contains(new Point(9, 30)));
            Assert.IsFalse(rect.Contains(new Point(9, 34)));
            Assert.IsFalse(rect.Contains(new Point(9, 35)));

            Assert.IsFalse(rect.Contains(new Point(10, 19)));
            Assert.IsTrue(rect.Contains (new Point(10, 20)));
            Assert.IsTrue(rect.Contains (new Point(10, 30)));
            Assert.IsTrue(rect.Contains (new Point(10, 34)));
            Assert.IsFalse(rect.Contains(new Point(10, 35)));

            Assert.IsFalse(rect.Contains(new Point(12, 19)));
            Assert.IsTrue(rect.Contains (new Point(12, 20)));
            Assert.IsTrue(rect.Contains (new Point(12, 30)));
            Assert.IsTrue(rect.Contains (new Point(12, 34)));
            Assert.IsFalse(rect.Contains(new Point(12, 35)));

            Assert.IsFalse(rect.Contains(new Point(14, 19)));
            Assert.IsTrue(rect.Contains (new Point(14, 20)));
            Assert.IsTrue(rect.Contains (new Point(14, 30)));
            Assert.IsTrue(rect.Contains (new Point(14, 34)));
            Assert.IsFalse(rect.Contains(new Point(14, 35)));

            Assert.IsFalse(rect.Contains(new Point(15, 19)));
            Assert.IsFalse(rect.Contains(new Point(15, 20)));
            Assert.IsFalse(rect.Contains(new Point(15, 30)));
            Assert.IsFalse(rect.Contains(new Point(15, 34)));
            Assert.IsFalse(rect.Contains(new Point(15, 35)));
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
            Assert.IsTrue(rect.ContainsY (20));
            Assert.IsTrue(rect.ContainsY (30));
            Assert.IsTrue(rect.ContainsY (34));
            Assert.IsFalse(rect.ContainsY(35));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToTest()
        {
            // above
            Assert.AreEqual((float)Math.Sqrt(5 * 5), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(10, 0, 5, 15)));
            Assert.AreEqual((float)Math.Sqrt((5 * 5) + (10 * 10)), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(-5, 0, 5, 15)));
            Assert.AreEqual((float)Math.Sqrt((5 * 5) + (15 * 15)), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(30, 0, 5, 15)));

            // below
            Assert.AreEqual((float)Math.Sqrt(5 * 5), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(10, 40, 5, 15)));
            Assert.AreEqual((float)Math.Sqrt((5 * 5) + (10 * 10)), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(-5, 40, 5, 15)));
            Assert.AreEqual((float)Math.Sqrt((5 * 5) + (15 * 15)), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(30, 40, 5, 15)));

            // to the left
            Assert.AreEqual((float)Math.Sqrt(10 * 10), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(-5, 20, 5, 15)));

            // to the right
            Assert.AreEqual((float)Math.Sqrt(15 * 15), new Rectangle(10, 20, 5, 15).DistanceTo(new Rectangle(30, 20, 5, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToSquaredTest()
        {
            // above
            Assert.AreEqual(5 * 5, new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(10, 0, 5, 15)));
            Assert.AreEqual((5 * 5) + (10 * 10), new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(-5, 0, 5, 15)));
            Assert.AreEqual((5 * 5) + (15 * 15), new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(30, 0, 5, 15)));

            // below
            Assert.AreEqual(5 * 5, new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(10, 40, 5, 15)));
            Assert.AreEqual((5 * 5) + (10 * 10), new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(-5, 40, 5, 15)));
            Assert.AreEqual((5 * 5) + (15 * 15), new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(30, 40, 5, 15)));

            // to the left
            Assert.AreEqual(10 * 10, new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(-5, 20, 5, 15)));

            // to the right
            Assert.AreEqual(15 * 15, new Rectangle(10, 20, 5, 15).DistanceToSquared(new Rectangle(30, 20, 5, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToXTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);

            // left
            Assert.AreEqual(5, rect.DistanceToX(new Rectangle(0, 20, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToX(new Rectangle(0, 20, 10, 15)));
            Assert.AreEqual(0, rect.DistanceToX(new Rectangle(0, 20, 12, 15)));

            // right
            Assert.AreEqual(10, rect.DistanceToX(new Rectangle(25, 20, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToX(new Rectangle(15, 20, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToX(new Rectangle(12, 20, 5, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void DistanceToYTest()
        {
            Rectangle rect = new Rectangle(10, 20, 5, 15);

            // top
            Assert.AreEqual(8, rect.DistanceToY(new Rectangle(10, 0, 5, 12)));
            Assert.AreEqual(0, rect.DistanceToY(new Rectangle(10, 0, 5, 20)));
            Assert.AreEqual(0, rect.DistanceToY(new Rectangle(10, 10, 5, 20)));

            // bottom
            Assert.AreEqual(5, rect.DistanceToY(new Rectangle(10, 40, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToY(new Rectangle(10, 35, 5, 15)));
            Assert.AreEqual(0, rect.DistanceToY(new Rectangle(10, 30, 5, 15)));
        }

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
        public void IntersectTest()
        {
            // with empty
            Rectangle rect = Rectangle.Empty;
            rect.Intersect(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(Rectangle.Empty, rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Intersect(Rectangle.Empty);
            Assert.AreEqual(Rectangle.Empty, rect);

            // with zero area
            rect = new Rectangle(10, 20, 0, 15);
            rect.Intersect(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 20, 0, 15), rect);

            rect = new Rectangle(10, 20, 5, 0);
            rect.Intersect(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 0), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Intersect(new Rectangle(10, 20, 0, 15));
            Assert.AreEqual(new Rectangle(10, 20, 0, 15), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Intersect(new Rectangle(10, 20, 5, 0));
            Assert.AreEqual(new Rectangle(10, 20, 5, 0), rect);

            // with itself
            rect = new Rectangle(10, 20, 5, 15);
            rect.Intersect(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            // unions
            rect = new Rectangle(5, 15, 10, 15);
            rect.Intersect(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 10), rect);

            rect = new Rectangle(15, 15, 10, 15);
            rect.Intersect(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(15, 20, 5, 10), rect);

            rect = new Rectangle(15, 25, 10, 15);
            rect.Intersect(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(15, 25, 5, 10), rect);

            rect = new Rectangle(5, 25, 10, 15);
            rect.Intersect(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(10, 25, 5, 10), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Intersect(new Rectangle(5, 15, 10, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 10), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Intersect(new Rectangle(15, 15, 10, 15));
            Assert.AreEqual(new Rectangle(15, 20, 5, 10), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Intersect(new Rectangle(15, 25, 10, 15));
            Assert.AreEqual(new Rectangle(15, 25, 5, 10), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Intersect(new Rectangle(5, 25, 10, 15));
            Assert.AreEqual(new Rectangle(10, 25, 5, 10), rect);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void IntersectTest_Static()
        {
            // with empty
            Assert.AreEqual(Rectangle.Empty, Rectangle.Intersect(Rectangle.Empty, new Rectangle(10, 20, 5, 15)));
            Assert.AreEqual(Rectangle.Empty, Rectangle.Intersect(new Rectangle(10, 20, 5, 15), Rectangle.Empty));

            // with zero area
            Assert.AreEqual(new Rectangle(10, 20, 0, 15), Rectangle.Intersect(new Rectangle(10, 20, 0, 15), new Rectangle(10, 20, 5, 15)));
            Assert.AreEqual(new Rectangle(10, 20, 5, 0), Rectangle.Intersect(new Rectangle(10, 20, 5, 0), new Rectangle(10, 20, 5, 15)));
            Assert.AreEqual(new Rectangle(10, 20, 0, 15), Rectangle.Intersect(new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 0, 15)));
            Assert.AreEqual(new Rectangle(10, 20, 5, 0), Rectangle.Intersect(new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 5, 0)));

            // with itself
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Intersect(new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 5, 15)));

            // unions
            Assert.AreEqual(new Rectangle(10, 20, 5, 10), Rectangle.Intersect(new Rectangle(5, 15, 10, 15), new Rectangle(10, 20, 10, 15)));
            Assert.AreEqual(new Rectangle(15, 20, 5, 10), Rectangle.Intersect(new Rectangle(15, 15, 10, 15), new Rectangle(10, 20, 10, 15)));
            Assert.AreEqual(new Rectangle(15, 25, 5, 10), Rectangle.Intersect(new Rectangle(15, 25, 10, 15), new Rectangle(10, 20, 10, 15)));
            Assert.AreEqual(new Rectangle(10, 25, 5, 10), Rectangle.Intersect(new Rectangle(5, 25, 10, 15), new Rectangle(10, 20, 10, 15)));

            Assert.AreEqual(new Rectangle(10, 20, 5, 10), Rectangle.Intersect(new Rectangle(10, 20, 10, 15), new Rectangle(5, 15, 10, 15)));
            Assert.AreEqual(new Rectangle(15, 20, 5, 10), Rectangle.Intersect(new Rectangle(10, 20, 10, 15), new Rectangle(15, 15, 10, 15)));
            Assert.AreEqual(new Rectangle(15, 25, 5, 10), Rectangle.Intersect(new Rectangle(10, 20, 10, 15), new Rectangle(15, 25, 10, 15)));
            Assert.AreEqual(new Rectangle(10, 25, 5, 10), Rectangle.Intersect(new Rectangle(10, 20, 10, 15), new Rectangle(5, 25, 10, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void IntersectsWithTest()
        {
            // with empty
            Assert.IsFalse(Rectangle.Empty.IntersectsWith(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWith(Rectangle.Empty));

            // with zero area
            Assert.IsFalse(new Rectangle(10, 20, 0, 15).IntersectsWith(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 0).IntersectsWith(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWith(new Rectangle(10, 20, 0, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWith(new Rectangle(10, 20, 5, 0)));

            // with itself
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).IntersectsWith(new Rectangle(10, 20, 5, 15)));

            // intersections
            Assert.IsTrue(new Rectangle(5, 15, 10, 15).IntersectsWith(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(15, 15, 10, 15).IntersectsWith(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(15, 25, 10, 15).IntersectsWith(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(5, 25, 10, 15).IntersectsWith(new Rectangle(10, 20, 10, 15)));

            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWith(new Rectangle(5, 15, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWith(new Rectangle(15, 15, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWith(new Rectangle(15, 25, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWith(new Rectangle(5, 25, 10, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void IntersectsWithXTest()
        {
            // with empty
            Assert.IsFalse(Rectangle.Empty.IntersectsWithX(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWithX(Rectangle.Empty));

            // with zero area
            Assert.IsFalse(new Rectangle(10, 20, 0, 15).IntersectsWithX(new Rectangle(10, 20, 5, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 0).IntersectsWithX(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWithX(new Rectangle(10, 20, 0, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).IntersectsWithX(new Rectangle(10, 20, 5, 0)));

            // with itself
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).IntersectsWithX(new Rectangle(10, 20, 5, 15)));

            // unions
            Assert.IsTrue(new Rectangle(5, 15, 10, 15).IntersectsWithX(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(15, 15, 10, 15).IntersectsWithX(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(15, 25, 10, 15).IntersectsWithX(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(5, 25, 10, 15).IntersectsWithX(new Rectangle(10, 20, 10, 15)));

            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithX(new Rectangle(5, 15, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithX(new Rectangle(15, 15, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithX(new Rectangle(15, 25, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithX(new Rectangle(5, 25, 10, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void IntersectsWithYTest()
        {
            // with empty
            Assert.IsFalse(Rectangle.Empty.IntersectsWithY(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWithY(Rectangle.Empty));

            // with zero area
            Assert.IsTrue(new Rectangle(10, 20, 0, 15).IntersectsWithY(new Rectangle(10, 20, 5, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 0).IntersectsWithY(new Rectangle(10, 20, 5, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).IntersectsWithY(new Rectangle(10, 20, 0, 15)));
            Assert.IsFalse(new Rectangle(10, 20, 5, 15).IntersectsWithY(new Rectangle(10, 20, 5, 0)));

            // with itself
            Assert.IsTrue(new Rectangle(10, 20, 5, 15).IntersectsWithY(new Rectangle(10, 20, 5, 15)));

            // unions
            Assert.IsTrue(new Rectangle(5, 15, 10, 15).IntersectsWithY(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(15, 15, 10, 15).IntersectsWithY(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(15, 25, 10, 15).IntersectsWithY(new Rectangle(10, 20, 10, 15)));
            Assert.IsTrue(new Rectangle(5, 25, 10, 15).IntersectsWithY(new Rectangle(10, 20, 10, 15)));

            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithY(new Rectangle(5, 15, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithY(new Rectangle(15, 15, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithY(new Rectangle(15, 25, 10, 15)));
            Assert.IsTrue(new Rectangle(10, 20, 10, 15).IntersectsWithY(new Rectangle(5, 25, 10, 15)));
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
        public void SerializeTest()
        {
            Rectangle rect1 = new Rectangle(10, 20, 5, 15);
            string s1 = JsonConvert.SerializeObject(rect1);
            Assert.AreEqual("\"10 20 5 15\"", s1);

            Rectangle rect2 = JsonConvert.DeserializeObject<Rectangle>(s1);
            string s2 = JsonConvert.SerializeObject(rect2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void UnionTest()
        {
            // with empty
            Rectangle rect = Rectangle.Empty;
            rect.Union(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(Rectangle.Empty);
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            // with zero area
            rect = new Rectangle(5, 20, 0, 15);
            rect.Union(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), rect);

            rect = new Rectangle(10, 15, 5, 0);
            rect.Union(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(new Rectangle(5, 20, 0, 15));
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(new Rectangle(10, 15, 5, 0));
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), rect);

            // with itself
            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(new Rectangle(10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            // unions
            rect = new Rectangle(5, 15, 10, 15);
            rect.Union(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), rect);

            rect = new Rectangle(15, 15, 10, 15);
            rect.Union(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), rect);

            rect = new Rectangle(15, 25, 10, 15);
            rect.Union(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), rect);

            rect = new Rectangle(5, 25, 10, 15);
            rect.Union(new Rectangle(10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(new Rectangle(5, 15, 10, 15));
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(new Rectangle(15, 15, 10, 15));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(new Rectangle(15, 25, 10, 15));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(new Rectangle(5, 25, 10, 15));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), rect);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void UnionTest_Static()
        {
            // with empty
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(Rectangle.Empty, new Rectangle(10, 20, 5, 15)));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle(10, 20, 5, 15), Rectangle.Empty));

            // with zero area
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), Rectangle.Union(new Rectangle(5, 20, 0, 15), new Rectangle(10, 20, 5, 15)));
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), Rectangle.Union(new Rectangle(10, 15, 5, 0), new Rectangle(10, 20, 5, 15)));
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), Rectangle.Union(new Rectangle(10, 20, 5, 15), new Rectangle(5, 20, 0, 15)));
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), Rectangle.Union(new Rectangle(10, 20, 5, 15), new Rectangle(10, 15, 5, 0)));

            // with itself
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 5, 15)));

            // unions
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), Rectangle.Union(new Rectangle(5, 15, 10, 15), new Rectangle(10, 20, 10, 15)));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), Rectangle.Union(new Rectangle(15, 15, 10, 15), new Rectangle(10, 20, 10, 15)));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), Rectangle.Union(new Rectangle(15, 25, 10, 15), new Rectangle(10, 20, 10, 15)));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), Rectangle.Union(new Rectangle(5, 25, 10, 15), new Rectangle(10, 20, 10, 15)));

            Assert.AreEqual(new Rectangle(5, 15, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), new Rectangle(5, 15, 10, 15)));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), new Rectangle(15, 15, 10, 15)));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), new Rectangle(15, 25, 10, 15)));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), new Rectangle(5, 25, 10, 15)));
        }

        [TestMethod, TestCategory("Rectangle")]
        public void UnionTest_WithArea()
        {
            // with empty
            Rectangle rect = Rectangle.Empty;
            rect.Union(10, 20, 5, 15);
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            rect = Rectangle.Empty;
            rect.Union(10, 20, 5, 15);
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            // with zero area
            rect = new Rectangle(5, 20, 0, 15);
            rect.Union(10, 20, 5, 15);
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), rect);

            rect = new Rectangle(10, 15, 5, 0);
            rect.Union(10, 20, 5, 15);
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(5, 20, 0, 15);
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), rect);

            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(10, 15, 5, 0);
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), rect);

            // with itself
            rect = new Rectangle(10, 20, 5, 15);
            rect.Union(10, 20, 5, 15);
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), rect);

            // unions
            rect = new Rectangle(5, 15, 10, 15);
            rect.Union(10, 20, 10, 15);
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), rect);

            rect = new Rectangle(15, 15, 10, 15);
            rect.Union(10, 20, 10, 15);
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), rect);

            rect = new Rectangle(15, 25, 10, 15);
            rect.Union(10, 20, 10, 15);
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), rect);

            rect = new Rectangle(5, 25, 10, 15);
            rect.Union(10, 20, 10, 15);
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(5, 15, 10, 15);
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(15, 15, 10, 15);
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(15, 25, 10, 15);
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), rect);

            rect = new Rectangle(10, 20, 10, 15);
            rect.Union(5, 25, 10, 15);
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), rect);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void UnionTest_WithArea_Static()
        {
            // with empty
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(Rectangle.Empty, 10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle(10, 20, 5, 15), 0, 0, 0, 0));

            // with zero area
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), Rectangle.Union(new Rectangle(5, 20, 0, 15), 10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), Rectangle.Union(new Rectangle(10, 15, 5, 0), 10, 20, 5, 15));
            Assert.AreEqual(new Rectangle(5, 20, 10, 15), Rectangle.Union(new Rectangle(10, 20, 5, 15), 5, 20, 0, 15));
            Assert.AreEqual(new Rectangle(10, 15, 5, 20), Rectangle.Union(new Rectangle(10, 20, 5, 15), 10, 15, 5, 0));

            // with itself
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle(10, 20, 5, 15), 10, 20, 5, 15));

            // unions
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), Rectangle.Union(new Rectangle(5, 15, 10, 15), 10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), Rectangle.Union(new Rectangle(15, 15, 10, 15), 10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), Rectangle.Union(new Rectangle(15, 25, 10, 15), 10, 20, 10, 15));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), Rectangle.Union(new Rectangle(5, 25, 10, 15), 10, 20, 10, 15));

            Assert.AreEqual(new Rectangle(5, 15, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), 5, 15, 10, 15));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), 15, 15, 10, 15));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), 15, 25, 10, 15));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), Rectangle.Union(new Rectangle(10, 20, 10, 15), 5, 25, 10, 15));
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnionTest_WithArea_Static_NegativeWidth()
        {
            Rectangle.Union(new Rectangle(10, 20, 5, 15), 10, 20, -5, 15);
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnionTest_WithArea_Static_NegativeHeight()
        {
            Rectangle.Union(new Rectangle(10, 20, 5, 15), 10, 20, 5, -15);
        }

        [TestMethod, TestCategory("Rectangle")]
        public void UnionTest_Sequence()
        {
            // with empty
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { Rectangle.Empty, new Rectangle(10, 20, 5, 15) }));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 0, 15), new Rectangle(10, 20, 5, 15) }));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 5, 0), new Rectangle(10, 20, 5, 15) }));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 5, 15), Rectangle.Empty }));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 0, 15) }));
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 5, 0) }));

            // with itself
            Assert.AreEqual(new Rectangle(10, 20, 5, 15), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 5, 15), new Rectangle(10, 20, 5, 15) }));

            // unions
            Assert.AreEqual(new Rectangle(5, 15, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(5, 15, 10, 15), new Rectangle(10, 20, 10, 15) }));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(15, 15, 10, 15), new Rectangle(10, 20, 10, 15) }));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(15, 25, 10, 15), new Rectangle(10, 20, 10, 15) }));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(5, 25, 10, 15), new Rectangle(10, 20, 10, 15) }));

            Assert.AreEqual(new Rectangle(5, 15, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 10, 15), new Rectangle(5, 15, 10, 15) }));
            Assert.AreEqual(new Rectangle(10, 15, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 10, 15), new Rectangle(15, 15, 10, 15) }));
            Assert.AreEqual(new Rectangle(10, 20, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 10, 15), new Rectangle(15, 25, 10, 15) }));
            Assert.AreEqual(new Rectangle(5, 20, 15, 20), Rectangle.Union(new Rectangle[] { new Rectangle(10, 20, 10, 15), new Rectangle(5, 25, 10, 15) }));
        }

        [TestMethod, TestCategory("Rectangle")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnionTest_Sequence_NullArgument()
        {
            Rectangle.Union(null);
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
    }
}
