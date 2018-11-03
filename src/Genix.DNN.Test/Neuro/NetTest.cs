namespace Genix.DNN.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Genix.DNN;
    using Genix.DNN.Learning;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Learning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class NetTest
    {
        private readonly string[] classes = { "0", "1", "2" };
        private ClassificationNetwork net;

        [TestInitialize]
        public void BeforeEach()
        {
            this.net = ClassificationNetwork.FromArchitecture("10x10x2~5N~5N~3N", this.classes);
        }

        [TestMethod]
        [Ignore]
        public void SaveToStringTest()
        {
            string s = this.net.SaveToString(NetworkFileFormat.JSON);
            ClassificationNetwork net2 = ClassificationNetwork.FromString(s);

            string s1 = JsonConvert.SerializeObject(this.net);
            string s2 = JsonConvert.SerializeObject(net2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Ignore]
        public void SaveToMemoryTest()
        {
            ClassificationNetwork net2 = ClassificationNetwork.FromMemory(this.net.SaveToMemory(NetworkFileFormat.JSON));
            string s1 = JsonConvert.SerializeObject(this.net);
            string s2 = JsonConvert.SerializeObject(net2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Ignore]
        public void SaveToFileTest()
        {
            string tempFileName = Path.GetTempFileName();
            try
            {
                this.net.SaveToFile(tempFileName, NetworkFileFormat.JSON);
                ClassificationNetwork net2 = ClassificationNetwork.FromFile(tempFileName);
                Assert.AreEqual(JsonConvert.SerializeObject(this.net), JsonConvert.SerializeObject(net2));
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        [Description("should be possible to initialize")]
        public void CheckNumberOfLayers()
        {
            Assert.AreEqual(7, this.net.Graph.Vertices.Count());
        }

        [TestMethod]
        [Description("should forward prop volumes to probabilities")]
        public void ForwardToProbability()
        {
            ClassificationNetwork network = ClassificationNetwork.FromArchitecture("1x1x2~5N~5N~3N", this.classes);

            Tensor x = new Tensor(null, new[] { 1, 1, 1, 2 });
            x.Set(new float[] { 0.2f, -0.3f });

            Tensor probability = network.Forward(null, x);

            Assert.AreEqual(3, probability.Length); // 3 classes output

            for (int i = 0; i < probability.Length; i++)
            {
                Assert.IsTrue(probability.Weights[i] > 0.0);
                Assert.IsTrue(probability.Weights[i] < 1.0);
            }

            Assert.AreEqual(1.0, probability.Weights[0] + probability.Weights[1] + probability.Weights[2], 1e-6);
        }

        [TestMethod]
        [Description("should forward prop volumes to probabilities")]
        public void ForwardVolumes()
        {
            Random random = new Random(0);

            ClassificationNetworkTrainer trainer = new ClassificationNetworkTrainer();

            SGD sgd = new SGD()
            {
                LearningRate = 0.0001f,
                Momentum = 0.0f
            };

            ClassificationNetwork network = ClassificationNetwork.FromArchitecture("1x1x2~5N~5N~3N", this.classes);

            // lets test 100 random point and label settings
            // note that this should work since l2 and l1 regularization are off
            // an issue is that if step size is too high, this could technically fail...
            for (int k = 0; k < 100; k++)
            {
                int gti = (int)Math.Floor(random.NextDouble() * 3);

                Tensor x = new Tensor(null, new[] { 1, 1, 1, 2 });
                x.Set(new float[] { ((float)random.NextDouble() * 2) - 1, ((float)random.NextDouble() * 2) - 1 });

                Tensor pv = network.Forward(null, x).Clone() as Tensor;

                trainer.RunEpoch(
                    k,
                    network,
                    Enumerable.Repeat((x, new string[] { this.classes[gti] }), 1),
                    sgd,
                    new LogLikelihoodLoss(),
                    CancellationToken.None);

                Tensor pv2 = network.Forward(null, x).Clone() as Tensor;
                Assert.IsTrue(pv2.Weights[gti] > pv.Weights[gti], "k: {0}, gti: {1}, pv2[gti]: {2}, pv[gti]: {3}", k, gti, pv2.Weights[gti], pv.Weights[gti]);
            }
        }

#if false
        [TestMethod]
        [Description("should compute correct gradient at data")]
        public void ComputeGradient()
        {
            Random random = new Random();
            StochasticGradientDescent<char> trainer = new StochasticGradientDescent<char>(this.net)
            {
                LearningRate = 0.0001,
                Momentum = 0.0,
                BatchSize = 1,
                L2Decay = 0.0
            };

            // here we only test the gradient at data, but if this is
            // right then that's comforting, because it is a function 
            // of all gradients above, for all layers.

            Volume volume = new Volume(new float[] { random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1 });
            int gti = (int)Math.Floor(random.NextDouble() * 3); // ground truth index

            trainer.Learn(Enumerable.Repeat(Tuple.Create(volume, this.classes[gti]), 1)); // computes gradients at all layers, and at x

            Volume gradient = this.net.Layers[0].InputGradient;

            float delta = 0.000001;
            for (int i = 0; i < volume.Length; i++)
            {
                float gradAnalytic = gradient[i];

                float xold = volume[i];
                volume[i] += delta;
                float c0 = this.net.CostLoss(this.net.Compute(volume, false), this.classes[gti]);
                volume[i] -= 2 * delta;
                float c1 = this.net.CostLoss(this.net.Compute(volume, false), this.classes[gti]);
                volume[i] = xold; // reset

                float gradNumeric = (c0 - c1) / (2 * delta);
                float relError = Math.Abs(gradAnalytic - gradNumeric) / Math.Abs(gradAnalytic + gradNumeric);

                Console.WriteLine("step: {0}, numeric: {1}, analytic: {2}, => relError: {3}", i, gradNumeric, gradAnalytic, relError);
                Assert.IsTrue(relError < 1e-2);
            }
        }
#endif
    }
}
