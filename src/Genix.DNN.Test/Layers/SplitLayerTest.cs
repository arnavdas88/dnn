namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SplitLayerTest
    {
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
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new SplitLayer(null, 3));
        }

        [TestMethod]
        public void ArchitectureConstructorTest1()
        {
            int[] shape = new[] { -1, 2, 2, 2 };
            SplitLayer layer = new SplitLayer(shape, "SP3", null);

            Assert.AreEqual(3, layer.NumberOfOutputs);
            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("SP3", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "SP";
            try
            {
                SplitLayer layer = new SplitLayer(new[] { -1, 2, 2, 2 }, architecture, null);
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
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new SplitLayer(null, "SP3", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new SplitLayer(new[] { -1, 2, 2, 2 }, null, null));
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

                Tensor x = new Tensor(null, TensorShape.Unknown, Shape.Reshape(shape, (int)Axis.B, mb));
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
