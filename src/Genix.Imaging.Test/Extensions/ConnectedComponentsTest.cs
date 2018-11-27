namespace Genix.Imaging.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Genix.Geometry;
    using Genix.Imaging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConnectedComponentsTest
    {
        [TestMethod]
        public void FindConnectedComponentsTest()
        {
            Imaging.Image image = new Imaging.Image(20, 35, 1, 200, 200);

            // 0 0 0 0 0
            // 0 x 0 x 0
            // 0 x x x 0
            // 0 x 0 x 0
            // 0 0 0 0 0
            image.SetPixel(1, 1, 1);
            image.SetPixel(3, 1, 1);
            image.SetPixel(1, 2, 1);
            image.SetPixel(2, 2, 1);
            image.SetPixel(3, 2, 1);
            image.SetPixel(1, 3, 1);
            image.SetPixel(3, 3, 1);

            ISet<ConnectedComponent> components = image.FindConnectedComponents(8);
            Assert.AreEqual(1, components.Count);

            ConnectedComponent component = components.First();

            Assert.AreEqual(new Rectangle(1, 1, 3, 3), component.Bounds);
            Assert.AreEqual(7, component.Power);

            (int y, int x, int length)[] strokes = component.EnumStrokes().ToArray();
            Assert.AreEqual(5, strokes.Length);
            Assert.AreEqual((1, 1, 1), strokes[0]);
            Assert.AreEqual((1, 3, 1), strokes[1]);
            Assert.AreEqual((2, 1, 3), strokes[2]);
            Assert.AreEqual((3, 1, 1), strokes[3]);
            Assert.AreEqual((3, 3, 1), strokes[4]);
        }

        [TestMethod]
        public void RemoveConnectedComponentTest()
        {
            Imaging.Image image = new Imaging.Image(20, 35, 1, 200, 200);
            image.SetPixel(1, 1, 1);

            ISet<Imaging.ConnectedComponent> components = image.FindConnectedComponents(8);
            Assert.AreEqual(1, components.Count);

            image.RemoveConnectedComponent(components.First());

            Assert.AreEqual(0u, image.GetPixel(0, 0));
            Assert.AreEqual(0ul, image.Power());
        }
    }
}
