namespace Genix.Imaging.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Imaging;

    [TestClass]
    public class ConnectedComponentsTest
    {
        [TestMethod]
        public void RemoveConnectedComponentTest()
        {
            Image image = new Image(20, 35, 1, 200, 200);
            image.SetPixel(1, 1, 1);

            ISet<Imaging.ConnectedComponent> components = image.FindConnectedComponents();
            Assert.AreEqual(1, components.Count);

            image.RemoveConnectedComponent(components.First());

            Assert.AreEqual(0u, image.GetPixel(0, 0));
            Assert.AreEqual(0, image.Power());
        }
    }
}
