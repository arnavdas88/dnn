// -----------------------------------------------------------------------
// <copyright file="CTCLoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Learning
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    using Accord.DNN;
    using Genix.Core;

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
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Use CTC notation.")]
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

            int L = expected.Length;    // Number of labels
            int T = y.Axes[0];          // Number of mini-batches (time)
            int A = y.Strides[0];       // Number of classes (alphabet size)

            int[] labels = CTCLoss.InsertBlanks(expected, A, this.BlankLabelIndex, out int repeats);
            if (L + repeats > T)
            {
                if (calculateGradient)
                {
                    SetCopy.Copy(y.Length, y.Weights, 0, y.Gradient, 0);
                }

                // not enough elements to compute
                return float.PositiveInfinity;
            }

            int S = labels.Length;      // Number of labels with blanks

            // convert predicted probabilities into log space
            float[] ylog = new float[y.Length];
            Mathematics.Log(y.Length, y.Weights, 0, ylog, 0);

            // compute alphas
            float[] alphas = new float[T * S];
            ////CTCLoss.CTCComputeAlphas(T, A, S, ylog, labels, alphas);
            NativeMethods.CTCComputeAlphas(T, A, S, ylog, labels, alphas);

            float logLossA = MKL.LogSumExp(alphas[alphas.Length - 1], alphas[alphas.Length - 2]);
            if (float.IsNegativeInfinity(logLossA))
            {
                if (calculateGradient)
                {
                    SetCopy.Copy(y.Length, y.Weights, 0, y.Gradient, 0);
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
                Mathematics.Add(alphas.Length, alphas, 0, betas, 0, alphas, 0);
                NativeMethods.CTCReduceAlphasBetas(T, A, S, alphas, labels, y.Gradient);
                Mathematics.Subtract(y.Length, y.Gradient, 0, ylog, 0, y.Gradient, 0);
                Mathematics.Subtract(y.Length, y.Gradient, 0, logLossA, y.Gradient, 0);
                Mathematics.Exp(y.Length, y.Gradient, 0, y.Gradient, 0);
                MKL.Replace(y.Length, float.NaN, 0.0f, y.Gradient, 0); // NaN may come from various sources (for instance log(y) where y = 0)

                Debug.Assert(!float.IsNaN(y.Gradient[0]), "Tensor contains invalid weight.");
            }

            Debug.Assert(!float.IsNaN(logLossA), "Calculated loss is invalid.");
            return -logLossA;
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Use CTC notation.")]
        private static int[] InsertBlanks(int[] labels, int A, int blankIndex, out int repeats)
        {
            repeats = 0;

            int L = labels.Length;      // Number of labels
            int S = (2 * L) + 1;        // Number of labels with blanks

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

        private static class NativeMethods
        {
            private const string DllName = "Genix.DNN.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void CTCComputeAlphas(int T, int A, int S, [In] float[] py, [In] int[] labels, [Out] float[] pa);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void CTCComputeBetas(int T, int A, int S, [In] float[] py, [In] int[] labels, [Out] float[] pb);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void CTCReduceAlphasBetas(int T, int A, int S, [In] float[] pab, [In] int[] labels, [Out] float[] pdy);
        }
    }
}
