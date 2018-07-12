namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Newtonsoft.Json;

    [TestClass]
    public class ReLULayerTest
    {
        private static Func<float, float> activation = (x) => Math.Max(x, 0);
        private static Func<float, float> derivative = (x) => (x == 0.0f ? 0.0f : 1.0f);

        private int[] shape;
        private Tensor source;
        private ReLULayer layer;

        [TestInitialize]
        public void BeforeEach()
        {
            this.shape = new[] { 2 };
            this.source = new Tensor(null, this.shape, new float[] { 2, -3 });

            this.layer = new ReLULayer(this.shape);
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            CollectionAssert.AreEqual(this.shape, this.layer.OutputShape);
            Assert.AreEqual("RELU", this.layer.Architecture);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            Layer layer2 = new ReLULayer(this.layer);
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new ReLULayer((int[])null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new ReLULayer((ReLULayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            Layer layer2 = this.layer.Clone() as ReLULayer;
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            string s1 = JsonConvert.SerializeObject(this.layer);
            ReLULayer layer2 = JsonConvert.DeserializeObject<ReLULayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            Session session = new Session();

            Tensor x = this.source.Clone() as Tensor;
            Tensor y = this.layer.Forward(session, new[] { x })[0];

            float[] expected = this.source.Weights.Select(w => ReLULayerTest.activation(w)).ToArray();
            Helpers.AreArraysEqual(expected, y.Weights);

            // unroll the graph
            y.SetGradient(Enumerable.Range(1, x.Length).Select(w => (float)w).ToArray());
            session.Unroll();

            Helpers.AreArraysEqual(
                expected.Zip(y.Gradient, (w, dy) => ReLULayerTest.derivative(w) * dy).ToArray(),
                x.Gradient);
        }
    }
}
