﻿namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class InputLayerTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer = new InputLayer(shape);

            CollectionAssert.AreEqual(shape, layer.Shape);
            CollectionAssert.AreEqual(shape, layer.OutputShape);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual("20x15x10", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest2()
        {
            Assert.IsNotNull(new InputLayer((int[])null));
        }

        [TestMethod]
        public void ArchitechtureConstructorTest1()
        {
            int[] shape = new[] { -1, 1, 1, 1 };
            InputLayer layer = new InputLayer(shape, "20x15x10", null);

            CollectionAssert.AreEqual(new[] { -1, 20, 15, 10 }, layer.Shape);
            CollectionAssert.AreEqual(new[] { -1, 20, 15, 10 }, layer.OutputShape);
            Assert.AreEqual(1, layer.NumberOfOutputs);
            Assert.AreEqual("20x15x10", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArchitechtureConstructorTest2()
        {
            string architecture = "20x15";
            try
            {
                InputLayer layer = new InputLayer(new[] { -1, 1, 1, 1 }, architecture, null);
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
        public void ArchitechtureConstructorTest3()
        {
            Assert.IsNotNull(new InputLayer(null, "20x15x10", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArchitechtureConstructorTest4()
        {
            Assert.IsNotNull(new InputLayer(new[] { -1, 20, 15, 10 }, null, null));
        }

        [TestMethod]
        public void CopyConstructorTest1()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer1 = new InputLayer(shape);
            InputLayer layer2 = new InputLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest2()
        {
            Assert.IsNotNull(new InputLayer((InputLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer1 = new InputLayer(shape);
            InputLayer layer2 = layer1.Clone() as InputLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer1 = new InputLayer(shape);
            string s1 = JsonConvert.SerializeObject(layer1);
            InputLayer layer2 = JsonConvert.DeserializeObject<InputLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Description("Shall pass thru input.")]
        public void ForwardBackwardTest1()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer = new InputLayer(shape);

            for (int i = 1; i <= 3; i++)
            {
                Tensor x = new Tensor(null, Shape.Reshape(shape, (int)Axis.B, i));
                x.Randomize();
                IList<Tensor> xs = new[] { x };
                IList<Tensor> ys = layer.Forward(null, xs);

                Assert.IsTrue(xs[0] == ys[0]);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Wrong count of input tensors.")]
        public void ForwardTest1()
        {
            int[] shape = new[] { 1, 20, 15, 10 };
            InputLayer layer = new InputLayer(shape);

            try
            {
                Tensor x = new Tensor(null, shape);
                x.Randomize();
                IList<Tensor> xs = new[] { x, x };

                layer.Forward(null, xs);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(Properties.Resources.E_InvalidInputTensor_InvalidCount).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Invalid number of input tensors.")]
        public void ForwardTest2()
        {
            int[] shape = new[] { 1, 20, 15, 10 };
            InputLayer layer = new InputLayer(shape);

            try
            {
                Tensor x = new Tensor(null, shape);
                layer.Forward(null, new[] { x, x });
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(Properties.Resources.E_InvalidInputTensor_InvalidCount, e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Invalid tensor rank.")]
        public void ForwardTest3()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer = new InputLayer(shape);

            try
            {
                Tensor x = new Tensor(null, new[] { 1, 2, 3 });
                layer.Forward(null, new[] { x });
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidInputTensor_InvalidRank,
                        3,
                        4)).Message,
                    e.Message);
                throw;
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        [Description("Invalid tensor dimension.")]
        public void ForwardTest4()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            InputLayer layer = new InputLayer(shape);

            try
            {
                Tensor x = new Tensor(null, new[] { 1, 30, 15, 10 });
                layer.Forward(null, new[] { x });
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(
                    new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Properties.Resources.E_InvalidInputTensor_InvalidDimension,
                        30,
                        1,
                        20)).Message,
                    e.Message);
                throw;
            }
        }
    }
}