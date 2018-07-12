namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class LRNLayerTest
    {
        private static int[] SourceShape = new[] { -1, 24, 24, 20 };

        [TestMethod]
        public void CreateLayerTest1()
        {
            const string Architecture = "LRN5";
            LRNLayer layer = (LRNLayer)NetworkGraphBuilder.CreateLayer(LRNLayerTest.SourceShape, Architecture, null);

            Assert.AreEqual(Architecture, layer.Architecture);
            Assert.AreEqual(5, layer.KernelSize);
            Assert.AreEqual(LRNLayer.DefaultAlpha, layer.Alpha);
            Assert.AreEqual(LRNLayer.DefaultBeta, layer.Beta);
            Assert.AreEqual(LRNLayer.DefaultK, layer.K);
        }

        [TestMethod]
        public void CreateLayerTest2()
        {
            const string Architecture = "LRN7(A=0.001;B=0.5;K=3)";
            LRNLayer layer = (LRNLayer)NetworkGraphBuilder.CreateLayer(LRNLayerTest.SourceShape, Architecture, null);

            Assert.AreEqual(Architecture, layer.Architecture);
            Assert.AreEqual(7, layer.KernelSize);
            Assert.AreEqual(0.001f, layer.Alpha);
            Assert.AreEqual(0.5f, layer.Beta);
            Assert.AreEqual(3f, layer.K);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateLayerTest3()
        {
            const string Architecture = "LRN";
            Assert.IsNotNull(NetworkGraphBuilder.CreateLayer(LRNLayerTest.SourceShape, Architecture, null));
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            LRNLayer layer = new LRNLayer(LRNLayerTest.SourceShape, 7, 0.001f, 0.5f, 3f);

            CollectionAssert.AreEqual(LRNLayerTest.SourceShape, layer.OutputShape);
            Assert.AreEqual("LRN7(A=0.001;B=0.5;K=3)", layer.Architecture);
            Assert.AreEqual(7, layer.KernelSize);
            Assert.AreEqual(0.001f, layer.Alpha);
            Assert.AreEqual(0.5f, layer.Beta);
            Assert.AreEqual(3f, layer.K);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            LRNLayer layer = new LRNLayer(LRNLayerTest.SourceShape, 7);

            CollectionAssert.AreEqual(LRNLayerTest.SourceShape, layer.OutputShape);
            Assert.AreEqual("LRN7", layer.Architecture);
            Assert.AreEqual(7, layer.KernelSize);
            Assert.AreEqual(LRNLayer.DefaultAlpha, layer.Alpha);
            Assert.AreEqual(LRNLayer.DefaultBeta, layer.Beta);
            Assert.AreEqual(LRNLayer.DefaultK, layer.K);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new LRNLayer((int[])null, 7));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new LRNLayer((LRNLayer)null));
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is a parameter of the function being tested.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest5()
        {
            try
            {
                Assert.IsNotNull(new LRNLayer(LRNLayerTest.SourceShape, 0));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_InvalidLRNKernelSize, "kernelSize").Message, e.Message);
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is a parameter of the function being tested.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest6()
        {
            try
            {
                Assert.IsNotNull(new LRNLayer(LRNLayerTest.SourceShape, 2));
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_InvalidLRNKernelSize, "kernelSize").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void CloneTest()
        {
            LRNLayer layer1 = new LRNLayer(LRNLayerTest.SourceShape, 7);
            LRNLayer layer2 = layer1.Clone() as LRNLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            LRNLayer layer1 = new LRNLayer(LRNLayerTest.SourceShape, 7);
            string s1 = JsonConvert.SerializeObject(layer1);
            LRNLayer layer2 = JsonConvert.DeserializeObject<LRNLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", Justification = "Need for testing.")]
        [TestMethod]
        public void ForwardBackwardTest()
        {
            int[] shape = new[] { -1, 1, 2, 5 };
            LRNLayer layer = new LRNLayer(shape, 3);

            Tensor xTemp = new Tensor(null, new[] { 1, 1, 2, 5 });
            xTemp.Set(new float[]
            {
                1, 11, 21, 31, 41,   2, 12, 22, 32, 42
            });

            Tensor expectedTemp = new Tensor(null, new[] { 1, 1, 2, 5 });
            expectedTemp.Set(new float[]
            {
                LRNLayerTest.Forward(layer, 1, 11),
                LRNLayerTest.Forward(layer, 11, 1, 21),
                LRNLayerTest.Forward(layer, 21, 11, 31),
                LRNLayerTest.Forward(layer, 31, 21, 41),
                LRNLayerTest.Forward(layer, 41, 31),

                LRNLayerTest.Forward(layer, 2, 12),
                LRNLayerTest.Forward(layer, 12, 2, 22),
                LRNLayerTest.Forward(layer, 22, 12, 32),
                LRNLayerTest.Forward(layer, 32, 22, 42),
                LRNLayerTest.Forward(layer, 42, 32),
            });

            Tensor dyTemp = new Tensor(null, new[] { 1, 1, 2, 5 });
            dyTemp.Set(new float[]
            {
                1, 11, 21, 31, 41,   2, 12, 22, 32, 42
            });

            Tensor expectedDxTemp = new Tensor(null, new[] { 1, 1, 2, 5 });
            expectedDxTemp.Set(new float[]
            {
                0.591914058f, 6.406341f, 11.8103943f, 16.4343262f, 22.1168289f,   1.18269f, 6.97113f, 12.3151608f, 16.8460541f, 22.5372047f
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the graph
                y.SetGradient(session.Tile(dyTemp, (int)Axis.B, i).Weights);
                session.Unroll();

                Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                Helpers.AreArraysEqual(expectedDx.Weights, x.Gradient);
            }
        }

        private static float Forward(LRNLayer layer, params float[] xs)
        {
            float scale = layer.K + layer.Alpha * xs.Sum(x => x * x) / layer.KernelSize;
            return xs[0] * (float)Math.Pow(scale, -layer.Beta);
        }
    }
}
