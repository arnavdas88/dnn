namespace Genix.MachineLearning.Test
{
    using System;
    using System.Linq;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MathOperationsTest
    {
        private readonly RandomNumberGenerator random = new RandomGenerator();

        [TestMethod]
        public void AddTest1()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor a = new Tensor(null, new[] { length });
                Tensor b = new Tensor(null, new[] { length });
                a.Randomize(this.random);
                b.Randomize(this.random);

                // simple multiplication
                Tensor y = MathOperations.Add(session, a, b);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    a.Weights.Zip(b.Weights, (aw, bw) => aw + bw).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreGradientsEqual(y, a);
                Helpers.AreGradientsEqual(y, b);
            }
        }

        [TestMethod]
        public void AddTest2()
        {
            const float alpha = (float)Math.PI;
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                // simple multiplication
                Tensor y = MathOperations.Add(session, x, alpha);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => xw + alpha).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(y.Gradient.Select(dyw => dyw).ToArray(), x.Gradient);
            }
        }

        [TestMethod]
        public void SubtractTest1()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor a = new Tensor(null, new[] { length });
                Tensor b = new Tensor(null, new[] { length });
                a.Randomize(this.random);
                b.Randomize(this.random);

                // simple multiplication
                Tensor y = MathOperations.Subtract(session, a, b);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    a.Weights.Zip(b.Weights, (aw, bw) => aw - bw).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(y.Gradient, a.Gradient);
                Helpers.AreArraysEqual(y.Gradient.Select(w => -w).ToArray(), b.Gradient);
            }
        }

        [TestMethod]
        public void MultiplyTest1()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                // simple multiplication
                Tensor y = MathOperations.Multiply(session, x, 2.0f);
                y.RandomizeGradient(this.random);

                Tensor expected = new Tensor(
                    null,
                    x.Axes,
                    x.Weights.Select(w => w * 2.0f).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                session.Unroll();

                Helpers.AreArraysEqual(y.Gradient.Select(w => 2.0f * w).ToArray(), x.Gradient);
            }
        }

        [TestMethod]
        public void MultiplyTest2()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor a = new Tensor(null, new[] { length });
                Tensor b = new Tensor(null, new[] { length });
                a.Randomize(this.random);
                b.Randomize(this.random);

                // simple multiplication
                Tensor y = MathOperations.Multiply(session, a, b);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    a.Weights.Zip(b.Weights, (aw, bw) => aw * bw).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y.Gradient.Zip(b.Weights, (yw, bw) => yw * bw).ToArray(),
                    a.Gradient);

                Helpers.AreArraysEqual(
                    y.Gradient.Zip(a.Weights, (yw, aw) => yw * aw).ToArray(),
                    b.Gradient);
            }
        }

        [TestMethod]
        public void SquareTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y = MathOperations.Square(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => xw * xw).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y.Gradient.Zip(x.Weights, (dyw, xw) => 2.0f * xw * dyw).ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void PowTest()
        {
            const float power = (float)Math.PI;
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y = MathOperations.Pow(session, x, power);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => (float)Math.Pow(xw, power)).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y.Gradient.Zip(x.Weights, (dyw, xw) => power * (float)Math.Pow(xw, power - 1) * dyw).ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void SqrtTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y = MathOperations.Sqrt(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => (float)Math.Sqrt(xw)).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y.Gradient.Zip(x.Weights, (dyw, xw) => (1 / (2.0f * (float)Math.Sqrt(xw))) * dyw).ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void AbsTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y = MathOperations.Abs(session, x);

                Tensor expected = new Tensor(
                    null,
                    y.Axes,
                    x.Weights.Select(xw => Math.Abs(xw)).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    x.Weights.Zip(y.Gradient, (xw, dyw) => xw >= 0.0f ? dyw : -dyw).ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void MaxTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor a = new Tensor(null, new[] { length });
                Tensor b = new Tensor(null, new[] { length });
                a.Randomize(this.random);
                b.Randomize(this.random);

                Tensor y = MathOperations.Max(session, a, b);

                Tensor expected = new Tensor(
                    null,
                    y.Axes,
                    a.Weights.Zip(b.Weights, (aw, bw) => Math.Max(aw, bw)).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    a.Weights.Zip(b.Weights, (aw, bw) => aw >= bw ? 1.0f : 0.0f)
                             .Zip(y.Gradient, (abw, dyw) => abw * dyw)
                             .ToArray(),
                    a.Gradient);

                Helpers.AreArraysEqual(
                    a.Weights.Zip(b.Weights, (aw, bw) => aw <= bw ? 1.0f : 0.0f)
                             .Zip(y.Gradient, (abw, dyw) => abw * dyw)
                             .ToArray(),
                    b.Gradient);
            }
        }

        [TestMethod]
        public void MinTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor a = new Tensor(null, new[] { length });
                Tensor b = new Tensor(null, new[] { length });
                a.Randomize(this.random);
                b.Randomize(this.random);

                Tensor y = MathOperations.Min(session, a, b);

                Tensor expected = new Tensor(
                    null,
                    y.Axes,
                    a.Weights.Zip(b.Weights, (aw, bw) => Math.Min(aw, bw)).ToArray());
                Helpers.AreTensorsEqual(expected, y);

                y.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    a.Weights.Zip(b.Weights, (aw, bw) => aw <= bw ? 1.0f : 0.0f)
                             .Zip(y.Gradient, (abw, dyw) => abw * dyw)
                             .ToArray(),
                    a.Gradient);

                Helpers.AreArraysEqual(
                    a.Weights.Zip(b.Weights, (aw, bw) => aw >= bw ? 1.0f : 0.0f)
                             .Zip(y.Gradient, (abw, dyw) => abw * dyw)
                             .ToArray(),
                    b.Gradient);
            }
        }

        [TestMethod]
        public void ReLUTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y1 = MathOperations.ReLU(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => Math.Max(xw, 0)).ToArray());
                Helpers.AreTensorsEqual(expected, y1);

                Tensor y2 = MathOperations.ReLU(session, x);

                y1.RandomizeGradient(this.random);
                y2.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    x.Weights.Zip(y1.Gradient, (xw, dyw) => xw > 0.0f ? dyw : 0.0f)
                             .Zip(x.Weights.Zip(y2.Gradient, (xw, dyw) => xw > 0.0f ? dyw : 0.0f), (a, b) => a + b)
                             .ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void TanhTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y1 = MathOperations.Tanh(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => Nonlinearity.Tanh(xw)).ToArray());
                Helpers.AreTensorsEqual(expected, y1);

                Tensor y2 = MathOperations.Tanh(session, x);

                y1.RandomizeGradient(this.random);
                y2.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y1.Weights.Zip(y1.Gradient, (yw, dyw) => Nonlinearity.TanhDerivative2(yw) * dyw)
                              .Zip(y2.Weights.Zip(y2.Gradient, (yw, dyw) => Nonlinearity.TanhDerivative2(yw) * dyw), (a, b) => a + b)
                              .ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void SigmoidTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y1 = MathOperations.Sigmoid(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => Nonlinearity.Sigmoid(xw)).ToArray());
                Helpers.AreTensorsEqual(expected, y1);

                Tensor y2 = MathOperations.Sigmoid(session, x);

                y1.RandomizeGradient(this.random);
                y2.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y1.Weights.Zip(y1.Gradient, (yw, dyw) => Nonlinearity.SigmoidDerivative2(yw) * dyw)
                              .Zip(y2.Weights.Zip(y2.Gradient, (yw, dyw) => Nonlinearity.SigmoidDerivative2(yw) * dyw), (a, b) => a + b)
                              .ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void SinTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y1 = MathOperations.Sin(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => (float)Math.Sin(xw)).ToArray());
                Helpers.AreTensorsEqual(expected, y1);

                Tensor y2 = MathOperations.Sin(session, x);

                y1.RandomizeGradient(this.random);
                y2.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    x.Weights.Zip(y1.Gradient, (xw, dyw) => (float)Math.Cos(xw) * dyw)
                              .Zip(x.Weights.Zip(y2.Gradient, (xw, dyw) => (float)Math.Cos(xw) * dyw), (a, b) => a + b)
                              .ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void CosTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { length });
                x.Randomize(this.random);

                Tensor y1 = MathOperations.Cos(session, x);

                Tensor expected = new Tensor(
                    null,
                    new[] { length },
                    x.Weights.Select(xw => (float)Math.Cos(xw)).ToArray());
                Helpers.AreTensorsEqual(expected, y1);

                Tensor y2 = MathOperations.Cos(session, x);

                y1.RandomizeGradient(this.random);
                y2.RandomizeGradient(this.random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    x.Weights.Zip(y1.Gradient, (xw, dyw) => -(float)Math.Sin(xw) * dyw)
                              .Zip(x.Weights.Zip(y2.Gradient, (xw, dyw) => -(float)Math.Sin(xw) * dyw), (a, b) => a + b)
                              .ToArray(),
                    x.Gradient);
            }
        }

        /// <summary>
        /// Column-major test.
        /// </summary>
        [TestMethod]
        public void VxVTest1()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor x = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });
            Tensor y = new Tensor(null, new[] { n }, new float[] { 4, 5 });

            // column-major
            // [1]
            // [2] x [4, 5]
            // [3]
            Tensor a = MathOperations.VxV(session, MatrixLayout.ColumnMajor, x, y);

            Tensor expected = new Tensor(
                null,
                new[] { n, m },
                new float[]
                {
                    (1 * 4),
                    (2 * 4),
                    (3 * 4),
                    (1 * 5),
                    (2 * 5),
                    (3 * 5)
                });
            Helpers.AreTensorsEqual(expected, a);

            a.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dx += dA * y
            // [1, 4]
            // [2, 5] x [4]
            // [3, 6]   [5]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 4) + (4 * 5),
                    (2 * 4) + (5 * 5),
                    (3 * 4) + (6 * 5)
                },
                x.Gradient);

            // dy += dA' * x
            // [1, 2, 3]   [1]
            // [4, 5, 6] x [2]
            //             [3]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (2 * 2) + (3 * 3),
                    (4 * 1) + (5 * 2) + (6 * 3)
                },
                y.Gradient);
        }

        /// <summary>
        /// Row-major test.
        /// </summary>
        [TestMethod]
        public void VxVTest2()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor x = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });
            Tensor y = new Tensor(null, new[] { n }, new float[] { 4, 5 });

            // row-major
            // [1]
            // [2] x [4, 5]
            // [3]
            Tensor a = MathOperations.VxV(session, MatrixLayout.RowMajor, x, y);

            Tensor expected = new Tensor(
                null,
                new[] { m, n },
                new float[]
                {
                    (1 * 4),
                    (1 * 5),
                    (2 * 4),
                    (2 * 5),
                    (3 * 4),
                    (3 * 5)
                });
            Helpers.AreTensorsEqual(expected, a);

            a.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dx += dA * y
            // [1, 2]
            // [3, 4] x [4]
            // [5, 6]   [5]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 4) + (2 * 5),
                    (3 * 4) + (4 * 5),
                    (5 * 4) + (6 * 5)
                },
                x.Gradient);

            // dy += dA' * x
            // [1, 3, 5]   [1]
            // [2, 4, 6] x [2]
            //             [3]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (3 * 2) + (5 * 3),
                    (2 * 1) + (4 * 2) + (6 * 3)
                },
                y.Gradient);
        }

        /// <summary>
        /// Column-major tests, no transpose.
        /// </summary>
        [TestMethod]
        public void MxVTest1()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { n, m }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { n }, new float[] { 7, 8 });

            // y = A * x
            // [1, 4]
            // [2, 5] x [7]
            // [3, 6]   [8]
            Tensor y = MathOperations.MxV(session, MatrixLayout.ColumnMajor, a, false, x, null);

            Tensor expected = new Tensor(
                null,
                new[] { m },
                new float[]
                {
                    (1 * 7) + (4 * 8),
                    (2 * 7) + (5 * 8),
                    (3 * 7) + (6 * 8)
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dy * x'
            // [1]
            // [2] x [7]'
            // [3]   [8]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 7),
                    (2 * 7),
                    (3 * 7),
                    (1 * 8),
                    (2 * 8),
                    (3 * 8)
                },
                a.Gradient);

            // dx += A' * dy
            // [1, 4]'   [1]
            // [2, 5]  x [2]
            // [3, 6]    [3]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (2 * 2) + (3 * 3),
                    (4 * 1) + (5 * 2) + (6 * 3)
                },
                x.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A.
        /// </summary>
        [TestMethod]
        public void MxVTest2()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { n, m }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { m }, new float[] { 7, 8, 9 });

            // y = A' * x
            // [1, 4]'   [7]
            // [2, 5]  x [8]
            // [3, 6]    [9]
            Tensor y = MathOperations.MxV(session, MatrixLayout.ColumnMajor, a, true, x, null);

            Tensor expected = new Tensor(
                null,
                new[] { n },
                new float[]
                {
                    (1 * 7) + (2 * 8) + (3 * 9),
                    (4 * 7) + (5 * 8) + (6 * 9)
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2 });
            session.Unroll();

            // dA += (dy * x')' = x * dy'
            // [7]
            // [8] x [1]'
            // [9]   [2]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (7 * 1),
                    (8 * 1),
                    (9 * 1),
                    (7 * 2),
                    (8 * 2),
                    (9 * 2)
                },
                a.Gradient);

            // dx += A * dy
            // [1, 4]
            // [2, 5] x [1]
            // [3, 6]   [2]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (4 * 2),
                    (2 * 1) + (5 * 2),
                    (3 * 1) + (6 * 2),
                },
                x.Gradient);
        }

        /// <summary>
        /// Row-major tests, no transpose.
        /// </summary>
        [TestMethod]
        public void MxVTest3()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, n }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { n }, new float[] { 7, 8 });

            // y = A * x
            // [1, 2]
            // [3, 4] x [7]
            // [5, 6]   [8]
            Tensor y = MathOperations.MxV(session, MatrixLayout.RowMajor, a, false, x, null);

            Tensor expected = new Tensor(
                null,
                new[] { m },
                new float[]
                {
                    (1 * 7) + (2 * 8),
                    (3 * 7) + (4 * 8),
                    (5 * 7) + (6 * 8)
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dy * x'
            // [1]
            // [2] x [7]'
            // [3]   [8]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 7),
                    (1 * 8),
                    (2 * 7),
                    (2 * 8),
                    (3 * 7),
                    (3 * 8)
                },
                a.Gradient);

            // dx += A' * dy
            // [1, 2]'   [1]
            // [3, 4]  x [2]
            // [5, 6]    [3]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (3 * 2) + (5 * 3),
                    (2 * 1) + (4 * 2) + (6 * 3)
                },
                x.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A.
        /// </summary>
        [TestMethod]
        public void MxVTest4()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, n }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { m }, new float[] { 7, 8, 9 });

            // y = A' * x
            // [1, 2]'   [7]
            // [3, 4]  x [8]
            // [5, 6]    [9]
            Tensor y = MathOperations.MxV(session, MatrixLayout.RowMajor, a, true, x, null);

            Tensor expected = new Tensor(
                null,
                new[] { n },
                new float[]
                {
                    (1 * 7) + (3 * 8) + (5 * 9),
                    (2 * 7) + (4 * 8) + (6 * 9),
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2 });
            session.Unroll();

            // dA += x * dy'
            // [7]   [1]'
            // [8] x [2]
            // [9]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (7 * 1),
                    (7 * 2),
                    (8 * 1),
                    (8 * 2),
                    (9 * 1),
                    (9 * 2)
                },
                a.Gradient);

            // dx += A * dy
            // [1, 2]   [1]
            // [3, 4] x [2]
            // [5, 6]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (2 * 2),
                    (3 * 1) + (4 * 2),
                    (5 * 1) + (6 * 2),
                },
                x.Gradient);
        }

        /// <summary>
        /// Column-major tests, no transpose, add bias.
        /// </summary>
        [TestMethod]
        public void MxVaddBiasTest1()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { n, m }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { n }, new float[] { 7, 8 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // y = A * x
            // [1, 4]         [1]
            // [2, 5] x [7] + [2]
            // [3, 6]   [8]   [3]
            Tensor y = MathOperations.MxV(session, MatrixLayout.ColumnMajor, a, false, x, bias);

            Tensor expected = new Tensor(
                null,
                new[] { m },
                new float[]
                {
                    (1 * 7) + (4 * 8) + 1,
                    (2 * 7) + (5 * 8) + 2,
                    (3 * 7) + (6 * 8) + 3
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dy * x'
            // [1]
            // [2] x [7]'
            // [3]   [8]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 7),
                    (2 * 7),
                    (3 * 7),
                    (1 * 8),
                    (2 * 8),
                    (3 * 8)
                },
                a.Gradient);

            // dx += A' * dy
            // [1, 4]'   [1]
            // [2, 5]  x [2]
            // [3, 6]    [3]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (2 * 2) + (3 * 3),
                    (4 * 1) + (5 * 2) + (6 * 3)
                },
                x.Gradient);

            // db += dy
            Helpers.AreArraysEqual(y.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A, add bias.
        /// </summary>
        [TestMethod]
        public void MxVaddBiasTest2()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { n, m }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { m }, new float[] { 7, 8, 9 });
            Tensor bias = new Tensor(null, new[] { n }, new float[] { 1, 2 });

            // y = A' * x
            // [1, 4]'   [7]
            // [2, 5]  x [8] + [1]
            // [3, 6]    [9]   [2]
            Tensor y = MathOperations.MxV(session, MatrixLayout.ColumnMajor, a, true, x, bias);

            Tensor expected = new Tensor(
                null,
                new[] { n },
                new float[]
                {
                    (1 * 7) + (2 * 8) + (3 * 9) + 1,
                    (4 * 7) + (5 * 8) + (6 * 9) + 2
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2 });
            session.Unroll();

            // dA += (dy * x')' = x * dy'
            // [7]
            // [8] x [1]'
            // [9]   [2]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (7 * 1),
                    (8 * 1),
                    (9 * 1),
                    (7 * 2),
                    (8 * 2),
                    (9 * 2)
                },
                a.Gradient);

            // dx += A * dy
            // [1, 4]
            // [2, 5] x [1]
            // [3, 6]   [2]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (4 * 2),
                    (2 * 1) + (5 * 2),
                    (3 * 1) + (6 * 2),
                },
                x.Gradient);

            // db += dy
            Helpers.AreArraysEqual(y.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, no transpose, add bias.
        /// </summary>
        [TestMethod]
        public void MxVaddBiasTest3()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, n }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { n }, new float[] { 7, 8 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // y = A * x
            // [1, 2]         [1]
            // [3, 4] x [7] + [2]
            // [5, 6]   [8]   [3]
            Tensor y = MathOperations.MxV(session, MatrixLayout.RowMajor, a, false, x, bias);

            Tensor expected = new Tensor(
                null,
                new[] { m },
                new float[]
                {
                    (1 * 7) + (2 * 8) + 1,
                    (3 * 7) + (4 * 8) + 2,
                    (5 * 7) + (6 * 8) + 3
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dy * x'
            // [1]
            // [2] x [7]'
            // [3]   [8]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 7),
                    (1 * 8),
                    (2 * 7),
                    (2 * 8),
                    (3 * 7),
                    (3 * 8)
                },
                a.Gradient);

            // dx += A' * dy
            // [1, 2]'   [1]
            // [3, 4]  x [2]
            // [5, 6]    [3]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (3 * 2) + (5 * 3),
                    (2 * 1) + (4 * 2) + (6 * 3)
                },
                x.Gradient);

            // db += dy
            Helpers.AreArraysEqual(y.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A, add bias.
        /// </summary>
        [TestMethod]
        public void MxVaddBiasTest4()
        {
            const int m = 3;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, n }, new float[] { 1, 2, 3, 4, 5, 6 });
            Tensor x = new Tensor(null, new[] { m }, new float[] { 7, 8, 9 });
            Tensor bias = new Tensor(null, new[] { n }, new float[] { 1, 2 });

            // y = A' * x
            // [1, 2]'   [7]
            // [3, 4]  x [8] + [1]
            // [5, 6]    [9]   [2]
            Tensor y = MathOperations.MxV(session, MatrixLayout.RowMajor, a, true, x, bias);

            Tensor expected = new Tensor(
                null,
                new[] { n },
                new float[]
                {
                    (1 * 7) + (3 * 8) + (5 * 9) + 1,
                    (2 * 7) + (4 * 8) + (6 * 9) + 2
                });
            Helpers.AreTensorsEqual(expected, y);

            y.SetGradient(new float[] { 1, 2 });
            session.Unroll();

            // dA += x * dy'
            // [7]   [1]'
            // [8] x [2]
            // [9]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (7 * 1),
                    (7 * 2),
                    (8 * 1),
                    (8 * 2),
                    (9 * 1),
                    (9 * 2)
                },
                a.Gradient);

            // dx += A * dy
            // [1, 2]   [1]
            // [3, 4] x [2]
            // [5, 6]
            Helpers.AreArraysEqual(
                new float[]
                {
                    (1 * 1) + (2 * 2),
                    (3 * 1) + (4 * 2),
                    (5 * 1) + (6 * 2),
                },
                x.Gradient);

            // db += dy
            Helpers.AreArraysEqual(y.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, no transpose.
        /// </summary>
        [TestMethod]
        public void MxMTest1()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A * B
            // [1, 4, 7, 10]   [21, 25]
            // [2, 5, 8, 11] x [22, 26]
            // [3, 6, 9, 12]   [23, 27]
            //                 [24, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, false, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B'
            // [1, 4]   [21, 25]'
            // [2, 5] x [22, 26]
            // [3, 6]   [23, 27]
            //          [24, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 4, 7, 10]'   [1, 4]
            // [2, 5, 8, 11]  x [2, 5]
            // [3, 6, 9, 12]    [3, 6]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A.
        /// </summary>
        [TestMethod]
        public void MxMTest2()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A' * B
            // [1, 5, 9 ]'   [21, 25]
            // [2, 6, 10]  x [22, 26]
            // [3, 7, 11]    [23, 27]
            // [4, 8, 12]    [24, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, false, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B * dC'
            // [21, 25]   [1, 4]'
            // [22, 26] x [2, 5]
            // [23, 27]   [3, 6]
            // [24, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1, 5, 9 ]   [1, 4]
            // [2, 6, 10] x [2, 5]
            // [3, 7, 11]   [3, 6]
            // [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose B.
        /// </summary>
        [TestMethod]
        public void MxMTest3()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A * B'
            // [1, 4, 7, 10]   [21, 23, 25, 27]'
            // [2, 5, 8, 11] x [22, 24, 26, 28]
            // [3, 6, 9, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, true, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B
            // [1, 4]   [21, 23, 25, 27]
            // [2, 5] x [22, 24, 26, 28]
            // [3, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1, 4]'  [1, 4, 7, 10]
            // [2, 5] x [2, 5, 8, 11]
            // [3, 6]   [3, 6, 9, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A and B.
        /// </summary>
        [TestMethod]
        public void MxMTest4()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A' * B'
            // [1, 5, 9 ]'   [21, 23, 25, 27]'
            // [2, 6, 10]  x [22, 24, 26, 28]
            // [3, 7, 11]
            // [4, 8, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, true, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B' * dC'
            // [21, 23, 25, 27]'  [1, 4]'
            // [22, 24, 26, 28] x [2, 5]
            //                    [3, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1, 4]'   [1, 5, 9 ]'
            // [2, 5]  x [2, 6, 10]
            // [3, 6]    [3, 7, 11]
            //           [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, no transpose.
        /// </summary>
        [TestMethod]
        public void MxMTest5()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A * B
            // [1, 2,  3,  4 ]   [21, 22]
            // [5, 6,  7,  8 ] x [23, 24]
            // [9, 10, 11, 12]   [25, 26]
            //                   [27, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, false, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B'
            // [1, 2]   [21, 22]'
            // [3, 4] x [23, 24]
            // [5, 6]   [25, 26]
            //          [27, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 2,  3,  4 ]'  [1, 2]
            // [5, 6,  7,  8 ] x [3, 4]
            // [9, 10, 11, 12]   [5, 6]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A.
        /// </summary>
        [TestMethod]
        public void MxMTest6()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A' * B
            // [1,  2,  3 ]'   [21, 22]
            // [4,  5,  6 ]  x [23, 24]
            // [7,  8,  9 ]    [25, 26]
            // [10, 11, 12]    [27, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, false, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B * dC'
            // [21, 22]   [1, 2]'
            // [23, 24] x [3, 4]
            // [25, 26]   [5, 6]
            // [27, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1,  2,  3 ]   [1, 2]
            // [4,  5,  6 ] x [3, 4]
            // [7,  8,  9 ]   [5, 6]
            // [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose B.
        /// </summary>
        [TestMethod]
        public void MxMTest7()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A * B'
            // [1, 2,  3,  4 ]   [21, 22, 23, 24]'
            // [5, 6,  7,  8 ] x [25, 26, 27, 28]
            // [9, 10, 11, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, true, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B
            // [1, 2]   [21, 22, 23, 24]
            // [3, 4] x [25, 26, 27, 28]
            // [5, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1, 2]'  [1, 2,  3,  4 ]
            // [3, 4] x [5, 6,  7,  8 ]
            // [5, 6]   [9, 10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A and B.
        /// </summary>
        [TestMethod]
        public void MxMTest8()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });

            // C = A' * B'
            // [1,  2,  3 ]'   [21, 22, 23, 24]'
            // [4,  5,  6 ]  x [25, 26, 27, 28]
            // [7,  8,  9 ]
            // [10, 11, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, true, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B' * dC'
            // [21, 22, 23, 24]'  [1, 2]'
            // [25, 26, 27, 28] x [3, 4]
            //                    [5, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1, 2]'  [1,  2,  3 ]'
            // [3, 4]  x[4,  5,  6 ]
            // [5, 6]   [7,  8,  9 ]
            //          [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, no transpose, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest11()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });

            // C = A * B
            // [1, 4, 7, 10]   [21]
            // [2, 5, 8, 11] x [22]
            // [3, 6, 9, 12]   [23]
            //                 [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, false, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B'
            // [1]   [21]'
            // [2] x [22]
            // [3]   [23]
            //          [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 4, 7, 10]'   [1]
            // [2, 5, 8, 11]  x [2]
            // [3, 6, 9, 12]    [3]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest12()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });

            // C = A' * B
            // [1, 5, 9 ]'   [21]
            // [2, 6, 10]  x [22]
            // [3, 7, 11]    [23]
            // [4, 8, 12]    [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, false, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B * dC'
            // [21]   [1]'
            // [22] x [2]
            // [23]   [3]
            // [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1, 5, 9 ]   [1]
            // [2, 6, 10] x [2]
            // [3, 7, 11]   [3]
            // [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose B, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest13()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });

            // C = A * B'
            // [1, 4, 7, 10]
            // [2, 5, 8, 11] x [21, 22, 23, 24]'
            // [3, 6, 9, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, true, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B
            // [1]
            // [2] x [21, 22, 23, 24]
            // [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1]'  [1, 4, 7, 10]
            // [2] x [2, 5, 8, 11]
            // [3]   [3, 6, 9, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A and B, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest14()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });

            // C = A' * B'
            // [1, 5, 9 ]'
            // [2, 6, 10]  x [21, 22, 23, 24]'
            // [3, 7, 11]
            // [4, 8, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, true, null);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B' * dC'
            //                     [1]'
            // [21, 22, 23, 24]' x [2]
            //                     [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1]'   [1, 5, 9 ]'
            // [2]  x [2, 6, 10]
            // [3]    [3, 7, 11]
            //        [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, no transpose, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest15()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });

            // C = A * B
            // [1, 2,  3,  4 ]   [21]
            // [5, 6,  7,  8 ] x [22]
            // [9, 10, 11, 12]   [23]
            //                   [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, false, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B'
            // [1]   [21]'
            // [2] x [22]
            // [3]   [23]
            //       [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 2,  3,  4 ]'  [1]
            // [5, 6,  7,  8 ] x [2]
            // [9, 10, 11, 12]   [3]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest16()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });

            // C = A' * B
            // [1,  2,  3 ]'   [21]
            // [4,  5,  6 ]  x [22]
            // [7,  8,  9 ]    [23]
            // [10, 11, 12]    [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, false, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B * dC'
            // [21]   [1]'
            // [22] x [2]
            // [23]   [3]
            // [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1,  2,  3 ]   [1]
            // [4,  5,  6 ] x [2]
            // [7,  8,  9 ]   [3]
            // [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose B, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest17()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });

            // C = A * B'
            // [1, 2,  3,  4 ]
            // [5, 6,  7,  8 ] x [21, 22, 23, 24]'
            // [9, 10, 11, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, true, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B
            // [1]
            // [2] x [21, 22, 23, 24]
            // [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1]'  [1, 2,  3,  4 ]
            // [2] x [5, 6,  7,  8 ]
            // [3]   [9, 10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A and B, N = 1.
        /// </summary>
        [TestMethod]
        public void MxMTest18()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });

            // C = A' * B'
            // [1,  2,  3 ]'
            // [4,  5,  6 ]  x [21, 22, 23, 24]'
            // [7,  8,  9 ]
            // [10, 11, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, true, null);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, true);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B' * dC'
            //                     [1]'
            // [21, 22, 23, 24]' x [2]
            //                     [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1]'   [1,  2,  3 ]'
            // [2]  x [4,  5,  6 ]
            // [3]    [7,  8,  9 ]
            //        [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);
        }

        /// <summary>
        /// Column-major tests, no transpose, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest1()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B
            // [1, 4, 7, 10]   [21, 25]   [1, 1]
            // [2, 5, 8, 11] x [22, 26] + [2, 2]
            // [3, 6, 9, 12]   [23, 27]   [3, 3]
            //                 [24, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, false, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B'
            // [1, 4]   [21, 25]'
            // [2, 5] x [22, 26]
            // [3, 6]   [23, 27]
            //          [24, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 4, 7, 10]'   [1, 4]
            // [2, 5, 8, 11]  x [2, 5]
            // [3, 6, 9, 12]    [3, 6]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 0, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest2()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B
            // [1, 5, 9 ]'   [21, 25]   [1, 1]
            // [2, 6, 10]  x [22, 26] + [2, 2]
            // [3, 7, 11]    [23, 27]   [3, 3]
            // [4, 8, 12]    [24, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, false, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B * dC'
            // [21, 25]   [1, 4]'
            // [22, 26] x [2, 5]
            // [23, 27]   [3, 6]
            // [24, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1, 5, 9 ]   [1, 4]
            // [2, 6, 10] x [2, 5]
            // [3, 7, 11]   [3, 6]
            // [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 0, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose B, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest3()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B'
            // [1, 4, 7, 10]   [21, 23, 25, 27]'   [1, 1]
            // [2, 5, 8, 11] x [22, 24, 26, 28]  + [2, 2]
            // [3, 6, 9, 12]                       [3, 3]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, true, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B
            // [1, 4]   [21, 23, 25, 27]
            // [2, 5] x [22, 24, 26, 28]
            // [3, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1, 4]'  [1, 4, 7, 10]
            // [2, 5] x [2, 5, 8, 11]
            // [3, 6]   [3, 6, 9, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 0, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A and B, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest4()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B'
            // [1, 5, 9 ]'   [21, 23, 25, 27]'   [1, 1]
            // [2, 6, 10]  x [22, 24, 26, 28]  + [2, 2]
            // [3, 7, 11]                        [3, 3]
            // [4, 8, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, true, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B' * dC'
            // [21, 23, 25, 27]'  [1, 4]'
            // [22, 24, 26, 28] x [2, 5]
            //                    [3, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1, 4]'   [1, 5, 9 ]'
            // [2, 5]  x [2, 6, 10]
            // [3, 6]    [3, 7, 11]
            //           [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 0, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, no transpose, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest5()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B
            // [1, 2,  3,  4 ]   [21, 22]   [1, 1]
            // [5, 6,  7,  8 ] x [23, 24] + [2, 2]
            // [9, 10, 11, 12]   [25, 26]   [3, 3]
            //                   [27, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, false, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B'
            // [1, 2]   [21, 22]'
            // [3, 4] x [23, 24]
            // [5, 6]   [25, 26]
            //          [27, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 2,  3,  4 ]'  [1, 2]
            // [5, 6,  7,  8 ] x [3, 4]
            // [9, 10, 11, 12]   [5, 6]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 1, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest6()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B
            // [1,  2,  3 ]'   [21, 22]   [1, 1]
            // [4,  5,  6 ]  x [23, 24] + [2, 2]
            // [7,  8,  9 ]    [25, 26]   [3, 3]
            // [10, 11, 12]    [27, 28]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, false, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B * dC'
            // [21, 22]   [1, 2]'
            // [23, 24] x [3, 4]
            // [25, 26]   [5, 6]
            // [27, 28]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1,  2,  3 ]   [1, 2]
            // [4,  5,  6 ] x [3, 4]
            // [7,  8,  9 ]   [5, 6]
            // [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 1, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose B, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest7()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B'
            // [1, 2,  3,  4 ]   [21, 22, 23, 24]'   [1, 1]
            // [5, 6,  7,  8 ] x [25, 26, 27, 28]  + [2, 2]
            // [9, 10, 11, 12]                       [3, 3]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, true, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += dC * B
            // [1, 2]   [21, 22, 23, 24]
            // [3, 4] x [25, 26, 27, 28]
            // [5, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1, 2]'  [1, 2,  3,  4 ]
            // [3, 4] x [5, 6,  7,  8 ]
            // [5, 6]   [9, 10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 1, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A and B, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest8()
        {
            const int m = 3;
            const int k = 4;
            const int n = 2;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24, 25, 26, 27, 28 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B'
            // [1,  2,  3 ]'   [21, 22, 23, 24]'   [1, 1]
            // [4,  5,  6 ]  x [25, 26, 27, 28]  + [2, 2]
            // [7,  8,  9 ]                        [3, 3]
            // [10, 11, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, true, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Arrays.Tile(m, n, bias.Weights, 0, expected.Weights, 0);
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, false);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3, 4, 5, 6 });
            session.Unroll();

            // dA += B' * dC'
            // [21, 22, 23, 24]'  [1, 2]'
            // [25, 26, 27, 28] x [3, 4]
            //                    [5, 6]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1, 2]'  [1,  2,  3 ]'
            // [3, 4]  x[4,  5,  6 ]
            // [5, 6]   [7,  8,  9 ]
            //          [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // dbias += sum(dC) by column
            float[] expectedDbias = ArrayOperations.Untile(session, new Tensor(null, c.Axes, c.Gradient), 1, n).Weights;
            Helpers.AreArraysEqual(expectedDbias, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, no transpose, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest11()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B
            // [1, 4, 7, 10]   [21]   [1]
            // [2, 5, 8, 11] x [22] + [2]
            // [3, 6, 9, 12]   [23]   [3]
            //                 [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, false, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B'
            // [1]   [21]'
            // [2] x [22]
            // [3]   [23]
            //          [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 4, 7, 10]'   [1]
            // [2, 5, 8, 11]  x [2]
            // [3, 6, 9, 12]    [3]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest12()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B
            // [1, 5, 9 ]'   [21]   [1]
            // [2, 6, 10]  x [22] + [2]
            // [3, 7, 11]    [23]   [3]
            // [4, 8, 12]    [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, false, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B * dC'
            // [21]   [1]'
            // [22] x [2]
            // [23]   [3]
            // [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1, 5, 9 ]   [1]
            // [2, 6, 10] x [2]
            // [3, 7, 11]   [3]
            // [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose B, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest13()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B'
            // [1, 4, 7, 10]                       [1]
            // [2, 5, 8, 11] x [21, 22, 23, 24]' + [2]
            // [3, 6, 9, 12]                       [3]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, false, b, true, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B
            // [1]
            // [2] x [21, 22, 23, 24]
            // [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1]'  [1, 4, 7, 10]
            // [2] x [2, 5, 8, 11]
            // [3]   [3, 6, 9, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Column-major tests, transpose A and B, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest14()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B'
            // [1, 5, 9 ]'                       [1]
            // [2, 6, 10]  x [21, 22, 23, 24]' + [2]
            // [3, 7, 11]                        [3]
            // [4, 8, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.ColumnMajor, a, true, b, true, bias);

            Tensor expected = new Tensor(null, new[] { n, m });
            Matrix.MxM(MatrixLayout.ColumnMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B' * dC'
            //                     [1]'
            // [21, 22, 23, 24]' x [2]
            //                     [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1]'   [1, 5, 9 ]'
            // [2]  x [2, 6, 10]
            // [3]    [3, 7, 11]
            //        [4, 8, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.ColumnMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, no transpose, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest15()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B
            // [1, 2,  3,  4 ]   [21]   [1]
            // [5, 6,  7,  8 ] x [22] + [2]
            // [9, 10, 11, 12]   [23]   [3]
            //                   [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, false, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, false, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B'
            // [1]   [21]'
            // [2] x [22]
            // [3]   [23]
            //       [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A' * dC
            // [1, 2,  3,  4 ]'  [1]
            // [5, 6,  7,  8 ] x [2]
            // [9, 10, 11, 12]   [3]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, true, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest16()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { k, n }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B
            // [1,  2,  3 ]'   [21]   [1]
            // [4,  5,  6 ]  x [22] + [2]
            // [7,  8,  9 ]    [23]   [3]
            // [10, 11, 12]    [24]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, false, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, false, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B * dC'
            // [21]   [1]'
            // [22] x [2]
            // [23]   [3]
            // [24]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, false, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += A * dC
            // [1,  2,  3 ]   [1]
            // [4,  5,  6 ] x [2]
            // [7,  8,  9 ]   [3]
            // [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, m, n, a.Weights, 0, false, c.Gradient, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose B, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest17()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { m, k }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A * B'
            // [1, 2,  3,  4 ]                       [1]
            // [5, 6,  7,  8 ] x [21, 22, 23, 24]' + [2]
            // [9, 10, 11, 12]                       [3]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, false, b, true, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, false, b.Weights, 0, true, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += dC * B
            // [1]
            // [2] x [21, 22, 23, 24]
            // [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, m, n, k, c.Gradient, 0, false, b.Weights, 0, false, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A
            // [1]'  [1, 2,  3,  4 ]
            // [2] x [5, 6,  7,  8 ]
            // [3]   [9, 10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, false, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }

        /// <summary>
        /// Row-major tests, transpose A and B, N = 1, add bias.
        /// </summary>
        [TestMethod]
        public void MxMAddBiasTest18()
        {
            const int m = 3;
            const int k = 4;
            const int n = 1;

            Session session = new Session();

            Tensor a = new Tensor(null, new[] { k, m }, new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Tensor b = new Tensor(null, new[] { n, k }, new float[] { 21, 22, 23, 24 });
            Tensor bias = new Tensor(null, new[] { m }, new float[] { 1, 2, 3 });

            // C = A' * B'
            // [1,  2,  3 ]'                       [1]
            // [4,  5,  6 ]  x [21, 22, 23, 24]' + [2]
            // [7,  8,  9 ]                        [3]
            // [10, 11, 12]
            Tensor c = MathOperations.MxM(session, MatrixLayout.RowMajor, a, true, b, true, bias);

            Tensor expected = new Tensor(null, new[] { m, n });
            Matrix.MxM(MatrixLayout.RowMajor, m, k, n, a.Weights, 0, true, b.Weights, 0, true, expected.Weights, 0, true);
            Mathematics.Add(m, bias.Weights, 0, expected.Weights, 0);
            Helpers.AreTensorsEqual(expected, c);

            c.SetGradient(new float[] { 1, 2, 3 });
            session.Unroll();

            // dA += B' * dC'
            //                     [1]'
            // [21, 22, 23, 24]' x [2]
            //                     [3]
            float[] expectedDA = new float[a.Length];
            Matrix.MxM(MatrixLayout.RowMajor, k, n, m, b.Weights, 0, true, c.Gradient, 0, true, expectedDA, 0, true);
            Helpers.AreArraysEqual(expectedDA, a.Gradient);

            // dB += dC' * A'
            // [1]'   [1,  2,  3 ]'
            // [2]  x [4,  5,  6 ]
            // [3]    [7,  8,  9 ]
            //        [10, 11, 12]
            float[] expectedDB = new float[b.Length];
            Matrix.MxM(MatrixLayout.RowMajor, n, m, k, c.Gradient, 0, true, a.Weights, 0, true, expectedDB, 0, true);
            Helpers.AreArraysEqual(expectedDB, b.Gradient);

            // db += dC
            Helpers.AreArraysEqual(c.Gradient, bias.Gradient);
        }
    }
}
