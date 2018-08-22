// -----------------------------------------------------------------------
// <copyright file="ArrayOperations.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define NOLEARNING

namespace Genix.MachineLearning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Represents array operations on tensors.
    /// </summary>
    public static class ArrayOperations
    {
        /// <summary>
        /// Copies a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to copy.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Copy(this Session session, Tensor x)
        {
            const string ActionName = "copy";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Array32f.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Math32f.Add(x.Length, y.Gradient, 0, x.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Repeats a tensor multiple times.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to repeat.</param>
        /// <param name="count">The number of times to repeat <c>x</c>.</param>
        /// <returns>
        /// The collection of repeated <see cref="Tensor"/> objects.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor[] Repeat(this Session session, Tensor x, int count)
        {
            const string ActionName = "repeat";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor[] ys = session.AllocateTensors(ActionName, count, x.Axes, calculateGradient);
                    for (int i = 0; i < count; i++)
                    {
                        Array32f.Copy(x.Length, x.Weights, 0, ys[i].Weights, 0);
                    }

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                float alpha = 1.0f / count;
                                for (int i = 0; i < count; i++)
                                {
                                    Math32f.AddProductC(x.Length, ys[i].Gradient, 0, alpha, x.Gradient, 0);
                                }
                            });

                        // return copy of the array; calling method can replace its content; our closure keeps the array, not its items
                        return ys.ToArray();
                    }
#endif

                    return ys;
                });
        }

        /// <summary>
        /// Concatenates the group of tensors along the specified axis.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="xs">The tensors to concatenate.</param>
        /// <param name="axis">The axis to concatenate along.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains values from input tensors stacked along <c>axis</c> dimension.
        /// The tensor shape matches that of <c>xs</c> except in <c>axis</c> where is has the sum of the sizes.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The input tensors ranks and sizes must match in all dimensions except <c>axis</c>.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Concat(this Session session, IList<Tensor> xs, int axis)
        {
            const string ActionName = "concat";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && xs.Any(x => x.CalculateGradient);

                    int[] yaxes = Shape.Concat(xs.Select(x => x.Axes).ToArray(), axis);
                    Tensor y = session.AllocateTensor(ActionName, yaxes, calculateGradient);

                    ArrayOperations.Concat(xs, axis, y, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Split(y, axis, xs, true));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Splits a tensor along the specified axis into the collection of sub tensors.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to split.</param>
        /// <param name="axis">The axis to split along.</param>
        /// <param name="sizes">The sizes of sub tensors along dimension <c>axis</c>.</param>
        /// <returns>
        /// The collection of <see cref="Tensor"/> objects that contains sub tensors.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor[] Split(this Session session, Tensor x, int axis, int[] sizes)
        {
            const string ActionName = "split";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // check sizes
                    int ydim = sizes.Length;
                    int sum = 0;
                    for (int i = 0; i < ydim; i++)
                    {
                        sum += sizes[i];
                    }

                    if (sum != x.Axes[axis])
                    {
                        throw new ArgumentException("The sub tensors sizes must be provided.");
                    }

                    // allocate destination
                    Tensor[] ys = new Tensor[ydim];
                    for (int i = 0; i < ydim; i++)
                    {
                        int[] yaxes = Shape.Reshape(x.Axes, axis, sizes[i]);
                        ys[i] = session.AllocateTensor(ActionName, yaxes, calculateGradient);
                    }

                    ArrayOperations.Split(x, axis, ys, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Concat(ys, axis, x, true));

                        // return copy of the array; calling method can replace its content; our closure keeps the array, not its items
                        return ys.ToArray();
                    }
#endif

                    return ys;
                });
        }

        /// <summary>
        /// Splits a tensor along the specified axis into the collection of sub tensors.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to split.</param>
        /// <param name="axis">The axis to split along.</param>
        /// <param name="numberOfSplits">The number of tensors to split into along dimension <c>axis</c>.</param>
        /// <returns>
        /// The collection of <see cref="Tensor"/> objects that contains sub tensors.
        /// </returns>
        /// <remarks>
        /// Requires that <c>numberOfSplits</c> evenly divides the tensor shape along dimension <c>axis</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor[] Split(this Session session, Tensor x, int axis, int numberOfSplits)
        {
            const string ActionName = "split";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            // check number of splits
            if ((x.Axes[axis] % numberOfSplits) != 0)
            {
                throw new ArgumentException("The number of tensors to split into must evenly divide the tensor split dimension.", nameof(numberOfSplits));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    int[] yaxes = Shape.Reshape(x.Axes, axis, x.Axes[axis] / numberOfSplits);
                    Tensor[] ys = session.AllocateTensors(ActionName, numberOfSplits, yaxes, calculateGradient);

                    ArrayOperations.Split(x, axis, ys, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Concat(ys, axis, x, true));

                        // return copy of the array; calling method can replace its content; our closure keeps the array, not its items
                        return ys.ToArray();
                    }
#endif

                    return ys;
                });
        }

        /// <summary>
        /// Stacks a range of rank-N tensors into one rank-(N+1) tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="xs">The tensor to stack.</param>
        /// <param name="axis">The axis to stack along.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Stack(this Session session, IList<Tensor> xs, int axis)
        {
            const string ActionName = "stack";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && xs.Any(x => x.CalculateGradient);

                    // check source
                    int xdim = xs.Count;
                    if (xdim == 0)
                    {
                        throw new ArgumentException("There should be at least one source tensor.");
                    }

                    if (!Shape.AreSame(xs))
                    {
                        throw new ArgumentException("All source tensors must have the same rank and shape.");
                    }

                    int[] yaxes = Shape.Expand(xs[0].Axes, axis, xdim);
                    Tensor y = session.AllocateTensor(ActionName, yaxes, calculateGradient);

                    ArrayOperations.Stack(xs, axis, y, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Unstack(y, axis, xs, true));
                    }
#endif

                    y.Validate();

                    return y;
                });
        }

        /// <summary>
        /// Unpacks a rank-N tensor into a range of rank-(N-1) tensors along the specified dimension.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to unstack.</param>
        /// <param name="axis">The axis to unstack along.</param>
        /// <returns>
        /// The <see cref="Tensor"/> objects that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor[] Unstack(this Session session, Tensor x, int axis)
        {
            const string ActionName = "unstack";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    int[] yaxes = Shape.Remove(x.Axes, axis);
                    Tensor[] ys = session.AllocateTensors(ActionName, x.Axes[axis], yaxes, calculateGradient);

                    ArrayOperations.Unstack(x, axis, ys, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Stack(ys, axis, x, true));

                        // return copy of the array; calling method can replace its content; our closure keeps the array, not its items
                        return ys.ToArray();
                    }
#endif

                    return ys;
                });
        }

        /// <summary>
        /// Replicates a tensor in another tensor multiple times along the specified axis.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensors to copy values from.</param>
        /// <param name="axis">The axis to copy along.</param>
        /// <param name="count">The number of times to replicate values.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Tile(this Session session, Tensor x, int axis, int count)
        {
            const string ActionName = "tile";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (count == 1)
            {
                return ArrayOperations.Copy(session, x);
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    int[] yaxes = Shape.Reshape(x.Axes, axis, x.Axes[axis] * count);
                    Tensor y = session.AllocateTensor(ActionName, yaxes, calculateGradient);

                    ArrayOperations.Tile(x, axis, count, y, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Untile(y, axis, count, x, true));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Compacts a tensor along the specified axis by summing elements.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to compact.</param>
        /// <param name="axis">The axis to compact along.</param>
        /// <param name="count">The number of values to compact.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Untile(this Session session, Tensor x, int axis, int count)
        {
            const string ActionName = "untile";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (count == 1)
            {
                return ArrayOperations.Copy(session, x);
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    int xsize = x.Axes[axis];
                    if ((xsize % count) != 0)
                    {
                        throw new ArgumentException("Count must be a multiple of axis dimension.");
                    }

                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    int[] yaxes = Shape.Reshape(x.Axes, axis, xsize / count);
                    Tensor y = session.AllocateTensor(ActionName, yaxes, calculateGradient);

                    ArrayOperations.Untile(x, axis, count, y, false);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => ArrayOperations.Tile(y, axis, count, x, true));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Crops the three-dimensional sub-tensors from a specified tensor by using the specified kernel,
        /// and then stacks the sub-tensors batch-wise in the destination tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="kernel">The kernel to apply to <c>x</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor StackKernels(this Session session, Tensor x, Kernel kernel)
        {
            const string ActionName = "stack kernels";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = Stack(calculateGradient);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Gradient(y));
                    }
#endif

                    return y;
                });

            Tensor Stack(bool calculateGradient)
            {
                int ksize1 = kernel.Width;
                int ksize2 = kernel.Height;
                int kstride1 = kernel.StrideX;
                int kstride2 = kernel.StrideY;
                int kpadding1 = kernel.PaddingX;
                int kpadding2 = kernel.PaddingY;

                int x0 = x.Axes[0];
                int x1 = x.Axes[1];
                int x2 = x.Axes[2];
                int x3 = x.Axes[3];
                int xstride0 = x.Strides[0];
                int xstride1 = x.Strides[1];
                int xstride2 = x.Strides[2];

                int y1 = kernel.CalculateOutputWidth(x1);
                int y2 = kernel.CalculateOutputHeight(x2);

                Tensor y = session.AllocateTensor(ActionName, new[] { x0 * y1 * y2, ksize1, ksize2, x3 }, calculateGradient);

                int ystride0 = y.Strides[0];
                int ystride1 = y.Strides[1];
                int ystride2 = y.Strides[2];

                float[] xw = x.Weights;
                float[] yw = y.Weights;

                for (int ix0 = 0, xpos0 = (-kpadding1 * xstride1) + (-kpadding2 * xstride2), ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
                {
                    for (int iy1 = 0, ix1 = -kpadding1, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1)
                    {
                        for (int iy2 = 0, ix2 = -kpadding2, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2)
                        {
                            // optimized version for BWHC layout
                            // copy inner HC part of the tensor in one operation
                            for (int ixk = ix1, ixke = ixk + ksize1, xposk = xpos2, yposk = ypos0;
                                 ixk < ixke;
                                 ixk++, xposk += xstride1, yposk += ystride1)
                            {
                                if (ixk >= 0 && ixk < x1)
                                {
                                    int xposk2 = xposk;
                                    int yposk2 = yposk;

                                    int start = ix2;
                                    int end = ix2 + ksize2;

                                    if (start < 0)
                                    {
                                        int count = -start * xstride2;
                                        Array32f.Set(count, 0.0f, yw, yposk2);
                                        xposk2 += count;
                                        yposk2 += count;

                                        start = 0;
                                    }

                                    if (end > x2)
                                    {
                                        Array32f.Set((end - x2) * xstride2, 0.0f, yw, yposk2 + ((x2 - start) * xstride2));

                                        end = x2;
                                    }

                                    Array32f.Copy((end - start) * xstride2, xw, xposk2, yw, yposk2);
                                }
                                else
                                {
                                    Array32f.Set(ksize2 * xstride2, 0.0f, yw, yposk);
                                }
                            }

                            ypos0 += ystride0;
                        }
                    }
                }

                return y;
            }

#if !NOLEARNING
            void Gradient(Tensor y)
            {
                int ksize1 = kernel.Width;
                int ksize2 = kernel.Height;
                int kstride1 = kernel.StrideX;
                int kstride2 = kernel.StrideY;
                int kpadding1 = kernel.PaddingX;
                int kpadding2 = kernel.PaddingY;

                int x0 = x.Axes[0];
                int x1 = x.Axes[1];
                int x2 = x.Axes[2];
                int xstride0 = x.Strides[0];
                int xstride1 = x.Strides[1];
                int xstride2 = x.Strides[2];

                int y1 = kernel.CalculateOutputWidth(x1);
                int y2 = kernel.CalculateOutputHeight(x2);
                int ystride0 = y.Strides[0];
                int ystride1 = y.Strides[1];
                int ystride2 = y.Strides[2];

                float[] dxw = x.Gradient;
                float[] dyw = y.Gradient;

                for (int ix0 = 0, xpos0 = (-kpadding1 * xstride1) + (-kpadding2 * xstride2), ypos0 = 0; ix0 < x0; ix0++, xpos0 += xstride0)
                {
                    for (int iy1 = 0, ix1 = -kpadding1, xpos1 = xpos0; iy1 < y1; iy1++, ix1 += kstride1, xpos1 += xstride1)
                    {
                        for (int iy2 = 0, ix2 = -kpadding2, xpos2 = xpos1; iy2 < y2; iy2++, ix2 += kstride2, xpos2 += xstride2)
                        {
                            for (int ixk = ix1, ixke = ixk + ksize1, xposk = xpos2, yposk = ypos0;
                                 ixk < ixke;
                                 ixk++, xposk += xstride1, yposk += ystride1)
                            {
                                if (ixk >= 0 && ixk < x1)
                                {
                                    int xposk2 = xposk;
                                    int yposk2 = yposk;

                                    int start = ix2;
                                    int end = ix2 + ksize2;

                                    if (start < 0)
                                    {
                                        int count = -start * xstride2;
                                        xposk2 += count;
                                        yposk2 += count;

                                        start = 0;
                                    }

                                    if (end > x2)
                                    {
                                        end = x2;
                                    }

                                    Math32f.Add((end - start) * xstride2, dyw, yposk2, dxw, xposk2);
                                }
                            }

                            ypos0 += ystride0;
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Compacts a tensor along the specified axis by choosing a maximum value out of group of elements.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensors to copy values from.</param>
        /// <param name="axis">The axis to copy along.</param>
        /// <param name="count">The number of elements in the group.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor MaxReduce(this Session session, Tensor x, int axis, int count)
        {
            const string ActionName = "max reduce";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (count == 1)
            {
                return ArrayOperations.Copy(session, x);
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    int xaxis = x.Axes[axis];
                    int xstride = x.Strides[axis];

                    if ((xaxis % count) != 0)
                    {
                        throw new ArgumentException("The axis dimension must be a count multiple.");
                    }

                    int yaxis = xaxis / count;

                    // calculate destination layout
                    int[] axes = Shape.Reshape(x.Axes, axis, yaxis);

                    // allocate destination
                    Tensor y = session.AllocateTensor(ActionName, axes, calculateGradient);

                    float[] xw = x.Weights;
                    float[] yw = y.Weights;

                    if (xstride == 1)
                    {
                        int length = x.Length / count;
                        Arrays.Pack(length, xw, 0, count, yw, 0);

                        float[] wsp = new float[length];
                        for (int i = 1; i < count; i++)
                        {
                            Arrays.Pack(length, xw, i, count, wsp, 0);
                            Maximum.Max(length, yw, 0, wsp, 0, yw, 0);
                        }
                    }
                    else
                    {
                        for (int offx = 0, offy = 0, ylen = y.Length; offy < ylen; offy += xstride)
                        {
                            Array32f.Copy(xstride, xw, offx, yw, offy);
                            offx += xstride;

                            for (int i = 1; i < count; i++, offx += xstride)
                            {
                                Maximum.Max(xstride, yw, offy, xw, offx, yw, offy);
                            }
                        }
                    }

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                float[] dxw = x.Gradient;
                                float[] dyw = y.Gradient;

                                if (xstride == 1)
                                {
                                    for (int xpos = 0, ypos = 0, len = y.Length; ypos < len; ypos++)
                                    {
                                        for (int i = 0; i < count; i++, xpos++)
                                        {
                                            if (xw[xpos] == yw[ypos])
                                            {
                                                dxw[xpos] += dyw[ypos];
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int xpos = 0, ypos = 0, len = y.Length; ypos < len; ypos += xstride)
                                    {
                                        for (int i = 0; i < count; i++, xpos += xstride)
                                        {
                                            Mathematics.MatchAndAdd(xstride, dyw, yw, ypos, dxw, xw, xpos);
                                        }
                                    }
                                }
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Removes a dimension of size 1 from the shape of a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="axis">The dimension to remove.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Squeeze(this Session session, Tensor x, int axis)
        {
            const string ActionName = "squeeze";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (x.Rank < 2)
            {
                throw new ArgumentException("The tensor must have the rank of at least 2.", nameof(axis));
            }

            if (x.Axes[axis] != 1)
            {
                throw new ArgumentException("The dimension to remove must be of size 1.", nameof(axis));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    int[] axes = Shape.Remove(x.Axes, axis);
                    Tensor y = session.AllocateTensor(ActionName, axes, calculateGradient);

                    // simply copy tensor content
                    Array32f.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // simply copy tensors
                        session.Push(ActionName, () => Math32f.Add(x.Length, y.Gradient, 0, x.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Inserts a dimension of size 1 into the shape of a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data.</param>
        /// <param name="axis">The dimension to insert.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains computed data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Expand(this Session session, Tensor x, int axis)
        {
            const string ActionName = "expand";

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    int[] axes = Shape.Expand(x.Axes, axis, 1);
                    Tensor y = session.AllocateTensor(ActionName, axes, calculateGradient);

                    // simply copy tensor content
                    Array32f.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // simply copy tensor content
                        session.Push(ActionName, () => Math32f.Add(x.Length, y.Gradient, 0, x.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Changes the <see cref="Tensor"/> dimensions.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to reshape.</param>
        /// <param name="shape">The new <see cref="Tensor"/> dimensions.</param>
        /// <returns>
        /// The <c>x</c> tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Reshape(this Session session, Tensor x, params int[] shape)
        {
            const string ActionName = "reshape";

            // validate new shape
            if (Shape.ShapeLength(shape) != x.Length)
            {
                throw new ArgumentException("The size of new shape must be the same as tensor length.", nameof(shape));
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    Tensor y = session.AllocateTensor(ActionName, shape, calculateGradient);

                    // simply copy tensor content
                    Array32f.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // simply copy tensor content
                        session.Push(ActionName, () => Math32f.Add(x.Length, y.Gradient, 0, x.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Changes the <see cref="Tensor"/> dimensions.
        /// </summary>
        /// <param name="session">The graph this operation should be added to.</param>
        /// <param name="x">The tensor to reshape.</param>
        /// <param name="shape">The new <see cref="Tensor"/> dimensions.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReshapeIP(this Session session, Tensor x, params int[] shape)
        {
            const string ActionName = "reshape";

            // validate new shape
            if (Shape.ShapeLength(shape) != x.Length)
            {
                throw new ArgumentException("The size of new shape must be the same as tensor length.", nameof(shape));
            }

            session.RunOperation(
                ActionName,
                () =>
                {
#if !NOLEARNING
                    if (session.CalculateGradients && x.CalculateGradient)
                    {
                        int[] oldshape = x.Axes.ToArray();
                        if (x.Reshape(shape))
                        {
                            session.Push(ActionName, () => x.Reshape(oldshape));
                        }
                    }
                    else
#endif
                    {
                        x.Reshape(shape);
                    }

                    return (Tensor)null;    // have to return something
                });
        }

        /// <summary>
        /// Concatenates the group of tensors along the specified axis.
        /// </summary>
        /// <param name="xs">The tensors to concatenate.</param>
        /// <param name="axis">The axis to concatenate along.</param>
        /// <param name="y">The tensor that receives the data. Can be <b>null</b>.</param>
        /// <param name="useGradients">Specifies whether the gradient weights should be used for computation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Concat(IList<Tensor> xs, int axis, Tensor y, bool useGradients)
        {
            float[] yw = useGradients ? y.Gradient : y.Weights;

            if (axis == 0)
            {
                for (int i = 0, ii = xs.Count, offy = 0; i < ii; i++)
                {
                    Tensor x = xs[i];
                    int xstride = x.Length;

                    if (useGradients)
                    {
                        Math32f.Add(xstride, x.Gradient, 0, yw, offy);
                    }
                    else
                    {
                        Array32f.Copy(xstride, x.Weights, 0, yw, offy);
                    }

                    offy += xstride;
                }
            }
            else
            {
                /*for (int n = 0, nn = y.Length / y.Strides[axis - 1], offy = 0; n < nn; n++)
                {
                    for (int i = 0, ii = xs.Count; i < ii; i++)
                    {
                        Tensor x = xs[i];
                        int xstride = x.Strides[axis - 1];

                        if (useGradients)
                        {
                            Mathematics.Add(xstride, x.Gradient, n * xstride, yw, offy);
                        }
                        else
                        {
                            SetCopy.Copy(xstride, x.Weights, n * xstride, yw, offy);
                        }

                        offy += xstride;
                    }
                }*/

                int ylen = y.Length;
                int ystride = y.Strides[axis - 1];
                for (int i = 0, offy = 0, ii = xs.Count; i < ii; i++)
                {
                    Tensor x = xs[i];
                    int xstride = x.Strides[axis - 1];

                    if (useGradients)
                    {
                        for (int offx = 0, offyy = offy; offyy < ylen; offx += xstride, offyy += ystride)
                        {
                            Math32f.Add(xstride, x.Gradient, offx, yw, offyy);
                        }
                    }
                    else
                    {
                        for (int offx = 0, offyy = offy; offyy < ylen; offx += xstride, offyy += ystride)
                        {
                            Array32f.Copy(xstride, x.Weights, offx, yw, offyy);
                        }
                    }

                    offy += xstride;
                }
            }
        }

        /// <summary>
        /// Splits a tensor along the specified axis into the collection of sub tensors.
        /// </summary>
        /// <param name="x">The tensor to split.</param>
        /// <param name="axis">The axis to split along.</param>
        /// <param name="ys">The tensors that receives the data. Can be <b>null</b>.</param>
        /// <param name="useGradients">Specifies whether the gradient weights should be used for computation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Split(Tensor x, int axis, IList<Tensor> ys, bool useGradients)
        {
            int xstride = axis == 0 ? x.Length : x.Strides[axis - 1];
            int ydim = axis == 0 ? 1 : x.Length / xstride;
            float[] xw = useGradients ? x.Gradient : x.Weights;

            for (int i = 0, offx = 0, ii = ys.Count; i < ii; i++)
            {
                Tensor y = ys[i];
                float[] yw = useGradients ? y.Gradient : y.Weights;
                int ystride = axis == 0 ? y.Length : y.Strides[axis - 1];

                for (int n = 0, offxx = offx, offy = 0; n < ydim; n++, offxx += xstride, offy += ystride)
                {
                    if (useGradients)
                    {
                        Math32f.Add(ystride, xw, offxx, yw, offy);
                    }
                    else
                    {
                        Array32f.Copy(ystride, xw, offxx, yw, offy);
                    }
                }

                offx += ystride;
            }
        }

        /// <summary>
        /// Stacks a range of rank-N tensors into one rank-(N+1) tensor.
        /// </summary>
        /// <param name="xs">The tensor to stack.</param>
        /// <param name="axis">The axis to stack along.</param>
        /// <param name="y">The tensor that receives the data. Can be <b>null</b>.</param>
        /// <param name="useGradients">Specifies whether the gradient weights should be used for computation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Stack(IList<Tensor> xs, int axis, Tensor y, bool useGradients)
        {
            int xdim = xs.Count;
            int ylen = y.Length;
            int ystride = y.Strides[axis];
            float[] yw = useGradients ? y.Gradient : y.Weights;

            for (int offx = 0, offy = 0; offy < ylen; offx += ystride)
            {
                for (int i = 0; i < xdim; i++, offy += ystride)
                {
                    if (useGradients)
                    {
                        Math32f.Add(ystride, xs[i].Gradient, offx, yw, offy);
                    }
                    else
                    {
                        Array32f.Copy(ystride, xs[i].Weights, offx, yw, offy);
                    }
                }
            }
        }

        /// <summary>
        /// Unpacks a rank-N tensor into a range of rank-(N+1) tensors along the specified dimension.
        /// </summary>
        /// <param name="x">The tensor to unstack.</param>
        /// <param name="axis">The axis to unstack along.</param>
        /// <param name="ys">The tensors that receive the data. Can be <b>null</b>.</param>
        /// <param name="useGradients">Specifies whether the gradient weights should be used for computation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Unstack(Tensor x, int axis, IList<Tensor> ys, bool useGradients)
        {
            int xlen = x.Length;
            int xdim = x.Axes[axis];
            int xstride0 = x.Strides[axis];                         // axis inner stride
            int xstride1 = axis == 0 ? xlen : x.Strides[axis - 1];  // axis outer stride
            float[] xw = useGradients ? x.Gradient : x.Weights;

            for (int i = 0, offx = 0; i < xdim; i++, offx += xstride0)
            {
                float[] yw = useGradients ? ys[i].Gradient : ys[i].Weights;

                for (int offxx = offx, offy = 0; offxx < xlen; offxx += xstride1, offy += xstride0)
                {
                    if (useGradients)
                    {
                        Math32f.Add(xstride0, xw, offxx, yw, offy);
                    }
                    else
                    {
                        Array32f.Copy(xstride0, xw, offxx, yw, offy);
                    }
                }
            }
        }

        /// <summary>
        /// Replicates a tensor in another tensor multiple times along the specified axis.
        /// </summary>
        /// <param name="x">The tensors to copy values from.</param>
        /// <param name="axis">The axis to copy along.</param>
        /// <param name="count">The number of times to replicate values.</param>
        /// <param name="y">The tensor that receives the data. Can be <b>null</b>.</param>
        /// <param name="useGradients">Specifies whether the gradient weights should be used for computation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Tile(Tensor x, int axis, int count, Tensor y, bool useGradients)
        {
            int xlen = x.Length;
            int xstride = axis == 0 ? xlen : x.Strides[axis - 1];

            float[] xw = useGradients ? x.Gradient : x.Weights;
            float[] yw = useGradients ? y.Gradient : y.Weights;

            for (int offx = 0, offy = 0; offx < xlen; offx += xstride)
            {
                for (int i = 0; i < count; i++, offy += xstride)
                {
                    if (useGradients)
                    {
                        Math32f.Add(xstride, xw, offx, yw, offy);
                    }
                    else
                    {
                        Array32f.Copy(xstride, xw, offx, yw, offy);
                    }
                }
            }
        }

        /// <summary>
        /// Compacts a tensor along the specified axis by summing elements.
        /// </summary>
        /// <param name="x">The tensor to compact.</param>
        /// <param name="axis">The axis to compact along.</param>
        /// <param name="count">The number of values to compact.</param>
        /// <param name="y">The tensor that receives the data. Can be <b>null</b>.</param>
        /// <param name="useGradients">Specifies whether the gradient weights should be used for computation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Untile(Tensor x, int axis, int count, Tensor y, bool useGradients)
        {
            int ylen = y.Length;
            int ystride = axis == 0 ? ylen : y.Strides[axis - 1];

            float[] xw = useGradients ? x.Gradient : x.Weights;
            float[] yw = useGradients ? y.Gradient : y.Weights;

            for (int offx = 0, offy = 0; offy < ylen; offy += ystride)
            {
                for (int i = 0; i < count; i++, offx += ystride)
                {
                    if (useGradients || i > 0)
                    {
                        Math32f.Add(ystride, xw, offx, yw, offy);
                    }
                    else
                    {
                        Array32f.Copy(ystride, xw, offx, yw, offy);
                    }
                }
            }
        }
    }
}