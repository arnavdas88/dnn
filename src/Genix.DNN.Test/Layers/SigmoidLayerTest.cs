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
    public class SigmoidLayerTest
    {
        private static Func<float, float> activation = (x) => 1.0f / (1.0f + (float)Math.Exp(-x));
        private static Func<float, float> derivative = (x) => (x * (1.0f - x));

        [TestMethod, TestCategory("Sigmoid")]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 2 };
            SigmoidLayer layer = new SigmoidLayer(shape);
            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("SIG", layer.Architecture);
        }

        [TestMethod, TestCategory("Sigmoid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new SigmoidLayer((int[])null));
        }

        [TestMethod, TestCategory("Sigmoid")]
        public void ArchitectureConstructorTest1()
        {
            int[] shape = new[] { 2 };
            SigmoidLayer layer = new SigmoidLayer(shape, "SIG", null);

            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("SIG", layer.Architecture);
        }

        [TestMethod, TestCategory("Sigmoid")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "SI";
            try
            {
                SigmoidLayer layer = new SigmoidLayer(new[] { 2 }, architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("Sigmoid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new SigmoidLayer(null, "SIG", null));
        }

        [TestMethod, TestCategory("Sigmoid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new SigmoidLayer(new[] { 2 }, null, null));
        }

        [TestMethod, TestCategory("Sigmoid")]
        public void CopyConstructorTest1()
        {
            SigmoidLayer layer1 = new SigmoidLayer(new[] { 2 });
            SigmoidLayer layer2 = new SigmoidLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("Sigmoid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new SigmoidLayer((SigmoidLayer)null));
        }

        [TestMethod, TestCategory("Sigmoid")]
        public void CloneTest()
        {
            SigmoidLayer layer1 = new SigmoidLayer(new[] { 2 });
            SigmoidLayer layer2 = layer1.Clone() as SigmoidLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("Sigmoid")]
        public void SerializeTest()
        {
            SigmoidLayer layer1 = new SigmoidLayer(new[] { 2 });
            string s1 = JsonConvert.SerializeObject(layer1);
            SigmoidLayer layer2 = JsonConvert.DeserializeObject<SigmoidLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod, TestCategory("Sigmoid")]
        public void ForwardBackwardTest()
        {
            int[] shape = new[] { 2 };
            SigmoidLayer layer = new SigmoidLayer(shape);

            Session session = new Session();

            Tensor source = new Tensor(null, shape, new float[] { 2, -3 });
            Tensor x = source.Clone() as Tensor;
            Tensor y = layer.Forward(session, new[] { x })[0];

            float[] expected = source.Weights.Take(source.Length).Select(w => SigmoidLayerTest.activation(w)).ToArray();
            Helpers.AreArraysEqual(x.Length, expected, y.Weights);

            // unroll the graph
            float[] dy = Enumerable.Range(1, x.Length).Select(w => (float)w).ToArray();
            y.SetGradient(dy);
            session.Unroll();

            Helpers.AreArraysEqual(
                expected.Length,
                expected.Zip(dy, (w, dw) => SigmoidLayerTest.derivative(w) * dw).ToArray(),
                x.Gradient);
        }
    }
}
