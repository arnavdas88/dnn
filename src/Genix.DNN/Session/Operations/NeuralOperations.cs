﻿// -----------------------------------------------------------------------
// <copyright file="NeuralOperations.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define NOLEARNING

namespace Genix.DNN
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading.Tasks;
    using Genix.Core;
    using Genix.MachineLearning;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor AveragePooling(this Session session, Tensor x, Kernel kernel)
        {
            const string ActionName = "avg pool";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, kernel.CalculateOutputShape(x.Shape), calculateGradient);

                    switch (x.Shape.Format)
                    {
                        case Shape.BWHC:
                            NativeMethods.avgpooling(
                                kernel.Width,
                                kernel.Height,
                                kernel.StrideX,
                                kernel.StrideY,
                                kernel.PaddingX,
                                kernel.PaddingY,
                                x.Weights,
                                x.Axes,
                                x.Strides,
                                y.Weights,
                                y.Axes,
                                y.Strides);
                            break;

                        case Shape.BHWC:
                            NativeMethods.avgpooling(
                                kernel.Height,
                                kernel.Width,
                                kernel.StrideY,
                                kernel.StrideX,
                                kernel.PaddingY,
                                kernel.PaddingX,
                                x.Weights,
                                x.Axes,
                                x.Strides,
                                y.Weights,
                                y.Axes,
                                y.Strides);
                            break;

                        case Shape.BCHW:
                            NativeMethods.avgpooling(
                                kernel.Height,
                                kernel.Width,
                                kernel.StrideY,
                                kernel.StrideX,
                                kernel.PaddingY,
                                kernel.PaddingX,
                                x.Weights,
                                new[] { x.Axes[0] * x.Axes[1], x.Axes[2], x.Axes[3] },
                                new[] { x.Strides[1], x.Strides[2], x.Strides[3] },
                                y.Weights,
                                new[] { y.Axes[0] * y.Axes[1], y.Axes[2], y.Axes[3] },
                                new[] { y.Strides[1], y.Strides[2], y.Strides[3] });
                            break;

                        default:
                            throw new NotSupportedException("The tensor shape is not supported by this operation.");
                    }

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                switch (x.Shape.Format)
                                {
                                    case Shape.BWHC:
                                        NativeMethods.avgpooling_gradient(
                                            kernel.Width,
                                            kernel.Height,
                                            kernel.StrideX,
                                            kernel.StrideY,
                                            kernel.PaddingX,
                                            kernel.PaddingY,
                                            x.Gradient,
                                            x.Axes,
                                            x.Strides,
                                            y.Gradient,
                                            y.Axes,
                                            y.Strides);
                                        break;

                                    case Shape.BHWC:
                                        NativeMethods.avgpooling_gradient(
                                            kernel.Height,
                                            kernel.Width,
                                            kernel.StrideY,
                                            kernel.StrideX,
                                            kernel.PaddingY,
                                            kernel.PaddingX,
                                            x.Gradient,
                                            x.Axes,
                                            x.Strides,
                                            y.Gradient,
                                            y.Axes,
                                            y.Strides);
                                        break;

                                    case Shape.BCHW:
                                        NativeMethods.avgpooling_gradient(
                                            kernel.Height,
                                            kernel.Width,
                                            kernel.StrideY,
                                            kernel.StrideX,
                                            kernel.PaddingY,
                                            kernel.PaddingX,
                                            x.Gradient,
                                            new[] { x.Axes[0] * x.Axes[1], x.Axes[2], x.Axes[3] },
                                            new[] { x.Strides[1], x.Strides[2], x.Strides[3] },
                                            y.Gradient,
                                            new[] { y.Axes[0] * y.Axes[1], y.Axes[2], y.Axes[3] },
                                            new[] { y.Strides[1], y.Strides[2], y.Strides[3] });
                                        break;
                                }
                            });
                    }
#endif

                    return y;
                });
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor MaxPooling(this Session session, Tensor x, Kernel kernel)
        {
            const string ActionName = "max pool";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, kernel.CalculateOutputShape(x.Shape), calculateGradient);

                    switch (x.Shape.Format)
                    {
                        case Shape.BWHC:
                            NativeMethods.maxpooling(
                                kernel.Width,
                                kernel.Height,
                                kernel.StrideX,
                                kernel.StrideY,
                                kernel.PaddingX,
                                kernel.PaddingY,
                                x.Weights,
                                x.Axes,
                                x.Strides,
                                y.Weights,
                                y.Axes,
                                y.Strides);
                            break;

                        case Shape.BHWC:
                            NativeMethods.maxpooling(
                                kernel.Height,
                                kernel.Width,
                                kernel.StrideY,
                                kernel.StrideX,
                                kernel.PaddingY,
                                kernel.PaddingX,
                                x.Weights,
                                x.Axes,
                                x.Strides,
                                y.Weights,
                                y.Axes,
                                y.Strides);
                            break;

                        case Shape.BCHW:
                            NativeMethods.maxpooling(
                                kernel.Height,
                                kernel.Width,
                                kernel.StrideY,
                                kernel.StrideX,
                                kernel.PaddingY,
                                kernel.PaddingX,
                                x.Weights,
                                new[] { x.Axes[0] * x.Axes[1], x.Axes[2], x.Axes[3] },
                                new[] { x.Strides[1], x.Strides[2], x.Strides[3] },
                                y.Weights,
                                new[] { y.Axes[0] * y.Axes[1], y.Axes[2], y.Axes[3] },
                                new[] { y.Strides[1], y.Strides[2], y.Strides[3] });
                            break;

                        default:
                            throw new NotSupportedException("The tensor shape is not supported by this operation.");
                    }
#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                switch (x.Shape.Format)
                                {
                                    case Shape.BWHC:
                                        NativeMethods.maxpooling_gradient(
                                            kernel.Width,
                                            kernel.Height,
                                            kernel.StrideX,
                                            kernel.StrideY,
                                            kernel.PaddingX,
                                            kernel.PaddingY,
                                            x.Weights,
                                            x.Gradient,
                                            x.Axes,
                                            x.Strides,
                                            y.Weights,
                                            y.Gradient,
                                            y.Axes,
                                            y.Strides);
                                        break;

                                    case Shape.BHWC:
                                        NativeMethods.maxpooling_gradient(
                                            kernel.Height,
                                            kernel.Width,
                                            kernel.StrideY,
                                            kernel.StrideX,
                                            kernel.PaddingY,
                                            kernel.PaddingX,
                                            x.Weights,
                                            x.Gradient,
                                            x.Axes,
                                            x.Strides,
                                            y.Weights,
                                            y.Gradient,
                                            y.Axes,
                                            y.Strides);
                                        break;

                                    case Shape.BCHW:
                                        NativeMethods.maxpooling_gradient(
                                            kernel.Height,
                                            kernel.Width,
                                            kernel.StrideY,
                                            kernel.StrideX,
                                            kernel.PaddingY,
                                            kernel.PaddingX,
                                            x.Weights,
                                            x.Gradient,
                                            new[] { x.Axes[0] * x.Axes[1], x.Axes[2], x.Axes[3] },
                                            new[] { x.Strides[1], x.Strides[2], x.Strides[3] },
                                            y.Weights,
                                            y.Gradient,
                                            new[] { y.Axes[0] * y.Axes[1], y.Axes[2], y.Axes[3] },
                                            new[] { y.Strides[1], y.Strides[2], y.Strides[3] });
                                        break;
                                }
                            });
                    }
#endif

                    return y;
                });
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Dropout(this Session session, Tensor x, RandomNumberGenerator<float> random, float probability)
        {
            const string ActionName = "dropout";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Shape, calculateGradient);

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
                        Mathematics.MulC(x.Length, xw, 0, probability, yw, 0);
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

                    Tensor y = session.AllocateTensor(ActionName, x.Shape, calculateGradient);
                    Tensor scale = session.AllocateTensor("lrn wsp", x.Shape, false);

                    // 1. calculate scale
                    // scale(i) = k + alpha / n * sum(x(j) ^ 2)
                    // scale will be later reused in back-propagation
                    // use output as a temporary buffer
                    Vectors.Square(x.Length, x.Weights, 0, scale.Weights, 0);
                    NeuralOperations.LRNKernel(scale.Axes, scale.Strides, scale.Weights, y.Weights, kernelSize);
                    scale.Set(k);
                    scale.AddProductC(y, alpha / kernelSize);

                    // 2. calculate forward tensor
                    // y(i) = x(i) * scale(i) ^ -beta
                    Vectors.Pow(scale.Length, scale.Weights, 0, -beta, y.Weights, 0);
                    y.Mul(x);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                Tensor work = session.AllocateTensor("lrn wsp2", x.Shape, false);

                                // 1. calculate x(i) * sum(y(j) * dy(j) / scale(j))
                                // use dx as a temporary buffer
                                Mathematics.Mul(y.Length, y.Weights, 0, y.Gradient, 0, x.Gradient, 0);
                                Mathematics.Div(x.Length, scale.Weights, 0, x.Gradient, 0);

                                NeuralOperations.LRNKernel(x.Shape.Axes, x.Shape.Strides, x.Gradient, work.Weights, kernelSize);
                                work.Mul(x);

                                // 2. calculate scale(i) ^ -beta * dy(i)
                                Vectors.Pow(scale.Length, scale.Weights, 0, -beta, x.Gradient, 0);
                                Mathematics.Mul(x.Length, y.Gradient, 0, x.Gradient, 0);

                                // 3. calculate final sum
                                Mathematics.AddProductC(x.Length, work.Weights, 0, -2.0f * alpha * beta / kernelSize, x.Gradient, 0);
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes fully connected cell.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="w">The tensor that contains the weights matrix <paramref name="w"/>.</param>
        /// <param name="b">The tensor that contains the bias vector <paramref name="b"/> to add to each column of matrix <paramref name="w"/>. Can be null.</param>
        /// <param name="matrixLayout">Specifies whether the matrices <paramref name="w"/> and <paramref name="b"/> are row-major or column-major.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor FullyConnected(
            this Session session,
            Tensor x,
            Tensor w,
            Tensor b,
            MatrixLayout matrixLayout)
        {
            // calculate output tensor in column-major mode
            // y += W * x (product of weight and input matrices)
            // input and output matrices are column major (one column per mini-batch item)
            // weights matrix might have to be transposed to have a row per neuron
            return session.MxM(
                MatrixLayout.ColumnMajor,
                w,
                matrixLayout == MatrixLayout.RowMajor,
                x,
                false,
                b);
        }

        /// <summary>
        /// Computes a convolution cell.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="w">The tensor that contains the weights matrix <paramref name="w"/>.</param>
        /// <param name="b">The tensor that contains the bias vector <paramref name="b"/> to add to each column of matrix <paramref name="w"/>. Can be null.</param>
        /// <param name="kernel">The convolution kernel.</param>
        /// <param name="numberOfFilters">The number of filters in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the matrices <paramref name="w"/> and <paramref name="b"/> are row-major or column-major.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Convolution(
            this Session session,
            Tensor x,
            Tensor w,
            Tensor b,
            Kernel kernel,
            int numberOfFilters,
            MatrixLayout matrixLayout)
        {
            const string ActionName = "convolution";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients;

                    Tensor y = session.AllocateTensor(
                        ActionName,
                        new Shape(
                            x.Shape.Format,
                            x.Shape.GetAxis(Axis.B),
                            kernel.CalculateOutputWidth(x.Shape.GetAxis(Axis.X)),
                            kernel.CalculateOutputHeight(x.Shape.GetAxis(Axis.Y)),
                            numberOfFilters),
                        calculateGradient);

                    NativeMethods.convolution(
                                kernel.Width,
                                kernel.Height,
                                kernel.StrideX,
                                kernel.StrideY,
                                kernel.PaddingX,
                                kernel.PaddingY,
                                w.Weights,
                                b.Weights,
                                w.Axes,
                                w.Strides,
                                x.Weights,
                                x.Axes,
                                x.Strides,
                                y.Weights,
                                y.Axes,
                                y.Strides);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                lock (w)
                                {
                                    lock (b)
                                    {
                                        NativeMethods.convolution_gradient(
                                            kernel.Width,
                                            kernel.Height,
                                            kernel.StrideX,
                                            kernel.StrideY,
                                            kernel.PaddingX,
                                            kernel.PaddingY,
                                            w.Weights,
                                            w.Gradient,
                                            b.Gradient,
                                            w.Axes,
                                            w.Strides,
                                            x.Weights,
                                            x.CalculateGradient ? x.Gradient : null,
                                            x.Axes,
                                            x.Strides,
                                            y.Gradient,
                                            y.Axes,
                                            y.Strides);
                                    }
                                }
                            });
                    }
#endif
                    return y;
                });
        }

        /// <summary>
        /// Computes SRN (simple recurrent network) cell.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="w">The tensor that contains the weights matrix <paramref name="w"/>.</param>
        /// <param name="u">The tensor that contains the hidden weights matrix <paramref name="u"/>.</param>
        /// <param name="b">The tensor that contains the bias vector <paramref name="b"/> to add to each column of matrix <paramref name="w"/>. Can be null.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the matrices <paramref name="w"/>, <paramref name="b"/>, and <paramref name="u"/> are row-major or column-major.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor SRN(
            this Session session,
            Tensor x,
            Tensor w,
            Tensor u,
            Tensor b,
            int numberOfNeurons,
            MatrixLayout matrixLayout)
        {
            const string ActionName = "srn";

            // calculate gates = W * x + b
            Tensor y = session.FullyConnected(x, w, b, matrixLayout);

            int tt = y.Shape.GetAxis(Axis.B);               // number of vectors in time sequence
            float[] uw = u.Weights;
            float[] yw = y.Weights;

            // add hidden layer to the output tensor
            // y += U * y(t-1) (product of hidden weight matrix and hidden vector)
            Nonlinearity.ReLU(numberOfNeurons, yw, 0, yw, 0);

            for (int t = 1, yi = numberOfNeurons; t < tt; t++, yi += numberOfNeurons)
            {
                Matrix.MxV(matrixLayout, numberOfNeurons, numberOfNeurons, uw, 0, false, yw, yi - numberOfNeurons, yw, yi, false);

                // TODO: customize activation function
                Nonlinearity.ReLU(numberOfNeurons, yw, yi, yw, yi);
            }

            if (session.CalculateGradients)
            {
                session.Push(
                    ActionName,
                    () =>
                    {
                        float[] duw = u.Gradient;
                        float[] dyw = y.Gradient;

                        for (int t = tt - 1, yi = t * numberOfNeurons; t > 0; t--, yi -= numberOfNeurons)
                        {
                            Nonlinearity.ReLUGradient(numberOfNeurons, dyw, yi, true, yw, yi, dyw, yi);

                            // dA += dy * x'
                            lock (u)
                            {
                                Matrix.VxV(matrixLayout, numberOfNeurons, numberOfNeurons, dyw, yi, yw, yi - numberOfNeurons, duw, 0, false);
                            }

                            // dx += A' * dy
                            Matrix.MxV(matrixLayout, numberOfNeurons, numberOfNeurons, uw, 0, true, dyw, yi, dyw, yi - numberOfNeurons, false);
                        }

                        Nonlinearity.ReLUGradient(numberOfNeurons, dyw, 0, true, yw, 0, dyw, 0);
                    });
            }

            return y;
        }

        /// <summary>
        /// Computes LSTM (long short-term memory) cell.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="w">The tensor that contains the weights matrix <paramref name="w"/>.</param>
        /// <param name="u">The tensor that contains the hidden weights matrix <paramref name="u"/>.</param>
        /// <param name="b">The tensor that contains the bias vector <paramref name="b"/> to add to each column of matrix <paramref name="w"/>. Can be null.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="forgetBias">The bias to add to forget gates.</param>
        /// <param name="matrixLayout">Specifies whether the matrices <paramref name="w"/>, <paramref name="b"/>, and <paramref name="u"/> are row-major or column-major.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor LSTM(
            this Session session,
            Tensor x,
            Tensor w,
            Tensor u,
            Tensor b,
            RNNDirection direction,
            int numberOfNeurons,
            float forgetBias,
            MatrixLayout matrixLayout)
        {
            const string ActionName = "lstm";

            // calculate gates = W * x + b
            Tensor g = session.FullyConnected(x, w, b, matrixLayout);

            int tt = g.Shape.GetAxis(0);                  // number of vectors in time sequence

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients;

                    Tensor h = session.AllocateTensor("lstm", new Shape(new int[] { tt, numberOfNeurons }), calculateGradient);
                    Tensor s = session.AllocateTensor("lstm cell", h.Shape, calculateGradient);

                    NativeMethods.lstm(
                        tt,
                        numberOfNeurons,
                        u.Weights,
                        g.Weights,
                        s.Weights,
                        h.Weights,
                        forgetBias,
                        true, ////direction == RNNDirection.BiDirectional,
                        matrixLayout == MatrixLayout.RowMajor);

                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                NativeMethods.lstm_gradient(
                                    tt,
                                    numberOfNeurons,
                                    u.Weights,
                                    u.Gradient,
                                    g.Weights,
                                    g.Gradient,
                                    s.Weights,
                                    s.Gradient,
                                    h.Weights,
                                    h.Gradient,
                                    true, ////direction == RNNDirection.BiDirectional,
                                    matrixLayout == MatrixLayout.RowMajor);
                            });
                    }

                    return h;
                });
        }

        /// <summary>
        /// Computes GRU (gated recurrent unit) cell.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="w">The tensor that contains the weights matrix <paramref name="w"/>.</param>
        /// <param name="u">The tensor that contains the hidden weights matrix <paramref name="u"/>.</param>
        /// <param name="b">The tensor that contains the bias vector <paramref name="b"/> to add to each column of matrix <paramref name="w"/>. Can be null.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the matrices <paramref name="w"/>, <paramref name="b"/>, and <paramref name="u"/> are row-major or column-major.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor GRU(
            this Session session,
            Tensor x,
            Tensor w,
            Tensor u,
            Tensor b,
            RNNDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout)
        {
            const string ActionName = "gru";

            // calculate gates = W * x + b
            Tensor g = session.FullyConnected(x, w, b, matrixLayout);

            int tt = g.Shape.GetAxis(0);                  // number of vectors in time sequence

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients;

                    Tensor h = session.AllocateTensor(ActionName, new Shape(new int[] { tt, numberOfNeurons }), calculateGradient);

                    NativeMethods.gru(
                        tt,
                        numberOfNeurons,
                        u.Weights,
                        g.Weights,
                        h.Weights,
                        direction == RNNDirection.BiDirectional,
                        matrixLayout == MatrixLayout.RowMajor);

                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                NativeMethods.gru_gradient(
                                    tt,
                                    numberOfNeurons,
                                    u.Weights,
                                    u.Gradient,
                                    g.Weights,
                                    g.Gradient,
                                    h.Weights,
                                    h.Gradient,
                                    direction == RNNDirection.BiDirectional,
                                    matrixLayout == MatrixLayout.RowMajor);
                            });
                    }

                    return h;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor SoftMax(this Session session, Tensor x)
        {
            const string ActionName = "soft max";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Shape, calculateGradient);

                    if (x.Rank == 1)
                    {
                        Vectors.SoftMax(x.Length, x.Weights, 0, y.Weights, 0);
                    }
                    else
                    {
                        Vectors.SoftMax(x.Length, x.Weights, 0, x.Strides[0], y.Weights, 0);
                    }

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => x.SetGradient(y.Gradient));

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
        /// <param name="axes">The input tensor dimensions.</param>
        /// <param name="strides">The input tensor strides.</param>
        /// <param name="xw">The input tensor weights.</param>
        /// <param name="yw">The output tensor weights.</param>
        /// <param name="kernelSize">The number of channels to normalize across. Should be odd number.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LRNKernel(int[] axes, int[] strides, float[] xw, float[] yw, int kernelSize)
        {
            CommonParallel.For(
                0,
                axes[0],
                (a, b) =>
                {
                    for (; a < b; a++)
                    {
                        NativeMethods.LRNKernel(
                            xw,
                            yw,
                            kernelSize,
                            a,
                            axes,
                            strides);
                    }
                },
                new ParallelOptions());
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.DNN.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void LRNKernel(
                [In] float[] x,
                [In] float[] y,
                int kernelSize,
                int b,
                [In] int[] axes,
                [In] int[] strides);

            [DllImport(NativeMethods.DllName)]
            public static extern void maxpooling(
                int ksize1,
                int ksize2,
                int kstride1,
                int kstride2,
                int kpadding1,
                int kpadding2,
                [In] float[] xw,
                [In] int[] xaxes,
                [In] int[] xstrides,
                [Out] float[] yw,
                [In] int[] yaxes,
                [In] int[] ystrides);

            [DllImport(NativeMethods.DllName)]
            public static extern void maxpooling_gradient(
                int ksize1,
                int ksize2,
                int kstride1,
                int kstride2,
                int kpadding1,
                int kpadding2,
                [In] float[] xw,
                [In, Out] float[] dxw,
                [In] int[] xaxes,
                [In] int[] xstrides,
                [In] float[] yw,
                [In] float[] dyw,
                [In] int[] yaxes,
                [In] int[] ystrides);

            [DllImport(NativeMethods.DllName)]
            public static extern void avgpooling(
                int ksize1,
                int ksize2,
                int kstride1,
                int kstride2,
                int kpadding1,
                int kpadding2,
                [In] float[] xw,
                [In] int[] xaxes,
                [In] int[] xstrides,
                [Out] float[] yw,
                [In] int[] yaxes,
                [In] int[] ystrides);

            [DllImport(NativeMethods.DllName)]
            public static extern void avgpooling_gradient(
                int ksize1,
                int ksize2,
                int kstride1,
                int kstride2,
                int kpadding1,
                int kpadding2,
                [In, Out] float[] dxw,
                [In] int[] xaxes,
                [In] int[] xstrides,
                [In] float[] dyw,
                [In] int[] yaxes,
                [In] int[] ystrides);

            [DllImport(NativeMethods.DllName)]
            public static extern void convolution(
                int ksize1,
                int ksize2,
                int kstride1,
                int kstride2,
                int kpadding1,
                int kpadding2,
                [In] float[] ww,
                [In] float[] bw,
                [In] int[] waxes,
                [In] int[] wstrides,
                [In] float[] xw,
                [In] int[] xaxes,
                [In] int[] xstrides,
                [Out] float[] yw,
                [In] int[] yaxes,
                [In] int[] ystrides);

            [DllImport(NativeMethods.DllName)]
            public static extern void convolution_gradient(
                int ksize1,
                int ksize2,
                int kstride1,
                int kstride2,
                int kpadding1,
                int kpadding2,
                [In] float[] ww,
                [Out] float[] dww,
                [Out] float[] dbw,
                [In] int[] waxes,
                [In] int[] wstrides,
                [In] float[] xw,
                [Out] float[] dxw,
                [In] int[] xaxes,
                [In] int[] xstrides,
                [In] float[] dyw,
                [In] int[] yaxes,
                [In] int[] ystrides);

            [DllImport(NativeMethods.DllName)]
            public static extern void lstm(
                int steps,
                int ylen,
                [In] float[] u,
                [Out] float[] g,
                [Out] float[] s,
                [Out] float[] y,
                float forgetBias,
                [MarshalAs(UnmanagedType.Bool)] bool forward,
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor);

            [DllImport(NativeMethods.DllName)]
            public static extern void lstm_gradient(
                int steps,
                int ylen,
                [In] float[] u,
                [Out] float[] du,
                [In] float[] g,
                [Out] float[] dg,
                [In] float[] s,
                [Out] float[] ds,
                [In] float[] y,
                [Out] float[] dy,
                [MarshalAs(UnmanagedType.Bool)] bool forward,
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor);

            [DllImport(NativeMethods.DllName)]
            public static extern void gru(
                int steps,
                int ylen,
                [In] float[] u,
                [In] float[] g,
                [Out] float[] y,
                [MarshalAs(UnmanagedType.Bool)] bool bidirectional,
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor);

            [DllImport(NativeMethods.DllName)]
            public static extern void gru_gradient(
                int steps,
                int ylen,
                [In] float[] u,
                [Out] float[] du,
                [In] float[] g,
                [Out] float[] dg,
                [In] float[] y,
                [Out] float[] dy,
                [MarshalAs(UnmanagedType.Bool)] bool bidirectional,
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor);
        }
    }
}