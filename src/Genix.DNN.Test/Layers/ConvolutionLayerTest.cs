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
        [TestMethod]
        public void StackKernelsTest()
        {
            Stopwatch stopwatch = new Stopwatch();
            const int Count = 1000;

            Tensor x = new Tensor(null, new Shape(Shape.BWHC, 1, 24, 24, 129));
            x.Randomize();
            Kernel kernel = new Kernel(5, 5, 1, 1, 1, 1);

            Session session = new Session(false);

            stopwatch.Restart();

            for (int i = 0; i < Count; i++)
            {
                Tensor y = session.StackKernels(x, kernel);

                session.EndSession();
            }

            stopwatch.Stop();
            Console.WriteLine("{0:F4} ms, {1:F4} ms", stopwatch.ElapsedMilliseconds, (float)stopwatch.ElapsedMilliseconds / Count);
        }

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
        public void ForwardBackwardTest1()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 2, 3, 2);
            const int NumberOfFilters = 2;
            Kernel kernel = new Kernel(1, 2, 1, 1, 0, 0);

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                ConvolutionLayer layer = new ConvolutionLayer(shape, NumberOfFilters, kernel, matrixLayout, null);
                CollectionAssert.AreEqual(new[] { -1, 2, 2, NumberOfFilters }, layer.OutputShape.Axes);

                layer.W.Set(new float[]
                {
                    1, 2, 3, 4,
                    5, 6, 7, 8
                });

                layer.B.Set(new float[] { 1, 2 });

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(new float[]
                {
                    1,  2,   3,  4,
                    5,  6,   7,  8,
                    9, 10,  11, 12
                });

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                expectedTemp.Set(ConvolutionLayerTest.CalculateConvolution(layer.W, xTemp, layer.B, kernel, NumberOfFilters, matrixLayout));

                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                dyTemp.Set(new float[]
                {
                    1, 2,  3, 4,
                    5, 6,  7, 8
                });

                // should be W' * dy
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);
                expectedDxTemp.Set(ConvolutionLayerTest.CalculateDx(layer.W, xTemp, dyTemp, kernel, NumberOfFilters, matrixLayout));

                Tensor expectedDBTemp = new Tensor(null, layer.B.Shape);
                expectedDBTemp.Set(ConvolutionLayerTest.CalculateDB(dyTemp, NumberOfFilters));

                Tensor expectedDWTemp = new Tensor(null, layer.W.Shape);
                expectedDWTemp.Set(ConvolutionLayerTest.CalculateDW(layer.W, xTemp, dyTemp, kernel, NumberOfFilters, matrixLayout));

                for (int i = 1; i <= 3; i++)
                {
                    Session session = new Session();

                    layer.W.ClearGradient();
                    layer.B.ClearGradient();

                    Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                    Tensor y = layer.Forward(session, new[] { x })[0];

                    Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                    Helpers.AreTensorsEqual(expected, y);

                    // unroll the graph
                    y.SetGradient(session.Tile(dyTemp, (int)Axis.B, i).Weights);
                    session.Unroll();

                    Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                    Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);

                    // should be dy
                    Tensor expectedDB = session.Multiply(expectedDBTemp, i);
                    Helpers.AreArraysEqual(expectedDB.Length, expectedDB.Weights, layer.B.Gradient);

                    // should be x * dy
                    Tensor expectedDW = session.Multiply(expectedDWTemp, i);
                    Helpers.AreArraysEqual(expectedDW.Length, expectedDW.Weights, layer.W.Gradient);
                }
            }
        }

        [TestMethod]
        public void ForwardBackwardTest2()
        {
            Shape shape = new Shape(Shape.BWHC, -1, 2, 3, 2);
            const int NumberOfFilters = 2;
            Kernel kernel = new Kernel(1, 2, 1, 1, 1, 1);

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                ConvolutionLayer layer = new ConvolutionLayer(shape, NumberOfFilters, kernel, matrixLayout, null);
                CollectionAssert.AreEqual(new[] { -1, 4, 4, NumberOfFilters }, layer.OutputShape.Axes);

                layer.W.Set(new float[]
                {
                    1, 2, 3, 4,
                    5, 6, 7, 8
                });

                layer.B.Set(new float[] { 1, 2 });

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(new float[]
                {
                    1,  2,   3,  4,
                    5,  6,   7,  8,
                    9, 10,  11, 12
                });

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                expectedTemp.Set(ConvolutionLayerTest.CalculateConvolution(layer.W, xTemp, layer.B, kernel, NumberOfFilters, matrixLayout));

                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                dyTemp.Set(new float[]
                {
                     1,  2,   3,  4,   5,  6,   7,  8,
                     9, 10,  11, 12,  13, 14,  15, 16,
                    17, 18,  19, 20,  21, 22,  23, 24,
                    25, 26,  27, 28,  29, 30,  31, 32
                });

                // should be sum(filter[i] * out_grad[i])
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);
                expectedDxTemp.Set(ConvolutionLayerTest.CalculateDx(layer.W, xTemp, dyTemp, kernel, NumberOfFilters, matrixLayout));

                Tensor expectedDBTemp = new Tensor(null, layer.B.Shape);
                expectedDBTemp.Set(ConvolutionLayerTest.CalculateDB(dyTemp, NumberOfFilters));

                Tensor expectedDWTemp = new Tensor(null, layer.W.Axes);
                expectedDWTemp.Set(ConvolutionLayerTest.CalculateDW(layer.W, xTemp, dyTemp, kernel, NumberOfFilters, matrixLayout));

                for (int i = 1; i <= 3; i++)
                {
                    Session session = new Session();

                    layer.W.ClearGradient();
                    layer.B.ClearGradient();

                    Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                    Tensor y = layer.Forward(session, new[] { x })[0];

                    Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                    Helpers.AreTensorsEqual(expected, y);

                    // unroll the graph
                    y.SetGradient(session.Tile(dyTemp, (int)Axis.B, i).Weights);
                    session.Unroll();

                    Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                    Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);

                    // should be dy
                    Tensor expectedDB = session.Multiply(expectedDBTemp, i);
                    Helpers.AreArraysEqual(expectedDB.Length, expectedDB.Weights, layer.B.Gradient);

                    // should be x * dy
                    Tensor expectedDW = session.Multiply(expectedDWTemp, i);
                    Helpers.AreArraysEqual(expectedDW.Length, expectedDW.Weights, layer.W.Gradient);
                }
            }
        }

        private static Tensor CropKernel(Tensor input, int x, int y, Kernel kernel)
        {
            Tensor res = new Tensor(null, new Shape(Shape.BWHC, 1, kernel.Width, kernel.Height, input.Shape.GetAxis(Axis.C)));

            for (int ix = x; ix < x + kernel.Width; ix++)
            {
                if (ix >= 0 && ix < input.Shape.GetAxis(Axis.X))
                {
                    for (int iy = y; iy < y + kernel.Height; iy++)
                    {
                        if (iy >= 0 && iy < input.Shape.GetAxis(Axis.Y))
                        {
                            for (int ic = 0; ic < input.Shape.GetAxis(Axis.C); ic++)
                            {
                                res[0, ix - x, iy - y, ic] = input[0, ix, iy, ic];
                            }
                        }
                    }
                }
            }

            return res;
        }

        private static float[] CalculateConvolution(Tensor w, Tensor x, Tensor b, Kernel kernel, int numberOfFilters, MatrixLayout matrixLayout)
        {
            List<float> res = new List<float>();

            for (int ix = 0, xpos = -kernel.PaddingX, iix = kernel.CalculateOutputWidth(x.Shape.GetAxis(Axis.X)); ix < iix; ix++, xpos += kernel.StrideX)
            {
                for (int iy = 0, ypos = -kernel.PaddingY, iiy = kernel.CalculateOutputHeight(x.Shape.GetAxis(Axis.Y)); iy < iiy; iy++, ypos += kernel.StrideY)
                {
                    res.AddRange(FullyConnectedLayerTest.CalculateNeurons(
                        w,
                        ConvolutionLayerTest.CropKernel(x, xpos, ypos, kernel),
                        b,
                        numberOfFilters,
                        matrixLayout));
                }
            }

            return res.ToArray();
        }

        private static float[] CalculateDx(Tensor w, Tensor x, Tensor dy, Kernel kernel, int numberOfFilters, MatrixLayout matrixLayout)
        {
            float[] res = new float[x.Length];

            for (int ix = 0, xpos = -kernel.PaddingX, iix = dy.Shape.GetAxis(Axis.X); ix < iix; ix++, xpos += kernel.StrideX)
            {
                for (int iy = 0, ypos = -kernel.PaddingY, iiy = dy.Shape.GetAxis(Axis.Y); iy < iiy; iy++, ypos += kernel.StrideY)
                {
                    Tensor subdy = new Tensor(null, new Shape(Shape.BWHC, 1, 1, 1, numberOfFilters));
                    subdy.Set(dy.Weights.Skip(dy.Shape.Position(0, ix, iy, 0)).Take(numberOfFilters).ToArray());

                    Tensor subdx = new Tensor(null, new Shape(Shape.BWHC, 1, kernel.Width, kernel.Height, numberOfFilters));
                    subdx.Set(FullyConnectedLayerTest.CalculateDx(w, subdy, numberOfFilters, matrixLayout));

                    for (int kx = 0; kx < kernel.Width; kx++)
                    {
                        for (int ky = 0; ky < kernel.Height; ky++)
                        {
                            for (int kc = 0; kc < numberOfFilters; kc++)
                            {
                                int kxpos = xpos + kx;
                                int kypos = ypos + ky;
                                if (kxpos >= 0 && kxpos < x.Shape.GetAxis(Axis.X) &&
                                    kypos >= 0 && kypos < x.Shape.GetAxis(Axis.Y))
                                {
                                    res[x.Shape.Position(0, kxpos, kypos, kc)] += subdx[0, kx, ky, kc];
                                }
                            }
                        }
                    }
                }
            }

            return res;
        }

        private static float[] CalculateDB(Tensor dy, int numberOfFilters)
        {
            int count = dy.Length / numberOfFilters;

            return Enumerable.Range(0, numberOfFilters).Select(neuron =>
            {
                return Enumerable.Range(0, count).Sum(i => dy[(numberOfFilters * i) + neuron]);
            }).ToArray();
        }

        private static float[] CalculateDW(Tensor w, Tensor x, Tensor dy, Kernel kernel, int numberOfFilters, MatrixLayout matrixLayout)
        {
            float[] res = new float[w.Length];

            for (int ix = 0, xpos = -kernel.PaddingX, iix = dy.Shape.GetAxis(Axis.X); ix < iix; ix++, xpos += kernel.StrideX)
            {
                for (int iy = 0, ypos = -kernel.PaddingY, iiy = dy.Shape.GetAxis(Axis.Y); iy < iiy; iy++, ypos += kernel.StrideY)
                {
                    Tensor subx = ConvolutionLayerTest.CropKernel(x, xpos, ypos, kernel);

                    Tensor subdy = new Tensor(null, new Shape(Shape.BWHC, 1, 1, 1, numberOfFilters));
                    subdy.Set(dy.Weights.Skip(dy.Shape.Position(0, ix, iy, 0)).Take(numberOfFilters).ToArray());

                    float[] dw = FullyConnectedLayerTest.CalculateDW(subx, subdy, matrixLayout);
                    Mathematics.Add(res.Length, dw, 0, res, 0);
                }
            }

            return res;
        }
    }
}
