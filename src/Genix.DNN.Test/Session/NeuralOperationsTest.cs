namespace Genix.DNN.Test
{
    using System;
    using System.Linq;
    using Genix.Core;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NeuralOperationsTest
    {
        private readonly RandomNumberGenerator<float> random = new RandomGenerator();

#if false
        [TestMethod]
        public void AddBiasTest1()
        {
            foreach (int length in new[] { 24, 128 })
            {
                Session session = new Session();

                Tensor x = new Tensor(null, 1, length);
                x.Randomize(random);

                Tensor b = new Tensor(null, new[] { length });
                b.Randomize(random);

                Tensor y1 = NeuralOperations.AddBias(session, x, b);
                Tensor y2 = NeuralOperations.AddBias(session, x, b);

                Tensor expected = new Tensor(null, 1, length);
                expected.Set(x.Weights.Zip(b.Weights, (xw, bw) => xw + bw).ToArray());
                Helpers.AreTensorsEqual(expected, y1);
                Helpers.AreTensorsEqual(expected, y2);

                Tensor dy1 = session.GetGradient(y1);
                Tensor dy2 = session.GetGradient(y2);
                dy1.Randomize(random);
                dy2.Randomize(random);
                session.Unroll();

                Tensor expectedDX = new Tensor(null, 1, length);
                expectedDX.Set(dy1.Weights.Zip(dy2.Weights, (dyw1, dyw2) => dyw1 + dyw2).ToArray());
                Helpers.AreTensorsEqual(expectedDX, session.GetGradient(x));

                Tensor expectedDB = new Tensor(null, new[] { length });
                expectedDB.Set(dy1.Weights.Zip(dy2.Weights, (dyw1, dyw2) => dyw1 + dyw2).ToArray());
                Helpers.AreTensorsEqual(expectedDB, session.GetGradient(b));
            }
        }

        [TestMethod]
        public void AddBiasTest2()
        {
            const int N = 3;

            foreach (int length in new[] { 24, 128 })
            {
                Session session = new Session();

                Tensor x = new Tensor(null, N, length);
                x.Randomize(random);

                Tensor b = new Tensor(null, new[] { length });
                b.Randomize(random);

                Tensor y1 = NeuralOperations.AddBias(session, x, b);
                Tensor y2 = NeuralOperations.AddBias(session, x, b);

                Tensor expected = new Tensor(null, N, length);
                expected.Set(x.Weights.Zip(session.Tile(b, 0, N).Weights, (xw, bw) => xw + bw).ToArray());
                Helpers.AreTensorsEqual(expected, y1);
                Helpers.AreTensorsEqual(expected, y2);

                Tensor dy1 = session.GetGradient(y1);
                Tensor dy2 = session.GetGradient(y2);
                dy1.Randomize(random);
                dy2.Randomize(random);
                session.Unroll();

                Tensor expectedDX = new Tensor(null, N, length);
                expectedDX.Set(dy1.Weights.Zip(dy2.Weights, (dyw1, dyw2) => dyw1 + dyw2).ToArray());
                Helpers.AreTensorsEqual(expectedDX, session.GetGradient(x));

                Tensor expectedDB = new Tensor(null, new[] { length });
                expectedDB.Set(ArrayOperations.Untile(session, dy1, 0, N)
                    .Weights
                    .Zip(ArrayOperations.Untile(session, dy2, 0, N).Weights, (dyw1, dyw2) => dyw1 + dyw2).ToArray());
                Helpers.AreTensorsEqual(expectedDB, session.GetGradient(b));
            }
        }

        [TestMethod]
        public void DropoutTest1()
        {
            const float P = 0.6f;

            Session session = new Session(false);

            Tensor x = new Tensor(null, 1024);
            x.Randomize(random);
            float xsum = x.Weights.Sum();

            Tensor y1 = NeuralOperations.Dropout(session, x, this.random, P);
            Tensor y2 = NeuralOperations.Dropout(session, x, this.random, P);

            Assert.AreEqual(P * xsum, y1.Weights.Sum(), 0.01f * xsum);
            Assert.AreEqual(P * xsum, y2.Weights.Sum(), 0.01f * xsum);

            Tensor dy1 = session.GetGradient(y1);
            Tensor dy2 = session.GetGradient(y2);
            dy1.Randomize(random);
            dy2.Randomize(random);
            session.Unroll();

            Assert.IsTrue(session.GetGradient(x).Weights.All(w => w == 0.0f));
        }
#endif

        [TestMethod]
        public void DropoutTest2()
        {
            const float P = 0.6f;

            Session session = new Session();

            Tensor x = new Tensor(null, new[] { 1024 });
            x.Randomize(this.random);
            float xsum = x.Weights.Sum();

            Tensor y1 = NeuralOperations.Dropout(session, x, this.random, P);
            Tensor y2 = NeuralOperations.Dropout(session, x, this.random, P);

            Assert.AreEqual(P * xsum, y1.Weights.Sum(), 0.02f * xsum);
            Assert.AreEqual(P * xsum, y2.Weights.Sum(), 0.02f * xsum);

            y1.RandomizeGradient(this.random);
            y2.RandomizeGradient(this.random);
            session.Unroll();

            Helpers.AreArraysEqual(
                y1.Weights.Zip(y1.Gradient, (yw, dyw) => yw == 0.0f ? 0.0f : dyw)
                          .Zip(y2.Weights.Zip(y2.Gradient, (yw, dyw) => yw == 0.0f ? 0.0f : dyw), (a, b) => a + b)
                          .ToArray(),
                x.Gradient);
        }
    }
}
