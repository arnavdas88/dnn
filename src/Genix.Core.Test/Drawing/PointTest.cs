namespace Genix.Drawing.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class PointTest
    {
        [TestMethod, TestCategory("Point")]
        public void ConstructorTest()
        {
            Point point = new Point(10, 20);
            Assert.AreEqual(10, point.X);
            Assert.AreEqual(20, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void ConstructorTest_SystemDrawingPoint()
        {
            Point point = new Point(new System.Drawing.Point(10, 20));
            Assert.AreEqual(10, point.X);
            Assert.AreEqual(20, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void XTest()
        {
            Point point = new Point(10, 20);
            Assert.AreEqual(10, point.X);
            point.X = 15;
            Assert.AreEqual(15, point.X);
        }

        [TestMethod, TestCategory("Point")]
        public void YTest()
        {
            Point point = new Point(10, 20);
            Assert.AreEqual(20, point.Y);
            point.Y = 15;
            Assert.AreEqual(15, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void IsEmptyTest()
        {
            Assert.IsTrue(Point.Empty.IsEmpty);
            Assert.IsTrue(new Point(0, 0).IsEmpty);
            Assert.IsFalse(new Point(10, 20).IsEmpty);
        }

        [TestMethod, TestCategory("Point")]
        public void EqualsTest()
        {
            Assert.IsTrue(new Point(10, 20).Equals(new Point(10, 20)));
            Assert.IsFalse(new Point(10, 20).Equals(new Point(10, 21)));
            Assert.IsFalse(new Point(10, 20).Equals(new Point(11, 20)));

            Assert.IsFalse(new Point(10, 20).Equals(0));
            Assert.IsTrue(new Point(10, 20).Equals(new Point(10, 20) as object));
            Assert.IsFalse(new Point(10, 20).Equals(new Point(10, 21) as object));
            Assert.IsFalse(new Point(10, 20).Equals(new Point(11, 20) as object));

            Assert.IsTrue(new Point(10, 20) == new Point(10, 20));
            Assert.IsFalse(new Point(10, 20) == new Point(10, 21));
            Assert.IsFalse(new Point(10, 20) == new Point(11, 20));

            Assert.IsFalse(new Point(10, 20) != new Point(10, 20));
            Assert.IsTrue(new Point(10, 20) != new Point(10, 21));
            Assert.IsTrue(new Point(10, 20) != new Point(11, 20));
        }

        [TestMethod, TestCategory("Point")]
        public void GetHashCodeTest()
        {
            Point point = new Point(10, 20);
            Assert.AreEqual(10 ^ 20, point.GetHashCode());
        }

        [TestMethod, TestCategory("Point")]
        public void ToStringTest()
        {
            Assert.AreEqual("10 20", new Point(10, 20).ToString());
            Assert.AreEqual("-10 -20", new Point(-10, -20).ToString());
        }

        [TestMethod, TestCategory("Point")]
        public void SetTest()
        {
            Point point = new Point(10, 20);
            point.Set(15, 25);
            Assert.AreEqual(15, point.X);
            Assert.AreEqual(25, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void ClearTest()
        {
            Point point = new Point(10, 20);
            point.Clear();
            Assert.AreEqual(0, point.X);
            Assert.AreEqual(0, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void ParseTest()
        {
            Assert.AreEqual(new Point(10, 20), Point.Parse("10 20"));
            Assert.AreEqual(new Point(-10, -20), Point.Parse("-10 -20"));
        }

        [TestMethod, TestCategory("Point")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParseTest_NullArgument()
        {
            Point.Parse(null);
        }

        [TestMethod, TestCategory("Point")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument1()
        {
            Point.Parse(string.Empty);
        }

        [TestMethod, TestCategory("Point")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument2()
        {
            Point.Parse("1");
        }

        [TestMethod, TestCategory("Point")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument3()
        {
            Point.Parse("1 A");
        }

        [TestMethod, TestCategory("Point")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument4()
        {
            Point.Parse("1 2 3");
        }

        [TestMethod, TestCategory("Point")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseTest_InvalidArgument5()
        {
            Point.Parse("1.0 2");
        }

        [TestMethod, TestCategory("Point")]
        public void ScaleTest()
        {
            Point point = new Point(10, 20);
            point.Scale(2, 3);
            Assert.AreEqual(20, point.X);
            Assert.AreEqual(60, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void ScaleTest_Static()
        {
            Point point = Point.Scale(new Point(10, 20), 2, 3);
            Assert.AreEqual(20, point.X);
            Assert.AreEqual(60, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void ScaleTest_Float()
        {
            Point point = new Point(10, 20);
            point.Scale(0.45f, 0.31f);
            Assert.AreEqual(5, point.X);
            Assert.AreEqual(6, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void ScaleTest_Float_Static()
        {
            Point point = Point.Scale(new Point(10, 20), 0.45f, 0.31f);
            Assert.AreEqual(5, point.X);
            Assert.AreEqual(6, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void OffsetTest()
        {
            Point point = new Point(10, 20);
            point.Offset(1, 2);
            Assert.AreEqual(11, point.X);
            Assert.AreEqual(22, point.Y);
            point.Offset(new Point(1, 2));
            Assert.AreEqual(12, point.X);
            Assert.AreEqual(24, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void OffsetTest_Static()
        {
            Point point = Point.Offset(new Point(10, 20), 1, 2);
            Assert.AreEqual(11, point.X);
            Assert.AreEqual(22, point.Y);

            point = Point.Offset(point, new Point(1, 2));
            Assert.AreEqual(12, point.X);
            Assert.AreEqual(24, point.Y);
        }

        [TestMethod, TestCategory("Point")]
        public void DistanceToTest()
        {
            Point point1 = new Point(10, 20);
            Point point2 = new Point(5, 18);
            Assert.AreEqual((float)Math.Sqrt((5 * 5) + (2 * 2)), point1.DistanceTo(point2));
        }

        [TestMethod, TestCategory("Point")]
        public void DistanceToXTest()
        {
            Point point1 = new Point(10, 20);
            Point point2 = new Point(5, 18);
            Assert.AreEqual(5, point1.DistanceToX(point2));
        }

        [TestMethod, TestCategory("Point")]
        public void DistanceToYTest()
        {
            Point point1 = new Point(10, 20);
            Point point2 = new Point(5, 18);
            Assert.AreEqual(2, point1.DistanceToY(point2));
        }

        [TestMethod, TestCategory("Point")]
        public void DistanceToSquaredTest()
        {
            Point point1 = new Point(10, 20);
            Point point2 = new Point(5, 18);
            Assert.AreEqual((5 * 5) + (2 * 2), point1.DistanceToSquared(point2));
        }

        [TestMethod, TestCategory("Point")]
        public void SerializeTest()
        {
            Point point1 = new Point(10, 20);
            string s1 = JsonConvert.SerializeObject(point1);
            Assert.AreEqual("\"10 20\"", s1);

            Point point2 = JsonConvert.DeserializeObject<Point>(s1);
            string s2 = JsonConvert.SerializeObject(point2);
            Assert.AreEqual(s1, s2);
        }
    }
}
