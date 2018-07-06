namespace Accord.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accord.DNN;
    using Accord.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class GRUCellTest
    {
        [TestMethod, TestCategory("GRU")]
        public void CreateLayerTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            const string Architecture = "100GRUC";
            GRUCell layer = (GRUCell)NetworkGraphBuilder.CreateLayer(shape, Architecture, null);

            Assert.AreEqual(100, layer.NumberOfNeurons);
            Assert.AreEqual(Architecture, layer.Architecture);
        }

        [TestMethod, TestCategory("GRU")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateLayerTest3()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            const string Architecture = "GRUC";
            Assert.IsNotNull(NetworkGraphBuilder.CreateLayer(shape, Architecture, null));
        }

        [TestMethod, TestCategory("GRU")]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 1, 10, 12, 3 };
            const int NumberOfNeurons = 100;

            GRUCell layer = new GRUCell(shape, NumberOfNeurons, MatrixLayout.ColumnMajor, null);
            Assert.AreEqual("100GRUC", layer.Architecture);

            CollectionAssert.AreEqual(new[] { 1, NumberOfNeurons }, layer.OutputShape);
            Assert.AreEqual(NumberOfNeurons, layer.NumberOfNeurons);

            CollectionAssert.AreEqual(new[] { 10 * 12 * 3, 3 * NumberOfNeurons }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { NumberOfNeurons, 2 * NumberOfNeurons }, layer.U.Axes);
            Assert.IsFalse(layer.U.Weights.All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.U.Weights.Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { NumberOfNeurons, NumberOfNeurons }, layer.UC.Axes);
            Assert.IsFalse(layer.U.Weights.All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.U.Weights.Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 3 * NumberOfNeurons }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.Take(2 * NumberOfNeurons).All(x => x == 1.0f));
            Assert.IsTrue(layer.B.Weights.Skip(2 * NumberOfNeurons).All(x => x == 0.0f));
        }

        [TestMethod, TestCategory("GRU")]
        public void ConstructorTest2()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            GRUCell layer1 = new GRUCell(shape, 100, MatrixLayout.RowMajor, null);
            GRUCell layer2 = new GRUCell(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("GRU")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new GRUCell(null, 100, MatrixLayout.RowMajor, null));
        }

        [TestMethod, TestCategory("GRU")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new GRUCell(null));
        }

        [TestMethod, TestCategory("GRU")]
        public void EnumGradientsTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            GRUCell layer = new GRUCell(shape, 100, MatrixLayout.RowMajor, null);
            Assert.AreEqual(4, layer.EnumGradients().Count());
        }

        [TestMethod, TestCategory("GRU")]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            GRUCell layer1 = new GRUCell(shape, 100, MatrixLayout.RowMajor, null);
            GRUCell layer2 = layer1.Clone() as GRUCell;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("GRU")]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            GRUCell layer1 = new GRUCell(shape, 100, MatrixLayout.RowMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            GRUCell layer2 = JsonConvert.DeserializeObject<GRUCell>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.ColumnMajor.
        /// </summary>
        [TestMethod, TestCategory("GRU")]
        public void ForwardTest1()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            Session session = new Session();

            GRUCell layer = new GRUCell(new[] { batchSize, inputSize }, numberOfNeurons, MatrixLayout.ColumnMajor, null);

            layer.W.Set(new float[] {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f,
                -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f, 0.65368795f, -0.07057703f,
                -0.2414839f, -0.08907348f, 0.42851567f, 0.2056688f, -0.4476247f, 0.02284491f,
            });

            layer.U.Set(new float[] {
                0.6758169f, 0.08030099f, 0.6760981f, 0.17049837f,
                -0.33939722f, 0.44897282f, 0.18849522f, 0.399871f,
            });

            layer.UC.Set(new float[] {
                0.6758169f, 0.08030099f,
                -0.33939722f, 0.44897282f,
            });

            layer.B.Set(new float[] {
                0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f
            });

            Tensor x = new Tensor(
                null,
                new[] { batchSize, inputSize },
                new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[] {
                    0.2988823f, -0.1389049f, 0.5474511f, -0.1845018f
                });

            // calculate
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[] {
                    0.01607361f, -0.007430535f, 0.001741877f, -0.000598697f, 0.04607954f, 0.08297627f,
                    0.02303241f, -0.01174307f, 0.002177347f, -0.0007483712f, 0.0663493f, 0.1174305f,
                    0.02999121f, -0.0160556f, 0.002612816f, -0.0008980455f, 0.08661906f, 0.1518847f
                },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.009080857f, -0.003106385f, 0.001301541f, -0.0004473499f,
                    -0.004220309f, 0.001443686f, -0.0006048883f, 0.0002079049f
                },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.0183215f, 0.03444418f, -0.006485323f, -0.01219232f
                },
                layer.UC.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.06958796f, -0.04312533f, 0.004354693f, -0.001496742f, 0.2026976f, 0.3445423f
                },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.1136149f, 0.03690277f, -0.05459791f, 0.09195063f, 0.02584295f, -0.03966833f
                },
                x.Gradient);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.RowMajor.
        /// </summary>
        [TestMethod, TestCategory("GRU")]
        public void ForwardTest2()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            GRUCell layer = new GRUCell(new[] { batchSize, inputSize }, numberOfNeurons, MatrixLayout.RowMajor, null);

            layer.W.Set(new float[] {
                0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f,
                -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f, 0.65368795f, -0.07057703f,
                -0.2414839f, -0.08907348f, 0.42851567f, 0.2056688f, -0.4476247f, 0.02284491f,
            });

            layer.U.Set(new float[] {
                0.6758169f, 0.08030099f, 0.6760981f, 0.17049837f,
                -0.33939722f, 0.44897282f, 0.18849522f, 0.399871f,
            });

            layer.UC.Set(new float[] {
                0.6758169f, 0.08030099f,
                -0.33939722f, 0.44897282f,
            });

            layer.B.Set(new float[] {
                0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f, 0.5805608f, -0.33490005f
            });

            Tensor x = new Tensor(
                null,
                new[] { batchSize, inputSize },
                new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[] {
                    0.3631181f, -0.1901545f, 0.5813864f, -0.382444f
                });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[] {
                    0.009786441f, 0.01411872f, 0.018451f, -0.01654311f, -0.02359727f, -0.03065143f,
                    0.0008334472f, 0.001041809f, 0.001250171f, -0.001588227f, -0.001985283f, -0.00238234f,
                    0.05585977f, 0.07548201f, 0.09510425f, 0.08687179f, 0.1219571f, 0.1570424f
                },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.006601683f, -0.003457112f, -0.01148537f, 0.006014556f,
                    0.0007565994f, -0.0003962094f, -0.001441784f, 0.0007550211f
                },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.02053534f, -0.01384354f, 0.02934673f, -0.01978358f
                },
                layer.UC.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.04332279f, -0.07054159f, 0.002083618f, -0.003970566f, 0.1962224f, 0.3508531f
                },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[] {
                    0.03742515f, -0.1009044f, 0.03375921f, 0.02073827f, -0.1018581f, 0.05336173f
                },
                x.Gradient);
        }
    }
}
