namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ReLULayerTest
    {
        private static Func<float, float> activation = (x) => Math.Max(x, 0);
        private static Func<float, float> derivative = (x) => (x == 0.0f ? 0.0f : 1.0f);

        [TestMethod, TestCategory("ReLU")]
        public void ConstructorTest1()
        {
            Shape shape = new Shape(2);
            ReLULayer layer = new ReLULayer(shape);
            CollectionAssert.AreEqual(shape.Axes, layer.OutputShape.Axes);
            Assert.AreEqual("RELU", layer.Architecture);
        }

        [TestMethod, TestCategory("ReLU")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new ReLULayer((Shape)null));
        }

        [TestMethod, TestCategory("ReLU")]
        public void ArchitectureConstructorTest1()
        {
            Shape shape = new Shape(2);
            ReLULayer layer = new ReLULayer(shape, "RELU", null);

            CollectionAssert.AreEqual(shape.Axes, layer.OutputShape.Axes);
            Assert.AreEqual("RELU", layer.Architecture);
        }

        [TestMethod, TestCategory("ReLU")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "REL";
            try
            {
                ReLULayer layer = new ReLULayer(new Shape(2), architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("ReLU")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new ReLULayer(null, "RELU", null));
        }

        [TestMethod, TestCategory("ReLU")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new ReLULayer(new Shape(2), null, null));
        }

        [TestMethod, TestCategory("ReLU")]
        public void CopyConstructorTest1()
        {
            ReLULayer layer1 = new ReLULayer(new Shape(2));
            ReLULayer layer2 = new ReLULayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("ReLU")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new ReLULayer((ReLULayer)null));
        }

        [TestMethod, TestCategory("ReLU")]
        public void CloneTest()
        {
            ReLULayer layer1 = new ReLULayer(new Shape(2));
            ReLULayer layer2 = layer1.Clone() as ReLULayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("ReLU")]
        public void SerializeTest()
        {
            ReLULayer layer1 = new ReLULayer(new Shape(2));
            string s1 = JsonConvert.SerializeObject(layer1);
            ReLULayer layer2 = JsonConvert.DeserializeObject<ReLULayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod, TestCategory("ReLU")]
        public void ForwardBackwardTest()
        {
            Shape shape = new Shape(2);
            ReLULayer layer = new ReLULayer(shape);

            Session session = new Session();

            Tensor source = new Tensor(null, TensorShape.Unknown, shape, new float[] { 2, -3 });
            Tensor x = source.Clone() as Tensor;
            Tensor y = layer.Forward(session, new[] { x })[0];

            float[] expected = source.Weights.Take(source.Length).Select(w => ReLULayerTest.activation(w)).ToArray();
            Helpers.AreArraysEqual(x.Length, expected, y.Weights);

            // unroll the graph
            float[] dy = Enumerable.Range(1, x.Length).Select(w => (float)w).ToArray();
            y.SetGradient(dy);
            session.Unroll();

            Helpers.AreArraysEqual(
                expected.Length,
                expected.Zip(dy, (w, dw) => ReLULayerTest.derivative(w) * dw).ToArray(),
                x.Gradient);
        }
    }
}
