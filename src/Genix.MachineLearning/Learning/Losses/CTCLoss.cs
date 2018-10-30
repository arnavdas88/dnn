// -----------------------------------------------------------------------
// <copyright file="CTCLoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Genix.MachineLearning;

    /// <summary>
    /// Square loss, also known as L2-loss or Euclidean loss.
    /// </summary>
    public class CTCLoss : ILoss<int[]>
    {
        ////private const float Eps = 1e-8f;

        /// <summary>
        /// Gets or sets the index of blank label in the alphabet.
        /// </summary>
        /// <value>
        /// The zero-based index of blank label in the alphabet. Default is 0.
        /// </value>
        public int BlankLabelIndex { get; set; } = 0;

        /// <inheritdoc />
        public float Loss(Tensor y, int[] expected, bool calculateGradient)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            int L = expected.Length;    // Number of labels
            int T = y.Axes[0];          // Number of mini-batches (time)
            int A = y.Strides[0];       // Number of classes (alphabet size)
#pragma warning restore SA1312 // Variable names must begin with lower-case letter

            int[] labels = CTCLoss.InsertBlanks(expected, A, this.BlankLabelIndex, out int repeats);
            if (L + repeats > T)
            {
                if (calculateGradient)
                {
                    Vectors.Copy(y.Length, y.Weights, 0, y.Gradient, 0);
                }

                // not enough elements to compute
                return float.PositiveInfinity;
            }

#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            int S = labels.Length;      // Number of labels with blanks
#pragma warning restore SA1312 // Variable names must begin with lower-case letter

            // convert predicted probabilities into log space
            float[] ylog = new float[y.Length];
            Vectors.Log(y.Length, y.Weights, 0, ylog, 0);

            // compute alphas
            float[] alphas = new float[T * S];
            ////CTCLoss.CTCComputeAlphas(T, A, S, ylog, labels, alphas);
            NativeMethods.CTCComputeAlphas(T, A, S, ylog, labels, alphas);

            float logLossA = Mathematics.LogSumExp(alphas[alphas.Length - 1], alphas[alphas.Length - 2]);
            if (float.IsNegativeInfinity(logLossA))
            {
                if (calculateGradient)
                {
                    Vectors.Copy(y.Length, y.Weights, 0, y.Gradient, 0);
                }

                return float.PositiveInfinity;
            }

            if (calculateGradient)
            {
                // compute betas
                float[] betas = new float[T * S];
                NativeMethods.CTCComputeBetas(T, A, S, ylog, labels, betas);
                ////float logLossB = MKL.LogSumExp(betas.Weights[0], betas.Weights[1]);

                // compute unnormalized gradient
                Vectors.Add(alphas.Length, betas, 0, alphas, 0);
                NativeMethods.CTCReduceAlphasBetas(T, A, S, alphas, labels, y.Gradient);
                Vectors.Sub(y.Length, ylog, 0, y.Gradient, 0);
                Vectors.SubC(y.Length, logLossA, y.Gradient, 0);
                Vectors.Exp(y.Length, y.Gradient, 0);

                // remove NaN
                // NaN may come from various sources (for instance log(y) where y = 0)
                Arrays.Replace(y.Length, y.Gradient, 0, float.NaN, 0.0f, y.Gradient, 0);

                Debug.Assert(!float.IsNaN(y.Gradient[0]), "Tensor contains invalid weight.");
            }

            Debug.Assert(!float.IsNaN(logLossA), "Calculated loss is invalid.");
            return -logLossA;
        }

#pragma warning disable SA1313 // Variable names must begin with lower-case letter
        private static int[] InsertBlanks(int[] labels, int A, int blankIndex, out int repeats)
#pragma warning restore SA1313 // Variable names must begin with lower-case letter
        {
            repeats = 0;

#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            int L = labels.Length;      // Number of labels
            int S = (2 * L) + 1;        // Number of labels with blanks
#pragma warning restore SA1312 // Variable names must begin with lower-case letter

            int[] labelsWithBlanks = new int[S];

            for (int i = 0; i < L; i++)
            {
                int label = labels[i];
                if (label >= A)
                {
                    throw new ArgumentException("The label index is greater than alphabet size.");
                }

                if (i > 0 && label == labels[i - 1])
                {
                    repeats++;
                }

                labelsWithBlanks[2 * i] = blankIndex;
                labelsWithBlanks[(2 * i) + 1] = label;
            }

            labelsWithBlanks[S - 1] = blankIndex;

            return labelsWithBlanks;
        }

        /*private static void CTCComputeAlphas(int T, int A, int S, float[] py, int[] labels, float[] pa)
        {
            MKL.Set(S * T, float.NegativeInfinity, pa, 0);
            ////std::fill(pa, pa + (S * T), -INFINITY);

            int start = MKL.Max(0, S - (2 * T));
            int end = MKL.Min(2, S);

            for (int i = start; i < end; i++)
            {
                pa[i] = py[labels[i]];
            }

            int paoff = S;
            int paoffprev = 0;
            int pyoff = A;

            for (int t = 1; t < T; t++, paoff += S, paoffprev += S, pyoff += A)
            {
                start = MKL.Max(0, S - (2 * (T - t)));
                end = MKL.Min(2 * (t + 1), S);

                int i = start;

                if (i == 0)
                {
                    pa[paoff + i] = pa[paoffprev + i] + py[pyoff + labels[i]];

                    i++;
                }

                for (; i < end; i++)
                {
                    if ((i % 2) != 0 && i > 1 && labels[i] != labels[i - 2])
                    {
                        pa[paoff + i] = MKL.LogSumExp(pa[paoffprev + i], MKL.LogSumExp(pa[paoffprev + i - 1], pa[paoffprev + i - 2])) + py[pyoff + labels[i]];
                    }
                    else
                    {
                        pa[paoff + i] = MKL.LogSumExp(pa[paoffprev + i], pa[paoffprev + i - 1]) + py[pyoff + labels[i]];
                    }
                }
            }
        }*/

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.DNN.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void CTCComputeAlphas(int T, int A, int S, [In] float[] py, [In] int[] labels, [Out] float[] pa);

            [DllImport(NativeMethods.DllName)]
            public static extern void CTCComputeBetas(int T, int A, int S, [In] float[] py, [In] int[] labels, [Out] float[] pb);

            [DllImport(NativeMethods.DllName)]
            public static extern void CTCReduceAlphasBetas(int T, int A, int S, [In] float[] pab, [In] int[] labels, [Out] float[] pdy);
        }
    }
}
