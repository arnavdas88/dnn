namespace Genix.DNN.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Genix.Core;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class FullyConnectedLayerTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            int[] shape = new[] { 1, 10, 12, 3 };
            const int NumberOfNeurons = 100;

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                FullyConnectedLayer layer = new FullyConnectedLayer(shape, NumberOfNeurons, matrixLayout, null);
                Assert.AreEqual("100N", layer.Architecture);
                CollectionAssert.AreEqual(new[] { 1, NumberOfNeurons }, layer.OutputShape);
                Assert.AreEqual(NumberOfNeurons, layer.NumberOfNeurons);
                Assert.AreEqual(matrixLayout, layer.MatrixLayout);

                CollectionAssert.AreEqual(
                    matrixLayout == MatrixLayout.RowMajor ?
                        new[] { NumberOfNeurons, 10 * 12 * 3 } :
                        new[] { 10 * 12 * 3, NumberOfNeurons },
                    layer.W.Axes);
                Assert.IsFalse(layer.W.Weights.All(x => x == 0.0f));
                Assert.AreEqual(0.0, layer.W.Weights.Average(), 0.01f);

                CollectionAssert.AreEqual(new[] { NumberOfNeurons }, layer.B.Axes);
                Assert.IsTrue(layer.B.Weights.All(x => x == 0.0f));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new FullyConnectedLayer(null, 100, MatrixLayout.ColumnMajor, null));
        }

        [TestMethod]
        public void ArchitectureConstructorTest1()
        {
            FullyConnectedLayer layer = new FullyConnectedLayer(new[] { -1, 10, 12, 3 }, "100N", null);

            Assert.AreEqual(100, layer.NumberOfNeurons);
            Assert.AreEqual("100N", layer.Architecture);

            CollectionAssert.AreEqual(new[] { -1, 100 }, layer.OutputShape);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual(MatrixLayout.RowMajor, layer.MatrixLayout);

            CollectionAssert.AreEqual(new[] { 100, 10 * 12 * 3 }, layer.W.Axes);
            Assert.IsFalse(layer.W.Weights.All(x => x == 0.0f));
            Assert.AreEqual(0.0, layer.W.Weights.Average(), 0.01f);

            CollectionAssert.AreEqual(new[] { 100 }, layer.B.Axes);
            Assert.IsTrue(layer.B.Weights.All(x => x == 0.0f));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "100NN";
            try
            {
                FullyConnectedLayer layer = new FullyConnectedLayer(new[] { -1, 20, 20, 10 }, architecture, null);
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
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new FullyConnectedLayer(null, "100N", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new FullyConnectedLayer(new[] { -1, 20, 20, 10 }, null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            FullyConnectedLayer layer1 = new FullyConnectedLayer(shape, 100, MatrixLayout.ColumnMajor, null);
            FullyConnectedLayer layer2 = new FullyConnectedLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new FullyConnectedLayer(null));
        }

        [TestMethod]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            FullyConnectedLayer layer1 = new FullyConnectedLayer(shape, 100, MatrixLayout.ColumnMajor, null);
            FullyConnectedLayer layer2 = layer1.Clone() as FullyConnectedLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 20, 20, 10 };
            FullyConnectedLayer layer1 = new FullyConnectedLayer(shape, 100, MatrixLayout.ColumnMajor, null);
            string s1 = JsonConvert.SerializeObject(layer1);
            FullyConnectedLayer layer2 = JsonConvert.DeserializeObject<FullyConnectedLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", Justification = "Need for testing.")]
        [TestMethod]
        public void ForwardBackwardTest()
        {
            int[] shape = new[] { -1, 2, 3, 2 };
            const int NumberOfNeurons = 2;

            foreach (MatrixLayout matrixLayout in Enum.GetValues(typeof(MatrixLayout)).OfType<MatrixLayout>())
            {
                FullyConnectedLayer layer = new FullyConnectedLayer(shape, NumberOfNeurons, matrixLayout, null);
                ////layer.SetLearningMode(true);

                layer.W.Set(new float[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12,
                    21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
                });

                layer.B.Set(new float[] { 1, 2 });

                Tensor xTemp = new Tensor(null, new[] { 1, 12 });
                xTemp.Set(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });

                // should be W * x + b
                Tensor expectedTemp = new Tensor(null, new[] { 1, NumberOfNeurons });
                expectedTemp.Set(FullyConnectedLayerTest.CalculateNeurons(layer.W, xTemp, layer.B, NumberOfNeurons, matrixLayout));

                Tensor dyTemp = new Tensor(null, new[] { 1, NumberOfNeurons });
                dyTemp.Set(new float[] { 1, 2 });

                // should be W' * dy
                Tensor expectedDxTemp = new Tensor(null, xTemp.Axes);
                expectedDxTemp.Set(FullyConnectedLayerTest.CalculateDx(layer.W, dyTemp, NumberOfNeurons, matrixLayout));

                Tensor expectedDBTemp = new Tensor(null, layer.B.Axes);
                expectedDBTemp.Set(FullyConnectedLayerTest.CalculateDB(dyTemp));

                // should be sum(x' * dy)
                Tensor expectedDWTemp = new Tensor(null, layer.W.Axes);
                expectedDWTemp.Set(FullyConnectedLayerTest.CalculateDW(xTemp, dyTemp, matrixLayout));

                for (int i = 1; i <= 3; i++)
                {
                    Session session = new Session();

                    layer.W.ClearGradient();
                    layer.B.ClearGradient();

                    Tensor x = session.Tile(xTemp, (int)Axis.B, i);
                    Tensor y = layer.Forward(session, new[] { x })[0];

                    Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                    Helpers.AreTensorsEqual(expected, y);

                    // unroll the graph
                    y.SetGradient(session.Tile(dyTemp, (int)Axis.B, i).Weights);
                    session.Unroll();

                    Tensor expectedDx = session.Tile(expectedDxTemp, (int)Axis.B, i);
                    Helpers.AreArraysEqual(expectedDx.Weights, x.Gradient);

                    // should be dy
                    Tensor expectedDB = session.Multiply(expectedDBTemp, i);
                    Helpers.AreArraysEqual(expectedDB.Weights, layer.B.Gradient);

                    // should be x * dy
                    Tensor expectedDW = session.Multiply(expectedDWTemp, i);
                    Helpers.AreArraysEqual(expectedDW.Weights, layer.W.Gradient);
                }
            }
        }

        internal static float[] CalculateNeurons(Tensor w, Tensor x, Tensor b, int numberOfNeurons, MatrixLayout matrixLayout)
        {
            int count = x.Length;

            return Enumerable.Range(0, numberOfNeurons).Select(neuron =>
            {
                return Matrix.DotProduct(
                    count,
                    matrixLayout == MatrixLayout.RowMajor ?
                        Enumerable.Range(neuron * count, count).Select(i => w[i]).ToArray() :
                        Enumerable.Range(0, count).Select(i => w[(i * numberOfNeurons) + neuron]).ToArray(),
                    0,
                    x.Weights,
                    0) + b[neuron];
            }).ToArray();
        }

        internal static float[] CalculateDx(Tensor w, Tensor dy, int numberOfNeurons, MatrixLayout matrixLayout)
        {
            int xlen = w.Length / numberOfNeurons;

            return Enumerable.Range(0, xlen).Select(i =>
            {
                return matrixLayout == MatrixLayout.RowMajor ?
                    Enumerable.Range(0, numberOfNeurons).Sum(neuron => w[i + (neuron * xlen)] * dy[neuron]) :
                    Enumerable.Range(0, numberOfNeurons).Sum(neuron => w[(i * numberOfNeurons) + neuron] * dy[neuron]);
            }).ToArray();
        }

        internal static float[] CalculateDB(Tensor dy)
        {
            return dy.Weights;
        }

        internal static float[] CalculateDW(Tensor x, Tensor dy, MatrixLayout matrixLayout)
        {
            return matrixLayout == MatrixLayout.RowMajor ?
                Enumerable.Range(0, dy.Length).SelectMany(j =>
                {
                    return Enumerable.Range(0, x.Length).Select(i => x[i] * dy[j]);
                }).ToArray() :
                Enumerable.Range(0, x.Length).SelectMany(i =>
                {
                    return Enumerable.Range(0, dy.Length).Select(j => x[i] * dy[j]);
                }).ToArray();
        }
    }
}
