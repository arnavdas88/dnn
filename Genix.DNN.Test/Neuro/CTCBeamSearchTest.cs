namespace Genix.DNN.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Learning;
    using System.Collections.Generic;
    using Genix.DNN.LanguageModel;

    [TestClass]
    public class CTCBeamSearchTest
    {
        [TestMethod]
        public void TestMethod0()
        {
            const int X = 0;
            const int A = 1;

            string[] classes = new string[] { "@", "A" };

            Tensor a = new Tensor(null, new[] { 2, 2 });
            a.Set(new float[] {
                0.2f, 0.5f,
                0.1f, 0.7f
            });

            CTCBeamSearch loss = new CTCBeamSearch(classes);
            IList<(string[], float)> results = loss.BeamSearch(a);

            float normalizer =
                path(A, A) +
                path(A, X) +
                path(X, A); 

            float path(int v1, int v2)
            {
                return a[(0 * 2) + v1] *
                       a[(1 * 2) + v2];
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            const int X = 0;
            const int A = 1;
            const int B = 2;

            string[] classes = new string[] { "@", "A", "B" };

            Tensor a = new Tensor(null, new[] { 3, 3 });
            a.Set(new float[] {
                0.2f, 0.5f, 0.3f,
                0.1f, 0.7f, 0.2f,
                0.1f, 0.3f, 0.6f
            });

            CTCBeamSearch loss = new CTCBeamSearch(classes);
            IList<(string[], float)> results = loss.BeamSearch(a);

            float normalizer =
                path(A, A, B) +
                path(A, B, B) +
                path(A, B, X) +
                path(A, X, B) +

                path(X, A, B); 

            float path(int v1, int v2, int v3)
            {
                return a[(0 * 3) + v1] *
                       a[(1 * 3) + v2] *
                       a[(2 * 3) + v3];
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            const int X = 0;
            const int A = 1;
            const int B = 2;

            string[] classes = new string[] { "@", "A", "B" };

            Tensor a = new Tensor(null, new[] { 4, 3 });
            a.Set(new float[] {
                0.2f, 0.5f, 0.3f,
                0.1f, 0.7f, 0.2f,
                0.1f, 0.3f, 0.6f,
                0.05f, 0.15f, 0.8f,
            });

            float normalizer =
                path(A, A, A, B) +
                path(A, A, B, B) +
                path(A, A, B, X) +
                path(A, A, X, B) +

                path(A, B, B, B) +
                path(A, B, B, X) +
                //path(A, B, X, B) +
                path(A, B, X, X) +

                //path(A, X, A, B) +
                path(A, X, B, B) +
                path(A, X, B, X) +
                path(A, X, X, B) +

                path(X, A, A, B) +
                path(X, A, B, B) +
                path(X, A, B, X) +
                path(X, A, X, B) +

                path(X, X, A, B); 

            float path(int v1, int v2, int v3, int v4)
            {
                float res = a[(0 * 3) + v1] *
                       a[(1 * 3) + v2] *
                       a[(2 * 3) + v3] *
                       a[(3 * 3) + v4];

                Console.WriteLine(res);
                return res;
            }

            Charset charset = new Charset("AB", 1, 2);

            CTCBeamSearch loss = new CTCBeamSearch(classes, charset);
            IList<(string[], float)> results = loss.BeamSearch(a);
        }
    }
}
