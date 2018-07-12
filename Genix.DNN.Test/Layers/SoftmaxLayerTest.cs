namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SoftMaxLayerTest
    {
        private static float[] weights = new float[] { 0.2f, -0.3f, 0.8f, 0.4f };
        private static float[] activations = new float[] {
            0.215051353f,
            0.130435243f,
            0.39184913f,
            0.262664318f
        };

        private int[] shape;
        private SoftMaxLayer layer;

        [TestInitialize]
        public void BeforeEach()
        {
            this.shape = new[] { 1, SoftMaxLayerTest.weights.Length };
            this.layer = new SoftMaxLayer(this.shape);
        }

        [TestMethod]
        public void CreateLayerTest1()
        {
            string architecture = "SM";
            this.layer = (SoftMaxLayer)NetworkGraphBuilder.CreateLayer(this.shape, architecture, null);

            Assert.AreEqual(SoftMaxLayerTest.weights.Length, this.layer.NumberOfClasses);
            Assert.AreEqual(architecture, this.layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateLayerTest2()
        {
            string architecture = "SMX";
            Assert.IsNotNull(NetworkGraphBuilder.CreateLayer(this.shape, architecture, null));
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            CollectionAssert.AreEqual(new[] { 1, SoftMaxLayerTest.weights.Length }, this.layer.OutputShape);
            Assert.AreEqual("SM", this.layer.Architecture);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            SoftMaxLayer layer2 = new SoftMaxLayer(this.layer);
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new SoftMaxLayer((int[])null));
        }

        [TestMethod]
        public void CloneTest()
        {
            SoftMaxLayer layer2 = this.layer.Clone() as SoftMaxLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            string s1 = JsonConvert.SerializeObject(this.layer);
            SoftMaxLayer layer2 = JsonConvert.DeserializeObject<SoftMaxLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            Session session = new Session();

            Tensor x = new Tensor(null, new[] { 1, SoftMaxLayerTest.weights.Length }, SoftMaxLayerTest.weights);
            Tensor y = layer.Forward(session, new[] { x })[0];

            Helpers.AreArraysEqual(SoftMaxLayerTest.activations, y.Weights);

            // unroll the graph
            y.Gradient[0] = 1.0f;
            session.Unroll();

            ////float[] expectedDx = SoftMaxLayerTest.activations.Select((w, i) => i == 0 ? w - 1.0f : w).ToArray();
            Helpers.AreArraysEqual(new float[] { 1.0f, 0, 0, 0 }, x.Gradient);
        }
    }
}
