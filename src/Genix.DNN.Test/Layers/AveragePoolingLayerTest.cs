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
        private static (Shape shape, float[] weights)[] sources = new (Shape shape, float[] weights)[]
        {
            (new Shape(Shape.BWHC, -1, 5, 4, 2), new float[]
            {
                1,  2,     3,  4,    7,  8,    5,  6,
                19, 20,   21, 22,   25, 26,   23, 24,
                7,  8,     9, 10,   13, 14,   11, 12,
                25, 26,   27, 28,   31, 32,   29, 30,
                13, 14,   15, 16,   19, 20,   17, 18,
            }),
            (new Shape(Shape.BHWC, -1, 4, 5, 2), new float[]
            {
                1,  2,     3,  4,    7,  8,    5,  6,
                19, 20,   21, 22,   25, 26,   23, 24,
                7,  8,     9, 10,   13, 14,   11, 12,
                25, 26,   27, 28,   31, 32,   29, 30,
                13, 14,   15, 16,   19, 20,   17, 18,
            }),
            (new Shape(Shape.BCHW, -1, 4, 5, 2), new float[]
            {
                1,      3,     7,     5,
                19,    21,    25,    23,
                7,      9,    13,    11,
                25,    27,    31,    29,
                13,    15,    19,    17,

                2,      4,     8,     6,
                20,    22,    26,    24,
                8,     10,    14,    12,
                26,    28,    32,    30,
                14,    16,    20,    18,
            }),
        };

        private readonly RandomNumberGenerator<float> random = new RandomGeneratorF();

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ConstructorTest1()
        {
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, new Kernel(2, 2, 2, 2));

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
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, new Kernel(3, 2, 1, 2));

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
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, architecture, null);

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
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, architecture, null);

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
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, architecture, null);

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
            AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, architecture, null);

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
                AveragePoolingLayer layer = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, architecture, null);
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
            Assert.IsNotNull(new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, null, null));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void CopyConstructorTest1()
        {
            AveragePoolingLayer layer1 = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, new Kernel(3, 2, 1, 2));
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
            AveragePoolingLayer layer1 = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, new Kernel(2, 2, 2, 2));
            AveragePoolingLayer layer2 = layer1.Clone() as AveragePoolingLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void SerializeTest()
        {
            AveragePoolingLayer layer1 = new AveragePoolingLayer(AveragePoolingLayerTest.sources[0].shape, new Kernel(2, 2, 2, 2));
            string s1 = JsonConvert.SerializeObject(layer1);
            AveragePoolingLayer layer2 = JsonConvert.DeserializeObject<AveragePoolingLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [TestCategory("AveragePoolingLayer")]
        public void ForwardBackwardTest()
        {
            (Shape shape, Kernel kernel)[] testCases = new (Shape, Kernel)[]
            {
                /*(new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(1, 2, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 1, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 2, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 3, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 2, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 3, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 3, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 4, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 4, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(5, 4, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 5, 1, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(5, 5, 1, 1)),

                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(1, 2, 2, 1)),*/
                (new Shape(Shape.BWHC, -1, 4, 3, 1), new Kernel(2, 1, 2, 1)),
                /*(new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 2, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 3, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 2, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 3, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 3, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 4, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 4, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(5, 4, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 5, 2, 1)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(5, 5, 2, 1)),

                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(1, 2, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 1, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 2, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(2, 3, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 2, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 3, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 3, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(3, 4, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 4, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(5, 4, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(4, 5, 1, 2)),
                (new Shape(Shape.BWHC, -1, 10, 9, 2), new Kernel(5, 5, 1, 2)),*/
            };

            foreach ((Shape shape, Kernel kernel) in testCases)
            {
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

        [TestMethod]
        [Description("Filter 2x2, stride 2x2.")]
        public void ForwardBackwardTest2X2X2X2()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(2, 2, 2, 2));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                            (1 + 3 + 19 + 21) / 4.0f,  (2 + 4 + 20 + 22) / 4.0f,           (7 + 5 + 25 + 23) / 4.0f,   (8 + 6 + 26 + 24) / 4.0f,
                            (7 + 9 + 25 + 27) / 4.0f,  (8 + 10 + 26 + 28) / 4.0f,          (13 + 11 + 31 + 29) / 4.0f, (14 + 12 + 32 + 30) / 4.0f,
                            (13 + 15) / 4.0f,          (14 + 16) / 4.0f,                   (19 + 17) / 4.0f,           (20 + 18) / 4.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1, 11,   2, 12,
                            3, 13,   4, 14,
                            5, 15,   6, 16,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                            1 / 4.0f, 11 / 4.0f,   1 / 4.0f, 11 / 4.0f,   2 / 4.0f, 12 / 4.0f,   2 / 4.0f, 12 / 4.0f,
                            1 / 4.0f, 11 / 4.0f,   1 / 4.0f, 11 / 4.0f,   2 / 4.0f, 12 / 4.0f,   2 / 4.0f, 12 / 4.0f,
                            3 / 4.0f, 13 / 4.0f,   3 / 4.0f, 13 / 4.0f,   4 / 4.0f, 14 / 4.0f,   4 / 4.0f, 14 / 4.0f,
                            3 / 4.0f, 13 / 4.0f,   3 / 4.0f, 13 / 4.0f,   4 / 4.0f, 14 / 4.0f,   4 / 4.0f, 14 / 4.0f,
                            5 / 4.0f, 15 / 4.0f,   5 / 4.0f, 15 / 4.0f,   6 / 4.0f, 16 / 4.0f,   6 / 4.0f, 16 / 4.0f,
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 3, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                            (1 + 3 + 19 + 21) / 4.0f,          (7 + 5 + 25 + 23) / 4.0f,
                            (7 + 9 + 25 + 27) / 4.0f,          (13 + 11 + 31 + 29) / 4.0f,
                            (13 + 15) / 4.0f,                  (19 + 17) / 4.0f,

                            (2 + 4 + 20 + 22) / 4.0f,          (8 + 6 + 26 + 24) / 4.0f,
                            (8 + 10 + 26 + 28) / 4.0f,         (14 + 12 + 32 + 30) / 4.0f,
                            (14 + 16) / 4.0f,                  (20 + 18) / 4.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1,    2,
                            3,    4,
                            5,    6,

                            11,   12,
                            13,   14,
                            15,   16,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                            1 / 4.0f,    1 / 4.0f,    2 / 4.0f,    2 / 4.0f,
                            1 / 4.0f,    1 / 4.0f,    2 / 4.0f,    2 / 4.0f,
                            3 / 4.0f,    3 / 4.0f,    4 / 4.0f,    4 / 4.0f,
                            3 / 4.0f,    3 / 4.0f,    4 / 4.0f,    4 / 4.0f,
                            5 / 4.0f,    5 / 4.0f,    6 / 4.0f,    6 / 4.0f,

                            11 / 4.0f,   11 / 4.0f,   12 / 4.0f,   12 / 4.0f,
                            11 / 4.0f,   11 / 4.0f,   12 / 4.0f,   12 / 4.0f,
                            13 / 4.0f,   13 / 4.0f,   14 / 4.0f,   14 / 4.0f,
                            13 / 4.0f,   13 / 4.0f,   14 / 4.0f,   14 / 4.0f,
                            15 / 4.0f,   15 / 4.0f,   16 / 4.0f,   16 / 4.0f,
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        [TestMethod]
        [Description("Filter 2x2, stride 1x1.")]
        public void ForwardBackwardTest2X2X1X1()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(2, 2, 1, 1));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 4, 3, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                              (1 + 3 + 19 + 21) / 4.0f,   (2 + 4 + 20 + 22) / 4.0f,     (3 + 7 + 21 + 25) / 4.0f,   (4 + 8 + 22 + 26) / 4.0f,     (7 + 5 + 25 + 23) / 4.0f,   (8 + 6 + 26 + 24) / 4.0f,
                              (19 + 21 + 7 + 9) / 4.0f,  (20 + 22 + 8 + 10) / 4.0f,    (21 + 25 + 9 + 13) / 4.0f, (22 + 26 + 10 + 14) / 4.0f,   (25 + 23 + 13 + 11) / 4.0f, (26 + 24 + 14 + 12) / 4.0f,
                              (7 + 9 + 25 + 27) / 4.0f,  (8 + 10 + 26 + 28) / 4.0f,    (9 + 13 + 27 + 31) / 4.0f, (10 + 14 + 28 + 32) / 4.0f,   (13 + 11 + 31 + 29) / 4.0f, (14 + 12 + 32 + 30) / 4.0f,
                            (25 + 27 + 13 + 15) / 4.0f, (26 + 28 + 14 + 16) / 4.0f,   (27 + 31 + 15 + 19) / 4.0f, (28 + 32 + 16 + 20) / 4.0f,   (31 + 29 + 19 + 17) / 4.0f, (32 + 30 + 20 + 18) / 4.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                             1, 11,   2, 12,   3, 13,
                             4, 14,   5, 15,   6, 16,
                             7, 17,   8, 18,   9, 19,
                            10, 20,  11, 21,  12, 22,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                   1 / 4.0f,        11 / 4.0f,             (1 + 2) / 4.0f,           (11 + 12) / 4.0f,             (2 + 3) / 4.0f,           (12 + 13) / 4.0f,          3 / 4.0f,        13 / 4.0f,
                             (1 + 4) / 4.0f, (11 + 14) / 4.0f,     (1 + 2 + 4 + 5) / 4.0f, (11 + 12 + 14 + 15) / 4.0f,     (2 + 3 + 5 + 6) / 4.0f, (12 + 13 + 15 + 16) / 4.0f,    (3 + 6) / 4.0f, (13 + 16) / 4.0f,
                             (4 + 7) / 4.0f, (14 + 17) / 4.0f,     (4 + 5 + 7 + 8) / 4.0f, (14 + 15 + 17 + 18) / 4.0f,     (5 + 6 + 8 + 9) / 4.0f, (15 + 16 + 18 + 19) / 4.0f,    (6 + 9) / 4.0f, (16 + 19) / 4.0f,
                            (7 + 10) / 4.0f, (17 + 20) / 4.0f,   (7 + 8 + 10 + 11) / 4.0f, (17 + 18 + 20 + 21) / 4.0f,   (8 + 9 + 11 + 12) / 4.0f, (18 + 19 + 21 + 22) / 4.0f,   (9 + 12) / 4.0f, (19 + 22) / 4.0f,
                                  10 / 4.0f,        20 / 4.0f,           (10 + 11) / 4.0f,           (20 + 21) / 4.0f,            (11 + 12)/ 4.0f,            (21 + 22)/ 4.0f,         12 / 4.0f,        22 / 4.0f
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 4, 3 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                              (1 + 3 + 19 + 21) / 4.0f,     (3 + 7 + 21 + 25) / 4.0f,     (7 + 5 + 25 + 23) / 4.0f,
                              (19 + 21 + 7 + 9) / 4.0f,    (21 + 25 + 9 + 13) / 4.0f,   (25 + 23 + 13 + 11) / 4.0f,
                              (7 + 9 + 25 + 27) / 4.0f,    (9 + 13 + 27 + 31) / 4.0f,   (13 + 11 + 31 + 29) / 4.0f,
                            (25 + 27 + 13 + 15) / 4.0f,   (27 + 31 + 15 + 19) / 4.0f,   (31 + 29 + 19 + 17) / 4.0f,

                              (2 + 4 + 20 + 22) / 4.0f,     (4 + 8 + 22 + 26) / 4.0f,     (8 + 6 + 26 + 24) / 4.0f,
                             (20 + 22 + 8 + 10) / 4.0f,   (22 + 26 + 10 + 14) / 4.0f,   (26 + 24 + 14 + 12) / 4.0f,
                             (8 + 10 + 26 + 28) / 4.0f,   (10 + 14 + 28 + 32) / 4.0f,   (14 + 12 + 32 + 30) / 4.0f,
                            (26 + 28 + 14 + 16) / 4.0f,   (28 + 32 + 16 + 20) / 4.0f,   (32 + 30 + 20 + 18) / 4.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                             1,   2,   3,
                             4,   5,   6,
                             7,   8,   9,
                            10,  11,  12,

                            11,  12,  13,
                            14,  15,  16,
                            17,  18,  19,
                            20,  21,  22,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                   1 / 4.0f,               (1 + 2) / 4.0f,               (2 + 3) / 4.0f,          3 / 4.0f,
                             (1 + 4) / 4.0f,       (1 + 2 + 4 + 5) / 4.0f,       (2 + 3 + 5 + 6) / 4.0f,    (3 + 6) / 4.0f,
                             (4 + 7) / 4.0f,       (4 + 5 + 7 + 8) / 4.0f,       (5 + 6 + 8 + 9) / 4.0f,    (6 + 9) / 4.0f,
                            (7 + 10) / 4.0f,     (7 + 8 + 10 + 11) / 4.0f,     (8 + 9 + 11 + 12) / 4.0f,   (9 + 12) / 4.0f,
                                  10 / 4.0f,             (10 + 11) / 4.0f,              (11 + 12)/ 4.0f,         12 / 4.0f,

                                   11 / 4.0f,            (11 + 12) / 4.0f,             (12 + 13) / 4.0f,          13 / 4.0f,
                            (11 + 14) / 4.0f,  (11 + 12 + 14 + 15) / 4.0f,   (12 + 13 + 15 + 16) / 4.0f,   (13 + 16) / 4.0f,
                            (14 + 17) / 4.0f,  (14 + 15 + 17 + 18) / 4.0f,   (15 + 16 + 18 + 19) / 4.0f,   (16 + 19) / 4.0f,
                            (17 + 20) / 4.0f,  (17 + 18 + 20 + 21) / 4.0f,   (18 + 19 + 21 + 22) / 4.0f,   (19 + 22) / 4.0f,
                                   20 / 4.0f,            (20 + 21) / 4.0f,              (21 + 22)/ 4.0f,          22 / 4.0f
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        [TestMethod]
        [Description("Filter 3x3, stride 3x3.")]
        public void ForwardBackwardTest3X3X3X3()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(3, 3, 3, 3));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 2, 2, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                            (1 + 3 + 7 + 19 + 21 + 25 + 7 + 9 + 13) / 9.0f, (2 + 4 + 8 + 20 + 22 + 26 + 8 + 10 + 14) / 9.0f,         (5 + 23 + 11) / 9.0f,  (6 + 24 + 12) / 9.0f,
                                      (25 + 27 + 31 + 13 + 15 + 19) / 9.0f,            (26 + 28 + 32 + 14 + 16 + 20) / 9.0f,             (29 + 17) / 9.0f,      (30 + 18) / 9.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1, 11,   2, 12,
                            3, 13,   4, 14,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                            1 / 9.0f, 11 / 9.0f,   1 / 9.0f, 11 / 9.0f,   1 / 9.0f, 11 / 9.0f,   2 / 9.0f, 12 / 9.0f,
                            1 / 9.0f, 11 / 9.0f,   1 / 9.0f, 11 / 9.0f,   1 / 9.0f, 11 / 9.0f,   2 / 9.0f, 12 / 9.0f,
                            1 / 9.0f, 11 / 9.0f,   1 / 9.0f, 11 / 9.0f,   1 / 9.0f, 11 / 9.0f,   2 / 9.0f, 12 / 9.0f,
                            3 / 9.0f, 13 / 9.0f,   3 / 9.0f, 13 / 9.0f,   3 / 9.0f, 13 / 9.0f,   4 / 9.0f, 14 / 9.0f,
                            3 / 9.0f, 13 / 9.0f,   3 / 9.0f, 13 / 9.0f,   3 / 9.0f, 13 / 9.0f,   4 / 9.0f, 14 / 9.0f,
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 2, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                             (1 + 3 + 7 + 19 + 21 + 25 + 7 + 9 + 13) / 9.0f,         (5 + 23 + 11) / 9.0f,
                                      (25 + 27 + 31 + 13 + 15 + 19) / 9.0f,             (29 + 17) / 9.0f,

                             (2 + 4 + 8 + 20 + 22 + 26 + 8 + 10 + 14) / 9.0f,        (6 + 24 + 12) / 9.0f,
                                       (26 + 28 + 32 + 14 + 16 + 20) / 9.0f,            (30 + 18) / 9.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1,   2,
                            3,   4,

                            11,   12,
                            13,   14,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                            1 / 9.0f,   1 / 9.0f,   1 / 9.0f,   2 / 9.0f,
                            1 / 9.0f,   1 / 9.0f,   1 / 9.0f,   2 / 9.0f,
                            1 / 9.0f,   1 / 9.0f,   1 / 9.0f,   2 / 9.0f,
                            3 / 9.0f,   3 / 9.0f,   3 / 9.0f,   4 / 9.0f,
                            3 / 9.0f,   3 / 9.0f,   3 / 9.0f,   4 / 9.0f,

                            11 / 9.0f,   11 / 9.0f,   11 / 9.0f,   12 / 9.0f,
                            11 / 9.0f,   11 / 9.0f,   11 / 9.0f,   12 / 9.0f,
                            11 / 9.0f,   11 / 9.0f,   11 / 9.0f,   12 / 9.0f,
                            13 / 9.0f,   13 / 9.0f,   13 / 9.0f,   14 / 9.0f,
                            13 / 9.0f,   13 / 9.0f,   13 / 9.0f,   14 / 9.0f,
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        [TestMethod]
        [Description("Filter 3x3, stride 2x2.")]
        public void ForwardBackwardTest3X3X2X2()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(3, 3, 2, 2));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 2, 2, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                               (1 + 3 + 7 + 19 + 21 + 25 + 7 + 9 + 13) / 9.0f,    (2 + 4 + 8 + 20 + 22 + 26 + 8 + 10 + 14) / 9.0f,      (7 + 5 + 25 + 23 + 13 + 11) / 9.0f,   (8 + 6 + 26 + 24 + 14 + 12) / 9.0f,
                            (7 + 9 + 13 + 25 + 27 + 31 + 13 + 15 + 19) / 9.0f, (8 + 10 + 14 + 26 + 28 + 32 + 14 + 16 + 20) / 9.0f,    (13 + 11 + 31 + 29 + 19 + 17) / 9.0f, (14 + 12 + 32 + 30 + 20 + 18) / 9.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1, 11,   2, 12,
                            3, 13,   4, 14,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                  1 / 9.0f,        11 / 9.0f,         1 / 9.0f,        11 / 9.0f,           (1 + 2) / 9.0f,           (11 + 12) / 9.0f,         2 / 9.0f,        12 / 9.0f,
                                  1 / 9.0f,        11 / 9.0f,         1 / 9.0f,        11 / 9.0f,           (1 + 2) / 9.0f,           (11 + 12) / 9.0f,         2 / 9.0f,        12 / 9.0f,
                            (1 + 3) / 9.0f, (11 + 13) / 9.0f,   (1 + 3) / 9.0f, (11 + 13) / 9.0f,   (1 + 2 + 3 + 4) / 9.0f, (11 + 12 + 13 + 14) / 9.0f,   (2 + 4) / 9.0f, (12 + 14) / 9.0f,
                                  3 / 9.0f,        13 / 9.0f,         3 / 9.0f,        13 / 9.0f,           (3 + 4) / 9.0f,           (13 + 14) / 9.0f,         4 / 9.0f,        14 / 9.0f,
                                  3 / 9.0f,        13 / 9.0f,         3 / 9.0f,        13 / 9.0f,           (3 + 4) / 9.0f,           (13 + 14) / 9.0f,         4 / 9.0f,        14 / 9.0f,
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 2, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                               (1 + 3 + 7 + 19 + 21 + 25 + 7 + 9 + 13) / 9.0f,      (7 + 5 + 25 + 23 + 13 + 11) / 9.0f,
                            (7 + 9 + 13 + 25 + 27 + 31 + 13 + 15 + 19) / 9.0f,    (13 + 11 + 31 + 29 + 19 + 17) / 9.0f,

                               (2 + 4 + 8 + 20 + 22 + 26 + 8 + 10 + 14) / 9.0f,     (8 + 6 + 26 + 24 + 14 + 12) / 9.0f,
                            (8 + 10 + 14 + 26 + 28 + 32 + 14 + 16 + 20) / 9.0f,   (14 + 12 + 32 + 30 + 20 + 18) / 9.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1,   2,
                            3,   4,

                            11,   12,
                            13,   14,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                  1 / 9.0f,             1 / 9.0f,              (1 + 2) / 9.0f,         2 / 9.0f,
                                  1 / 9.0f,             1 / 9.0f,              (1 + 2) / 9.0f,         2 / 9.0f,
                            (1 + 3) / 9.0f,       (1 + 3) / 9.0f,      (1 + 2 + 3 + 4) / 9.0f,   (2 + 4) / 9.0f,
                                  3 / 9.0f,             3 / 9.0f,              (3 + 4) / 9.0f,         4 / 9.0f,
                                  3 / 9.0f,             3 / 9.0f,              (3 + 4) / 9.0f,         4 / 9.0f,

                                   11 / 9.0f,          11 / 9.0f,            (11 + 12) / 9.0f,          12 / 9.0f,
                                   11 / 9.0f,          11 / 9.0f,            (11 + 12) / 9.0f,          12 / 9.0f,
                            (11 + 13) / 9.0f,   (11 + 13) / 9.0f,  (11 + 12 + 13 + 14) / 9.0f,   (12 + 14) / 9.0f,
                                   13 / 9.0f,          13 / 9.0f,            (13 + 14) / 9.0f,          14 / 9.0f,
                                   13 / 9.0f,          13 / 9.0f,            (13 + 14) / 9.0f,          14 / 9.0f,
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        [TestMethod]
        [Description("Filter 3x3, stride 1x1.")]
        public void ForwardBackwardTest3X3X1X1()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(3, 3, 1, 1));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                               (1 + 3 + 7 + 19 + 21 + 25 + 7 + 9 + 13) / 9.0f,    (2 + 4 + 8 + 20 + 22 + 26 + 8 + 10 + 14) / 9.0f,      (3 + 7 + 5 + 21 + 25 + 23 + 9 + 13 + 11) / 9.0f,    (4 + 8 + 6 + 22 + 26 + 24 + 10 + 14 + 12) / 9.0f,
                            (19 + 21 + 25 + 7 + 9 + 13 + 25 + 27 + 31) / 9.0f, (20 + 22 + 26 + 8 + 10 + 14 + 26 + 28 + 32) / 9.0f,   (21 + 25 + 23 + 9 + 13 + 11 + 27 + 31 + 29) / 9.0f, (22 + 26 + 24 + 10 + 14 + 12 + 28 + 32 + 30) / 9.0f,
                            (7 + 9 + 13 + 25 + 27 + 31 + 13 + 15 + 19) / 9.0f, (8 + 10 + 14 + 26 + 28 + 32 + 14 + 16 + 20) / 9.0f,   (9 + 13 + 11 + 27 + 31 + 29 + 15 + 19 + 17) / 9.0f, (10 + 14 + 12 + 28 + 32 + 30 + 16 + 20 + 18) / 9.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1, 11,   2, 12,
                            3, 13,   4, 14,
                            5, 15,   6, 16,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                      1 / 9.0f,             11 / 9.0f,                   (1 + 2) / 9.0f,                     (11 + 12) / 9.0f,                   (1 + 2) / 9.0f,                     (11 + 12) / 9.0f,             2 / 9.0f,             12 / 9.0f,
                                (1 + 3) / 9.0f,      (11 + 13) / 9.0f,           (1 + 2 + 3 + 4) / 9.0f,           (11 + 12 + 13 + 14) / 9.0f,           (1 + 2 + 3 + 4) / 9.0f,           (11 + 12 + 13 + 14) / 9.0f,       (2 + 4) / 9.0f,      (12 + 14) / 9.0f,
                            (1 + 3 + 5) / 9.0f, (11 + 13 + 15) / 9.0f,   (1 + 2 + 3 + 4 + 5 + 6) / 9.0f, (11 + 12 + 13 + 14 + 15 + 16) / 9.0f,   (1 + 2 + 3 + 4 + 5 + 6) / 9.0f, (11 + 12 + 13 + 14 + 15 + 16) / 9.0f,   (2 + 4 + 6) / 9.0f, (12 + 14 + 16) / 9.0f,
                                (3 + 5) / 9.0f,      (13 + 15) / 9.0f,           (3 + 4 + 5 + 6) / 9.0f,           (13 + 14 + 15 + 16) / 9.0f,           (3 + 4 + 5 + 6) / 9.0f,           (13 + 14 + 15 + 16) / 9.0f,       (4 + 6) / 9.0f,      (14 + 16) / 9.0f,
                                      5 / 9.0f,             15 / 9.0f,                   (5 + 6) / 9.0f,                     (15 + 16) / 9.0f,                   (5 + 6) / 9.0f,                     (15 + 16) / 9.0f,             6 / 9.0f,             16 / 9.0f,
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 3, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                               (1 + 3 + 7 + 19 + 21 + 25 + 7 + 9 + 13) / 9.0f,      (3 + 7 + 5 + 21 + 25 + 23 + 9 + 13 + 11) / 9.0f,
                            (19 + 21 + 25 + 7 + 9 + 13 + 25 + 27 + 31) / 9.0f,   (21 + 25 + 23 + 9 + 13 + 11 + 27 + 31 + 29) / 9.0f,
                            (7 + 9 + 13 + 25 + 27 + 31 + 13 + 15 + 19) / 9.0f,   (9 + 13 + 11 + 27 + 31 + 29 + 15 + 19 + 17) / 9.0f,

                               (2 + 4 + 8 + 20 + 22 + 26 + 8 + 10 + 14) / 9.0f,      (4 + 8 + 6 + 22 + 26 + 24 + 10 + 14 + 12) / 9.0f,
                            (20 + 22 + 26 + 8 + 10 + 14 + 26 + 28 + 32) / 9.0f,   (22 + 26 + 24 + 10 + 14 + 12 + 28 + 32 + 30) / 9.0f,
                            (8 + 10 + 14 + 26 + 28 + 32 + 14 + 16 + 20) / 9.0f,   (10 + 14 + 12 + 28 + 32 + 30 + 16 + 20 + 18) / 9.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1,    2,
                            3,    4,
                            5,    6,

                            11,   12,
                            13,   14,
                            15,   16,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                      1 / 9.0f,                            (1 + 2) / 9.0f,                         (1 + 2) / 9.0f,                2 / 9.0f,
                                (1 + 3) / 9.0f,                    (1 + 2 + 3 + 4) / 9.0f,                 (1 + 2 + 3 + 4) / 9.0f,          (2 + 4) / 9.0f,
                            (1 + 3 + 5) / 9.0f,            (1 + 2 + 3 + 4 + 5 + 6) / 9.0f,         (1 + 2 + 3 + 4 + 5 + 6) / 9.0f,      (2 + 4 + 6) / 9.0f,
                                (3 + 5) / 9.0f,                    (3 + 4 + 5 + 6) / 9.0f,                 (3 + 4 + 5 + 6) / 9.0f,          (4 + 6) / 9.0f,
                                      5 / 9.0f,                            (5 + 6) / 9.0f,                         (5 + 6) / 9.0f,                6 / 9.0f,

                                        11 / 9.0f,                       (11 + 12) / 9.0f,                       (11 + 12) / 9.0f,               12 / 9.0f,
                                 (11 + 13) / 9.0f,             (11 + 12 + 13 + 14) / 9.0f,             (11 + 12 + 13 + 14) / 9.0f,        (12 + 14) / 9.0f,
                            (11 + 13 + 15) / 9.0f,   (11 + 12 + 13 + 14 + 15 + 16) / 9.0f,   (11 + 12 + 13 + 14 + 15 + 16) / 9.0f,   (12 + 14 + 16) / 9.0f,
                                 (13 + 15) / 9.0f,             (13 + 14 + 15 + 16) / 9.0f,             (13 + 14 + 15 + 16) / 9.0f,        (14 + 16) / 9.0f,
                                        15 / 9.0f,                       (15 + 16) / 9.0f,                       (15 + 16) / 9.0f,               16 / 9.0f,
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        [TestMethod]
        [Description("Filter 4x4, stride 4x4.")]
        public void ForwardBackwardTest4X4X4X4()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(4, 4, 4, 4));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 2, 1, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                            (1 + 3 + 7 + 5 + 19 + 21 + 25 + 23 + 7 + 9 + 13 + 11 + 25 + 27 + 31 + 29) / 16.0f, (2 + 4 + 8 + 6 + 20 + 22 + 26 + 24 + 8 + 10 + 14 + 12 + 26 + 28 + 32 + 30) / 16.0f,
                                                                                  (13 + 15 + 19 + 17) / 16.0f,                                                        (14 + 16 + 20 + 18) / 16.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1, 11,
                            2, 12,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                            1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,
                            1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,
                            1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,
                            1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,   1 / 16.0f, 11 / 16.0f,
                            2 / 16.0f, 12 / 16.0f,   2 / 16.0f, 12 / 16.0f,   2 / 16.0f, 12 / 16.0f,   2 / 16.0f, 12 / 16.0f,
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 2, 1 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                            (1 + 3 + 7 + 5 + 19 + 21 + 25 + 23 + 7 + 9 + 13 + 11 + 25 + 27 + 31 + 29) / 16.0f,
                                                                                  (13 + 15 + 19 + 17) / 16.0f,

                            (2 + 4 + 8 + 6 + 20 + 22 + 26 + 24 + 8 + 10 + 14 + 12 + 26 + 28 + 32 + 30) / 16.0f,
                                                                                   (14 + 16 + 20 + 18) / 16.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1,
                            2,

                            11,
                            12,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                            1 / 16.0f,   1 / 16.0f,   1 / 16.0f,   1 / 16.0f,
                            1 / 16.0f,   1 / 16.0f,   1 / 16.0f,   1 / 16.0f,
                            1 / 16.0f,   1 / 16.0f,   1 / 16.0f,   1 / 16.0f,
                            1 / 16.0f,   1 / 16.0f,   1 / 16.0f,   1 / 16.0f,
                            2 / 16.0f,   2 / 16.0f,   2 / 16.0f,   2 / 16.0f,

                            11 / 16.0f,   11 / 16.0f,   11 / 16.0f,   11 / 16.0f,
                            11 / 16.0f,   11 / 16.0f,   11 / 16.0f,   11 / 16.0f,
                            11 / 16.0f,   11 / 16.0f,   11 / 16.0f,   11 / 16.0f,
                            11 / 16.0f,   11 / 16.0f,   11 / 16.0f,   11 / 16.0f,
                            12 / 16.0f,   12 / 16.0f,   12 / 16.0f,   12 / 16.0f,
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        [TestMethod]
        [Description("Filter 4x4, stride 1x1.")]
        public void ForwardBackwardTest4X4X1X1()
        {
            foreach ((Shape shape, float[] weights) in AveragePoolingLayerTest.sources)
            {
                Layer layer = new AveragePoolingLayer(shape, new Kernel(4, 4, 1, 1));

                Tensor xTemp = new Tensor(null, shape.Reshape(Axis.B, 1));
                xTemp.Set(weights);

                Tensor expectedTemp = new Tensor(null, layer.OutputShape.Reshape(Axis.B, 1));
                Tensor dyTemp = new Tensor(null, expectedTemp.Shape);
                Tensor expectedDxTemp = new Tensor(null, xTemp.Shape);

                switch (shape.Format)
                {
                    case Shape.BWHC:
                    case Shape.BHWC:
                        CollectionAssert.AreEqual(new[] { -1, 2, 1, 2 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                                (1 + 3 + 7 + 5 + 19 + 21 + 25 + 23 + 7 + 9 + 13 + 11 + 25 + 27 + 31 + 29) / 16.0f,     (2 + 4 + 8 + 6 + 20 + 22 + 26 + 24 + 8 + 10 + 14 + 12 + 26 + 28 + 32 + 30) / 16.0f,
                            (19 + 21 + 25 + 23 + 7 + 9 + 13 + 11 + 25 + 27 + 31 + 29 + 13 + 15 + 19 + 17) / 16.0f, (20 + 22 + 26 + 24 + 8 + 10 + 14 + 12 + 26 + 28 + 32 + 30 + 14 + 16 + 20 + 18) / 16.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1, 11,
                            2, 12,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                  1 / 16.0f,        11 / 16.0f,         1 / 16.0f,        11 / 16.0f,         1 / 16.0f,        11 / 16.0f,         1 / 16.0f,        11 / 16.0f,
                            (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,
                            (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,
                            (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,   (1 + 2) / 16.0f, (11 + 12) / 16.0f,
                                  2 / 16.0f,        12 / 16.0f,         2 / 16.0f,        12 / 16.0f,         2 / 16.0f,        12 / 16.0f,         2 / 16.0f,        12 / 16.0f,
                        });
                        break;

                    case Shape.BCHW:
                        CollectionAssert.AreEqual(new[] { -1, 2, 2, 1 }, layer.OutputShape.Axes);

                        expectedTemp.Set(new float[]
                        {
                                (1 + 3 + 7 + 5 + 19 + 21 + 25 + 23 + 7 + 9 + 13 + 11 + 25 + 27 + 31 + 29) / 16.0f,
                            (19 + 21 + 25 + 23 + 7 + 9 + 13 + 11 + 25 + 27 + 31 + 29 + 13 + 15 + 19 + 17) / 16.0f,

                                (2 + 4 + 8 + 6 + 20 + 22 + 26 + 24 + 8 + 10 + 14 + 12 + 26 + 28 + 32 + 30) / 16.0f,
                            (20 + 22 + 26 + 24 + 8 + 10 + 14 + 12 + 26 + 28 + 32 + 30 + 14 + 16 + 20 + 18) / 16.0f,
                        });

                        dyTemp.Set(new float[]
                        {
                            1,
                            2,

                            11,
                            12,
                        });

                        expectedDxTemp.Set(new float[]
                        {
                                  1 / 16.0f,         1 / 16.0f,         1 / 16.0f,         1 / 16.0f,
                            (1 + 2) / 16.0f,   (1 + 2) / 16.0f,   (1 + 2) / 16.0f,   (1 + 2) / 16.0f,
                            (1 + 2) / 16.0f,   (1 + 2) / 16.0f,   (1 + 2) / 16.0f,   (1 + 2) / 16.0f,
                            (1 + 2) / 16.0f,   (1 + 2) / 16.0f,   (1 + 2) / 16.0f,   (1 + 2) / 16.0f,
                                  2 / 16.0f,         2 / 16.0f,         2 / 16.0f,         2 / 16.0f,

                                   11 / 16.0f,         11 / 16.0f,         11 / 16.0f,         11 / 16.0f,
                            (11 + 12) / 16.0f,  (11 + 12) / 16.0f,  (11 + 12) / 16.0f,  (11 + 12) / 16.0f,
                            (11 + 12) / 16.0f,  (11 + 12) / 16.0f,  (11 + 12) / 16.0f,  (11 + 12) / 16.0f,
                            (11 + 12) / 16.0f,  (11 + 12) / 16.0f,  (11 + 12) / 16.0f,  (11 + 12) / 16.0f,
                                   12 / 16.0f,         12 / 16.0f,         12 / 16.0f,         12 / 16.0f,
                        });
                        break;
                }

                RunTest(layer, xTemp, expectedTemp, dyTemp, expectedDxTemp);
            }
        }

        private void RunTest(Layer layer, Tensor xTemp, Tensor expectedTemp, Tensor dyTemp, Tensor expectedDxTemp)
        {
            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(session.Tile(dyTemp, (int)Axis.B, i).Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
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

            for (int ib = 0, iib = y.Shape.GetAxis(Axis.B); ib < iib; ib++)
            {
                for (int ix = 0, xpos = -kernel.PaddingX, iix = y.Shape.GetAxis(Axis.X); ix < iix; ix++, xpos += kernel.StrideX)
                {
                    for (int iy = 0, ypos = -kernel.PaddingY, iiy = y.Shape.GetAxis(Axis.Y); iy < iiy; iy++, ypos += kernel.StrideY)
                    {
                        Tensor k = x.CropKernel(ib, xpos, ypos, kernel, false, out int kernelArea);

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

                            y[y.Shape.Position(ib, ix, iy, ic)] = sum / kernel.Size;
                        }
                    }
                }
            }

            return y;
        }

        private static Tensor CalculateDX(Tensor x, Tensor dy, Kernel kernel)
        {
            Tensor dx = new Tensor(null, x.Shape);

            for (int ib = 0; ib < dy.Shape.GetAxis(Axis.B); ib++)
            {
                for (int iy1 = 0, ix1 = -kernel.PaddingX; iy1 < dy.Shape.GetAxis(Axis.X); iy1++, ix1 += kernel.StrideX)
                {
                    for (int iy2 = 0, ix2 = -kernel.PaddingY; iy2 < dy.Shape.GetAxis(Axis.Y); iy2++, ix2 += kernel.StrideY)
                    {
                        int ikb1 = MinMax.Max(ix1, 0);
                        int ike1 = MinMax.Min(ix1 + kernel.Width, x.Shape.GetAxis(Axis.X));
                        int ikb2 = MinMax.Max(ix2, 0);
                        int ike2 = MinMax.Min(ix2 + kernel.Height, x.Shape.GetAxis(Axis.Y));

                        float alpha = 1.0f / kernel.Size;

                        for (int ic = 0; ic < dy.Shape.GetAxis(Axis.C); ic++)
                        {
                            float value = dy.Gradient[dy.Shape.Position(ib, iy1, iy2, ic)] * alpha;

                            for (int ik1 = ikb1; ik1 < ike1; ik1++)
                            {
                                for (int ik2 = ikb2; ik2 < ike2; ik2++)
                                {
                                    dx.Gradient[dx.Shape.Position(ib, ik1, ik2, ic)] += value;
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