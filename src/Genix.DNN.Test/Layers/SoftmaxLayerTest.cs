namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SoftMaxLayerTest
    {
        private static float[] weights = new float[] { 0.2f, -0.3f, 0.8f, 0.4f };
        private static float[] activations = new float[]
        {
            0.215051353f,
            0.130435243f,
            0.39184913f,
            0.262664318f
        };

        [TestMethod]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 1, SoftMaxLayerTest.weights.Length };
            SoftMaxLayer layer = new SoftMaxLayer(shape);
            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("SM", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new SoftMaxLayer((int[])null));
        }

        [TestMethod]
        public void ArchitechtureConstructorTest1()
        {
            int[] shape = new[] { 1, SoftMaxLayerTest.weights.Length };
            SoftMaxLayer layer = new SoftMaxLayer(shape, "SM", null);

            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual("SM", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitechtureConstructorTest2()
        {
            string architecture = "SMX";
            try
            {
                SoftMaxLayer layer = new SoftMaxLayer(new[] { 1, SoftMaxLayerTest.weights.Length }, architecture, null);
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
        public void ArchitechtureConstructorTest3()
        {
            Assert.IsNotNull(new SoftMaxLayer(null, "SM", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitechtureConstructorTest4()
        {
            Assert.IsNotNull(new SoftMaxLayer(new[] { 1, SoftMaxLayerTest.weights.Length }, null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { 1, SoftMaxLayerTest.weights.Length };
            SoftMaxLayer layer1 = new SoftMaxLayer(shape);
            SoftMaxLayer layer2 = new SoftMaxLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new SoftMaxLayer((SoftMaxLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            SoftMaxLayer layer1 = new SoftMaxLayer(new[] { 1, SoftMaxLayerTest.weights.Length });
            SoftMaxLayer layer2 = layer1.Clone() as SoftMaxLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            SoftMaxLayer layer1 = new SoftMaxLayer(new[] { 1, SoftMaxLayerTest.weights.Length });
            string s1 = JsonConvert.SerializeObject(layer1);
            SoftMaxLayer layer2 = JsonConvert.DeserializeObject<SoftMaxLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest()
        {
            int[] shape = new[] { 1, SoftMaxLayerTest.weights.Length };
            SoftMaxLayer layer = new SoftMaxLayer(shape);

            Session session = new Session();

            Tensor x = new Tensor(null, shape, SoftMaxLayerTest.weights);
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
