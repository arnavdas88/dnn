namespace Genix.Imaging.Test
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.Core;
    using Genix.Imaging;

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

            ISet<ConnectedComponent> components = image.FindConnectedComponents();
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

            ISet<Imaging.ConnectedComponent> components = image.FindConnectedComponents();
            Assert.AreEqual(1, components.Count);

            image.RemoveConnectedComponent(components.First());

            Assert.AreEqual(0u, image.GetPixel(0, 0));
            Assert.AreEqual(0, image.Power());
        }

        [TestMethod]
        public void XXXTest()
        {
            const int Count = 1;
            Stopwatch stopwatch = new Stopwatch();

            foreach ((Imaging.Image image, int? frameIndex, _) in Imaging.Image.FromFile(@"L:\FormXtra\HCFA\BW\SET1\07227200002.tif"))
            {
                ////Histogram hist1 = image.HistogramY();

                Imaging.Image workImage = image.Despeckle().CleanBorderNoise(0.5f, 0.5f)/*.Reduce1x2()*/;

                stopwatch.Start();
                for (int i = 0; i < Count; i++)
                {
                    ISet<ConnectedComponent> components = workImage.FindConnectedComponents();

                    Histogram histp = ConnectedComponent.PowerHistogram(1000, components);
                    Histogram histw = ConnectedComponent.WidthHistogram(100, components);
                    Histogram histh = ConnectedComponent.HeightHistogram(100, components);

                    QuadTree<ConnectedComponent> quadtree = new QuadTree<ConnectedComponent>(workImage.Bounds, components.Where(x => x.Bounds.Height < 100));

                    Imaging.Image show = new Imaging.Image(workImage.Width, workImage.Height, 1, 200, 200);
                    Imaging.Image show2 = workImage.Copy();

                    int argmaxh = histh.ArgMax();
                    while (true)
                    {
                        ConnectedComponent candidate = components.FirstOrDefault(x => x.Bounds.Height == argmaxh);
                        if (candidate == null)
                        {
                            break;
                        }

                        IList<ConnectedComponent> neighbors = quadtree.GetNodes(new Rectangle(0, candidate.Bounds.Y, workImage.Width, candidate.Bounds.Height)).ToList();

                        show.AddConnectedComponents(neighbors);
                        show2.RemoveConnectedComponents(neighbors);

                        components.ExceptWith(neighbors);
                        quadtree.Remove(neighbors);
                    }

                    ////IList<ConnectedComponentOld> components2 = workImage.FindConnectedComponentsOld();
                    ////workImage.RemoveConnectedComponents(components);
                    ////List<ConnectedComponent> components1 = workImage.FindConnectedComponents().OrderBy(x => x, new ConnectedComponentComparer()).ToList();
                    ////List<ConnectedComponent> components2 = workImage.FindConnectedComponentsOld().OrderBy(x => x, new ConnectedComponentComparer()).ToList();

                    /*Histogram hist = ConnectedComponent.PowerHistogram(components);
                    Histogram histH = new Histogram(workImage.Bounds.Height + 1, components.Select(x => x.Bounds.Height));*/

                    int sum1 = image.Power();
                    int sum2 = components.Sum(x => x.Power);
                    /*int sum3 = components2.Sum(x => x.Power);*/

                    /*QuadTree<ConnectedComponent> quadtree = new QuadTree<ConnectedComponent>(workImage.Bounds, components);

                    int count = quadtree.GetNodes().Count();

                    Histogram hist2 = ConnectedComponent.HistogramY(workImage.Bounds, components.Where(x => x.Bounds.Width <= 30 && x.Bounds.Height <= 30));
                    ////CollectionAssert.AreEqual(hist1.Bins, hist2.Bins);*/
                }

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds / Count);

                ////int sum1 = image.Power();
                ////int sum2 = components.Sum(x => x.Power);
            }
        }

        private sealed class ConnectedComponentComparer : IComparer<ConnectedComponent>
        {
            public int Compare(ConnectedComponent x, ConnectedComponent y)
            {
                int res = x.Bounds.Y - y.Bounds.Y;
                return res != 0 ? res : x.Bounds.X - y.Bounds.X;
            }
        }
    }
}
