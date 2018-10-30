namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ConcatLayerTest
    {
        private static int[] inputShape1 = new[] { -1, 2, 3, 2 };
        private static int[] inputShape2 = new[] { -1, 2, 3, 3 };

        private static int[] outputShape = Shape.Reshape(
            ConcatLayerTest.inputShape1,
            (int)Axis.C,
            ConcatLayerTest.inputShape1[(int)Axis.C] + ConcatLayerTest.inputShape2[(int)Axis.C]);

        private static float[] weights1 = new float[]
        {
            1, 11,   2, 12,   3, 13,
            4, 14,   5, 15,   6, 16,
        };

        private static float[] weights2 = new float[]
        {
            21, 31, 41,   22, 32, 42,   23, 33, 43,
            24, 34, 44,   25, 35, 45,   26, 36, 46,
        };

        [TestMethod, TestCategory("Concat")]
        public void ConstructorTest1()
        {
            ConcatLayer layer = new ConcatLayer(
                new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 });

            CollectionAssert.AreEqual(ConcatLayerTest.outputShape, layer.OutputShape);
            Assert.AreEqual("CONCAT", layer.Architecture);
        }

        [TestMethod, TestCategory("Concat")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new ConcatLayer((IList<int[]>)null));
        }

        [TestMethod, TestCategory("Concat")]
        public void ArchitectureConstructorTest1()
        {
            ConcatLayer layer = new ConcatLayer(
                new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 },
                "CONCAT",
                null);

            CollectionAssert.AreEqual(ConcatLayerTest.outputShape, layer.OutputShape);
            Assert.AreEqual("CONCAT", layer.Architecture);
        }

        [TestMethod, TestCategory("Concat")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "CON";
            try
            {
                ConcatLayer layer = new ConcatLayer(
                    new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 },
                    architecture,
                    null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("Concat")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new ConcatLayer(null, "CONCAT", null));
        }

        [TestMethod, TestCategory("Concat")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new ConcatLayer(
                new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 },
                null,
                null));
        }

        [TestMethod, TestCategory("Concat")]
        public void CopyConstructorTest1()
        {
            ConcatLayer layer1 = new ConcatLayer(
                new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 });
            ConcatLayer layer2 = new ConcatLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("Concat")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new ConcatLayer((ConcatLayer)null));
        }

        [TestMethod, TestCategory("Concat")]
        public void CloneTest()
        {
            ConcatLayer layer1 = new ConcatLayer(
                new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 });
            ConcatLayer layer2 = layer1.Clone() as ConcatLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("Concat")]
        public void SerializeTest()
        {
            ConcatLayer layer1 = new ConcatLayer(
                new int[][] { ConcatLayerTest.inputShape1, ConcatLayerTest.inputShape2 });
            string s1 = JsonConvert.SerializeObject(layer1);
            ConcatLayer layer2 = JsonConvert.DeserializeObject<ConcatLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod, TestCategory("Concat")]
        [Description("Filter 2x2, stride 2x2.")]
        public void ForwardBackwardTest()
        {
            ConcatLayer layer = new ConcatLayer(new int[][]
            {
                ConcatLayerTest.inputShape1,
                ConcatLayerTest.inputShape2
            });

            Tensor xTemp1 = new Tensor(null, Shape.Reshape(ConcatLayerTest.inputShape1, (int)Axis.B, 1));
            xTemp1.Set(ConcatLayerTest.weights1);
            Tensor xTemp2 = new Tensor(null, Shape.Reshape(ConcatLayerTest.inputShape2, (int)Axis.B, 1));
            xTemp2.Set(ConcatLayerTest.weights2);

            Tensor expectedTemp = new Tensor(null, new[] { 1, 2, 3, 5 });
            expectedTemp.Set(new float[]
            {
                1, 11, 21, 31, 41,   2, 12, 22, 32, 42,   3, 13, 23, 33, 43,
                4, 14, 24, 34, 44,   5, 15, 25, 35, 45,   6, 16, 26, 36, 46
            });

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x1 = session.Tile(xTemp1, (int)Axis.B, i);
                Tensor x2 = session.Tile(xTemp2, (int)Axis.B, i);
                Tensor y = layer.Forward(session, new[] { x1, x2 })[0];

                Tensor expected = session.Tile(expectedTemp, (int)Axis.B, i);
                Helpers.AreTensorsEqual(expected, y);

                // unroll the graph
                Array.Copy(y.Weights, y.Gradient, y.Length);
                session.Unroll();

                Helpers.AreArraysEqual(x1.Length, x1.Weights, x1.Gradient);
                Helpers.AreArraysEqual(x2.Length, x2.Weights, x2.Gradient);
            }
        }
    }
}
