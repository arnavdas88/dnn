namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Learning;

    [TestClass]
    public class LSTMLayerTest
    {
        [TestMethod, TestCategory("LSTM")]
        public void CreateLayerTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            string architecture = "20-30-40LSTM";
            LSTMLayer layer = (LSTMLayer)NetworkGraphBuilder.CreateLayer(shape, architecture, null);

            Assert.AreEqual(20, ((StochasticLayer)layer.Graph.Vertices.ElementAt(0)).NumberOfNeurons);
            Assert.AreEqual(30, ((StochasticLayer)layer.Graph.Vertices.ElementAt(1)).NumberOfNeurons);
            Assert.AreEqual(40, ((StochasticLayer)layer.Graph.Vertices.ElementAt(2)).NumberOfNeurons);
            Assert.AreEqual(architecture, layer.Architecture);
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateLayerTest2()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            string architecture = "100LSTM";
            Assert.IsNotNull(NetworkGraphBuilder.CreateLayer(shape, architecture, null));
        }

        [TestMethod, TestCategory("LSTM")]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 1, 10, 12, 3 };
            LSTMLayer layer = new LSTMLayer(shape, new[] { 20, 30 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);

            Assert.AreEqual(20, ((StochasticLayer)layer.Graph.Vertices.ElementAt(0)).NumberOfNeurons);
            Assert.AreEqual(30, ((StochasticLayer)layer.Graph.Vertices.ElementAt(1)).NumberOfNeurons);
            Assert.AreEqual("20-30LSTM", layer.Architecture);
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new LSTMLayer((int[])null, new[] { 20, 20 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            Assert.IsNotNull(new LSTMLayer(shape, null, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest5()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            Assert.IsNotNull(new LSTMLayer(shape, new[] { 20 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("LSTM")]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            LSTMLayer layer1 = new LSTMLayer(shape, new[] { 20, 20 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            LSTMLayer layer2 = new LSTMLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new LSTMLayer((LSTMLayer)null));
        }

        [TestMethod, TestCategory("LSTM")]
        public void EnumGradientsTest()
        {
            int[] shape = new[] { 1, 20, 20, 10 };
            LSTMLayer layer = new LSTMLayer(shape, new[] { 20, 30 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            Assert.AreEqual(5, layer.EnumGradients().Count());
        }

        [TestMethod, TestCategory("LSTM")]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            LSTMLayer layer1 = new LSTMLayer(shape, new[] { 20, 20 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            LSTMLayer layer2 = layer1.Clone() as LSTMLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("LSTM")]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            LSTMLayer layer1 = new LSTMLayer(shape, new[] { 20, 20 }, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            LSTMLayer layer2 = JsonConvert.DeserializeObject<LSTMLayer>(s1);
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
        [TestMethod, TestCategory("LSTM")]
        [Ignore]
        public void XorTest1()
        {
            const int AlphabetSize = 16;
            const int VectorSize = 4;

            const int BatchSize = 3000;
            const int Epochs = 200;
            const int TestBatchSize = 3000;
            Random random = new Random(0);

            string[] classes = Enumerable.Range(0, AlphabetSize).Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
            ClassificationNetwork network = ClassificationNetwork.FromArchitecture("1x1x4~80-80-80-16LSTM", classes);

            float[] vectors = new RandomGenerator().Generate(AlphabetSize * VectorSize);

            (Tensor, int[]) createSample(int size)
            {
                Tensor input = new Tensor(null, new[] { size, 1, 1, VectorSize });
                int[] truth = new int[size];

                int v = 0;
                for (int i = 0; i < size; i++)
                {
                    v ^= random.Next(0, AlphabetSize);
                    SetCopy.Copy(VectorSize, vectors, v * VectorSize, input.Weights, i * VectorSize);

                    if (i > 0)
                    {
                        truth[i - 1] = v;
                    }
                }

                return (input, truth);
            };

            // train the network
            NetworkTrainer<int[]> trainer = new NetworkTrainer<int[]>()
            {
                ClipValue = 2.0f
            };

            SGD sgd = new SGD();
            ILoss<int[]> loss = new LogLikelihoodLoss();

            for (int epoch = 0; epoch < Epochs; epoch++)
            {
                (Tensor, int[]) sample = createSample(BatchSize);

                TrainingResult result = trainer.RunEpoch(
                    network,
                    Enumerable.Repeat(sample, 1),
                    epoch,
                    sgd,
                    loss,
                    CancellationToken.None);
                Console.WriteLine(result.CostLoss);
            }

            // test the network
            (Tensor x, int[] expected) = createSample(TestBatchSize);
            Tensor y = network.Forward(null, x);
            ////y.Reshape(testBatchSize - 1);
            ////expected.Reshape(testBatchSize - 1);
            float error = loss.Loss(y, expected, false);

            Console.WriteLine(y);
            Console.WriteLine(expected);
            ////Console.WriteLine(y.Axes[1]);
            Console.WriteLine(error);

            ////Assert.IsTrue(errorL1 < 0.01, errorL1.ToString(CultureInfo.InvariantCulture));
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
        [TestMethod, TestCategory("LSTM")]
        [Ignore]
        public void SinTest1()
        {
            const int batchSize = 10;
            const double batchStep = 0.1;
            const int epochs = 10000;
            const int testBatchSize = 5;
            Random random = new Random(0);
            Network network = Network.FromArchitecture("1x1x1~10-10-1LSTM");

            (Tensor, Tensor) createSample(int size)
            {
                Tensor input = new Tensor(null, new[] { size, 1, 1, 1 });
                Tensor expected = new Tensor(null, input.Axes);

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
            };

            // train the network
            SquareLoss loss = new SquareLoss();
            NetworkTrainer<Tensor> trainer = new NetworkTrainer<Tensor>();
            SGD sgd = new SGD();

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                (Tensor, Tensor) sample = createSample(batchSize);
                TrainingResult result = trainer.RunEpoch(network, Enumerable.Repeat(sample, 1), epoch, sgd, loss, CancellationToken.None);
                Console.WriteLine(result.CostLoss);
            }

            // test the network
            double errorL1 = 0.0;
            double errorL2 = 0.0;
            for (int test = 0; test < 100; test++)
            {
                (Tensor x, Tensor ye) = createSample(testBatchSize);
                Tensor output = network.Forward(null, x);

                float diff = Math.Abs(ye.Weights[testBatchSize - 1] - output.Weights[testBatchSize - 1]);
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

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "This is just a test.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is just a test.")]
        [TestMethod, TestCategory("LSTM")]
        [Ignore]
        public void TinyShakespeareTest1()
        {
            const int letterSize = 5;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = Path.Combine(Path.GetDirectoryName(assembly.Location), @"..\TestData\tinyshakespeare.txt");

            string alltext = File.ReadAllText(path).ToUpperInvariant();

            // calculate alphabet size
            HashSet<char> chars = new HashSet<char>();
            for (int i = 0, ii = alltext.Length; i < ii; i++)
            {
                chars.Add(alltext[i]);
            }

            Dictionary<char, int> alphabet = chars.OrderBy(x => x)
                                                  .Select((x, i) => Tuple.Create(x, i))
                                                  .ToDictionary(x => x.Item1, x => x.Item2);

            // create character vectors
            Tensor letters = new Tensor(null, new[] { alphabet.Count + 1, 1, 1, letterSize });
            letters.Randomize();

            // create network
            string architechture = string.Format(
                CultureInfo.InvariantCulture,
                "1x1x{0}~20-{1}SRN~TH~SM",
                alphabet.Count,
                alphabet.Count + 1);

            Network network = Network.FromArchitecture(architechture);
            ////string a = network.Architecture;

            // learn network
            /*ILoss<int[]> loss = new CTCLoss()
            {
                BlankLabelIndex = alphabet.Count
            };*/
            ILoss<int[]> loss = new LogLikelihoodLoss();
            ITrainingAlgorithm algorithm = new SGD()
            {
                LearningRate = 0.01f
            };
            NetworkTrainer<int[]> trainer = new NetworkTrainer<int[]>()
            {
                BatchSize = 1,
                ClipValue = 10.0f
            };

            ////string[] words = alltext.Split(new char[] { ' ', '\n', '.', ',', ';', ':', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
            string[] words = Enumerable.Repeat("FIRST", 1000).ToArray();
            trainer.RunEpoch(
                network,
                words.Select(w =>
                {
                    /*Tensor x = new Tensor(new Layout(network.InputLayout, w.Length));
                    for (int i = 0, ii = w.Length; i < ii; i++)
                    {
                        Tensor.Copy(letterSize, letters, alphabet[w[i]] * letterSize, x, i * letterSize);
                    }*/
                    Tensor x = new Session().Concat(
                        w.Select(ch => Tensor.OneHot(null, network.InputShape, 0, 0, 0, alphabet[ch])).ToArray(),
                        (int)Axis.B);

                    int[] y = new int[w.Length];
                    for (int i = 0, ii = y.Length; i < ii; i++)
                    {
                        y[i] = alphabet[i == ii - 1 ? ' ' : w[i + 1]];
                    }

                    return (x, y);
                }),
                0,
                algorithm,
                loss,
                CancellationToken.None);

            // test network
            foreach (string w in words.Where(x => x.Length > 1))
            {
                string w1 = w.Substring(0, w.Length - 1);

                Tensor x = new Tensor(null, Shape.Reshape(network.InputShape, (int)Axis.B, w1.Length));
                for (int i = 0, ii = w1.Length; i < ii; i++)
                {
                    SetCopy.Copy(letterSize, letters.Weights, alphabet[w1[i]] * letterSize, x.Weights, i * letterSize);
                }
                /*                Tensor x = Tensor.OneHot(
                                    new Layout(network.InputLayout, w1.Length),
                                    w1.Select(ch => alphabet[ch]).ToList());*/

                Tensor y = network.Forward(null, x);
                int start = y.Strides[0] * (y.Axes[(int)Axis.B] - 1);
                int argmax = Maximum.ArgMax(y.Strides[0], y.Weights, start);
                int classIndex = argmax - start;
                char res = alphabet.ElementAt(classIndex).Key;

                Console.WriteLine("{0} -> {1} + {2} {3} {4}", w, w1, classIndex, res, y.Weights[argmax]);
            }

            /*foreach (string w in new string[] { "AAAA", "AA" })
            {
                Tensor x = Tensor.OneHot(
                    new Layout(network.InputLayout, w.Length),
                    w.Select(ch => alphabet[ch]).ToList());

                Tensor y = network.Forward(x, true);
                network.Backward(x, y, y);
            }*/
        }
    }
}
