namespace Genix.DNN.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Genix.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ArrayOperationsTest
    {
        private const float Eps = 1e-6f;
        private readonly RandomNumberGenerator random = new RandomGenerator();

        [TestMethod]
        public void CopyTest()
        {
            Session session = new Session();

            foreach (int length in new[] { 24, 128 })
            {
                Tensor x = new Tensor(null, new[] { 1, 1, 1, length });
                x.Randomize(random);

                Tensor y1 = ArrayOperations.Copy(session, x);
                Helpers.AreTensorsEqual(x, y1);

                Tensor y2 = ArrayOperations.Copy(session, x);
                Helpers.AreTensorsEqual(x, y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();

                Helpers.AreArraysEqual(
                    y1.Gradient.Zip(y2.Gradient, (a, b) => a + b).ToArray(),
                    x.Gradient);
            }
        }

        [TestMethod]
        public void ConcatTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x1 = new Tensor(null, axes);
                Tensor x2 = new Tensor(null, Shape.Reshape(axes, axis, axes[axis] + 1));
                x1.Randomize(random);
                x2.Randomize(random);
                Tensor[] xs = new[] { x1, x2 };

                Tensor y1 = ArrayOperations.Concat(session, xs, axis);
                validate(y1);

                Tensor y2 = ArrayOperations.Concat(session, xs, axis);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    int[] yaxes = Shape.Reshape(axes, axis, 2 * axes[axis] + 1);
                    CollectionAssert.AreEqual(yaxes, y.Axes);

                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < yaxes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < yaxes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < yaxes[2]; i[2]++)
                            {
                                float expected = i[axis] < axes[axis] ?
                                    x1[i] : x2[Shape.Reshape(i, axis, i[axis] - axes[axis])];

                                Assert.AreEqual(expected, y[i], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < y1.Axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < y1.Axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < y1.Axes[2]; i[2]++)
                            {
                                float expected = y1.Gradient[y1.Position(i)] + y2.Gradient[y2.Position(i)];

                                Assert.AreEqual(
                                    expected,
                                    i[axis] < axes[axis] ? x1.Gradient[x1.Position(i)] : x2.Gradient[x2.Position(Shape.Reshape(i, axis, i[axis] - axes[axis]))],
                                    ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ConcatTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Concat(session, new[] { new Tensor(null, new[] { 1 }) }, -1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void SplitArrayTest0()
        {
            int[] axes = new[] { 3, 5, 7 };
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);
                int[] sizes = new[] { axes[axis] / 2, axes[axis] - (axes[axis] / 2) };

                IList<Tensor> ys1 = ArrayOperations.Split(session, x, axis, sizes);
                validate(ys1);

                IList<Tensor> ys2 = ArrayOperations.Split(session, x, axis, sizes);
                validate(ys2);

                foreach (Tensor y in ys1)
                {
                    y.RandomizeGradient(random);
                }

                foreach (Tensor y in ys2)
                {
                    y.RandomizeGradient(random);
                }

                session.Unroll();
                validateGradient();

                void validate(IList<Tensor> ys)
                {
                    Assert.AreEqual(sizes.Length, ys.Count);
                    CollectionAssert.AreEqual(sizes, ys.Select(y => y.Axes[axis]).ToArray());

                    int start = 0;
                    foreach (Tensor y in ys)
                    {
                        int[] i = new int[3];
                        for (i[0] = 0; i[0] < y.Axes[0]; i[0]++)
                        {
                            for (i[1] = 0; i[1] < y.Axes[1]; i[1]++)
                            {
                                for (i[2] = 0; i[2] < y.Axes[2]; i[2]++)
                                {
                                    float expected = x[Shape.Reshape(i, axis, i[axis] + start)];

                                    Assert.AreEqual(expected, y[i], ArrayOperationsTest.Eps);
                                }
                            }
                        }

                        start += y.Axes[axis];
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                float expected = 0.0f;
                                int start = 0;
                                foreach ((Tensor y1, Tensor y2) in ys1.Zip(ys2, (y1, y2) => (y1, y2)))
                                {
                                    if (i[axis] - start < y1.Axes[axis])
                                    {
                                        int[] yaxes = Shape.Reshape(i, axis, i[axis] - start);
                                        expected = y1.Gradient[y1.Position(yaxes)] + y2.Gradient[y2.Position(yaxes)];
                                        break;
                                    }

                                    start += y1.Axes[axis];
                                }

                                Assert.AreEqual(expected, x.Gradient[x.Position(i)], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SplitArrayTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Split(session, new Tensor(null, new[] { 1 }), -1, new[] { 1 });
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void SplitNumberTest0()
        {
            const int numsplits = 2;
            int[] axes = new[] { 4, 6, 8 };
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);

                IList<Tensor> ys1 = ArrayOperations.Split(session, x, axis, numsplits);
                validate(ys1);

                IList<Tensor> ys2 = ArrayOperations.Split(session, x, axis, numsplits);
                validate(ys2);

                foreach (Tensor y in ys1)
                {
                    y.RandomizeGradient(random);
                }

                foreach (Tensor y in ys2)
                {
                    y.RandomizeGradient(random);
                }

                session.Unroll();
                validateGradient();

                void validate(IList<Tensor> ys)
                {
                    Assert.AreEqual(numsplits, ys.Count);
                    CollectionAssert.AreEqual(
                        Enumerable.Repeat(axes[axis] / numsplits, numsplits).ToArray(),
                        ys.Select(y => y.Axes[axis]).ToArray());

                    int count = 0;
                    foreach (Tensor y in ys)
                    {
                        int[] i = new int[3];
                        for (i[0] = 0; i[0] < y.Axes[0]; i[0]++)
                        {
                            for (i[1] = 0; i[1] < y.Axes[1]; i[1]++)
                            {
                                for (i[2] = 0; i[2] < y.Axes[2]; i[2]++)
                                {
                                    float expected = x[Shape.Reshape(i, axis, i[axis] + (y.Axes[axis] * count))];

                                    Assert.AreEqual(expected, y[i], ArrayOperationsTest.Eps);
                                }
                            }
                        }

                        count++;
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                float expected = 0.0f;
                                int start = 0;
                                foreach ((Tensor y1, Tensor y2) in ys1.Zip(ys2, (y1, y2) => (y1, y2)))
                                {
                                    if (i[axis] - start < y1.Axes[axis])
                                    {
                                        int[] yaxes = Shape.Reshape(i, axis, i[axis] - start);
                                        expected = y1.Gradient[y1.Position(yaxes)] + y2.Gradient[y2.Position(yaxes)];
                                        break;
                                    }

                                    start += y1.Axes[axis];
                                }

                                Assert.AreEqual(expected, x.Gradient[x.Position(i)], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SplitNumberTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Split(session, new Tensor(null, new[] { 1 }), -1, 1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }


        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SplitNumberTest2()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Split(session, new Tensor(null, new[] { 3 }), 0, 2);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException("The number of tensors to split into must evenly divide the tensor split dimension.", "numberOfSplits").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void StackTest0()
        {
            const int N = 2;
            int[] axes = new[] { 3, 4 };
            Session session = new Session();

            for (int axis = 0; axis <= axes.Length; axis++)
            {
                Tensor[] xs = new Tensor[N];
                for (int i = 0; i < N; i++)
                {
                    xs[i] = new Tensor(null, axes);
                    xs[i].Randomize(random);
                }

                Tensor y1 = ArrayOperations.Stack(session, xs, axis);
                validate(y1);

                Tensor y2 = ArrayOperations.Stack(session, xs, axis);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    Assert.AreEqual(axes.Length + 1, y.Axes.Length);
                    Assert.AreEqual(xs.Length, y.Axes[axis]);

                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < y.Axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < y.Axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < y.Axes[2]; i[2]++)
                            {
                                Assert.AreEqual(xs[i[axis]][i.RemoveAt(axis)], y[i]);
                            }
                        }
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < y1.Axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < y1.Axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < y1.Axes[2]; i[2]++)
                            {
                                Tensor x = xs[i[axis]];

                                Assert.AreEqual(
                                    y1.Gradient[y1.Position(i)] + y2.Gradient[y2.Position(i)],
                                    x.Gradient[x.Position(i.RemoveAt(axis))]);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void StackTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Stack(session, new[] { new Tensor(null, new[] { 1 }) }, -1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void UnstackTest0()
        {
            int[] axes = new[] { 2, 3, 4 };
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);

                IList<Tensor> ys1 = ArrayOperations.Unstack(session, x, axis);
                validate(ys1);

                IList<Tensor> ys2 = ArrayOperations.Unstack(session, x, axis);
                validate(ys2);

                foreach (Tensor y in ys1)
                {
                    y.RandomizeGradient(random);
                }

                foreach (Tensor y in ys2)
                {
                    y.RandomizeGradient(random);
                }

                session.Unroll();
                validateGradient();

                void validate(IList<Tensor> ys)
                {
                    Assert.AreEqual(axes[axis], ys.Count);

                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                Assert.AreEqual(x[i], ys[i[axis]][i.RemoveAt(axis)]);
                            }
                        }
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                Tensor y1 = ys1[i[axis]];
                                Tensor y2 = ys2[i[axis]];

                                int[] yaxes = i.RemoveAt(axis);
                                Assert.AreEqual(y1.Gradient[y1.Position(yaxes)] + y2.Gradient[y2.Position(yaxes)], x.Gradient[x.Position(i)]);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void UnstackTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Unstack(session, new Tensor(null, new[] { 1 }), -1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void TileTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            const int count = 2;
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);

                Tensor y1 = ArrayOperations.Tile(session, x, axis, count);
                validate(y1);

                Tensor y2 = ArrayOperations.Tile(session, x, axis, count);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    int[] yaxes = Shape.Reshape(axes, axis, axes[axis] * count);
                    CollectionAssert.AreEqual(yaxes, y.Axes);

                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < yaxes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < yaxes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < yaxes[2]; i[2]++)
                            {
                                int[] xaxes = Shape.Reshape(i, axis, i[axis] % axes[axis]);
                                float expected = x[xaxes];

                                Assert.AreEqual(expected, y[i], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                float expected = Enumerable.Range(0, count).Sum(idx =>
                                {
                                    int[] yaxes = Shape.Reshape(i, axis, i[axis] + (axes[axis] * idx));
                                    return y1.Gradient[y1.Position(yaxes)] + y2.Gradient[y2.Position(yaxes)];
                                });

                                Assert.AreEqual(expected, x.Gradient[x.Position(i)], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TileTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Tile(session, new Tensor(null, new[] { 1 }), -1, 1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void UntileTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            const int count = 2;
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);

                Tensor y1 = ArrayOperations.Untile(session, x, axis, count);
                validate(y1);

                Tensor y2 = ArrayOperations.Untile(session, x, axis, count);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    int[] yaxes = Shape.Reshape(axes, axis, axes[axis] / count);
                    CollectionAssert.AreEqual(yaxes, y.Axes);

                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < yaxes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < yaxes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < yaxes[2]; i[2]++)
                            {
                                float expected = Enumerable.Range(0, count).Sum(idx =>
                                {
                                    int[] xaxes = Shape.Reshape(i, axis, i[axis] + (yaxes[axis] * idx));
                                    return x[xaxes];
                                });

                                Assert.AreEqual(expected, y[i], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                int[] yaxes = Shape.Reshape(i, axis, i[axis] % (axes[axis] / count));
                                float expected = y1.Gradient[y1.Position(yaxes)] + y2.Gradient[y2.Position(yaxes)];

                                Assert.AreEqual(expected, x.Gradient[x.Position(i)], ArrayOperationsTest.Eps);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void UntileTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Untile(session, new Tensor(null, new[] { 1 }), -1, 1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void MaxReduceTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            const int count = 2;
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);

                Tensor y1 = ArrayOperations.MaxReduce(session, x, axis, count);
                validate(y1);

                Tensor y2 = ArrayOperations.MaxReduce(session, x, axis, count);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    int[] yaxes = axes.ToArray();
                    yaxes[axis] /= count;

                    CollectionAssert.AreEqual(yaxes, y.Axes);

                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < yaxes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < yaxes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < yaxes[2]; i[2]++)
                            {
                                float expected = Enumerable.Range(i[axis] * count, count).Max(idx =>
                                {
                                    int[] xaxes = i.ToArray();
                                    xaxes[axis] = idx;
                                    return x[xaxes];
                                });

                                Assert.AreEqual(expected, y[i]);
                            }
                        }
                    }
                };

                void validateGradient()
                {
                    int[] i = new int[3];
                    for (i[0] = 0; i[0] < axes[0]; i[0]++)
                    {
                        for (i[1] = 0; i[1] < axes[1]; i[1]++)
                        {
                            for (i[2] = 0; i[2] < axes[2]; i[2]++)
                            {
                                int[] yaxes = i.ToArray();
                                yaxes[axis] /= count;

                                float expected = y1[yaxes] == x[i] ? y1.Gradient[y1.Position(yaxes)] + y2.Gradient[y2.Position(yaxes)] : 0.0f;

                                Assert.AreEqual(expected, x.Gradient[x.Position(i)]);
                            }
                        }
                    }
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void MaxReduceTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.MaxReduce(session, new Tensor(null, new[] { 1 }), -1, 1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void SqueezeTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                int[] newaxes = axes.ToArray();
                newaxes[axis] = 1;
                Tensor x = new Tensor(null, newaxes);
                x.Randomize(random);

                Tensor y1 = ArrayOperations.Squeeze(session, x, axis);
                validate(y1);

                Tensor y2 = ArrayOperations.Squeeze(session, x, axis);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    CollectionAssert.AreEqual(Shape.Remove(axes, axis), y.Axes);
                    Helpers.AreArraysEqual(x.Weights, y.Weights);
                };

                void validateGradient()
                {
                    Helpers.AreArraysEqual(
                        y1.Gradient.Zip(y2.Gradient, (a, b) => a + b).ToArray(),
                        x.Gradient);
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SqueezeTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Squeeze(session, new Tensor(null, new[] { 1 }), -1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SqueezeTest2()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Squeeze(session, new Tensor(null, new[] { 2, 1 }), 0);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException("The dimension to remove must be of size 1.", "axis").Message, e.Message);
                throw;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SqueezeTest3()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Squeeze(session, new Tensor(null, new[] { 2 }), 0);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException("The tensor must have the rank of at least 2.", "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void ExpandTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            Session session = new Session();

            for (int axis = 0; axis < axes.Length; axis++)
            {
                Tensor x = new Tensor(null, axes);
                x.Randomize(random);

                Tensor y1 = ArrayOperations.Expand(session, x, axis);
                validate(y1);

                Tensor y2 = ArrayOperations.Expand(session, x, axis);
                validate(y2);

                y1.RandomizeGradient(random);
                y2.RandomizeGradient(random);
                session.Unroll();
                validateGradient();

                void validate(Tensor y)
                {
                    CollectionAssert.AreEqual(Shape.Expand(axes, axis, 1), y.Axes);
                    Helpers.AreArraysEqual(x.Weights, y.Weights);
                };

                void validateGradient()
                {
                    Helpers.AreArraysEqual(
                        y1.Gradient.Zip(y2.Gradient, (a, b) => a + b).ToArray(),
                        x.Gradient);
                };
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ExpandTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Expand(session, new Tensor(null, new[] { 1 }), -1);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException(Properties.Resources.E_NegativeAxisIndex, "axis").Message, e.Message);
                throw;
            }
        }

        [TestMethod]
        public void ReshapeTest0()
        {
            int[] axes = new[] { 2, 4, 6 };
            int[] axes1 = new[] { 4, 2, 6 };
            int[] axes2 = new[] { 2, 4, 3, 2 };
            Session session = new Session();

            Tensor x = new Tensor(null, axes);
            x.Randomize(random);

            Tensor y1 = ArrayOperations.Reshape(session, x, axes1);
            validate(y1, axes1);

            Tensor y2 = ArrayOperations.Reshape(session, x, axes2);
            validate(y2, axes2);

            y1.RandomizeGradient(random);
            y2.RandomizeGradient(random);
            session.Unroll();
            validateGradient();

            void validate(Tensor y, int[] yaxes)
            {
                CollectionAssert.AreEqual(yaxes, y.Axes);
                Helpers.AreArraysEqual(x.Weights, y.Weights);
            };

            void validateGradient()
            {
                Helpers.AreArraysEqual(
                    y1.Gradient.Zip(y2.Gradient, (a, b) => a + b).ToArray(),
                    x.Gradient);
            };
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is an argument of tested method.")]
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ReshapeTest1()
        {
            try
            {
                Session session = new Session();
                ArrayOperations.Reshape(session, new Tensor(null, new[] { 1, 2 }), 3, 4);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual(new ArgumentException("The size of new shape must be the same as tensor length.", "shape").Message, e.Message);
                throw;
            }
        }
    }
}
