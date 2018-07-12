namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SplitLayerTest
    {
        [TestMethod]
        public void CreateLayerTest1()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            string architecture = "SP3";
            SplitLayer layer = (SplitLayer)NetworkGraphBuilder.CreateLayer(shape, architecture, null);

            Assert.AreEqual(architecture, layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateLayerTest2()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            string architecture = "SP";
            Assert.IsNotNull(NetworkGraphBuilder.CreateLayer(shape, architecture, null));
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            SplitLayer layer = new SplitLayer(shape, 3);

            Assert.AreEqual(3, layer.NumberOfOutputs);
            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("SP3", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new SplitLayer((int[])null, 3));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            SplitLayer layer1 = new SplitLayer(shape, 3);
            SplitLayer layer2 = new SplitLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new SplitLayer((SplitLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            SplitLayer layer1 = new SplitLayer(shape, 3);
            SplitLayer layer2 = layer1.Clone() as SplitLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            SplitLayer layer1 = new SplitLayer(shape, 3);
            string s1 = JsonConvert.SerializeObject(layer1);
            SplitLayer layer2 = JsonConvert.DeserializeObject<SplitLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest1()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            SplitLayer layer = new SplitLayer(shape, 3);

            for (int mb = 1; mb <= 3; mb++)
            {
                Session session = new Session();

                Tensor x = new Tensor(null, Shape.Reshape(shape, (int)Axis.B, mb));
                x.Randomize();

                IList<Tensor> ys = layer.Forward(session, new[] { x });

                Assert.AreEqual(layer.NumberOfOutputs, ys.Count);
                foreach (Tensor y in ys)
                {
                    Helpers.AreTensorsEqual(x, y);
                }

                // unroll the graph
                // during backward pass we should average all gradients into a single tensor
                for (int i = 0; i < layer.NumberOfOutputs; i++)
                {
                    ys[i].SetGradient(i + 2);
                }

                session.Unroll();

                Helpers.IsArrayEqual(x.Length, 3.0f, x.Gradient, 0);
            }
        }
    }
}
