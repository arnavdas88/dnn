namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Genix.Core;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class LSTMCellTest
    {
        [TestMethod, TestCategory("LSTM")]
        public void ConstructorTest1()
        {
            Shape shape = new Shape(1, 10, 12, 3);
            int numberOfNeurons = 100;
            float forgetBias = 2.0f;

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                LSTMCell layer = new LSTMCell(shape, RNNDirection.ForwardOnly, numberOfNeurons, forgetBias, matrixLayout, null);

                Assert.AreEqual(numberOfNeurons, layer.NumberOfNeurons);
                Assert.AreEqual(forgetBias, layer.ForgetBias);
                Assert.AreEqual("100LSTMC(ForgetBias=2)", layer.Architecture);

                Assert.AreEqual(1, layer.NumberOfOutputs);
                CollectionAssert.AreEqual(new[] { 1, numberOfNeurons }, layer.OutputShape.Axes);

                CollectionAssert.AreEqual(
                    matrixLayout == MatrixLayout.RowMajor ?
                        new[] { 4 * numberOfNeurons, 10 * 12 * 3 } :
                        new[] { 10 * 12 * 3, 4 * numberOfNeurons },
                    layer.W.Axes);
                Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
                Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

                CollectionAssert.AreEqual(
                    matrixLayout == MatrixLayout.RowMajor ?
                        new[] { 4 * numberOfNeurons, numberOfNeurons } :
                        new[] { numberOfNeurons, 4 * numberOfNeurons },
                    layer.U.Axes);
                Assert.IsFalse(layer.U.Weights.Take(layer.U.Length).All(x => x == 0.0f));
                Assert.AreEqual(0.0, layer.U.Weights.Take(layer.U.Length).Average(), 0.01f);

                CollectionAssert.AreEqual(new[] { 4 * numberOfNeurons }, layer.B.Axes);
                Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
            }
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new LSTMCell(TensorShape.Unknown, null, RNNDirection.ForwardOnly, 100, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("LSTM")]
        public void ArchitectureConstructorTest1()
        {
            const string Architecture = "100LSTMC(ForgetBias=3.6)";
            LSTMCell layer = new LSTMCell(new Shape(-1, 10, 12, 3), Architecture, null);

            Assert.AreEqual(RNNDirection.ForwardOnly, layer.Direction);
            Assert.AreEqual(100, layer.NumberOfNeurons);
            Assert.AreEqual(3.6f, layer.ForgetBias);
            Assert.AreEqual(Architecture, layer.Architecture);

            CollectionAssert.AreEqual(new[] { -1, 100 }, layer.OutputShape.Axes);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual(MatrixLayout.RowMajor, layer.MatrixLayout);

            CollectionAssert.AreEqual(new[] { 4 * 100, 10 * 12 * 3 }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 4 * 100, 100 }, layer.U.Axes);
            Assert.IsFalse(layer.U.Weights.Take(layer.U.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.U.Weights.Take(layer.U.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 4 * 100 }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
        }

        [TestMethod, TestCategory("LSTM")]
        public void ArchitectureConstructorTest2()
        {
            const string Architecture = "100LSTMC(Bi=1,ForgetBias=3.6)";
            LSTMCell layer = new LSTMCell(new Shape(-1, 10, 12, 3), Architecture, null);

            Assert.AreEqual(RNNDirection.BiDirectional, layer.Direction);
            Assert.AreEqual(100, layer.NumberOfNeurons);
            Assert.AreEqual(3.6f, layer.ForgetBias);
            Assert.AreEqual(Architecture, layer.Architecture);

            CollectionAssert.AreEqual(new[] { -1, 100 }, layer.OutputShape.Axes);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual(MatrixLayout.RowMajor, layer.MatrixLayout);

            CollectionAssert.AreEqual(new[] { 4 * 100, 10 * 12 * 3 }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 4 * 100, 100 }, layer.U.Axes);
            Assert.IsFalse(layer.U.Weights.Take(layer.U.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.U.Weights.Take(layer.U.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 4 * 100 }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest3()
        {
            string architecture = "100LSTM";
            try
            {
                LSTMCell layer = new LSTMCell(new Shape(-1, 10, 12, 3), architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new LSTMCell(null, "100LSTMC", null));
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest5()
        {
            Assert.IsNotNull(new LSTMCell(new Shape(-1, 10, 12, 3), null, null));
        }

        [TestMethod, TestCategory("LSTM")]
        public void CopyConstructorTest1()
        {
            Shape shape = new Shape(-1, 20, 20, 10);
            LSTMCell layer1 = new LSTMCell(shape, RNNDirection.ForwardOnly, 100, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            LSTMCell layer2 = new LSTMCell(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("LSTM")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new LSTMCell(null));
        }

        [TestMethod, TestCategory("LSTM")]
        public void EnumGradientsTest()
        {
            Shape shape = new Shape(-1, 20, 20, 10);
            LSTMCell layer = new LSTMCell(shape, RNNDirection.ForwardOnly, 100, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            Assert.AreEqual(3, layer.EnumGradients().Count());
        }

        [TestMethod, TestCategory("LSTM")]
        public void CloneTest()
        {
            Shape shape = new Shape(-1, 20, 20, 10);
            LSTMCell layer1 = new LSTMCell(shape, RNNDirection.ForwardOnly, 100, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            LSTMCell layer2 = layer1.Clone() as LSTMCell;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("LSTM")]
        public void SerializeTest()
        {
            Shape shape = new Shape(-1, 20, 20, 10);
            LSTMCell layer1 = new LSTMCell(shape, RNNDirection.ForwardOnly, 100, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            LSTMCell layer2 = JsonConvert.DeserializeObject<LSTMCell>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.ColumnMajor, forgetBias = 0
        /// </summary>
        [TestMethod, TestCategory("LSTM")]
        public void ForwardTest1()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            LSTMCell layer = new LSTMCell(new Shape(batchSize, inputSize), RNNDirection.ForwardOnly, numberOfNeurons, 0, MatrixLayout.ColumnMajor, null);

            layer.W.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f,
                -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f, 0.65368795f, -0.07057703f, -0.46016663f, 0.00298941f,
                -0.2414839f, -0.08907348f, 0.42851567f, 0.2056688f, -0.4476247f, 0.02284491f, -0.3713316f, 0.6078448f
            });

            layer.U.Set(new float[]
            {
                0.6758169f, 0.08030099f, 0.6760981f, 0.17049837f, -0.08858025f, -0.46412382f, 0.30718505f, -0.4382419f,
                -0.33939722f, 0.44897282f, 0.18849522f, 0.399871f, -0.63440335f, 0.5429312f, 0.12797189f, 0.24060631f
            });

            layer.B.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f
            });

            Tensor x = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { batchSize, inputSize },
                new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[]
                {
                    0.1842714f, -0.02906899f, 0.3444413f, -0.05849501f
                });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[]
                {
                    0.01070222f, -0.004574825f, 0.02015805f, 0.03693998f, 0.003658599f, -0.001115061f, 0.0176655f, -0.00546921f,
                    0.01489833f, -0.006261217f, 0.03067097f, 0.0526433f, 0.004573248f, -0.001393827f, 0.02298556f, -0.007095588f,
                    0.01909443f, -0.00794761f, 0.04118389f, 0.06834662f, 0.005487898f, -0.001672592f, 0.02830561f, -0.008721967f
                },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.003996308f, -0.001774185f, 0.005924406f, 0.01304437f, 0.001685438f, -0.0005136849f, 0.007583042f, -0.002360414f,
                    -0.0006304212f, 0.0002798793f, -0.0009345806f, -0.002057761f, -0.0002658794f, 8.103426E-05f, -0.001196232f, 0.0003723574f
                },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.04196102f, -0.01686392f, 0.1051292f, 0.1570332f, 0.009146497f, -0.002787654f, 0.05320057f, -0.01626378f
                },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.03871498f, -0.04285951f, 0.03818508f, 0.04794622f, -0.05159616f, -0.003268317f
                },
                x.Gradient);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.ColumnMajor, forgetBias = LSTMCell.DefaultForgetBias
        /// </summary>
        [TestMethod, TestCategory("LSTM")]
        public void ForwardTest2()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            LSTMCell layer = new LSTMCell(new Shape(batchSize, inputSize), RNNDirection.ForwardOnly, numberOfNeurons, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null);

            layer.W.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f,
                -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f, 0.65368795f, -0.07057703f, -0.46016663f, 0.00298941f,
                -0.2414839f, -0.08907348f, 0.42851567f, 0.2056688f, -0.4476247f, 0.02284491f, -0.3713316f, 0.6078448f
            });

            layer.U.Set(new float[]
            {
                0.6758169f, 0.08030099f, 0.6760981f, 0.17049837f, -0.08858025f, -0.46412382f, 0.30718505f, -0.4382419f,
                -0.33939722f, 0.44897282f, 0.18849522f, 0.399871f, -0.63440335f, 0.5429312f, 0.12797189f, 0.24060631f
            });

            layer.B.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f
            });

            Tensor x = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { batchSize, inputSize },
                new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[]
                {
                    0.1842714f, -0.02906899f, 0.3693406f, -0.06449991f
                });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[]
                {
                    0.01027776f, -0.004698483f, 0.01997082f, 0.03847838f, 0.002243571f, -0.0007621321f, 0.01884598f, -0.005999637f,
                    0.01452434f, -0.006519128f, 0.03100058f, 0.05579798f, 0.002804463f, -0.0009526651f, 0.02445407f, -0.007761949f,
                    0.01877091f, -0.008339772f, 0.04203034f, 0.07311759f, 0.00336535624f, -0.00114319811f, 0.03006217f, -0.00952426251f
                },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.003704588f, -0.001767678f, 0.005491941f, 0.01299652f, 0.001033565f, -0.0003510979f, 0.008131213f, -0.002602726f,
                    -0.0005844022f, 0.0002788528f, -0.0008663588f, -0.002050214f, -0.0001630458f, 5.538602E-05f, -0.001282706f, 0.0004105824f
                },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.04246571f, -0.01820644f, 0.1102976f, 0.173196f, 0.005608927f, -0.00190533f, 0.05608095f, -0.01762313f
                },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.04109392f, -0.04983611f, 0.04440974f, 0.04776936f, -0.05458257f, -0.004248527f
                },
                x.Gradient);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.RowMajor, forgetBias = 0
        /// </summary>
        [TestMethod, TestCategory("LSTM")]
        public void ForwardTest3()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            LSTMCell layer = new LSTMCell(new Shape(batchSize, inputSize), RNNDirection.ForwardOnly, numberOfNeurons, 0, MatrixLayout.RowMajor, null);

            layer.W.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f,
                -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f, 0.65368795f, -0.07057703f, -0.46016663f, 0.00298941f,
                -0.2414839f, -0.08907348f, 0.42851567f, 0.2056688f, -0.4476247f, 0.02284491f, -0.3713316f, 0.6078448f
            });

            layer.U.Set(new float[]
            {
                0.6758169f, 0.08030099f, 0.6760981f, 0.17049837f, -0.08858025f, -0.46412382f, 0.30718505f, -0.4382419f,
                -0.33939722f, 0.44897282f, 0.18849522f, 0.399871f, -0.63440335f, 0.5429312f, 0.12797189f, 0.24060631f
            });

            layer.B.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f
            });

            Tensor x = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { batchSize, inputSize },
                new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[]
                {
                    0.06790479f, -0.04298752f, 0.05931183f, -0.07943144f
                });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[]
                {
                    0.001331401f, 0.002100369f, 0.002869338f, -0.004855725f, -0.006591389f, -0.008327054f, 0.06464833f, 0.08877664f,
                    0.112905f, 0.04203695f, 0.05733923f, 0.07264151f, 0.002008002f, 0.002510003f, 0.003012004f, -0.001737835f,
                    -0.002172294f, -0.002606753f, 0.002907461f, 0.003824373f, 0.004741285f, -0.007056035f, -0.008993263f, -0.01093049f
                },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.0001273063f, -8.059201E-05f, -0.0007062234f, 0.0004470788f, 0.00917168f, -0.005806184f, 0.006051375f, -0.003830857f,
                    0.0003408825f, -0.0002157976f, -0.0002950183f, 0.000186763f, 0.0004505594f, -0.0002852292f, -0.001158638f, 0.0007334827f
                },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.007689682f, -0.01735665f, 0.2412831f, 0.1530228f, 0.005020006f, -0.004344588f, 0.009169118f, -0.01937228f
                },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.06747339f, -0.0260333f, -0.1002267f, 0.0872632f, -0.02436972f, -0.1453215f
                },
                x.Gradient);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.RowMajor, forgetBias = LSTMCell.DefaultForgetBias
        /// </summary>
        [TestMethod, TestCategory("LSTM")]
        public void ForwardTest4()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            LSTMCell layer = new LSTMCell(new Shape(batchSize, inputSize), RNNDirection.ForwardOnly, numberOfNeurons, LSTMCell.DefaultForgetBias, MatrixLayout.RowMajor, null);

            layer.W.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f,
                -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f, 0.65368795f, -0.07057703f, -0.46016663f, 0.00298941f,
                -0.2414839f, -0.08907348f, 0.42851567f, 0.2056688f, -0.4476247f, 0.02284491f, -0.3713316f, 0.6078448f
            });

            layer.U.Set(new float[]
            {
                0.6758169f, 0.08030099f, 0.6760981f, 0.17049837f, -0.08858025f, -0.46412382f, 0.30718505f, -0.4382419f,
                -0.33939722f, 0.44897282f, 0.18849522f, 0.399871f, -0.63440335f, 0.5429312f, 0.12797189f, 0.24060631f
            });

            layer.B.Set(new float[]
            {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f
            });

            Tensor x = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { batchSize, inputSize },
                new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                TensorShape.Unknown,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[]
                {
                    0.06790479f, -0.04298752f, 0.07423161f, -0.08913845f
                });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[]
                {
                    0.001471336f, 0.002383115f, 0.003294893f, -0.005021602f, -0.006947685f, -0.008873769f, 0.06699824f, 0.09368362f,
                    0.120369f,  0.04358105f, 0.06063772f, 0.07769438f, 0.001469417f, 0.001836772f, 0.002204126f, -0.001277732f,
                    -0.001597165f,  -0.001916599f, 0.003573181f, 0.004655094f, 0.005737007f, -0.007892253f, -0.01004015f, -0.0121880472f
                },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.0001266555f, -8.018001E-05f, -0.0007006686f, 0.0004435623f, 0.009124792f, -0.005776502f, 0.006003778f, -0.003800726f,
                    0.0002494512f, -0.0001579165f, -0.0002169104f, 0.0001373164f, 0.0005638967f, -0.000356978f, -0.001300231f, 0.0008231187f,
                },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.009117784f, -0.01926083f, 0.2668537f, 0.1705666f, 0.003673543f, -0.003194331f, 0.01081913f, -0.02147897f
                },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[]
                {
                    0.08394533f, -0.03244966f, -0.1254941f, 0.0866316f, -0.02330277f, -0.146042f
                },
                x.Gradient);
        }
    }
}
