// -----------------------------------------------------------------------
// <copyright file="Shape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a tensor shape.
    /// </summary>
    public class Shape
    {
        /// <summary>
        /// Batch, width, height, channels.
        /// </summary>
        public const string BWHC = "BWHC";

        /// <summary>
        /// Batch, height, width, channels.
        /// </summary>
        public const string BHWC = "BHWC";

        /// <summary>
        /// Batch, channels, height, width.
        /// </summary>
        public const string BCHW = "BCHW";

        private static readonly int[] BWHCAxes = { 0, 1, 2, 3 };
        private static readonly int[] BHWCAxes = { 0, 2, 1, 3 };
        private static readonly int[] BCHWAxes = { 0, 3, 2, 1 };

        [JsonProperty("axes")]
        private int[] axes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class with known layout.
        /// </summary>
        /// <param name="format">The shape format.</param>
        /// <param name="b">The shape dimensions along b axes.</param>
        /// <param name="x">The shape dimensions along x axes.</param>
        /// <param name="y">The shape dimensions along y axes.</param>
        /// <param name="c">The shape dimensions along c axes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape(string format, int b, int x, int y, int c)
        {
            switch (format)
            {
                case Shape.BWHC:
                    this.InitializeShape(new int[] { b, x, y, c });
                    break;

                case Shape.BHWC:
                    this.InitializeShape(new int[] { b, y, x, c });
                    break;

                case Shape.BCHW:
                    this.InitializeShape(new int[] { b, c, y, x });
                    break;

                default:
                    throw new NotSupportedException("The tensor shape is not supported by this operation.");
            }

            /*if (axes == null)
            {
                throw new ArgumentNullException(nameof(axes));
            }

            if (axes.Any(x => x <= 0))
            {
                throw new ArgumentException(Properties.Resources.E_CannotCreateTensor_LayoutIsFlexible);
            }*/

            this.Format = format;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class with unknown layout.
        /// </summary>
        /// <param name="axes">The shape dimensions.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape(int[] axes)
        {
            this.InitializeShape(axes ?? throw new ArgumentNullException(nameof(axes)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        /// <param name="other">The <see cref="Shape"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape(Shape other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Format = other.Format;
            this.Length = other.Length;
            this.axes = other.axes;
            this.Strides = other.Strides;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        protected Shape()
        {
        }

        /// <summary>
        /// Gets the shape format.
        /// </summary>
        /// <value>
        /// The shape format.
        /// </value>
        [JsonProperty("format")]
        public string Format { get; private set; }

        /// <summary>
        /// Gets the total number of elements in all the dimensions of the <see cref="Shape"/>.
        /// </summary>
        /// <value>
        /// The total number of elements in all the dimensions of the <see cref="Shape"/>.
        /// </value>
        [JsonProperty("length")]
        public int Length { get; private set; }

        /// <summary>
        /// Gets the rank (number of dimensions) of the <see cref="Shape"/>.
        /// </summary>
        /// <value>
        /// The rank (number of dimensions) of the <see cref="Shape"/>.
        /// </value>
        [JsonIgnore]
        public int Rank => this.Axes.Length;

        /// <summary>
        /// Gets the axes dimensions.
        /// </summary>
        /// <value>
        /// The axes dimensions.
        /// </value>
        public int[] Axes => this.axes;

        /// <summary>
        /// Gets the axes strides.
        /// </summary>
        /// <value>
        /// The axes strides.
        /// </value>
        [JsonProperty("strides")]
        public int[] Strides { get; private set; }

        /// <summary>
        /// Creates a new shape by concatenating a group of shapes along the specified axis.
        /// </summary>
        /// <param name="shapes">The shapes to concatenate.</param>
        /// <param name="axis">The axis to concatenate along.</param>
        /// <returns>
        /// The concatenated <see cref="Shape"/>.
        /// </returns>
        /// <remarks>
        /// The <paramref name="shapes"/> must have the same rank and dimensions along all axes other than <paramref name="axis"/>.
        /// </remarks>
        public static Shape Concat(IList<Shape> shapes, int axis)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            if (shapes.Count == 0)
            {
                throw new ArgumentException("At least one shape must be specified.", nameof(shapes));
            }

            Shape shape0 = shapes[0];
            int rank = shape0.Rank;

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (axis >= rank)
            {
                throw new ArgumentException("Invalid axis.", nameof(axis));
            }

            int[] axes0 = shape0.Axes;
            int sum = axes0[axis];
            for (int i = 1, ii = shapes.Count; i < ii; i++)
            {
                Shape shape = shapes[i];
                if (shape.Rank != rank)
                {
                    throw new ArgumentException("The shapes must have the same rank.", nameof(shapes));
                }

                for (int j = 0; j < rank; j++)
                {
                    if (j == axis)
                    {
                        sum += shape.Axes[j];
                    }
                    else if (axes0[j] != shape.Axes[j])
                    {
                        throw new ArgumentException("The shapes must have the same dimensions.", nameof(shapes));
                    }
                }
            }

            return shape0.Reshape(axis, sum);
        }

        /// <summary>
        /// Creates a new shape by concatenating a group of shapes along the specified axis.
        /// </summary>
        /// <param name="shapes">The shapes to concatenate.</param>
        /// <param name="axis">The axis to concatenate along.</param>
        /// <returns>
        /// The concatenated <see cref="Shape"/>.
        /// </returns>
        /// <remarks>
        /// The <paramref name="shapes"/> must have the same rank and dimensions along all axes other than <paramref name="axis"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Shape Concat(IList<Shape> shapes, Axis axis)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            return Shape.Concat(shapes, shapes[0].GetAxisIndex(axis));
        }

        /// <summary>
        /// Determines whether two shapes are identical.
        /// </summary>
        /// <param name="shape1">The first shape to evaluate.</param>
        /// <param name="shape2">The second shape to evaluate.</param>
        /// <returns><b>true</b> if <paramref name="shape1"/> is the same as <paramref name="shape2"/>; otherwise; <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreSame(int[] shape1, int[] shape2)
        {
            return shape1.Length == shape2.Length && Vectors.Equals(shape1.Length, shape1, 0, shape2, 0);
        }

        /// <summary>
        /// Determines whether the shapes of all tensors in the collection are identical.
        /// </summary>
        /// <param name="xs">The tensors to evaluate.</param>
        /// <returns>
        /// <b>true</b> if shapes of all tensors in <paramref name="xs"/> are identical; otherwise; <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreSame(IList<Tensor> xs)
        {
            for (int i = 1, ii = xs.Count; i < ii; i++)
            {
                if (!Shape.AreSame(xs[0].Shape.Axes, xs[i].Shape.Axes))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attaches the data from specified <see cref="Shape"/> to this <see cref="Shape"/>.
        /// </summary>
        /// <param name="shape">The <see cref="Shape"/> to attach.</param>
        public void Attach(Shape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            this.Format = shape.Format;
            this.axes = shape.axes;
            this.Strides = shape.Strides;
        }

        /// <summary>
        /// Calculates the element position in the tensor.
        /// </summary>
        /// <param name="b">The element position along b axes.</param>
        /// <param name="x">The element position along x axes.</param>
        /// <param name="y">The element position along y axes.</param>
        /// <param name="c">The element position along c axes.</param>
        /// <returns>The dot product of element coordinates and corresponding strides.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Position(int b, int x, int y, int c)
        {
            int[] strides = this.Strides;
            switch (this.Format)
            {
                case Shape.BWHC:
                    return (b * strides[0]) + (x * strides[1]) + (y * strides[2]) + (c * strides[3]);

                case Shape.BHWC:
                    return (b * strides[0]) + (x * strides[2]) + (y * strides[1]) + (c * strides[3]);

                case Shape.BCHW:
                    return (b * strides[0]) + (x * strides[3]) + (y * strides[2]) + (c * strides[1]);

                default:
                    throw new NotSupportedException("The tensor shape is not supported by this operation.");
            }
        }

        /// <summary>
        /// Calculates the element position in the tensor.
        /// </summary>
        /// <param name="axes">The element coordinates.</param>
        /// <returns>The dot product of element coordinates and corresponding strides.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Position(params int[] axes)
        {
            int pos = 0;

            int[] strides = this.Strides;
            for (int i = 0, ii = axes.Length; i < ii; i++)
            {
                pos += strides[i] * axes[i];
            }

            return pos;
        }

        /// <summary>
        /// Gets the index of the specified axis in this shape.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <returns>The axis index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAxisIndex(Axis axis)
        {
            switch (this.Format)
            {
                case Shape.BWHC:
                    return Shape.BWHCAxes[(int)axis];
                case Shape.BHWC:
                    return Shape.BHWCAxes[(int)axis];
                case Shape.BCHW:
                    return Shape.BCHWAxes[(int)axis];

                default:
                    throw new NotSupportedException("The tensor shape is not supported by this operation.");
            }
        }

        /// <summary>
        /// Gets the specified axis dimension in this shape.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <returns>The axis dimension.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAxis(int axis)
        {
            return this.axes[axis];
        }

        /// <summary>
        /// Gets the specified axis dimension in this shape.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <returns>The axis dimension.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAxis(Axis axis)
        {
            return this.axes[this.GetAxisIndex(axis)];
        }

        /// <summary>
        /// Gets the specified axis stride in this shape.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <returns>The axis stride.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStride(Axis axis)
        {
            return this.Strides[this.GetAxisIndex(axis)];
        }

        /// <summary>
        /// Creates a new shape by reshaping this <see cref="Shape"/> along the specified axis.
        /// </summary>
        /// <param name="axis">The axis to reshape along.</param>
        /// <param name="dimension">The new dimension along the <paramref name="axis"/>.</param>
        /// <returns>
        /// The reshaped shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape Reshape(int axis, int dimension)
        {
            return new Shape(this.axes.UpdateAt(axis, dimension))
            {
                Format = this.Format,
            };
        }

        /// <summary>
        /// Creates a new shape by reshaping this <see cref="Shape"/> along the specified axis.
        /// </summary>
        /// <param name="axis">The axis to reshape along.</param>
        /// <param name="dimension">The new dimension along the <paramref name="axis"/>.</param>
        /// <returns>
        /// The reshaped shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape Reshape(Axis axis, int dimension) => this.Reshape(this.GetAxisIndex(axis), dimension);

        /// <summary>
        /// Extracts a slice of size <paramref name="size"/> from this <see cref="Shape"/> starting at location specified by <paramref name="begin"/>.
        /// </summary>
        /// <param name="begin">The starting location for the slice as an offset in each dimension of this shape.</param>
        /// <param name="size">The number of elements in each dimension of this shape you want to slice.</param>
        /// <returns>
        /// The reshaped shape.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <paramref name="begin"/> is zero based. <paramref name="size"/> is one-based.
        /// If <paramref name="size"/>[i] is -1, all remaining elements in the dimension i are included in the slice.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape Slice(int[] begin, int[] size)
        {
            int[] axes = this.axes;
            int rank = axes.Length;

            if (begin.Length != rank)
            {
                throw new ArgumentException("The parameter dimension must match the number of axes.", nameof(begin));
            }

            if (size.Length != rank)
            {
                throw new ArgumentException("The parameter dimension must match the number of axes.", nameof(size));
            }

            int[] newaxes = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                int axis = axes[i];
                int b = begin[i];
                int s = size[i];

                if (!b.Between(0, axis - 1))
                {
                    throw new ArgumentOutOfRangeException(nameof(begin));
                }

                if (s == -1)
                {
                    newaxes[i] = axis - b;
                }
                else
                {
                    newaxes[i] = s;

                    if (!(b + newaxes[i]).Between(1, axis))
                    {
                        throw new ArgumentOutOfRangeException(nameof(size));
                    }
                }
            }

            return new Shape(newaxes)
            {
                Format = this.Format,
            };
        }

        /// <summary>
        /// Inserts a new dimension into the shape at the specified index.
        /// </summary>
        /// <param name="axis">The axis to insert.</param>
        /// <param name="dimension">The new dimension along the <paramref name="axis"/>.</param>
        /// <returns>
        /// The expanded shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape InsertAxis(int axis, int dimension) => new Shape(this.axes.InsertAt(axis, dimension));

        /// <summary>
        /// Removes the dimension from the shape.
        /// </summary>
        /// <param name="axis">The axis to remove.</param>
        /// <returns>
        /// The squeezed shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape RemoveAxis(int axis) => new Shape(this.axes.RemoveAt(axis));

        /// <summary>
        /// Initializes the shape after it has been constructed.
        /// </summary>
        /// <param name="axes">The shape dimensions.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeShape(int[] axes)
        {
            int length = 1;

            this.axes = axes;
            int rank = axes.Length;

            int[] strides = this.Strides = new int[rank];

            for (int i = rank - 1; i >= 0; i--)
            {
                length *= axes[i];

                if (i == rank - 1)
                {
                    strides[i] = 1;
                }
                else
                {
                    strides[i] = strides[i + 1] * axes[i + 1];
                }
            }

            this.Length = length;
        }
    }
}
