namespace Genix.DNN.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Genix.Core;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class AveragePoolingLayerTest
    {
        private static Shape shape = new Shape(Shape.BWHC, -1, 5, 4, 2);

        private readonly RandomNumberGenerator<float> random = new RandomGeneratorF();

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ConstructorTest1()
        {
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, new Kernel(2, 2, 2, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape.Axes);
            Assert.AreEqual("AP2", layer.Architecture);
            Assert.AreEqual(2, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ConstructorTest2()
        {
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, new Kernel(3, 2, 1, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape.Axes);
            Assert.AreEqual("AP3x2+1x2(S)", layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(1, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ArchitectureConstructorTest1()
        {
            string architecture = "AP3x2";
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(3, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "AP3x2+2(S)";
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ArchitectureConstructorTest3()
        {
            string architecture = "AP3x2+2x1(S)";
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(1, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ArchitectureConstructorTest4()
        {
            string architecture = "AP2";
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(2, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest5()
        {
            string architecture = "AP";
            try
            {
                AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.shape, architecture, null);
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
        [TestCategory("AveragePoolingLayer")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest6()
        {
            Assert.IsNotNull(new AveragePoolingLayer(null, "AP2", null));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest7()
        {
            Assert.IsNotNull(new AveragePoolingLayer(AveragePoolingLayerTest.shape, null, null));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void CopyConstructorTest1()
        {
            AveragePoolingLayer layer1 = new AveragePoolingLayer(AveragePoolingLayerTest.shape, new Kernel(3, 2, 1, 2));
            AveragePoolingLayer layer2 = new AveragePoolingLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new AveragePoolingLayer(null, new Kernel(2, 2, 2, 2)));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new AveragePoolingLayer((AveragePoolingLayer)null));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void CloneTest()
        {
            AveragePoolingLayer layer1 = new AveragePoolingLayer(AveragePoolingLayerTest.shape, new Kernel(2, 2, 2, 2));
            AveragePoolingLayer layer2 = layer1.Clone() as AveragePoolingLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void SerializeTest()
        {
            AveragePoolingLayer layer1 = new AveragePoolingLayer(AveragePoolingLayerTest.shape, new Kernel(2, 2, 2, 2));
            string s1 = JsonConvert.SerializeObject(layer1);
            AveragePoolingLayer layer2 = JsonConvert.DeserializeObject<AveragePoolingLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
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
                                        AveragePoolingLayer layer = new AveragePoolingLayer(shape, kernel);

                                        for (int mb = 1; mb <= 3; mb++)
                                        {
                                            Session session = new Session(true);

                                            Tensor x = new Tensor(null, shape.Reshape(Axis.B, mb));
                                            x.Randomize(this.random);

                                            Tensor y = layer.Forward(session, new[] { x })[0];

                                            Tensor expected = AveragePoolingLayerTest.CalculateY(x, kernel);
                                            Helpers.AreTensorsEqual(expected, y);

                                            // unroll the graph
                                            y.RandomizeGradient(this.random);
                                            session.Unroll();

                                            Tensor expectedDX = AveragePoolingLayerTest.CalculateDX(x, y, kernel);
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
                            float sum = 0;
                            for (int ikx = 0; ikx < kernel.Width; ikx++)
                            {
                                for (int iky = 0; iky < kernel.Height; iky++)
                                {
                                    sum += k[k.Shape.Position(0, ikx, iky, ic)];
                                }
                            }

                            y[y.Shape.Position(ib, dstx, dsty, ic)] = sum / kernel.Size;
                        }
                    }
                }
            }

            return y;
        }

        private static Tensor CalculateDX(Tensor x, Tensor dy, Kernel kernel)
        {
            Tensor dx = new Tensor(null, x.Shape);

            int xb = -kernel.PaddingX;
            int xe = x.Shape.GetAxis(Axis.X) + kernel.PaddingX;
            int yb = -kernel.PaddingY;
            int ye = x.Shape.GetAxis(Axis.Y) + kernel.PaddingY;

            for (int ib = 0, iib = dy.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = xb, dstx = 0; ix < xe && dstx < dy.Shape.GetAxis(Axis.X); ix += kernel.StrideX, dstx++)
                {
                    for (int iy = yb, dsty = 0; iy < ye && dsty < dy.Shape.GetAxis(Axis.Y); iy += kernel.StrideY, dsty++)
                    {
                        int kxb = MinMax.Max(ix, 0);
                        int kxe = MinMax.Min(ix + kernel.Width, xe, x.Shape.GetAxis(Axis.X));
                        int kyb = MinMax.Max(iy, 0);
                        int kye = MinMax.Min(iy + kernel.Height, ye, x.Shape.GetAxis(Axis.Y));

                        float alpha = 1.0f / kernel.Size;

                        for (int ic = 0; ic < dy.Shape.GetAxis(Axis.C); ic++)
                        {
                            float value = dy.Gradient[dy.Shape.Position(ib, dstx, dsty, ic)] * alpha;

                            for (int ikx = kxb; ikx < kxe; ikx++)
                            {
                                for (int iky = kyb; iky < kye; iky++)
                                {
                                    dx.Gradient[dx.Shape.Position(ib, ikx, iky, ic)] += value;
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