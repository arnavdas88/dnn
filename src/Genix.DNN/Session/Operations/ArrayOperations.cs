// -----------------------------------------------------------------------
// <copyright file="ArrayOperations.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define NOLEARNING

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    using Genix.MachineLearning;

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

                    Tensor y = session.AllocateTensor(ActionName, x.Shape, calculateGradient);
                    Vectors.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => x.AddGradient(y.Gradient));
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

                    Tensor[] ys = session.AllocateTensors(ActionName, count, x.Shape, calculateGradient);
                    for (int i = 0; i < count; i++)
                    {
                        Vectors.Copy(x.Length, x.Weights, 0, ys[i].Weights, 0);
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
                                    Mathematics.AddProductC(x.Length, ys[i].Gradient, 0, alpha, x.Gradient, 0);
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
        /// The <see cref="Tensor"/> that contains values from input tensors stacked along <paramref name="axis"/> dimension.
        /// The tensor shape matches that of <paramref name="xs"/> except in <paramref name="axis"/> where is has the sum of the sizes.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The input tensors ranks and sizes must match in all dimensions except <paramref name="axis"/>.
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

                    Tensor y = session.AllocateTensor(ActionName, Shape.Concat(xs.Select(x => x.Shape).ToArray(), axis), calculateGradient);

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
        /// Extracts a slice of size <paramref name="size"/> from a tensor starting at location specified by <paramref name="begin"/>.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to slice.</param>
        /// <param name="begin">The starting location for the slice as an offset in each dimension of <paramref name="x"/>.</param>
        /// <param name="size">The number of elements in each dimension of <paramref name="x"/> you want to slice.</param>
        /// <returns>
        /// The <see cref="Tensor"/> object that contains a slice of <paramref name="x"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <paramref name="begin"/> is zero based. <paramref name="size"/> is one-based.
        /// If <paramref name="size"/>[i] is -1, all remaining elements in the dimension i are included in the slice.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Slice(this Session session, Tensor x, int[] begin, int[] size)
        {
            const string ActionName = "slice";
            int dims = x.Axes.Length;

            if (begin.Length != dims)
            {
                throw new ArgumentException("The parameter dimension must match the number of axes in the tensor.", nameof(begin));
            }

            if (size.Length != dims)
            {
                throw new ArgumentException("The parameter dimension must match the number of axes in the tensor.", nameof(size));
            }

            Shape yshape = x.Shape.Slice(begin, size);
            if (x.Length == yshape.Length)
            {
                // copy entire tensor
                return session.Copy(x);
            }

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    Tensor y = session.AllocateTensor(ActionName, yshape, calculateGradient);

                    int[] xaxes = x.Axes;
                    int[] yaxes = yshape.Axes;

                    // 1. find last axis of the slice that occupies the entire tensor
                    int blocksize = 1;
                    int lastaxis = dims;
                    while (lastaxis > 0 && xaxes[lastaxis - 1] == yaxes[lastaxis - 1])
                    {
                        blocksize *= yaxes[lastaxis - 1];
                        lastaxis--;
                    }

                    if (lastaxis == dims)
                    {
                        blocksize = yaxes[dims - 1];
                    }

                    // now point to the axis where the copying should start
                    lastaxis--;

                    // 2. do slicing
                    Slice();

                    void Slice()
                    {
                        float[] xw = x.Weights;
                        float[] yw = y.Weights;
                        int[] xstrides = x.Strides;

                        int offy = 0;
                        Do(0, 0);
                        Debug.Assert(offy == y.Length, "Entire tensor must be filled.");

                        void Do(int axis, int offx)
                        {
                            offx += begin[axis] * xstrides[axis];

                            if (axis == lastaxis)
                            {
                                Vectors.Copy(blocksize, xw, offx, yw, offy);
                                offy += blocksize;
                            }
                            else
                            {
                                for (int i = 0; i < yaxes[axis]; i++, offx += xstrides[axis])
                                {
                                    Do(axis + 1, offx);
                                }
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
                                int[] xstrides = x.Strides;

                                int offy = 0;
                                Unslice(0, 0);
                                Debug.Assert(offy == y.Length, "Entire tensor must be filled.");

                                void Unslice(int axis, int offx)
                                {
                                    offx += begin[axis] * xstrides[axis];

                                    if (axis == lastaxis)
                                    {
                                        Mathematics.Add(blocksize, dyw, offy, dxw, offx);
                                        offy += blocksize;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < yaxes[axis]; i++, offx += xstrides[axis])
                                        {
                                            Unslice(axis + 1, offx);
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
        /// Splits a tensor along the specified axis into the collection of sub tensors.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor to split.</param>
        /// <param name="axis">The axis to split along.</param>
        /// <param name="sizes">The sizes of sub tensors along dimension <paramref name="axis"/>.</param>
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

                    if (sum != x.Shape.Axes[axis])
                    {
                        throw new ArgumentException("The sub tensors sizes must be provided.");
                    }

                    // allocate destination
                    Tensor[] ys = new Tensor[ydim];
                    for (int i = 0; i < ydim; i++)
                    {
                        ys[i] = session.AllocateTensor(ActionName, x.Shape.Reshape(axis, sizes[i]), calculateGradient);
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
        /// <param name="numberOfSplits">The number of tensors to split into along dimension <paramref name="axis"/>.</param>
        /// <returns>
        /// The collection of <see cref="Tensor"/> objects that contains sub tensors.
        /// </returns>
        /// <remarks>
        /// Requires that <c>numberOfSplits</c> evenly divides the tensor shape along dimension <paramref name="axis"/>.
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
                    Tensor[] ys = session.AllocateTensors(
                        ActionName,
                        numberOfSplits,
                        x.Shape.Reshape(axis, x.Axes[axis] / numberOfSplits),
                        calculateGradient);

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

                    Tensor y = session.AllocateTensor(ActionName, xs[0].Shape.InsertAxis(axis, xdim), calculateGradient);

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
                    Tensor[] ys = session.AllocateTensors(ActionName, x.Axes[axis], x.Shape.RemoveAxis(axis), calculateGradient);

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
                    Tensor y = session.AllocateTensor(ActionName, x.Shape.Reshape(axis, x.Axes[axis] * count), calculateGradient);

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
                    Tensor y = session.AllocateTensor(ActionName, x.Shape.Reshape(axis, xsize / count), calculateGradient);

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
                    int xaxis = x.Axes[axis];
                    int xstride = x.Strides[axis];

                    if ((xaxis % count) != 0)
                    {
                        throw new ArgumentException("The axis dimension must be a count multiple.");
                    }

                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    // allocate destination
                    Tensor y = session.AllocateTensor(ActionName, x.Shape.Reshape(axis, xaxis / count), calculateGradient);

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
                            Vectors.Max(length, wsp, 0, yw, 0);
                        }
                    }
                    else
                    {
                        for (int offx = 0, offy = 0, ylen = y.Length; offy < ylen; offy += xstride)
                        {
                            Vectors.Copy(xstride, xw, offx, yw, offy);
                            offx += xstride;

                            for (int i = 1; i < count; i++, offx += xstride)
                            {
                                Vectors.Max(xstride, xw, offx, yw, offy);
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
                    Tensor y = session.AllocateTensor(ActionName, x.Shape.RemoveAxis(axis), calculateGradient);

                    // simply copy tensor content
                    Vectors.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // simply copy tensors
                        session.Push(ActionName, () => x.AddGradient(y.Gradient));
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
                    Tensor y = session.AllocateTensor(ActionName, x.Shape.InsertAxis(axis, 1), calculateGradient);

                    // simply copy tensor content
                    Vectors.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // simply copy tensor content
                        session.Push(ActionName, () => x.AddGradient(y.Gradient));
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
        /// <param name="shape">The new tensor shape.</param>
        /// <returns>
        /// The <c>x</c> tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Reshape(this Session session, Tensor x, Shape shape)
        {
            const string ActionName = "reshape";

            // validate new shape
            if (shape.Length != x.Length)
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
                    Vectors.Copy(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // simply copy tensor content
                        session.Push(ActionName, () => x.AddGradient(y.Gradient));
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
        /// <param name="shape">The new tensor shape.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReshapeIP(this Session session, Tensor x, Shape shape)
        {
            const string ActionName = "reshape";

            session.RunOperation(
                ActionName,
                () =>
                {
#if !NOLEARNING
                    if (session.CalculateGradients && x.CalculateGradient)
                    {
                        Shape oldshape = new Shape(x.Shape);
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
                    float[] xw = useGradients ? x.Gradient : x.Weights;
                    int xstride = x.Length;

                    if (useGradients)
                    {
                        Mathematics.Add(xstride, xw, 0, yw, offy);
                    }
                    else
                    {
                        Vectors.Copy(xstride, xw, 0, yw, offy);
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
                    float[] xw = useGradients ? x.Gradient : x.Weights;
                    int xstride = x.Strides[axis - 1];

                    if (useGradients)
                    {
                        for (int offx = 0, offyy = offy; offyy < ylen; offx += xstride, offyy += ystride)
                        {
                            Mathematics.Add(xstride, xw, offx, yw, offyy);
                        }
                    }
                    else
                    {
                        for (int offx = 0, offyy = offy; offyy < ylen; offx += xstride, offyy += ystride)
                        {
                            Vectors.Copy(xstride, xw, offx, yw, offyy);
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
                        Mathematics.Add(ystride, xw, offxx, yw, offy);
                    }
                    else
                    {
                        Vectors.Copy(ystride, xw, offxx, yw, offy);
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
                    Tensor x = xs[i];
                    float[] xw = useGradients ? x.Gradient : x.Weights;

                    if (useGradients)
                    {
                        Mathematics.Add(ystride, xw, offx, yw, offy);
                    }
                    else
                    {
                        Vectors.Copy(ystride, xw, offx, yw, offy);
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
                Tensor y = ys[i];
                float[] yw = useGradients ? y.Gradient : y.Weights;

                for (int offxx = offx, offy = 0; offxx < xlen; offxx += xstride1, offy += xstride0)
                {
                    if (useGradients)
                    {
                        Mathematics.Add(xstride0, xw, offxx, yw, offy);
                    }
                    else
                    {
                        Vectors.Copy(xstride0, xw, offxx, yw, offy);
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
                        Mathematics.Add(xstride, xw, offx, yw, offy);
                    }
                    else
                    {
                        Vectors.Copy(xstride, xw, offx, yw, offy);
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
                        Mathematics.Add(ystride, xw, offx, yw, offy);
                    }
                    else
                    {
                        Vectors.Copy(ystride, xw, offx, yw, offy);
                    }
                }
            }
        }
    }
}