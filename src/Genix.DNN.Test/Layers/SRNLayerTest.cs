namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Genix.Core;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.DNN.Learning;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Learning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SRNLayerTest
    {
        [TestMethod, TestCategory("SRN")]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 1, 10, 12, 3 };
            SRNLayer layer = new SRNLayer(shape, new[] { 20, 30 }, MatrixLayout.ColumnMajor, null);

            Assert.AreEqual(20, ((StochasticLayer)layer.Graph.Vertices.ElementAt(0)).NumberOfNeurons);
            Assert.AreEqual(30, ((StochasticLayer)layer.Graph.Vertices.ElementAt(1)).NumberOfNeurons);
            Assert.AreEqual("20-30SRN", layer.Architecture);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            CollectionAssert.AreEqual(new[] { 1, 30 }, layer.OutputShape);
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new SRNLayer(null, new[] { 20, 20 }, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            Assert.IsNotNull(new SRNLayer(shape, null, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest4()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            Assert.IsNotNull(new SRNLayer(shape, new[] { 20 }, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("SRN")]
        public void ArchitechtureConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            string architecture = "20-30-40SRN";
            SRNLayer layer = new SRNLayer(shape, "20-30-40SRN", null);

            Assert.AreEqual(20, ((StochasticLayer)layer.Graph.Vertices.ElementAt(0)).NumberOfNeurons);
            Assert.AreEqual(30, ((StochasticLayer)layer.Graph.Vertices.ElementAt(1)).NumberOfNeurons);
            Assert.AreEqual(40, ((StochasticLayer)layer.Graph.Vertices.ElementAt(2)).NumberOfNeurons);
            Assert.AreEqual(architecture, layer.Architecture);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            CollectionAssert.AreEqual(new[] { -1, 40 }, layer.OutputShape);
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitechtureConstructorTest2()
        {
            string architecture = "100SRN";
            try
            {
                SRNLayer layer = new SRNLayer(new[] { 1, 20, 20, 10 }, architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitechtureConstructorTest3()
        {
            Assert.IsNotNull(new SRNLayer(null, "20-30SRN", null));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitechtureConstructorTest4()
        {
            Assert.IsNotNull(new SRNLayer(new[] { 1, 20, 20, 10 }, null, null));
        }

        [TestMethod, TestCategory("SRN")]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            SRNLayer layer1 = new SRNLayer(shape, new[] { 20, 20 }, MatrixLayout.ColumnMajor, null);
            SRNLayer layer2 = new SRNLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new SRNLayer(null));
        }

        [TestMethod, TestCategory("SRN")]
        public void EnumGradientsTest()
        {
            int[] shape = new[] { 1, 20, 20, 10 };
            SRNLayer layer = new SRNLayer(shape, new[] { 20, 30 }, MatrixLayout.ColumnMajor, null);
            Assert.AreEqual(5, layer.EnumGradients().Count());
        }

        [TestMethod, TestCategory("SRN")]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 2, 2, 1 };
            SRNLayer layer1 = new SRNLayer(shape, new[] { 2, 3 }, MatrixLayout.ColumnMajor, null);
            SRNLayer layer2 = layer1.Clone() as SRNLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("SRN")]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 2, 2, 1 };
            SRNLayer layer1 = new SRNLayer(shape, new[] { 2, 3 }, MatrixLayout.ColumnMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            SRNLayer layer2 = JsonConvert.DeserializeObject<SRNLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        /// <summary>
        /// Train and test the recurrent network.
        /// </summary>
        /// <remarks>
        /// The test tries to predict values of sin(x) sequence.
        /// The network is trained on sequences of sin(x + a * t), where x is random, t belongs to [0, n] and a is a constant.
        /// Then we test the network on another similar sequences.
        /// The output for the last element in testing sequence is expected to be sin(x + a * (t + 1)).
        /// The network has two hidden layers of 10 neurons each, and final decoder layer of 1 neuron.
        /// </remarks>
        [TestMethod, TestCategory("SRN")]
        public void SinTest1()
        {
            const int batchSize = 10;
            const double batchStep = 0.1;
            const int epochs = 10000;
            const int testBatchSize = 5;
            Random random = new Random(0);
            Network network = Network.FromArchitecture("1x1x1~10-10-1SRN");

            (Tensor, Tensor) createSample(int size)
            {
                Tensor input = new Tensor(null, new[] { size, 1, 1, 1 });
                Tensor expected = new Tensor(null, new[] { size, 1 });

                double rv = (float)(random.NextDouble() * Math.PI * 2);
                for (int b = 0; b <= size; b++, rv += batchStep)
                {
                    float value = (float)((Math.Sin(rv) / 2.0) + 0.5);
                    if (b < size)
                    {
                        input.Weights[b] = value;
                    }

                    if (b - 1 >= 0)
                    {
                        expected.Weights[b - 1] = value;
                    }
                }

                return (input, expected);
            }

            // train the network
            SquareLoss loss = new SquareLoss();
            Trainer<Tensor> trainer = new Trainer<Tensor>() { /*ClipValue = 2.0f*/ };
            SGD sgd = new SGD();

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                (Tensor, Tensor) sample = createSample(batchSize);
                trainer.RunEpoch(network, Enumerable.Repeat(sample, 1), epoch, sgd, loss, CancellationToken.None);
            }

            // test the network
            double errorL1 = 0.0;
            double errorL2 = 0.0;
            for (int test = 0; test < 100; test++)
            {
                (Tensor x, Tensor expected) = createSample(testBatchSize);
                Tensor output = network.Forward(null, x);

                float diff = Math.Abs(expected.Weights[testBatchSize - 1] - output.Weights[testBatchSize - 1]);
                Console.WriteLine(diff);
                errorL1 += Math.Abs(diff);
                errorL2 += Math.Pow(diff, 2.0);
            }

            errorL1 /= 100;
            errorL2 = Math.Sqrt(errorL2) / 100;
            Console.WriteLine(errorL1);
            Console.WriteLine(errorL2);

            Assert.IsTrue(errorL1 < 0.01, errorL1.ToString(CultureInfo.InvariantCulture));
            Assert.IsTrue(errorL2 < 0.001, errorL2.ToString(CultureInfo.InvariantCulture));
        }
    }
}
