namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using Genix.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class MaxOutLayerTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            MaxOutLayer layer = new MaxOutLayer(new[] { -1, 3, 2, 4 }, 2);
            CollectionAssert.AreEqual(new[] { -1, 3, 2, 2 }, layer.OutputShape);
            Assert.AreEqual(2, layer.GroupSize);
            Assert.AreEqual("MO2", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new MaxOutLayer(null, 2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new MaxOutLayer(new[] { -1, 3, 2, 4 }, 3));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            MaxOutLayer layer1 = new MaxOutLayer(new[] { -1, 3, 2, 4 }, 2);
            MaxOutLayer layer2 = new MaxOutLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new MaxOutLayer(null));
        }

        [TestMethod]
        public void CloneTest()
        {
            MaxOutLayer layer1 = new MaxOutLayer(new[] { -1, 3, 2, 4 }, 2);
            MaxOutLayer layer2 = layer1.Clone() as MaxOutLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            MaxOutLayer layer1 = new MaxOutLayer(new[] { -1, 3, 2, 4 }, 2);
            string s1 = JsonConvert.SerializeObject(layer1);
            MaxOutLayer layer2 = JsonConvert.DeserializeObject<MaxOutLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest1()
        {
            int[] shape = new[] { -1, 3, 2, 4 };
            MaxOutLayer layer = new MaxOutLayer(shape, 2);

            Tensor xTemp = new Tensor(null, Shape.Reshape(shape, (int)Axis.B, 1));
            xTemp.Set(new float[]
            {
                11, 12, 13, 14,  12, -11, 15, 16,
                21, 22, 23, 24,  22, -21, 25, 26,
                31, 32, 33, 34,  32, -31, 35, 36,
            });

            Tensor expectedTemp = new Tensor(null, Shape.Reshape(layer.OutputShape, (int)Axis.B, 1));
            expectedTemp.Set(new float[]
            {
                12, 14,  12, 16,
                22, 24,  22, 26,
                32, 34,  32, 36,
            });

            Tensor dyTemp = new Tensor(null, expectedTemp.Axes);
            dyTemp.Set(new float[]
            {
                12, 14,  12, 16,
                22, 24,  22, 26,
                32, 34,  32, 36,
            });

            Tensor expectedDxTemp = new Tensor(null, xTemp.Axes);
            expectedDxTemp.Set(new float[]
            {
                0, 12, 0, 14,  12, 0, 0, 16,
                0, 22, 0, 24,  22, 0, 0, 26,
                0, 32, 0, 34,  32, 0, 0, 36,
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
    }
}
