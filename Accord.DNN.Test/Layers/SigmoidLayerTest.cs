namespace Accord.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accord.DNN;
    using Accord.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SigmoidLayerTest
    {
        private static Func<float, float> activation = (x) => 1.0f / (1.0f + (float)Math.Exp(-x));
        private static Func<float, float> derivative = (x) => (x * (1.0f - x));

        private int[] shape;
        private Tensor source;
        private SigmoidLayer layer;

        [TestInitialize]
        public void BeforeEach()
        {
            this.shape = new[] { 2 };
            this.source = new Tensor(null, this.shape, new float[] { 2, -3 });

            this.layer = new SigmoidLayer(this.shape);
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            CollectionAssert.AreEqual(this.shape, this.layer.OutputShape);
            Assert.AreEqual("SIG", this.layer.Architecture);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            Layer layer2 = new SigmoidLayer(this.layer);
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new SigmoidLayer((int[])null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new SigmoidLayer((SigmoidLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            Layer layer2 = this.layer.Clone() as SigmoidLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            string s1 = JsonConvert.SerializeObject(this.layer);
            SigmoidLayer layer2 = JsonConvert.DeserializeObject<SigmoidLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            Session session = new Session();

            Tensor x = this.source.Clone() as Tensor;
            Tensor y = this.layer.Forward(session, new[] { x })[0];

            float[] expected = this.source.Weights.Select(w => SigmoidLayerTest.activation(w)).ToArray();
            Helpers.AreArraysEqual(expected, y.Weights);

            // unroll the graph
            y.SetGradient(Enumerable.Range(1, x.Length).Select(w => (float)w).ToArray());
            session.Unroll();

            Helpers.AreArraysEqual(
                expected.Zip(y.Gradient, (w, dy) => SigmoidLayerTest.derivative(w) * dy).ToArray(),
                x.Gradient);
        }
    }
}
