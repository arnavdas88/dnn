namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Genix.DNN;
    using Genix.DNN.Layers;
    using Newtonsoft.Json;

    [TestClass]
    public class DropoutLayerTest
    {
        int[] shape;
        DropoutLayer layer;

        [TestInitialize]
        public void BeforeEach()
        {
            this.shape = new[] { -1, 20, 20, 10 };
            this.layer = new DropoutLayer(this.shape, 0.5);
        }

        [TestMethod]
        public void ConstructorTest1()
        {
            CollectionAssert.AreEqual(this.shape, this.layer.OutputShape);
            Assert.AreEqual("D0.5", this.layer.Architecture);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            DropoutLayer layer2 = new DropoutLayer(this.layer);
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest3()
        {
            Assert.IsNotNull(new DropoutLayer((int[])null, 0.5));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest4()
        {
            Assert.IsNotNull(new DropoutLayer((DropoutLayer)null));
        }

        [TestMethod]
        public void CloneTest()
        {
            DropoutLayer layer2 = this.layer.Clone() as DropoutLayer;
            Assert.AreEqual(JsonConvert.SerializeObject(this.layer), JsonConvert.SerializeObject(layer2));
        }

        [TestMethod]
        public void SerializeTest()
        {
            string s1 = JsonConvert.SerializeObject(this.layer);
            DropoutLayer layer2 = JsonConvert.DeserializeObject<DropoutLayer>(s1);
            string s2 = JsonConvert.SerializeObject(layer2);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [Description("Shall multiply all weights by probability.")]
        public void ForwardTest1()
        {
            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session(false);

                Tensor x = new Tensor(null, Shape.Reshape(this.shape, (int)Axis.B, i));
                x.Randomize();

                IList<Tensor> xs = new[] { x };
                IList<Tensor> ys = this.layer.Forward(session, xs);

                Assert.AreEqual(x.Weights.Sum() * this.layer.Probability, ys[0].Weights.Sum());
            }
        }

        [TestMethod]
        [Description("Shall drop some weights based on probability.")]
        public void ForwardBackwardTest2()
        {
            for (int i = 1; i <= 3; i++)
            {
                Session session = new Session();

                Tensor x = new Tensor(null, Shape.Reshape(this.shape, (int)Axis.B, i));
                x.Randomize();

                Tensor y = this.layer.Forward(session, new[] { x })[0];

                Assert.AreEqual(0.0f, y.Weights.Sum(), 1.0f);
                Assert.AreEqual((int)(y.Length * this.layer.Probability), y.Weights.Count(w => w == 0.0f), y.Length / 50);

                // unroll the graph
                y.SetGradient(1.0f);
                session.Unroll();

                CollectionAssert.AreEqual(y.Weights.Select(w => w == 0.0f ? 0.0f : 1.0f).ToArray(), x.Gradient);
            }
        }
    }
}
