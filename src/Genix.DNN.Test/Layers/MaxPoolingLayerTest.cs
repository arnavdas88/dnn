namespace Genix.DNN.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Genix.Core;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class MaxPoolingLayerTest
    {
        private static Shape shape = new Shape(Shape.BWHC, -1, 5, 4, 2);

        private readonly RandomNumberGenerator<float> random = new RandomGeneratorF();

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ConstructorTest1()
        {
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, new Kernel(2, 2, 2, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape.Axes);
            Assert.AreEqual("MP2", layer.Architecture);
            Assert.AreEqual(2, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ConstructorTest2()
        {
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, new Kernel(3, 2, 1, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape.Axes);
            Assert.AreEqual("MP3x2+1x2(S)", layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(1, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest1()
        {
            string architecture = "MP3x2";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(3, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "MP3x2+2(S)";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest3()
        {
            string architecture = "MP3x2+2x1(S)";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(1, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest4()
        {
            string architecture = "MP2";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(2, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest5()
        {
            string architecture = "MP";
            try
            {
                MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.shape, architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest6()
        {
            Assert.IsNotNull(new MaxPoolingLayer(null, "MP2", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("MaxPoolingLayer")]
        public void ArchitectureConstructorTest7()
        {
            Assert.IsNotNull(new MaxPoolingLayer(MaxPoolingLayerTest.shape, null, null));
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void CopyConstructorTest1()
        {
            MaxPoolingLayer layer1 = new MaxPoolingLayer(MaxPoolingLayerTest.shape, new Kernel(3, 2, 1, 2));
            MaxPoolingLayer layer2 = new MaxPoolingLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("MaxPoolingLayer")]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new MaxPoolingLayer(null, new Kernel(2, 2, 2, 2)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("MaxPoolingLayer")]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new MaxPoolingLayer((MaxPoolingLayer)null));
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void CloneTest()
        {
            MaxPoolingLayer layer1 = new MaxPoolingLayer(MaxPoolingLayerTest.shape, new Kernel(2, 2, 2, 2));
            MaxPoolingLayer layer2 = layer1.Clone() as MaxPoolingLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void SerializeTest()
        {
            MaxPoolingLayer layer1 = new MaxPoolingLayer(MaxPoolingLayerTest.shape, new Kernel(2, 2, 2, 2));
            string s1 = JsonConvert.SerializeObject(layer1);
            MaxPoolingLayer layer2 = JsonConvert.DeserializeObject<MaxPoolingLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [TestCategory("MaxPoolingLayer")]
        public void ForwardBackwardTest()
        {
            foreach (string format in new[] { Shape.BWHC, Shape.BHWC, Shape.BCHW })
            {
                Shape shape = new Shape(format, -1, 13, 11, 2);
                foreach (int kw in new[] { 1, 2, 3, 4, 5 })
                {
                    foreach (int kh in new[] { 1, 2, 3, 4, 5 })
                    {
                        foreach (int kstridex in new[] { 1, 2, 3 })
                        {
                            foreach (int kstridey in new[] { 1, 2, 3 })
                            {
                                foreach (int kpaddingx in new[] { 0, 2, -2 })
                                {
                                    foreach (int kpaddingy in new[] { 0, 2, -2 })
                                    {
                                        Kernel kernel = new Kernel(kw, kh, kstridex, kstridey, kpaddingx, kpaddingy);
                                        MaxPoolingLayer layer = new MaxPoolingLayer(shape, kernel);

                                        for (int mb = 1; mb <= 3; mb++)
                                        {
                                            Session session = new Session(true);

                                            Tensor x = new Tensor(null, shape.Reshape(Axis.B, mb));
                                            x.Randomize(this.random);

                                            Tensor y = layer.Forward(session, new[] { x })[0];

                                            Tensor expected = MaxPoolingLayerTest.CalculateY(x, kernel);
                                            Helpers.AreTensorsEqual(expected, y);

                                            // unroll the graph
                                            y.RandomizeGradient(this.random);
                                            session.Unroll();

                                            Tensor expectedDX = MaxPoolingLayerTest.CalculateDX(x, y, kernel);
                                            Helpers.AreGradientsEqual(expectedDX, x);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Tensor CalculateY(Tensor x, Kernel kernel)
        {
            Tensor y = new Tensor(null,
                new Shape(
                    x.Shape.Format,
                    x.Shape.GetAxis(Axis.B),
                    kernel.CalculateOutputWidth(x.Shape.GetAxis(Axis.X)),
                    kernel.CalculateOutputHeight(x.Shape.GetAxis(Axis.Y)),
                    x.Shape.GetAxis(Axis.C)));

            int xb = -kernel.PaddingX;
            int xe = x.Shape.GetAxis(Axis.X) + kernel.PaddingX;
            int yb = -kernel.PaddingY;
            int ye = x.Shape.GetAxis(Axis.Y) + kernel.PaddingY;

            for (int ib = 0, iib = y.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = xb, dstx = 0; ix < xe && dstx < y.Shape.GetAxis(Axis.X); ix += kernel.StrideX, dstx++)
                {
                    for (int iy = yb, dsty = 0; iy < ye && dsty < y.Shape.GetAxis(Axis.Y); iy += kernel.StrideY, dsty++)
                    {
                        Tensor k = x.CropKernel(ib, ix, iy, kernel, false, out int kernelArea);

                        for (int ic = 0; ic < y.Shape.GetAxis(Axis.C); ic++)
                        {
                            float value = float.NegativeInfinity;
                            for (int ikx = 0; ikx < kernel.Width; ikx++)
                            {
                                for (int iky = 0; iky < kernel.Height; iky++)
                                {
                                    value = Math.Max(value, k[k.Shape.Position(0, ikx, iky, ic)]);
                                }
                            }

                            y[y.Shape.Position(ib, dstx, dsty, ic)] = value;
                        }
                    }
                }
            }

            return y;
        }

        private static Tensor CalculateDX(Tensor x, Tensor y, Kernel kernel)
        {
            Tensor dx = new Tensor(null, x.Shape);

            int xb = -kernel.PaddingX;
            int xe = x.Shape.GetAxis(Axis.X) + kernel.PaddingX;
            int yb = -kernel.PaddingY;
            int ye = x.Shape.GetAxis(Axis.Y) + kernel.PaddingY;

            for (int ib = 0, iib = y.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = xb, dstx = 0; ix < xe && dstx < y.Shape.GetAxis(Axis.X); ix += kernel.StrideX, dstx++)
                {
                    for (int iy = yb, dsty = 0; iy < ye && dsty < y.Shape.GetAxis(Axis.Y); iy += kernel.StrideY, dsty++)
                    {
                        int kxb = MinMax.Max(ix, 0);
                        int kxe = MinMax.Min(ix + kernel.Width, xe, x.Shape.GetAxis(Axis.X));
                        int kyb = MinMax.Max(iy, 0);
                        int kye = MinMax.Min(iy + kernel.Height, ye, x.Shape.GetAxis(Axis.Y));

                        for (int ic = 0; ic < y.Shape.GetAxis(Axis.C); ic++)
                        {
                            int ypos = y.Shape.Position(ib, dstx, dsty, ic);

                            for (int ikx = kxb; ikx < kxe; ikx++)
                            {
                                for (int iky = kyb; iky < kye; iky++)
                                {
                                    int xpos = dx.Shape.Position(ib, ikx, iky, ic);
                                    if (x.Weights[xpos] == y.Weights[ypos])
                                    {
                                        dx.Gradient[xpos] += y.Gradient[ypos];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dx;
        }
    }
}
