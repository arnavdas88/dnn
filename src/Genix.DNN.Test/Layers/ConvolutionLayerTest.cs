namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Genix.Core;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ConvolutionLayerTest
    {
        private readonly RandomNumberGenerator<float> random = new RandomGeneratorF();

        [TestMethod]
        public void ConstructorTest1()
        {
            Shape shape = new Shape(Shape.BWHC, 2, 10, 12, 3);
            const int NumberOfFilters = 100;
            Kernel kernel = new Kernel(3, 4, 3, 3, 2, 1);

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                ConvolutionLayer layer = new ConvolutionLayer(shape, NumberOfFilters, kernel, matrixLayout, null);

                Assert.AreEqual(NumberOfFilters, layer.NumberOfNeurons);
                Assert.AreEqual(matrixLayout, layer.MatrixLayout);

                Assert.AreEqual(1, layer.NumberOfOutputs);
                CollectionAssert.AreEqual(new[] { 2, 5, 5, NumberOfFilters }, layer.OutputShape.Axes);

                CollectionAssert.AreEqual(
                    matrixLayout == MatrixLayout.RowMajor ?
                        new[] { NumberOfFilters, kernel.Size * shape.GetAxis(Axis.C) } :
                        new[] { kernel.Size * shape.GetAxis(Axis.C), NumberOfFilters },
                    layer.W.Axes);
                Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
                Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

                CollectionAssert.AreEqual(new[] { NumberOfFilters }, layer.B.Axes);
                Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new ConvolutionLayer(null, 100, new Kernel(3, 4, 3, 3, 2, 1), MatrixLayout.ColumnMajor, null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 20, 20, 10);
            Assert.IsNotNull(new ConvolutionLayer(shape, 100, null, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("SRN")]
        public void ArchitectureConstructorTest1()
        {
            Shape shape = new Shape(Shape.BWHC, 2, 10, 12, 3);
            const string Architecture = "16C3+4x1(S)+-1(P)";

            ConvolutionLayer layer = new ConvolutionLayer(shape, Architecture, null);

            Assert.AreEqual(16, layer.NumberOfNeurons);
            Assert.AreEqual(Architecture, layer.Architecture);

            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(3, layer.Kernel.Height);
            Assert.AreEqual(4, layer.Kernel.StrideX);
            Assert.AreEqual(1, layer.Kernel.StrideY);
            Assert.AreEqual(-1, layer.Kernel.PaddingX);
            Assert.AreEqual(-1, layer.Kernel.PaddingY);

            CollectionAssert.AreEqual(new[] { 2, 3, 8, 16 }, layer.OutputShape.Axes);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual(MatrixLayout.RowMajor, layer.MatrixLayout);

            CollectionAssert.AreEqual(new[] { 16, 9 * shape.GetAxis(Axis.C) }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.05f);

            CollectionAssert.AreEqual(new[] { 16 }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "16C";
            try
            {
                ConvolutionLayer layer = new ConvolutionLayer(new Shape(Shape.BWHC, -1, 10, 12, 3), architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new ConvolutionLayer(null, "16C3", null));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new ConvolutionLayer(new Shape(Shape.BWHC, -1, 10, 12, 3), null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 20, 20, 10);
            ConvolutionLayer layer1 = new ConvolutionLayer(shape, 100, new Kernel(3, 4, 3, 3, 2, 1), MatrixLayout.ColumnMajor, null);
            ConvolutionLayer layer2 = new ConvolutionLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new ConvolutionLayer(null));
        }

        [TestMethod]
        public void EnumGradientsTest()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 20, 20, 10);
            ConvolutionLayer layer = new ConvolutionLayer(shape, 100, new Kernel(3, 4, 3, 3, 2, 1), MatrixLayout.ColumnMajor, null);
            Assert.AreEqual(2, layer.EnumGradients().Count());
        }

        [TestMethod]
        public void CloneTest()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 20, 20, 10);
            ConvolutionLayer layer1 = new ConvolutionLayer(shape, 100, new Kernel(3, 4, 3, 3, 2, 1), MatrixLayout.ColumnMajor, null);
            ConvolutionLayer layer2 = layer1.Clone() as ConvolutionLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 20, 20, 10);
            ConvolutionLayer layer1 = new ConvolutionLayer(shape, 100, new Kernel(3, 4, 3, 3, 2, 1), MatrixLayout.ColumnMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            ConvolutionLayer layer2 = JsonConvert.DeserializeObject<ConvolutionLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            (Shape shape, Kernel kernel, int numberOfFilters)[] testCases = new (Shape, Kernel, int)[]
            {
                // no padding
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(1, 2, 1, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 1, 1, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 2, 1, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 3, 1, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 2, 1, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 3, 1, 1, 0, 0), 2),

                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(1, 2, 2, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 1, 2, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 2, 2, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 3, 2, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 2, 2, 1, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 3, 2, 1, 0, 0), 2),

                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(1, 2, 1, 2, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 1, 1, 2, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 2, 1, 2, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 3, 1, 2, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 2, 1, 2, 0, 0), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 3, 1, 2, 0, 0), 2),

                // positive padding (expand)
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(1, 2, 1, 1, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 1, 2, 1, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 2, 1, 1, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 3, 1, 1, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 2, 1, 1, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 3, 1, 1, 2, 3), 2),

                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(1, 2, 2, 1, 3, 2), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 1, 2, 1, 3, 2), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 2, 2, 1, 3, 2), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 3, 2, 1, 3, 2), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 2, 2, 1, 3, 2), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 3, 2, 1, 3, 2), 2),

                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(1, 2, 1, 2, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 1, 1, 2, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 2, 1, 2, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(2, 3, 1, 2, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 2, 1, 2, 2, 3), 2),
                (new Shape(Shape.BWHC, -1, 5, 4, 2), new Kernel(3, 3, 1, 2, 2, 3), 2),

                // negative padding (shrinking)
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(1, 2, 1, 1, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 1, 2, 1, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 2, 1, 1, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 3, 1, 1, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(3, 2, 1, 1, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(3, 3, 1, 1, -1, -2), 2),

                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(1, 2, 2, 1, -2, -1), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 1, 2, 1, -2, -1), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 2, 2, 1, -2, -1), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 3, 2, 1, -2, -1), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(3, 2, 2, 1, -2, -1), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(3, 3, 2, 1, -2, -1), 2),

                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(1, 2, 1, 2, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 1, 1, 2, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 2, 1, 2, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(2, 3, 1, 2, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(3, 2, 1, 2, -1, -2), 2),
                (new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(3, 3, 1, 2, -1, -2), 2),

                //(new Shape(Shape.BWHC, -1, 10, 8, 2), new Kernel(1, 2, 1, 1, -1, -2), 2),

                //(new Shape(Shape.BWHC, -1, 1, 6, 1), new Kernel(1, 2, 1, 1, 0, -2), 1),
            };

            foreach ((Shape shape, Kernel kernel, int numberOfFilters) in testCases)
            {
                foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
                {
                    if (matrixLayout != MatrixLayout.ColumnMajor)
                    {
                        continue;
                    }

                    ConvolutionLayer layer = new ConvolutionLayer(shape, numberOfFilters, kernel, matrixLayout, null);
                    layer.W.Randomize(this.random);
                    layer.B.Randomize(this.random);

                    for (int mb = 1; mb <= 3; mb++)
                    {
                        Session session = new Session();

                        layer.W.ClearGradient();
                        layer.B.ClearGradient();

                        Tensor x = new Tensor(null, shape.Reshape(Axis.B, mb));
                        x.Randomize(this.random);

                        Tensor y = layer.Forward(session, new[] { x })[0];

                        Tensor expected = ConvolutionLayerTest.CalculateY(layer.W, x, layer.B, kernel, numberOfFilters, matrixLayout);
                        Helpers.AreTensorsEqual(expected, y);

                        // unroll the graph
                        y.RandomizeGradient(this.random);
                        session.Unroll();

                        // should be dy * numberOfCells in y
                        Tensor expectedDB = ConvolutionLayerTest.CalculateDB(y);
                        Helpers.AreGradientsEqual(expectedDB, layer.B);

                        // should be dy * x'
                        Tensor expectedDW = ConvolutionLayerTest.CalculateDW(layer.W, x, y, kernel, numberOfFilters, matrixLayout);
                        Helpers.AreGradientsEqual(expectedDW, layer.W);

                        // should be dW' * dy
                        Tensor expectedDX = ConvolutionLayerTest.CalculateDX(layer.W, x, y, kernel, numberOfFilters, matrixLayout);
                        Helpers.AreGradientsEqual(expectedDX, x);
                    }
                }
            }
        }

        private static Tensor CropKernel(Tensor input, int b, int x, int y, Kernel kernel, bool cropGradient)
        {
            Tensor res = new Tensor(null, new Shape(Shape.BWHC, 1, kernel.Width, kernel.Height, input.Shape.GetAxis(Axis.C)));

            int xb = Math.Max(-kernel.PaddingX, 0);
            int xe = input.Shape.GetAxis(Axis.X) - 1 - xb;
            int yb = Math.Max(-kernel.PaddingY, 0);
            int ye = input.Shape.GetAxis(Axis.Y) - 1 - yb;

            for (int ix = x; ix < x + kernel.Width; ix++)
            {
                if (ix.Between(xb, xe))
                {
                    for (int iy = y; iy < y + kernel.Height; iy++)
                    {
                        if (iy.Between(yb, ye))
                        {
                            for (int ic = 0; ic < input.Shape.GetAxis(Axis.C); ic++)
                            {
                                res[0, ix - x, iy - y, ic] = cropGradient ?
                                    input.Gradient[input.Shape.Position(b, ix, iy, ic)] :
                                    input.Weights[input.Shape.Position(b, ix, iy, ic)];
                            }
                        }
                    }
                }
            }

            return res;
        }

        private static Tensor CalculateY(Tensor w, Tensor x, Tensor b, Kernel kernel, int numberOfFilters, MatrixLayout matrixLayout)
        {
            Tensor y = new Tensor(null,
                new Shape(
                    Shape.BWHC,
                    x.Shape.GetAxis(Axis.B),
                    kernel.CalculateOutputWidth(x.Shape.GetAxis(Axis.X)),
                    kernel.CalculateOutputHeight(x.Shape.GetAxis(Axis.Y)),
                    x.Shape.GetAxis(Axis.C)));

            for (int ib = 0, iib = y.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = 0, xpos = -kernel.PaddingX, iix = y.Shape.GetAxis(Axis.X); ix < iix; ix++, xpos += kernel.StrideX)
                {
                    for (int iy = 0, ypos = -kernel.PaddingY, iiy = y.Shape.GetAxis(Axis.Y); iy < iiy; iy++, ypos += kernel.StrideY)
                    {
                        Tensor k = ConvolutionLayerTest.CropKernel(x, ib, xpos, ypos, kernel, false);
                        float[] features = FullyConnectedLayerTest.CalculateNeurons(w, k, b, numberOfFilters, matrixLayout);

                        for (int ic = 0, iic = y.Shape.GetAxis(Axis.C); ic < iic; ic++)
                        {
                            y[ib, ix, iy, ic] = features[ic];
                        }
                    }
                }
            }

            return y;
        }

        private static Tensor CalculateDX(Tensor w, Tensor x, Tensor y, Kernel kernel, int numberOfFilters, MatrixLayout matrixLayout)
        {
            Tensor dx = new Tensor(null, x.Axes);

            int xb = Math.Max(-kernel.PaddingX, 0);
            int xe = x.Shape.GetAxis(Axis.X) - 1 - xb;
            int yb = Math.Max(-kernel.PaddingY, 0);
            int ye = x.Shape.GetAxis(Axis.Y) - 1 - yb;

            for (int ib = 0, iib = x.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = 0, xpos = -kernel.PaddingX, iix = y.Shape.GetAxis(Axis.X); ix < iix; ix++, xpos += kernel.StrideX)
                {
                    for (int iy = 0, ypos = -kernel.PaddingY, iiy = y.Shape.GetAxis(Axis.Y); iy < iiy; iy++, ypos += kernel.StrideY)
                    {
                        Tensor kdy = ConvolutionLayerTest.CropKernel(y, ib, ix, iy, new Kernel(1, 1, 1, 1), true);

                        Tensor kdx = new Tensor(null, new Shape(Shape.BWHC, 1, kernel.Width, kernel.Height, numberOfFilters));
                        kdx.Set(FullyConnectedLayerTest.CalculateDx(w, kdy, numberOfFilters, matrixLayout));

                        for (int kx = xpos; kx < xpos + kernel.Width; kx++)
                        {
                            for (int ky = ypos; ky < ypos + kernel.Height; ky++)
                            {
                                if (kx.Between(xb, xe) && ky.Between(yb, ye))
                                {
                                    for (int kc = 0; kc < numberOfFilters; kc++)
                                    {
                                        dx.Gradient[x.Shape.Position(ib, kx, ky, kc)] += kdx[0, kx - xpos, ky - ypos, kc];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dx;
        }

        private static Tensor CalculateDB(Tensor y)
        {
            int y0 = y.Shape.GetAxis(Axis.B);
            int y1 = y.Shape.GetAxis(Axis.X);
            int y2 = y.Shape.GetAxis(Axis.Y);
            int y3 = y.Shape.GetAxis(Axis.C);
            int ystride3 = y.Shape.GetStride(Axis.C);

            Tensor db = new Tensor(null, new[] { y3 });

            for (int iy0 = 0; iy0 < y0; iy0++)
            {
                for (int iy1 = 0; iy1 < y1; iy1++)
                {
                    for (int iy2 = 0; iy2 < y2; iy2++)
                    {
                        Mathematics.Add(y3, y.Gradient, y.Shape.Position(iy0, iy1, iy2, 0), ystride3, db.Gradient, 0, 1);
                    }
                }
            }

            return db;
        }

        private static Tensor CalculateDW(Tensor w, Tensor x, Tensor y, Kernel kernel, int numberOfFilters, MatrixLayout matrixLayout)
        {
            Tensor dw = new Tensor(null, w.Axes);

            for (int ib = 0, iib = y.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = 0, xpos = -kernel.PaddingX, iix = y.Shape.GetAxis(Axis.X); ix < iix; ix++, xpos += kernel.StrideX)
                {
                    for (int iy = 0, ypos = -kernel.PaddingY, iiy = y.Shape.GetAxis(Axis.Y); iy < iiy; iy++, ypos += kernel.StrideY)
                    {
                        Tensor k = ConvolutionLayerTest.CropKernel(x, ib, xpos, ypos, kernel, false);
                        Tensor kdy = ConvolutionLayerTest.CropKernel(y, ib, ix, iy, new Kernel(1, 1, 1, 1), true);

                        float[] dww = FullyConnectedLayerTest.CalculateDW(k, kdy, matrixLayout);
                        Mathematics.Add(dw.Length, dww, 0, dw.Gradient, 0);
                    }
                }
            }

            return dw;
        }
    }
}
