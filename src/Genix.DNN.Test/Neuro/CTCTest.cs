namespace Genix.DNN.Test
{
    using System;
    using Genix.MachineLearning;
    using Learning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CTCTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            const int L = 5;    // Alphabet size
            const int T = 2;    // Length of utterance (time)

            Session session = new Session();

            Tensor activations = new Tensor(null, new[] { T, L }, new float[] { 0.1f, 0.6f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.6f, 0.1f, 0.1f });

            Tensor y = NeuralOperations.SoftMax(session, activations);

            CTCLoss loss = new CTCLoss();

            int[] labels = { 1, 2 };
            float expected = -(float)Math.Log(y.Weights[(0 * L) + labels[0]] *
                                              y.Weights[(1 * L) + labels[1]]);

            float score = loss.Loss(y, labels, false);
            Assert.AreEqual(expected, score, 1e-6);

            score = loss.Loss(y, labels, true);
            Assert.AreEqual(expected, score, 1e-6);
        }

        [TestMethod]
        public void TestMethod2()
        {
            const int L = 6;
            const int T = 5;

            Tensor y = new Tensor(null, new[] { T, L });
            y.Set(new float[]
            {
                0.633766f, 0.221185f, 0.0917319f, 0.0129757f, 0.0142857f, 0.0260553f,
                0.111121f, 0.588392f, 0.278779f, 0.0055756f, 0.00569609f, 0.010436f,
                0.0357786f, 0.633813f, 0.321418f, 0.00249248f, 0.00272882f, 0.0037688f,
                0.0663296f, 0.643849f, 0.280111f, 0.00283995f, 0.0035545f, 0.00331533f,
                0.458235f, 0.396634f, 0.123377f, 0.00648837f, 0.00903441f, 0.00623107f
            });

            CTCLoss loss = new CTCLoss() { BlankLabelIndex = 5 };

            int[] labels = { 0, 1, 2, 1, 0/*, 0, 1, 1, 0*/ };
            float expected = -(float)Math.Log(y.Weights[(0 * L) + labels[0]] *
                                              y.Weights[(1 * L) + labels[1]] *
                                              y.Weights[(2 * L) + labels[2]] *
                                              y.Weights[(3 * L) + labels[3]] *
                                              y.Weights[(4 * L) + labels[4]]);

            float score = loss.Loss(y, labels, true);
            Helpers.AreArraysEqual(
                new float[]
                {
                    1, 0, 0, 0, 0, 0,
                    0, 1, 0, 0, 0, 0,
                    0, 0, 1, 0, 0, 0,
                    0, 1, 0, 0, 0, 0,
                    1, 0, 0, 0, 0, 0
                },
                y.Gradient);
        }

        [TestMethod]
        public void TestMethod3()
        {
            const int L = 6;
            const int T = 5;

            Tensor y = new Tensor(null, new[] { T, L });
            y.Set(new float[]
            {
                0.30176f, 0.28562f, 0.0831517f, 0.0862751f, 0.0816851f, 0.161508f,
                0.24082f, 0.397533f, 0.0557226f, 0.0546814f, 0.0557528f, 0.19549f,
                0.230246f, 0.450868f, 0.0389607f, 0.038309f, 0.0391602f, 0.202456f,
                0.280884f, 0.429522f, 0.0326593f, 0.0339046f, 0.0326856f, 0.190345f,
                0.423286f, 0.315517f, 0.0338439f, 0.0393744f, 0.0339315f, 0.154046f
            });

            CTCLoss loss = new CTCLoss() { BlankLabelIndex = 5 };

            int[] labels = { 0, 1, 1, 0 };
            float expected = 5.42262f; // from tensorflow

            float score = loss.Loss(y, labels, true);
            Assert.AreEqual(expected, score, 1e-4);
            Helpers.AreArraysEqual(
                new float[]
                {
                    1, 0, 0, 0, 0, 0,
                    0, 1, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 1,
                    0, 1, 0, 0, 0, 0,
                    1, 0, 0, 0, 0, 0
                },
                y.Gradient);
        }

        [TestMethod]
        public void TestMethod4()
        {
            const int L = 5;    // Alphabet size
            const int T = 3;    // Length of utterance (time)

            Session session = new Session();

            Tensor activations = new Tensor(null, new[] { T, L }, 1.0f);

            Tensor y = NeuralOperations.SoftMax(session, activations);

            CTCLoss loss = new CTCLoss();

            int[] labels = { 1, 2, 3 };
            float expected = -(float)Math.Log(y.Weights[(0 * L) + labels[0]] *
                                              y.Weights[(1 * L) + labels[1]] *
                                              y.Weights[(2 * L) + labels[2]]);

            float score = loss.Loss(y, labels, false);
            Assert.AreEqual(expected, score, 1e-6);

            score = loss.Loss(y, labels, true);
            Assert.AreEqual(expected, score, 1e-6);

            labels = new[] { 1, 2, 2 };
            expected = float.PositiveInfinity;

            score = loss.Loss(y, labels, false);
            Assert.AreEqual(expected, score, 1e-6);

            score = loss.Loss(y, labels, true);
            Assert.AreEqual(expected, score, 1e-6);
        }

        [TestMethod, Description("Zero weight in y tensor.")]
        public void TestMethod5()
        {
            const int L = 5;    // Alphabet size
            const int T = 3;    // Length of utterance (time)

            Session session = new Session();

            Tensor activations = new Tensor(null, new[] { T, L }, 1.0f);

            Tensor y = NeuralOperations.SoftMax(session, activations);
            y.Weights[0] = 0;

            CTCLoss loss = new CTCLoss();

            int[] labels = { 1, 2, 3 };
            float expected = -(float)Math.Log(y.Weights[(0 * L) + labels[0]] *
                                              y.Weights[(1 * L) + labels[1]] *
                                              y.Weights[(2 * L) + labels[2]]);

            float score = loss.Loss(y, labels, false);
            Assert.AreEqual(expected, score, 1e-6);

            score = loss.Loss(y, labels, true);
            Assert.AreEqual(expected, score, 1e-6);
        }
    }
}
