#if false
namespace Accord.DNN.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class LayoutTest
    {
        [TestMethod]
        public void ConstructorTest2()
        {
            Layout layout = new Layout(2, 3, 4, 5);
            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5 }, layout.Axes);
            CollectionAssert.AreEqual(new[] { 60, 20, 5, 1 }, layout.Strides);
            Assert.AreEqual(120, layout.Length);
        }

        [TestMethod]
        public void ConstructorTest6()
        {
            Layout layout1 = new Layout(2, 3, 4, 5);
            Layout layout2 = new Layout(layout1);
            Assert.AreEqual(layout1, layout2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest7()
        {
            Assert.IsNotNull(new Layout((Layout)null));
        }

        [TestMethod]
        public void ConstructorTest9()
        {
            Layout layout1 = new Layout(2, 3, 4, 5);
            Layout layout2 = Layout.Reshape(layout1, (int)TensorAxis.B, 3);
            Assert.AreNotEqual(layout1, layout2);
            CollectionAssert.AreEqual(new[] { 3, 3, 4, 5 }, layout2.Axes);
            CollectionAssert.AreEqual(new[] { 3 * 4 * 5, 4 * 5, 5, 1 }, layout2.Strides);
            Assert.AreEqual(3 * 3 * 4 * 5, layout2.Length);
        }

        [TestMethod]
        public void PositionTest1()
        {
            Layout layout = new Layout(2, 3, 4, 5);
            Assert.AreEqual((1 * 5) + (2 * 1), layout.Position(0, 0, 1, 2));
            Assert.AreEqual((1 * 4 * 5) + (2 * 5) + (3 * 1), layout.Position(0, 1, 2, 3));
            Assert.AreEqual((1 * 3 * 4 * 5) + (1 * 4 * 5) + (2 * 5) + (3 * 1), layout.Position(1, 1, 2, 3));
        }

        [TestMethod]
        public void EqualsTest()
        {
            Layout layout = new Layout(2, 3, 4, 5);

            Assert.IsFalse(layout.Equals(null));
            Assert.IsTrue(layout.Equals(layout));
            Assert.IsFalse(layout.Equals(new { x = 1 }));
            Assert.IsFalse(layout.Equals(new Layout(2, 3, 4, 6)));
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            Layout layout = new Layout(2, 3, 4, 5);

            Assert.AreEqual(layout.Length, layout.GetHashCode());
        }

        [TestMethod]
        public void SerializeTest()
        {
            Layout layout1 = new Layout(2, 3, 4, 5);
            string s = JsonConvert.SerializeObject(layout1);
            Layout layout2 = JsonConvert.DeserializeObject<Layout>(s);
            Assert.AreEqual(layout1, layout2);
        }
    }
}
#endif