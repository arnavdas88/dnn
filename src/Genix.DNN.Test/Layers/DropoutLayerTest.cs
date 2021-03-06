﻿namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Genix.DNN.Layers;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class DropoutLayerTest
    {
        [TestMethod, TestCategory("Dropout")]
        public void ConstructorTest1()
        {
            Shape shape = new Shape(new int[] { -1, 10000 });
            DropoutLayer layer = new DropoutLayer(shape, 0.5);

            Assert.AreEqual(0.5, layer.Probability);
            CollectionAssert.AreEqual(shape.Axes, layer.OutputShape.Axes);
            Assert.AreEqual("D0.5", layer.Architecture);
        }

        [TestMethod, TestCategory("Dropout")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new DropoutLayer(null, 0.5));
        }

        [TestMethod, TestCategory("Dropout")]
        public void ArchitectureConstructorTest1()
        {
            Shape shape = new Shape(new int[] { -1, 10000 });
            DropoutLayer layer = new DropoutLayer(shape, "D0.5", null);

            Assert.AreEqual(0.5, layer.Probability);
            CollectionAssert.AreEqual(shape.Axes, layer.OutputShape.Axes);
            Assert.AreEqual("D0.5", layer.Architecture);
        }

        [TestMethod, TestCategory("Dropout")]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitectureConstructorTest2()
        {
            string architecture = "DD";
            try
            {
                DropoutLayer layer = new DropoutLayer(new Shape(new int[] { -1, 10000 }), architecture, null);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture), nameof(architecture)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, TestCategory("Dropout")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest3()
        {
            Assert.IsNotNull(new DropoutLayer(null, "D0.5", null));
        }

        [TestMethod, TestCategory("Dropout")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitectureConstructorTest4()
        {
            Assert.IsNotNull(new DropoutLayer(new Shape(new int[] { -1, 10000 }), null, null));
        }

        [TestMethod, TestCategory("Dropout")]
        public void CopyConstructorTest1()
        {
            DropoutLayer layer1 = new DropoutLayer(new Shape(new int[] { -1, 10000 }), 0.5);
            DropoutLayer layer2 = new DropoutLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("Dropout")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new DropoutLayer((DropoutLayer)null));
        }

        [TestMethod, TestCategory("Dropout")]
        public void CloneTest()
        {
            DropoutLayer layer1 = new DropoutLayer(new Shape(new int[] { -1, 10000 }), 0.5);
            DropoutLayer layer2 = layer1.Clone() as DropoutLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod, TestCategory("Dropout")]
        public void SerializeTest()
        {
            DropoutLayer layer1 = new DropoutLayer(new Shape(new int[] { -1, 10000 }), 0.5);
            string s1 = JsonConvert.SerializeObject(layer1);
            DropoutLayer layer2 = JsonConvert.DeserializeObject<DropoutLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod, TestCategory("Dropout")]
        [Description("Shall multiply all weights by probability.")]
        public void ForwardTest1()
        {
            Shape shape = new Shape(new int[] { -1, 10000 });
            DropoutLayer layer = new DropoutLayer(shape, 0.5);

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session(false);

                Tensor x = new Tensor(null, shape.Reshape(0, i));
                x.Randomize();

                IList<Tensor> xs = new[] { x };
                IList<Tensor> ys = layer.Forward(session, xs);

                Assert.AreEqual(x.Weights.Sum() * layer.Probability, ys[0].Weights.Sum());
            }
        }

        [TestMethod, TestCategory("Dropout")]
        [Description("Shall drop some weights based on probability.")]
        public void ForwardBackwardTest2()
        {
            Shape shape = new Shape(new int[] { -1, 10000 });
            DropoutLayer layer = new DropoutLayer(shape, 0.5);

            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session(true);

                Tensor x = new Tensor(null, shape.Reshape(0, i));
                x.Randomize();

                Tensor y = layer.Forward(session, new[] { x })[0];

                Assert.AreEqual(0.0f, y.Weights.Sum(), 1.0f);
                Assert.AreEqual((int)(y.Length * layer.Probability), y.Weights.Count(w => w == 0.0f), y.Length / 50);

                // unroll the graph
                y.SetGradient(1.0f);
                session.Unroll();

                CollectionAssert.AreEqual(y.Weights.Select(w => w == 0.0f ? 0.0f : 1.0f).ToArray(), x.Gradient);
            }
        }
    }
}
