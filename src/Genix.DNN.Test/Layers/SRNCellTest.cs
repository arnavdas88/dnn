namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Genix.Core;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SRNCellTest
    {
        private readonly RandomNumberGenerator<float> random = new RandomGenerator();

        [TestMethod, TestCategory("SRN")]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 1, 10, 12, 3 };
            int numberOfNeurons = 100;

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                SRNCell layer = new SRNCell(shape, RNNDirection.ForwardOnly, numberOfNeurons, matrixLayout, null);

                Assert.AreEqual(numberOfNeurons, layer.NumberOfNeurons);
                Assert.AreEqual("100SRNC", layer.Architecture);

                Assert.AreEqual(1, layer.NumberOfOutputs);
                CollectionAssert.AreEqual(new[] { 1, numberOfNeurons }, layer.OutputShape);

                CollectionAssert.AreEqual(
                    matrixLayout == MatrixLayout.RowMajor ?
                        new[] { numberOfNeurons, 10 * 12 * 3 } :
                        new[] { 10 * 12 * 3, numberOfNeurons },
                    layer.W.Axes);
                Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
                Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

                CollectionAssert.AreEqual(new[] { numberOfNeurons, numberOfNeurons }, layer.U.Axes);
                Assert.IsFalse(layer.U.Weights.Take(layer.U.Length).All(x => x == 0.0f));
                Assert.AreEqual(0.0, layer.U.Weights.Take(layer.U.Length).Average(), 0.01f);

                CollectionAssert.AreEqual(new[] { numberOfNeurons }, layer.B.Axes);
                Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
            }
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new SRNCell(null, RNNDirection.ForwardOnly, 100, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod, TestCategory("SRN")]
        public void ArchitectureConstructorTest1()
        {
            const string Architecture = "100SRNC";
            SRNCell layer = new SRNCell(new[] { -1, 10, 12, 3 }, Architecture, null);

            Assert.AreEqual(RNNDirection.ForwardOnly, layer.Direction);
            Assert.AreEqual(100, layer.NumberOfNeurons);
            Assert.AreEqual(Architecture, layer.Architecture);

            CollectionAssert.AreEqual(new[] { -1, 100 }, layer.OutputShape);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual(MatrixLayout.RowMajor, layer.MatrixLayout);

            CollectionAssert.AreEqual(new[] { 100, 10 * 12 * 3 }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 100, 100 }, layer.U.Axes);
            Assert.IsFalse(layer.U.Weights.Take(layer.U.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.U.Weights.Take(layer.U.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 100 }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
        }

        [TestMethod, TestCategory("SRN")]
        public void ArchitectureConstructorTest2()
        {
            const string Architecture = "100SRNC(Bi=1)";
            SRNCell layer = new SRNCell(new[] { -1, 10, 12, 3 }, Architecture, null);

            Assert.AreEqual(RNNDirection.BiDirectional, layer.Direction);
            Assert.AreEqual(100, layer.NumberOfNeurons);
            Assert.AreEqual(Architecture, layer.Architecture);

            CollectionAssert.AreEqual(new[] { -1, 100 }, layer.OutputShape);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual(MatrixLayout.RowMajor, layer.MatrixLayout);

            CollectionAssert.AreEqual(new[] { 100, 10 * 12 * 3 }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.Take(layer.W.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Take(layer.W.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 100, 50 }, layer.U.Axes);
            Assert.IsFalse(layer.U.Weights.Take(layer.U.Length).All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.U.Weights.Take(layer.U.Length).Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 100 }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.Take(layer.B.Length).All(x => x == 0.0f));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest3()
        {
            string architecture = "100SRN";
            try
            {
                SRNCell layer = new SRNCell(new[] { -1, 10, 12, 3 }, architecture, null);
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
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new SRNCell(null, "100SRNC", null));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest5()
        {
            Assert.IsNotNull(new SRNCell(new[] { -1, 10, 12, 3 }, null, null));
        }

        [TestMethod, TestCategory("SRN")]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            SRNCell layer1 = new SRNCell(shape, RNNDirection.ForwardOnly, 100, MatrixLayout.ColumnMajor, null);
            SRNCell layer2 = new SRNCell(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("SRN")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new SRNCell(null));
        }

        [TestMethod, TestCategory("SRN")]
        public void EnumGradientsTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            SRNCell layer = new SRNCell(shape, RNNDirection.ForwardOnly, 100, MatrixLayout.ColumnMajor, null);
            Assert.AreEqual(3, layer.EnumGradients().Count());
        }

        [TestMethod, TestCategory("SRN")]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            SRNCell layer1 = new SRNCell(shape, RNNDirection.ForwardOnly, 100, MatrixLayout.ColumnMajor, null);
            SRNCell layer2 = layer1.Clone() as SRNCell;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("SRN")]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            SRNCell layer1 = new SRNCell(shape, RNNDirection.ForwardOnly, 100, MatrixLayout.ColumnMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            SRNCell layer2 = JsonConvert.DeserializeObject<SRNCell>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod, TestCategory("SRN")]
        public void ForwardBackwardTest1()
        {
            const int T = 2;
            const int N = 3;

            Session session = new Session();

            SRNCell layer = new SRNCell(new[] { -1, N }, RNNDirection.ForwardOnly, 2, MatrixLayout.RowMajor, null);

            layer.W.Randomize(this.random);
            layer.U.Randomize(this.random);
            layer.B.Randomize(this.random);
            ////layer.W.Set(new float[] { 0.1f, 0.2f, -0.3f, 0.4f, 0.5f, 0.6f });        // 3x2 matrix
            ////layer.U.Set(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });                     // 2x2 matrix
            ////layer.B.Set(new float[] { 0.1f, 0.2f });                                 // 2x1 vector

            Tensor x = new Tensor(null, new[] { T, N });
            x.Randomize(this.random);
            ////x.Set(new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f });
            IList<Tensor> xs = new[] { x };
            IList<Tensor> ys = layer.Forward(session, xs);

            float[] bw = layer.B.Weights;
            Tensor expected = new Tensor(null, new[] { 2, 2 });
            expected.Weights[0] = Matrix.DotProduct(3, layer.W.Weights, 0, x.Weights, 0) + bw[0];
            expected.Weights[1] = Matrix.DotProduct(3, layer.W.Weights, 3, x.Weights, 0) + bw[1];
            Nonlinearity.ReLU(2, expected.Weights, 0, expected.Weights, 0);
            expected.Weights[2] = Matrix.DotProduct(3, layer.W.Weights, 0, x.Weights, 3) + bw[0] + Matrix.DotProduct(2, layer.U.Weights, 0, expected.Weights, 0);
            expected.Weights[3] = Matrix.DotProduct(3, layer.W.Weights, 3, x.Weights, 3) + bw[1] + Matrix.DotProduct(2, layer.U.Weights, 2, expected.Weights, 0);
            Nonlinearity.ReLU(2, expected.Weights, 2, expected.Weights, 2);
            Helpers.AreTensorsEqual(expected, ys[0]);

            // unroll the graph
            ////session.GetGradient(ys[0]).Randomize(this.random);
            ys[0].SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            ////float[] dy = session.GetGradient(ys[0]).Weights.ToArray();
            float[] dy = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };
            float[] expectedWG = new float[layer.W.Length];
            float[] expectedUG = new float[layer.U.Length];
            float[] expectedBG = new float[layer.B.Length];
            float[] expectedDx = new float[x.Length];

            for (int oi = 2, ii = 3; oi >= 0; oi -= 2, ii -= 3)
            {
                Nonlinearity.ReLUGradient(2, dy, oi, true, expected.Weights, oi, dy, oi);

                // should be x' * dy
                Matrix.VxV(MatrixLayout.ColumnMajor, 3, 2, x.Weights, ii, dy, oi, expectedWG, 0);

                // should be W' * dy
                Matrix.MxV(MatrixLayout.ColumnMajor, 3, 2, layer.W.Weights, 0, false, dy, oi, expectedDx, ii, true);

                if (oi > 0)
                {
                    // should be x(t-1)' * dy
                    Matrix.VxV(MatrixLayout.ColumnMajor, 2, 2, expected.Weights, oi - 2, dy, oi, expectedUG, 0);

                    // should be U' * dy
                    Matrix.MxV(MatrixLayout.ColumnMajor, 2, 2, layer.U.Weights, 0, false, dy, oi, dy, oi - 2, false);
                    ////MKL.MxV(MatrixLayout.RowMajor, 2, 2, layer.U.Weights, 0, false, dy, oi, dy, oi - 2, false);
                }

                // should be dy
                Vectors.Add(2, dy, oi, expectedBG, 0);
            }

            Helpers.AreArraysEqual(expectedWG, layer.W.Gradient);
            ////Helpers.AreArraysEqual(expectedUG, layer.U.Gradient);
            Helpers.AreArraysEqual(expectedBG, layer.B.Gradient);
            Helpers.AreArraysEqual(expectedDx, x.Gradient);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.ColumnMajor.
        /// </summary>
        [TestMethod, TestCategory("SRN")]
        public void ForwardTest_Forward_ColumnMajor()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            SRNCell layer = new SRNCell(new[] { batchSize, inputSize }, RNNDirection.ForwardOnly, numberOfNeurons, MatrixLayout.ColumnMajor, null);

            layer.W.Set(new float[] { 0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f });      // 3x2 matrix
            layer.U.Set(new float[] { -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f });                                 // 2x2 matrix
            layer.B.Set(new float[] { -0.2414839f, -0.08907348f });                                                         // 2x1 vector

            Tensor x = new Tensor(null, new[] { batchSize, inputSize }, new float[] { -0.1f, 0.2f, 0.3f, 0.4f, -0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[] { 0f, 0.0008311934f, 0f, 0.07146961f, });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[] { 0f, 0.1568262f, 0f, -0.1936524f, 0f, 0.2495215f, },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0f, 0f, 0f, 0.0003324774f, },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0f, 0.4317382f, },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[] { -0.006405319f, -0.003603125f, 0.009778349f, -0.08072696f, -0.04541058f, 0.1232376f, },
                x.Gradient);
        }

        /// <summary>
        /// MatrixLayout = MatrixLayout.RowMajor.
        /// </summary>
        [TestMethod, TestCategory("SRN")]
        public void ForwardTest_Forward_RowMajor()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            SRNCell layer = new SRNCell(new[] { batchSize, inputSize }, RNNDirection.ForwardOnly, numberOfNeurons, MatrixLayout.RowMajor, null);

            layer.W.Set(new float[] { 0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f });      // 3x2 matrix
            layer.U.Set(new float[] { -0.6668052f, 0.0096491f, 0.17214662f, -0.4206545f });                                 // 2x2 matrix
            layer.B.Set(new float[] { -0.2414839f, -0.08907348f });                                                         // 2x1 vector

            Tensor x = new Tensor(null, new[] { batchSize, inputSize }, new float[] { -0.1f, 0.2f, 0.3f, 0.4f, -0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[] { 0f, 0.06266524f, 0.3149685f, 0f, });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[] { 0.12f, -0.15f, 0.18f, -0.02028947f, 0.04057895f, 0.06086842f, },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0f, 0.01879957f, 0f, 0f, },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0.3f, 0.2028947f, },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[] { -0.02303392f, 0.04865196f, 0.06251067f, 0.1738062f, -0.06054522f, 0.1115987f, },
                x.Gradient);
        }

        /// <summary>
        /// BiDirectional, MatrixLayout = MatrixLayout.ColumnMajor.
        /// </summary>
        [TestMethod, TestCategory("SRN")]
        public void ForwardTest_BiDirectional_ColumnMajor()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            SRNCell layer = new SRNCell(new[] { batchSize, inputSize }, RNNDirection.BiDirectional, numberOfNeurons, MatrixLayout.ColumnMajor, null);

            layer.W.Set(new float[] { 0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f });      // 3x2 matrix
            layer.U.Set(new float[] { -0.6668052f, 0.0096491f });                                                           // 1x2 matrix
            layer.B.Set(new float[] { -0.2414839f, -0.08907348f });                                                         // 2x1 vector

            Tensor x = new Tensor(null, new[] { batchSize, inputSize }, new float[] { -0.1f, 0.2f, 0.3f, 0.4f, -0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[] { 0, 0.001524193f, 0, 0.07181925f });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[] { 0, 0.14077194f, 0, -0.1609649f, 0, 0.3011579f, },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0, 0.01436385f, },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0, 0.601929843f, },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[] { -0.04036348f, -0.0227052923f, 0.0616188161f, -0.08111643f, -0.0456296727f, 0.1238322f, },
                x.Gradient);
        }

        /// <summary>
        /// BiDirectional, MatrixLayout = MatrixLayout.RowMajor.
        /// </summary>
        [TestMethod, TestCategory("SRN")]
        public void ForwardTest_BiDirectional_RowMajor()
        {
            const int batchSize = 2;
            const int inputSize = 3;
            const int numberOfNeurons = 2;

            SRNCell layer = new SRNCell(new[] { batchSize, inputSize }, RNNDirection.BiDirectional, numberOfNeurons, MatrixLayout.RowMajor, null);

            layer.W.Set(new float[] { 0.57935405f, -0.2018174f, 0.3719957f, -0.11352646f, 0.23978919f, 0.30809408f });      // 3x2 matrix
            layer.U.Set(new float[] { -0.6668052f, 0.0096491f });                                                           // 2x1 matrix
            layer.B.Set(new float[] { -0.2414839f, -0.08907348f });                                                         // 2x1 vector

            Tensor x = new Tensor(null, new[] { batchSize, inputSize }, new float[] { -0.1f, 0.2f, 0.3f, 0.4f, -0.5f, 0.6f });

            // set expectations
            Tensor expected = new Tensor(
                null,
                new[] { numberOfNeurons, numberOfNeurons },
                new float[] { 0, 0.06266524f, 0.3143638f, 0 });

            // calculate
            Session session = new Session();
            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 0.1f, 0.2f, 0.3f, 0.4f });
            session.Unroll();

            Helpers.AreArraysEqual(
                new float[] { 0.120000005f, -0.15f, 0.18f, -0.0200000014f, 0.0400000028f, 0.0600000024f, },
                layer.W.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0f, 0f, },
                layer.U.Gradient);
            Helpers.AreArraysEqual(
                new float[] { 0.3f, 0.2f, },
                layer.B.Gradient);
            Helpers.AreArraysEqual(
                new float[] { -0.0227052923f, 0.0479578376f, 0.0616188161f, 0.17380622f, -0.06054522f, 0.111598708f, },
                x.Gradient);
        }
    }
}
