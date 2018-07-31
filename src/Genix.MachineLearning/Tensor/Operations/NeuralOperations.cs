// -----------------------------------------------------------------------
// <copyright file="NeuralOperations.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define NOLEARNING

namespace Genix.MachineLearning
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using Genix.Core;

    /// <summary>
    /// Represents neural net operations on tensors.
    /// </summary>
    public static class NeuralOperations
    {
#if false
        /// <summary>
        /// Adds a bias vector to the tensor.
        /// </summary>
        /// <param name="session">The graph this operation should be added to.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <param name="b">The biases tensor <c>b</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The <c>x</c> tensor can represent a mini-batch and contain multiple vectors.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor AddBias(this Session session, Tensor x, Tensor b)
        {
            int mb = x.Axes[0];             // number of items in a mini-batch
            if (mb == 1)
            {
                return session.Add(x, b);
            }

            Tensor y = session.Allocate("addBias", x.Axes);

            int stride = x.Strides[0];      // item length
            Debug.Assert(stride == b.Length, "Biases tensor has invalid size.");

            // repeat for each item in mini-batch
            for (int i = 0, off = 0; i < mb; i++, off += stride)
            {
                Mathematics.Add(stride, x.Weights, off, b.Weights, 0, y.Weights, off);
            }

            if (session.CalculateGradients && (x.CalculateGradient || b.CalculateGradient))
            {
                session.Push(() =>
                {
                    Tensor dy = session.GetGradient(y);

                    // dx += dy
                    if (x.CalculateGradient)
                    {
                        Tensor dx = session.GetGradient(x);
                        lock (dx)
                        {
                            dx.Add(dy);
                        }
                    }

                    // db += sum(dy)
                    if (b.CalculateGradient)
                    {
                        float[] ones = new float[mb];
                        MKL.Set(mb, 1.0f, ones, 0);

                        Tensor db = session.GetGradient(b);
                        lock (db)
                        {
                            MKL.MxV(MatrixLayout.ColumnMajor, stride, mb, y.Gradient, 0, false, ones, 0, db.Weights, 0, false);

                            /*for (int i = 0, off = 0; i < mb; i++, off += stride)
                            {
                                Mathematics.Add(stride, y.Gradient, off, db.Weights, 0);
                            }*/
                        }
                    }
                });
            }

            return y;
        }
#endif

        /// <summary>
        /// Applies average pooling filter.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="kernel">The kernel that describes the size and stride of the sliding window to apply.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Need to pass as a reference to reallocate.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Unroll cycles to improve performance.")]
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Unroll cycles to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor AveragePooling(this Session session, Tensor x, Kernel kernel)
        {
            const string ActionName = "avg pool";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = Pool(calculateGradient);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Gradient(y));
                    }
#endif

                    return y;
                });

            Tensor Pool(bool calculateGradient)
            {
                int ksize1 = kernel.Width;
                int ksize2 = kernel.Height;
                int kstride1 = kernel.StrideX;
                int kstride2 = kernel.StrideY;

                int x0 = x.Axes[0];
                int x1 = x.Axes[1];
                int x2 = x.Axes[2];
                int x3 = x.Axes[3];

                int xstride0 = x.Strides[0];
                int xstride1 = x.Strides[1];
                int xstride2 = x.Strides[2];

                int xstride1K = xstride1 * kstride1;
                int xstride2K = xstride2 * kstride2;

                int y1 = kernel.CalculateOutputWidth(x1);
                int y2 = kernel.CalculateOutputHeight(x2);

                Tensor y = session.AllocateTensor("avg pool", new[] { x0, y1, y2, x3 }, calculateGradient);

                int ystride0 = y.Strides[0];
                int ystride1 = y.Strides[1];
                int ystride2 = y.Strides[2];

                float[] wspw = new float[xstride1];
                float[] xw = x.Weights;
                float[] yw = y.Weights;

                if (ksize1 == 2 && ksize2 == 2)
                {
                    Pool2x2();
                }
                else if (ksize1 == 2 && ksize2 == 1 && kstride2 == 1)
                {
                    Pool2x1();
                }
                else
                {
                    PoolNxN();
                }

                Mathematics.Divide(y.Length, ksize1 * ksize2, yw, 0, yw, 0);

                return y;

                void Pool2x2()
                {
                    for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0, ypos0 += ystride0)
                    {
                        for (int iy1 = 0, ix1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                        {
                            if (ix1 + 1 < x1)
                            {
                                Mathematics.Add(xstride1, xw, xpos1, xw, xpos1 + xstride1, wspw, 0);
                            }
                            else
                            {
                                Arrays.Copy(xstride1, xw, xpos1, wspw, 0);
                            }

                            for (int iy2 = 0, ix2 = 0, ypos2 = ypos1, wspos = 0; iy2 < y2; iy2++, ix2 += kstride2, ypos2 += xstride2, wspos += xstride2K)
                            {
                                if (ix2 + 1 < x2)
                                {
                                    Mathematics.Add(xstride2, wspw, wspos, wspw, wspos + xstride2, yw, ypos2);
                                }
                                else
                                {
                                    Arrays.Copy(xstride2, wspw, wspos, yw, ypos2);
                                }
                            }
                        }
                    }
                }

                void Pool2x1()
                {
                    for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0, ypos0 += ystride0)
                    {
                        for (int iy1 = 0, ix1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                        {
                            if (ix1 + 1 < x1)
                            {
                                Mathematics.Add(xstride1, xw, xpos1, xw, xpos1 + xstride1, yw, ypos1);
                            }
                            else
                            {
                                Arrays.Copy(xstride1, xw, xpos1, yw, ypos1);
                            }
                        }
                    }
                }

                void PoolNxN()
                {
                    for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0, ypos0 += ystride0)
                    {
                        for (int ix1 = 0, iy1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                        {
                            int ix1e = Maximum.Min(ix1 + ksize1, x1);
                            if (ix1e - ix1 == 1)
                            {
                                Arrays.Copy(xstride1, xw, xpos1, wspw, 0);
                            }
                            else
                            {
                                Mathematics.Add(xstride1, xw, xpos1, xw, xpos1 + xstride1, wspw, 0);

                                for (int i = ix1 + 2, pos = xpos1 + (2 * xstride1); i < ix1e; i++, pos += xstride1)
                                {
                                    Mathematics.Add(xstride1, xw, pos, wspw, 0, wspw, 0);
                                }
                            }

                            for (int ix2 = 0, iy2 = 0, ypos2 = ypos1, wspos = 0; iy2 < y2; iy2++, ix2 += kstride2, ypos2 += xstride2, wspos += xstride2K)
                            {
                                int ix2e = Maximum.Min(ix2 + ksize2, x2);
                                if (ix2e - ix2 == 1)
                                {
                                    Arrays.Copy(xstride2, wspw, wspos, yw, ypos2);
                                }
                                else
                                {
                                    Mathematics.Add(xstride2, wspw, wspos, wspw, wspos + xstride2, yw, ypos2);

                                    for (int i = ix2 + 2, pos = wspos + (2 * xstride2); i < ix2e; i++, pos += xstride2)
                                    {
                                        Mathematics.Add(xstride2, wspw, pos, yw, ypos2, yw, ypos2);
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if !NOLEARNING
            void Gradient(Tensor y)
            {
                int ksize1 = kernel.Width;
                int ksize2 = kernel.Height;
                int kstride1 = kernel.StrideX;
                int kstride2 = kernel.StrideY;

                int x0 = x.Axes[0];
                int x1 = x.Axes[1];
                int x2 = x.Axes[2];

                int xstride0 = x.Strides[0];
                int xstride1 = x.Strides[1];
                int xstride2 = x.Strides[2];

                int xstride1K = xstride1 * kstride1;
                int xstride2K = xstride2 * kstride2;

                int y1 = y.Axes[1];
                int y2 = y.Axes[2];

                int ystride0 = y.Strides[0];
                int ystride1 = y.Strides[1];
                int ystride2 = y.Strides[2];

                float[] dxw = x.Gradient;
                float[] dyw = y.Gradient;
                float alpha = 1.0f / (ksize1 * ksize2);

                // cycle by the output
                for (int ix0 = 0, ypos0 = 0, xpos0 = 0; ix0 < x0; ix0++, ypos0 += ystride0, xpos0 += xstride0)
                {
                    for (int iy1 = 0, ix1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                    {
                        for (int iy2 = 0, ix2 = 0, ypos2 = ypos1, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, ypos2 += ystride2, xpos2 += xstride2K)
                        {
                            // cycle by the kernel
                            int ike1 = Maximum.Min(ix1 + ksize1, x1);
                            int ike2 = Maximum.Min(ix2 + ksize2, x2);
                            for (int ik1 = ix1, xpos1K = xpos2; ik1 < ike1; ik1++, xpos1K += xstride1)
                            {
                                for (int ik2 = ix2, xpos2K = xpos1K; ik2 < ike2; ik2++, xpos2K += xstride2)
                                {
                                    Mathematics.MultiplyAndAdd(ystride2, alpha, dyw, ypos2, dxw, xpos2K);
                                }
                            }
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Applies max pooling filter.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="kernel">The kernel to apply.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Need to pass as a reference to reallocate.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Unroll cycles to improve performance.")]
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Unroll cycles to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor MaxPooling(this Session session, Tensor x, Kernel kernel)
        {
            const string ActionName = "max pool";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = Pool(calculateGradient);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Gradient(y));
                    }
#endif

                    return y;
                });

            Tensor Pool(bool calculateGradient)
            {
                int ksize1 = kernel.Width;
                int ksize2 = kernel.Height;
                int kstride1 = kernel.StrideX;
                int kstride2 = kernel.StrideY;

                int x0 = x.Axes[0];
                int x1 = x.Axes[1];
                int x2 = x.Axes[2];
                int x3 = x.Axes[3];

                int xstride0 = x.Strides[0];
                int xstride1 = x.Strides[1];
                int xstride2 = x.Strides[2];

                int xstride1K = xstride1 * kstride1;
                int xstride2K = xstride2 * kstride2;

                int y1 = kernel.CalculateOutputWidth(x1);
                int y2 = kernel.CalculateOutputHeight(x2);

                Tensor y = session.AllocateTensor(ActionName, new[] { x0, y1, y2, x3 }, calculateGradient);

                int ystride0 = y.Strides[0];
                int ystride1 = y.Strides[1];

                float[] xw = x.Weights;
                float[] yw = y.Weights;

                if (ksize1 == 2 && ksize2 == 2)
                {
                    Pool2x2();
                }
                else if (ksize1 == 2 && ksize2 == 1 && kstride2 == 1)
                {
                    Pool2x1();
                }
                else
                {
                    PoolNxN();
                }

                return y;

                void Pool2x2()
                {
                    float[] wspw = new float[xstride1];

                    for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0, ypos0 += ystride0)
                    {
                        for (int iy1 = 0, ix1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                        {
                            if (ix1 + 1 < x1)
                            {
                                Maximum.Max(xstride1, xw, xpos1, xw, xpos1 + xstride1, wspw, 0);
                            }
                            else
                            {
                                Arrays.Copy(xstride1, xw, xpos1, wspw, 0);
                            }

                            for (int iy2 = 0, ix2 = 0, ypos2 = ypos1, wspos = 0; iy2 < y2; iy2++, ix2 += kstride2, ypos2 += xstride2, wspos += xstride2K)
                            {
                                if (ix2 + 1 < x2)
                                {
                                    Maximum.Max(xstride2, wspw, wspos, wspw, wspos + xstride2, yw, ypos2);
                                }
                                else
                                {
                                    Arrays.Copy(xstride2, wspw, wspos, yw, ypos2);
                                }
                            }
                        }
                    }
                }

                void Pool2x1()
                {
                    for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0, ypos0 += ystride0)
                    {
                        for (int iy1 = 0, ix1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                        {
                            if (ix1 + 1 < x1)
                            {
                                Maximum.Max(xstride1, xw, xpos1, xw, xpos1 + xstride1, yw, ypos1);
                            }
                            else
                            {
                                Arrays.Copy(xstride1, xw, xpos1, yw, ypos1);
                            }
                        }
                    }
                }

                void PoolNxN()
                {
                    float[] wspw = new float[xstride1];

                    for (int ix0 = 0, xpos0 = 0, ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0, ypos0 += ystride0)
                    {
                        for (int ix1 = 0, iy1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                        {
                            int ix1e = Maximum.Min(ix1 + ksize1, x1);
                            if (ix1e - ix1 == 1)
                            {
                                Arrays.Copy(xstride1, xw, xpos1, wspw, 0);
                            }
                            else
                            {
                                Maximum.Max(xstride1, xw, xpos1, xw, xpos1 + xstride1, wspw, 0);

                                for (int i = ix1 + 2, pos = xpos1 + (2 * xstride1); i < ix1e; i++, pos += xstride1)
                                {
                                    Maximum.Max(xstride1, xw, pos, wspw, 0, wspw, 0);
                                }
                            }

                            for (int ix2 = 0, iy2 = 0, ypos2 = ypos1, wspos = 0; iy2 < y2; iy2++, ix2 += kstride2, ypos2 += xstride2, wspos += xstride2K)
                            {
                                int ix2e = Maximum.Min(ix2 + ksize2, x2);
                                if (ix2e - ix2 == 1)
                                {
                                    Arrays.Copy(xstride2, wspw, wspos, yw, ypos2);
                                }
                                else
                                {
                                    Maximum.Max(xstride2, wspw, wspos, wspw, wspos + xstride2, yw, ypos2);

                                    for (int i = ix2 + 2, pos = wspos + (2 * xstride2); i < ix2e; i++, pos += xstride2)
                                    {
                                        Maximum.Max(xstride2, wspw, pos, yw, ypos2, yw, ypos2);
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if !NOLEARNING
            void Gradient(Tensor y)
            {
                int ksize1 = kernel.Width;
                int ksize2 = kernel.Height;
                int kstride1 = kernel.StrideX;
                int kstride2 = kernel.StrideY;

                int x0 = x.Axes[0];
                int x1 = x.Axes[1];
                int x2 = x.Axes[2];

                int xstride0 = x.Strides[0];
                int xstride1 = x.Strides[1];
                int xstride2 = x.Strides[2];

                int xstride1K = xstride1 * kstride1;
                int xstride2K = xstride2 * kstride2;

                int y1 = y.Axes[1];
                int y2 = y.Axes[2];

                int ystride0 = y.Strides[0];
                int ystride1 = y.Strides[1];
                int ystride2 = y.Strides[2];

                float[] xw = x.Weights;
                float[] yw = y.Weights;
                float[] dxw = x.Gradient;
                float[] dyw = y.Gradient;

                // cycle by the output
                for (int ix0 = 0, ypos0 = 0, xpos0 = 0; ix0 < x0; ix0++, ypos0 += ystride0, xpos0 += xstride0)
                {
                    for (int iy1 = 0, ix1 = 0, ypos1 = ypos0, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, ypos1 += ystride1, xpos1 += xstride1K)
                    {
                        for (int iy2 = 0, ix2 = 0, ypos2 = ypos1, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, ypos2 += ystride2, xpos2 += xstride2K)
                        {
                            // cycle by the kernel
                            int ike1 = Maximum.Min(ix1 + ksize1, x1);
                            int ike2 = Maximum.Min(ix2 + ksize2, x2);
                            for (int ik1 = ix1, xpos1K = xpos2; ik1 < ike1; ik1++, xpos1K += xstride1)
                            {
                                for (int ik2 = ix2, xpos2K = xpos1K; ik2 < ike2; ik2++, xpos2K += xstride2)
                                {
                                    // cycle inside the kernel
                                    Mathematics.MatchAndAdd(xstride2, dyw, yw, ypos2, dxw, xw, xpos2K);
                                }
                            }
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Zeros out random weights in the tensor.
        /// </summary>
        /// <param name="session">The graph this operation should be added to.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="probability">The probability <c>p</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        /// <remarks>
        /// <para>
        /// During learning stage, weights are either "dropped out" with probability 1-<c>p</c> or kept with probability <c>p</c>.
        /// </para>
        /// <para>
        /// During testing stage, weights are multiplied by <c>p</c>.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Dropout(this Session session, Tensor x, RandomNumberGenerator random, float probability)
        {
            const string ActionName = "dropout";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);

                    float[] xw = x.Weights;
                    float[] yw = y.Weights;

#if !NOLEARNING
                    if (session.CalculateGradients)
                    {
                        lock (random)
                        {
                            for (int i = 0, ii = x.Length; i < ii; i++)
                            {
                                yw[i] = random.Generate() >= probability ? 0.0f : xw[i];
                            }
                        }

                        if (calculateGradient)
                        {
                            session.Push(
                                ActionName,
                                () =>
                                {
                                    float[] dxw = x.Gradient;
                                    float[] dyw = y.Gradient;

                                    for (int i = 0, ii = x.Length; i < ii; i++)
                                    {
                                        dxw[i] += xw[i] == yw[i] ? dyw[i] : 0.0f;
                                    }
                                });
                        }
                    }
                    else
#endif
                    {
                        Mathematics.Multiply(x.Length, probability, xw, 0, yw, 0);
                    }

                    return y;
                });
        }

        /// <summary>
        /// Applies local response normalization filter.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="kernelSize">The number of channels to normalize across. Should be odd number.</param>
        /// <param name="alpha">The α parameter.</param>
        /// <param name="beta">The β parameter.</param>
        /// <param name="k">The k parameter.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method performs operation defined as:
        /// </para>
        /// <para>
        /// y(i) = x(i) * (k + (alpha / kernelSize) * sum(x(j) ^ 2)) ^ -beta.
        /// </para>
        /// <para>
        /// The gradient operation is defined as:
        /// </para>
        /// <para>
        /// dx(i) = scale(i) ^ -beta * dy(i) - (2 * alpha * beta / kernelSize) * x(i) * sum(y(j) * dy(j) / scale(j)).
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Need to pass as a reference to reallocate.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor LRN(
            this Session session,
            Tensor x,
            int kernelSize,
            float alpha,
            float beta,
            float k)
        {
            const string ActionName = "lrn";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Tensor scale = session.AllocateTensor("lrn wsp", x.Axes, false);

                    // 1. calculate scale
                    // scale(i) = k + alpha / n * sum(x(j) ^ 2)
                    // scale will be later reused in back-propagation
                    // use output as a temporary buffer
                    Mathematics.Square(x.Length, x.Weights, 0, scale.Weights, 0);
                    NeuralOperations.LRNKernel(scale, scale.Weights, y.Weights, kernelSize);
                    scale.Set(k);
                    scale.MultiplyAndAdd(alpha / kernelSize, y);

                    // 2. calculate forward tensor
                    // y(i) = x(i) * scale(i) ^ -beta
                    Mathematics.Pow(scale.Length, scale.Weights, 0, -beta, y.Weights, 0);
                    y.Multiply(x);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                Tensor work = session.AllocateTensor("lrn wsp2", x.Axes, false);

                                // 1. calculate x(i) * sum(y(j) * dy(j) / scale(j))
                                // use dx as a temporary buffer
                                Mathematics.Multiply(y.Length, y.Weights, 0, y.Gradient, 0, x.Gradient, 0);
                                Mathematics.Divide(x.Length, x.Gradient, 0, scale.Weights, 0, x.Gradient, 0);

                                NeuralOperations.LRNKernel(x, x.Gradient, work.Weights, kernelSize);
                                work.Multiply(x);

                                // 2. calculate scale(i) ^ -beta * dy(i)
                                Mathematics.Pow(scale.Length, scale.Weights, 0, -beta, x.Gradient, 0);
                                Mathematics.Multiply(x.Length, x.Gradient, 0, y.Gradient, 0, x.Gradient, 0);

                                // 3. calculate final sum
                                Mathematics.MultiplyAndAdd(x.Length, -2.0f * alpha * beta / kernelSize, work.Weights, 0, x.Gradient, 0);
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Calculates softmax probabilities for each batch in one tensor and stores calculated values in another tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor SoftMax(this Session session, Tensor x)
        {
            const string ActionName = "soft max";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);

                    if (x.Rank == 1)
                    {
                        Maximum.SoftMax(x.Length, x.Weights, 0, y.Weights, 0);
                    }
                    else
                    {
                        int mb = x.Axes[0];
                        int stride = x.Strides[0];

                        float[] xw = x.Weights;
                        float[] yw = y.Weights;

                        for (int b = 0, bi = 0; b < mb; b++, bi += stride)
                        {
                            Maximum.SoftMax(stride, xw, bi, yw, bi);
                        }
                    }

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Arrays.Copy(x.Length, y.Gradient, 0, x.Gradient, 0));

                        /* TODO:
                                Mathematics.Add(x.Length, y.Gradient, 0, x.Gradient, 0);
                                ////MKL.Subtract(y.Length, y.Weights, 0, y.Gradient.Weights, 0, x.Gradient.Weights, 0);
                                */
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// The local response normalization kernel.
        /// </summary>
        /// <param name="shape">The input tensor shapes.</param>
        /// <param name="xw">The input tensor weights.</param>
        /// <param name="yw">The output tensor weights.</param>
        /// <param name="kernelSize">The number of channels to normalize across. Should be odd number.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LRNKernel(Shape shape, float[] xw, float[] yw, int kernelSize)
        {
            int x0 = shape.Axes[0];
            int x1 = shape.Axes[1];
            int x2 = shape.Axes[2];
            int x3 = shape.Axes[3];
            int xstride0 = shape.Strides[0];
            int xstride1 = shape.Strides[1];
            int xstride2 = shape.Strides[2];
            int xstride3 = shape.Strides[3];

            CommonParallel.For(
                0,
                x0,
                (a, b) =>
                {
                    for (; a < b; a++)
                    {
                        NativeMethods.LRNKernel(
                            xw,
                            yw,
                            kernelSize,
                            a/*x0*/,
                            x1,
                            x2,
                            x3,
                            xstride0,
                            xstride1,
                            xstride2,
                            xstride3);
                    }
                },
                CancellationToken.None);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.DNN.Native.dll";

            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Use DNN notation.")]
            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void LRNKernel(
                [In] float[] x,
                [In] float[] y,
                int kernelSize,
                int B,
                int W,
                int H,
                int C,
                int BStride,
                int WStride,
                int HStride,
                int CStride);
        }
    }
}