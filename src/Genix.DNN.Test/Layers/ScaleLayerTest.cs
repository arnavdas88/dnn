namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ScaleLayerTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            ScaleLayer layer = new ScaleLayer(shape, 0.7f);

            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual(0.7f, layer.Alpha);
            Assert.AreEqual("S0.7", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new ScaleLayer(null, 0.7f));
        }

        [TestMethod]
        public void ArchitectureConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            ScaleLayer layer = new ScaleLayer(shape, "S0.7", null);

            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("S0.7", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "S";
            try
            {
                ScaleLayer layer = new ScaleLayer(new[] { -1, 20, 20, 10 }, architecture, null);
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
            Assert.IsNotNull(new ScaleLayer(null, "S0.7", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new ScaleLayer(new[] { -1, 20, 20, 10 }, null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            ScaleLayer layer1 = new ScaleLayer(new[] { -1, 20, 20, 10 }, 0.7f);
            ScaleLayer layer2 = new ScaleLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new ScaleLayer((ScaleLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            ScaleLayer layer1 = new ScaleLayer(new[] { -1, 20, 20, 10 }, 0.7f);
            ScaleLayer layer2 = layer1.Clone() as ScaleLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            ScaleLayer layer1 = new ScaleLayer(new[] { -1, 20, 20, 10 }, 0.7f);
            string s1 = JsonConvert.SerializeObject(layer1);
            ScaleLayer layer2 = JsonConvert.DeserializeObject<ScaleLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Description("Shall multiply all weights by scale factor.")]
        public void ForwardBackwardTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            ScaleLayer layer = new ScaleLayer(shape, 0.7f);

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = new Tensor(null, Shape.Reshape(shape, (int)Axis.B, i));
                x.Randomize();

                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = new Tensor(null, x.Axes, x.Weights.Take(x.Length).Select(w => w * layer.Alpha).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                // unroll the graph
                y.SetGradient(y.Weights);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y.Length,
                    y.Gradient.Take(y.Length).Select(w => w * layer.Alpha).ToArray(),
                    x.Gradient);
            }
        }
    }
}
