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
    public class TanhLayerTest
    {
        private static Func<float, float> activation = (x) => (float)Math.Tanh(x);
        private static Func<float, float> derivative = (x) => (1.0f - (x * x));

        [TestMethod]
        public void ConstructorTest1()
        {
            Shape shape = new Shape(new int[2]);
            TanhLayer layer = new TanhLayer(shape);
            CollectionAssert.AreEqual(shape.Axes, layer.OutputShape.Axes);
            Assert.AreEqual("TH", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new TanhLayer((Shape)null));
        }

        [TestMethod]
        public void ArchitectureConstructorTest1()
        {
            Shape shape = new Shape(new int[2]);
            TanhLayer layer = new TanhLayer(shape, "TH", null);

            CollectionAssert.AreEqual(shape.Axes, layer.OutputShape.Axes);
            Assert.AreEqual("TH", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "THH";
            try
            {
                TanhLayer layer = new TanhLayer(new Shape(new int[2]), architecture, null);
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
            Assert.IsNotNull(new TanhLayer(null, "TH", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new TanhLayer(new Shape(new int[2]), null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            TanhLayer layer1 = new TanhLayer(new Shape(new int[2]));
            TanhLayer layer2 = new TanhLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new TanhLayer((TanhLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            TanhLayer layer1 = new TanhLayer(new Shape(new int[2]));
            TanhLayer layer2 = layer1.Clone() as TanhLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            TanhLayer layer1 = new TanhLayer(new Shape(new int[2]));
            string s1 = JsonConvert.SerializeObject(layer1);
            TanhLayer layer2 = JsonConvert.DeserializeObject<TanhLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            Shape shape = new Shape(new int[2]);
            TanhLayer layer = new TanhLayer(shape);

            Session session = new Session();

            Tensor source = new Tensor(null, shape);
            source.Set(new float[] { 2, -3 });

            Tensor x = source.Clone() as Tensor;
            Tensor y = layer.Forward(session, new[] { x })[0];

            float[] expected = source.Weights.Take(source.Length).Select(w => TanhLayerTest.activation(w)).ToArray();
            Helpers.AreArraysEqual(x.Length, expected, y.Weights);

            // unroll the graph
            float[] dy = Enumerable.Range(1, x.Length).Select(w => (float)w).ToArray();
            y.SetGradient(dy);
            session.Unroll();

            Helpers.AreArraysEqual(
                expected.Length,
                expected.Zip(dy, (w, dw) => TanhLayerTest.derivative(w) * dw).ToArray(),
                x.Gradient);
        }
    }
}
