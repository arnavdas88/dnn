namespace Genix.DNN.Test
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Genix.MachineLearning;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class Helpers
    {
        /// <summary>
        /// Creates a class using private constructor.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <returns>The object this method creates.</returns>
        public static T CreateClassWithPrivateConstructor<T>()
            where T : class
        {
            var constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            return (T)constructor.Invoke(null);
        }

        public static Tensor CropKernel(this Tensor input, int b, int x, int y, Kernel kernel, bool cropGradient, out int kernelArea)
        {
            Tensor output = new Tensor(null, new Shape(input.Shape.Format, 1, kernel.Width, kernel.Height, input.Shape.GetAxis(Axis.C)));

            int xb = Math.Max(-kernel.PaddingX, 0);
            int xe = input.Shape.GetAxis(Axis.X) - 1 - xb;
            int yb = Math.Max(-kernel.PaddingY, 0);
            int ye = input.Shape.GetAxis(Axis.Y) - 1 - yb;

            for (int ix = x; ix < x + kernel.Width; ix++)
            {
                if (ix.Between(xb, xe))
                {
                    for (int iy = y; iy < y + kernel.Height; iy++)
                    {
                        if (iy.Between(yb, ye))
                        {
                            for (int ic = 0; ic < input.Shape.GetAxis(Axis.C); ic++)
                            {
                                int inpos = input.Shape.Position(b, ix, iy, ic);
                                int outpos = output.Shape.Position(0, ix - x, iy - y, ic);

                                output[outpos] = cropGradient ? input.Gradient[inpos] : input[inpos];
                            }
                        }
                    }
                }
            }

            int xarea = 0;
            int yarea = 0;
            for (int ix = x; ix < x + kernel.Width; ix++)
            {
                if (ix.Between(xb, xe))
                {
                    xarea++;
                }
            }

            for (int iy = y; iy < y + kernel.Height; iy++)
            {
                if (iy.Between(yb, ye))
                {
                    yarea++;
                }
            }

            kernelArea = xarea * yarea;
            return output;
        }

        public static void AreTensorsEqual(Tensor expected, Tensor actual)
        {
            CollectionAssert.AreEqual(expected.Axes, actual.Axes);

            Assert.AreEqual(expected.Length, actual.Length);

            Helpers.AreArraysEqual(expected.Length, expected.Weights, actual.Weights);
        }

        public static void AreGradientsEqual(Tensor expected, Tensor actual)
        {
            CollectionAssert.AreEqual(expected.Axes, actual.Axes);

            Assert.AreEqual(expected.Length, actual.Length);

            Helpers.AreArraysEqual(expected.Length, expected.Gradient, actual.Gradient);
        }

        public static void AreArraysEqual(float[] expected, float[] actual)
        {
            Assert.IsTrue(expected.Length <= actual.Length);

            for (int i = 0, ii = expected.Length; i < ii; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-4f, i.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static void AreArraysEqual(int length, float[] expected, float[] actual)
        {
            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], 1e-4f, i.ToString(CultureInfo.InvariantCulture));
            }
        }

        /*public static void AreArraysEqual(float[] expected, float[] actual, Func<float, float, Tuple<float, float>> selector)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0, ii = expected.Length; i < ii; i++)
            {
                Tuple<float, float> values = selector(expected[i], actual[i]);
                Assert.AreEqual(values.Item1, values.Item2, 1e-6f);
            }
        }*/

        public static void AreArraysEqual(int length, float[] expected, int expectedIndex, float[] actual, int actualIndex)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected[expectedIndex + i], actual[actualIndex + i], 1e-6f);
            }
        }

        /*public static void AreArraysEqual(int length, float[] expected, int expectedIndex, int expectedIncrement, float[] actual, int actualIndex, int actualIncrement)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected[expectedIndex + (expectedIncrement * i)], actual[actualIndex + (actualIncrement * i)], 1e-6f);
            }
        }*/

        public static void IsArrayEqual(int length, float expected, float[] actual, int actualIndex)
        {
            for (int i = 0, ii = length; i < ii; i++)
            {
                Assert.AreEqual(expected, actual[actualIndex + i], 1e-6f);
            }
        }
    }
}
