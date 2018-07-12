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
    public class ScaleLayerTest
    {
        private int[] shape;
        private ScaleLayer layer;

        [TestInitialize]
        public void BeforeEach()
        {
            this.shape = new[] { -1, 20, 20, 10 };
            this.layer = new ScaleLayer(this.shape, 0.7f);
        }

        [TestMethod]
        public void CreateLayerTest1()
        {
            string architecture = "S0.7";
            this.layer = (ScaleLayer)NetworkGraphBuilder.CreateLayer(this.shape, architecture, null);

            Assert.AreEqual(0.7f, this.layer.Alpha);
            Assert.AreEqual(architecture, this.layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateLayerTest2()
        {
            string architecture = "S";
            Assert.IsNotNull(NetworkGraphBuilder.CreateLayer(this.shape, architecture, null));
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            CollectionAssert.AreEqual(this.shape, this.layer.OutputShape);
            Assert.AreEqual("S0.7", this.layer.Architecture);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            ScaleLayer layer2 = new ScaleLayer(this.layer);
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new ScaleLayer((int[])null, 0.7f));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new ScaleLayer((ScaleLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            ScaleLayer layer2 = this.layer.Clone() as ScaleLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            string s1 = JsonConvert.SerializeObject(this.layer);
            ScaleLayer layer2 = JsonConvert.DeserializeObject<ScaleLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Description("Shall multiply all weights by scale factor.")]
        public void ForwardBackwardTest()
        {
            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = new Tensor(null, Shape.Reshape(shape, (int)Axis.B, i));
                x.Randomize();

                Tensor y = layer.Forward(session, new[] { x })[0];

                Tensor expected = new Tensor(null, x.Axes, x.Weights.Select(w => w * this.layer.Alpha).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                // unroll the graph
                y.SetGradient(y.Weights);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y.Gradient.Select(w => w * this.layer.Alpha).ToArray(),
                    x.Gradient);
            }
        }
    }
}
