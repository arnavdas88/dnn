﻿namespace Accord.DNN.Test
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Layers;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [TestClass]
    public class Map2SequenceLayerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest1()
        {
            Assert.IsNotNull(new Map2SequenceLayer((int[])null));
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            Map2SequenceLayer layer = new Map2SequenceLayer(shape);

            CollectionAssert.AreEqual(new[] { 20, 150 }, layer.OutputShape);
            Assert.AreEqual("M2S", layer.Architecture);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyConstructorTest1()
        {
            Assert.IsNotNull(new Map2SequenceLayer((Map2SequenceLayer)null));
        }

        [TestMethod]
        public void CopyConstructorTest2()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            Map2SequenceLayer layer1 = new Map2SequenceLayer(shape);
            Map2SequenceLayer layer2 = new Map2SequenceLayer(layer1);
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void CloneTest()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            Map2SequenceLayer layer1 = new Map2SequenceLayer(shape);
            Map2SequenceLayer layer2 = layer1.Clone() as Map2SequenceLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(layer1), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            int[] shape = new[] { -1, 20, 15, 10 };
            Map2SequenceLayer layer1 = new Map2SequenceLayer(shape);
            string s1 = JsonConvert.SerializeObject(layer1);
            Map2SequenceLayer layer2 = JsonConvert.DeserializeObject<Map2SequenceLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void ForwardBackwardTest1()
        {
            Session session = new Session();

            int[] shape = new[] { 1, 20, 15, 10 };
            Map2SequenceLayer layer = new Map2SequenceLayer(shape);

            Tensor x = new Tensor(null, shape);
            x.Randomize();

            Tensor y = layer.Forward(session, new[] { x })[0];
            Helpers.AreArraysEqual(x.Weights, y.Weights);

            // unroll the graph
            y.SetGradient(y.Weights);
            session.Unroll();

            Helpers.AreArraysEqual(x.Weights, x.Gradient);
        }
    }
}