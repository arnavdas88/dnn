namespace Genix.DNN.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class MaxPoolingLayerTest
    {
        private static int[] sourceShape = new[] { -1, 5, 4, 2 };

        private static float[] weights = new float[]
        {
            1,  2,     3,  4,    7,  8,    5,  6,
            19, 20,   21, 22,   25, 26,   23, 24,
            7,  8,     9, 10,   13, 14,   11, 12,
            25, 26,   27, 28,   31, 32,   29, 30,
            13, 14,   15, 16,   19, 20,   17, 18,
        };

        [TestMethod]
        public void ConstructorTest1()
        {
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(2, 2, 2, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape);
            Assert.AreEqual("MP2", layer.Architecture);
            Assert.AreEqual(2, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(3, 2, 1, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape);
            Assert.AreEqual("MP3x2+1x2(S)", layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(1, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
        }

        [TestMethod]
        public void ArchitectureConstructorTest1()
        {
            string architecture = "MP3x2";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(3, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "MP3x2+2(S)";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(2, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        public void ArchitectureConstructorTest3()
        {
            string architecture = "MP3x2+2x1(S)";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(3, layer.Kernel.Width);
            Assert.AreEqual(2, layer.Kernel.Height);
            Assert.AreEqual(2, layer.Kernel.StrideX);
            Assert.AreEqual(1, layer.Kernel.StrideY);
            Assert.AreEqual(0, layer.Kernel.PaddingX);
            Assert.AreEqual(0, layer.Kernel.PaddingY);
        }

        [TestMethod]
        public void ArchitectureConstructorTest4()
        {
            string architecture = "MP2";
            MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, architecture, null);

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
        public void ArchitectureConstructorTest5()
        {
            string architecture = "MP";
            try
            {
                MaxPoolingLayer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, architecture, null);
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
        public void ArchitectureConstructorTest6()
        {
            Assert.IsNotNull(new MaxPoolingLayer(null, "MP2", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest7()
        {
            Assert.IsNotNull(new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            MaxPoolingLayer layer1 = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(3, 2, 1, 2));
            MaxPoolingLayer layer2 = new MaxPoolingLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new MaxPoolingLayer(null, new Kernel(2, 2, 2, 2)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new MaxPoolingLayer((MaxPoolingLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            MaxPoolingLayer layer1 = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(2, 2, 2, 2));
            MaxPoolingLayer layer2 = layer1.Clone() as MaxPoolingLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            MaxPoolingLayer layer1 = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(2, 2, 2, 2));
            string s1 = JsonConvert.SerializeObject(layer1);
            MaxPoolingLayer layer2 = JsonConvert.DeserializeObject<MaxPoolingLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Description("Filter 2x2, stride 2x2.")]
        public void ForwardBackwardTest2X2X2X2()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(2, 2, 2, 2));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 3, 2, 2 });
            expectedTemp.Set(new float[]
            {
                21, 22,   25, 26,
                27, 28,   31, 32,
                15, 16,   19, 20,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,    0,  0,    0,  0,   0, 0,
                0, 0,   21, 22,   25, 26,   0, 0,
                0, 0,    0,  0,    0,  0,   0, 0,
                0, 0,   27, 28,   31, 32,   0, 0,
                0, 0,   15, 16,   19, 20,   0, 0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }

        [TestMethod]
        [Description("Filter 2x2, stride 1x1.")]
        public void ForwardBackwardTest2X2X1X1()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(2, 2, 1, 1));

            CollectionAssert.AreEqual(new[] { -1, 4, 3, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 4, 3, 2 });
            expectedTemp.Set(new float[]
            {
                21, 22,   25, 26,   25, 26,
                21, 22,   25, 26,   25, 26,
                27, 28,   31, 32,   31, 32,
                27, 28,   31, 32,   31, 32,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,      0,    0,      0,    0,   0, 0,
                0, 0,   2*21, 2*22,   4*25, 4*26,   0, 0,
                0, 0,      0,    0,      0,    0,   0, 0,
                0, 0,   2*27, 2*28,   4*31, 4*32,   0, 0,
                0, 0,      0,    0,      0,    0,   0, 0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }

        [TestMethod]
        [Description("Filter 3x3, stride 3x3.")]
        public void ForwardBackwardTest3X3X3X3()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(3, 3, 3, 3));

            CollectionAssert.AreEqual(new[] { -1, 2, 2, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 2, 2, 2 });
            expectedTemp.Set(new float[]
            {
                25, 26,   23, 24,
                31, 32,   29, 30,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,   0, 0,    0,  0,    0,  0,
                0, 0,   0, 0,   25, 26,   23, 24,
                0, 0,   0, 0,    0,  0,    0,  0,
                0, 0,   0, 0,   31, 32,   29, 30,
                0, 0,   0, 0,    0,  0,    0,  0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }

        [TestMethod]
        [Description("Filter 3x3, stride 2x2.")]
        public void ForwardBackwardTest3X3X2X2()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(3, 3, 2, 2));

            CollectionAssert.AreEqual(new[] { -1, 2, 2, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 2, 2, 2 });
            expectedTemp.Set(new float[]
            {
                25, 26,   25, 26,
                31, 32,   31, 32,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,   0, 0,      0,    0,   0, 0,
                0, 0,   0, 0,   2*25, 2*26,   0, 0,
                0, 0,   0, 0,      0,    0,   0, 0,
                0, 0,   0, 0,   2*31, 2*32,   0, 0,
                0, 0,   0, 0,      0,    0,   0, 0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }

        [TestMethod]
        [Description("Filter 3x3, stride 1x1.")]
        public void ForwardBackwardTest3X3X1X1()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(3, 3, 1, 1));

            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 3, 2, 2 });
            expectedTemp.Set(new float[]
            {
                25, 26,   25, 26,
                31, 32,   31, 32,
                31, 32,   31, 32,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,   0, 0,      0,    0,   0, 0,
                0, 0,   0, 0,   2*25, 2*26,   0, 0,
                0, 0,   0, 0,      0,    0,   0, 0,
                0, 0,   0, 0,   4*31, 4*32,   0, 0,
                0, 0,   0, 0,      0,    0,   0, 0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }

        [TestMethod]
        [Description("Filter 4x4, stride 4x4.")]
        public void ForwardBackwardTest4X4X4X4()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(4, 4, 4, 4));

            CollectionAssert.AreEqual(new[] { -1, 2, 1, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 2, 1, 2 });
            expectedTemp.Set(new float[]
            {
                31, 32,
                19, 20,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,   0, 0,    0,  0,   0, 0,
                0, 0,   0, 0,    0,  0,   0, 0,
                0, 0,   0, 0,    0,  0,   0, 0,
                0, 0,   0, 0,   31, 32,   0, 0,
                0, 0,   0, 0,   19, 20,   0, 0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }

        [TestMethod]
        [Description("Filter 4x4, stride 1x1.")]
        public void ForwardBackwardTest4X4X1X1()
        {
            Layer layer = new MaxPoolingLayer(MaxPoolingLayerTest.sourceShape, new Kernel(4, 4, 1, 1));

            CollectionAssert.AreEqual(new[] { -1, 2, 1, 2 }, layer.OutputShape);

            Tensor xTemp = new Tensor(null, Shape.Reshape(MaxPoolingLayerTest.sourceShape, (int)Axis.B, 1));
            xTemp.Set(MaxPoolingLayerTest.weights);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 2, 1, 2 });
            expectedTemp.Set(new float[]
            {
                31, 32,
                31, 32,
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 5, 4, 2 });
            expectedDxTemp.Set(new float[]
            {
                0, 0,   0, 0,   0, 0,         0, 0,
                0, 0,   0, 0,   0, 0,         0, 0,
                0, 0,   0, 0,   0, 0,         0, 0,
                0, 0,   0, 0,   2*31, 2*32,   0, 0,
                0, 0,   0, 0,   0, 0,         0, 0,
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the session
                y.SetGradient(expected.Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Length, expectedDx.Weights, x.Gradient);
            }
        }
    }
}
