namespace Accord.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Accord.DNN;
    using Accord.DNN.Layers;
    using Newtonsoft.Json;

    [TestClass]
    public class TanhLayerTest
    {
        private static Func<float, float> activation = (x) => (float)Math.Tanh(x);
        private static Func<float, float> derivative = (x) => (1.0f - x * x);

        private int[] shape;
        private Tensor source;
        private TanhLayer layer;

        [TestInitialize]
        public void BeforeEach()
        {
            this.shape = new[] { 2 };
            this.source = new Tensor(null, this.shape, new float[] { 2, -3 });

            this.layer = new TanhLayer(this.shape);
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            CollectionAssert.AreEqual(this.shape, this.layer.OutputShape);
            Assert.AreEqual("TH", this.layer.Architecture);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            Layer layer2 = new TanhLayer(this.layer);
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new TanhLayer((int[])null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest1()
        {
            Assert.IsNotNull(new TanhLayer((TanhLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            Layer layer2 = this.layer.Clone() as TanhLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            string s1 = JsonConvert.SerializeObject(this.layer);
            TanhLayer layer2 = JsonConvert.DeserializeObject<TanhLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            Session session = new Session();

            Tensor x = this.source.Clone() as Tensor;
            Tensor y = this.layer.Forward(session, new[] { x })[0];

            float[] expected = this.source.Weights.Select(w => TanhLayerTest.activation(w)).ToArray();
            Helpers.AreArraysEqual(expected, y.Weights);

            // unroll the graph
            y.SetGradient(Enumerable.Range(1, x.Length).Select(w => (float)w).ToArray());
            session.Unroll();

            Helpers.AreArraysEqual(
                expected.Zip(y.Gradient, (w, dy) => TanhLayerTest.derivative(w) * dy).ToArray(),
                x.Gradient);
        }
    }
}
