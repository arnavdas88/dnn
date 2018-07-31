// -----------------------------------------------------------------------
// <copyright file="Shape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        /// Initializes a new instance of the <see cref="Shape"/> class with the specified dimensions.
        /// </summary>
        /// <param name="shape">The shape dimensions.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape(params int[] shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            if (shape.Any(x => x <= 0))
            {
                throw new ArgumentException(Properties.Resources.E_CannotCreateTensor_LayoutIsFlexible);
            }

            this.InitializeShape(shape);
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

            this.Length = other.Length;
            this.Axes = other.Axes.ToArray();
            this.Strides = other.Strides.ToArray();
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
        public int Rank => this.Axes.Length;

        /// <summary>
        /// Gets the axes dimensions.
        /// </summary>
        /// <value>
        /// The axes dimensions.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Need a fast access to the collection.")]
        [JsonProperty("axes")]
        public int[] Axes { get; private set; }

        /// <summary>
        /// Gets the axes strides.
        /// </summary>
        /// <value>
        /// The axes strides.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Need a fast access to the collection.")]
        [JsonProperty("strides")]
        public int[] Strides { get; private set; }

        /// <summary>
        /// Calculates the element position in the tensor.
        /// </summary>
        /// <param name="axes">The element coordinates.</param>
        /// <returns>The dot product of element coordinates and corresponding strides.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
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
        /// Calculate the total shape size.
        /// </summary>
        /// <param name="shape">The shape to evaluate.</param>
        /// <returns>The product of shape dimensions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ShapeLength(int[] shape)
        {
            int length = 1;
            for (int i = 0, ii = shape.Length; i < ii; i++)
            {
                length *= shape[i];
            }

            return length;
        }

        /// <summary>
        /// Determines whether two shapes are identical.
        /// </summary>
        /// <param name="shape1">The first shape to evaluate.</param>
        /// <param name="shape2">The second shape to evaluate.</param>
        /// <returns><b>true</b> if <c>shape1</c> is the same as <c>shape2</c>; otherwise; <b>false</b>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AreSame(int[] shape1, int[] shape2)
        {
            return shape1.Length == shape2.Length && Arrays.Equals(shape1.Length, shape1, 0, shape2, 0);
        }

        /// <summary>
        /// Determines whether all shapes in the collection are identical.
        /// </summary>
        /// <param name="shapes">The shapes to evaluate.</param>
        /// <returns>
        /// <b>true</b> if all shapes in <c>shapes</c> are identical; otherwise; <b>false</b>.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AreSame(IList<int[]> shapes)
        {
            for (int i = 1, ii = shapes.Count; i < ii; i++)
            {
                if (!Shape.AreSame(shapes[0], shapes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the shapes of all tensors in the collection are identical.
        /// </summary>
        /// <param name="xs">The tensors to evaluate.</param>
        /// <returns>
        /// <b>true</b> if shapes of all tensors in <c>xs</c> are identical; otherwise; <b>false</b>.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AreSame(IList<Tensor> xs)
        {
            for (int i = 1, ii = xs.Count; i < ii; i++)
            {
                if (!Shape.AreSame(xs[0].Axes, xs[i].Axes))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new shape by concatenating a group of shapes along the specified axis.
        /// </summary>
        /// <param name="shapes">The shapes to concatenate.</param>
        /// <param name="axis">The axis to concatenate along.</param>
        /// <returns>
        /// The concatenated shape.
        /// </returns>
        /// <remarks>
        /// The <c>shapes</c> must have the same rank and dimensions along all axes other than <c>axis</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int[] Concat(IList<int[]> shapes, int axis)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            if (shapes.Count == 0)
            {
                throw new ArgumentException("At least one shape must be specified.", nameof(shapes));
            }

            int[] shape0 = shapes[0].ToArray();
            int rank = shape0.Length;

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (axis >= rank)
            {
                throw new ArgumentException("Invalid axis.", nameof(axis));
            }

            for (int i = 1, ii = shapes.Count; i < ii; i++)
            {
                int[] shape = shapes[i];
                if (shape.Length != rank)
                {
                    throw new ArgumentException("The shapes must have the same rank.", nameof(shapes));
                }

                for (int j = 0; j < rank; j++)
                {
                    if (j == axis)
                    {
                        shape0[j] += shape[j];
                    }
                    else if (shape0[j] != shape[j])
                    {
                        throw new ArgumentException("The shapes must have the same dimensions.", nameof(shapes));
                    }
                }
            }

            return shape0;
        }

        /// <summary>
        /// Inserts a new dimension into the shape at the dimension index.
        /// </summary>
        /// <param name="shape">The shape to reshape.</param>
        /// <param name="axis">The new dimension axis.</param>
        /// <param name="dimension">The new dimension along the <c>axis</c>.</param>
        /// <returns>
        /// The expanded shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int[] Expand(int[] shape, int axis, int dimension)
        {
            return shape.InsertAt(axis, dimension);
        }

        /// <summary>
        /// Removes the dimension from the shape.
        /// </summary>
        /// <param name="shape">The shape to reshape.</param>
        /// <param name="axis">The dimension axis to remove.</param>
        /// <returns>
        /// The squeezed shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int[] Remove(int[] shape, int axis)
        {
            return shape.RemoveAt(axis);
        }

        /// <summary>
        /// Creates a new shape by reshaping the shape along the specified axis.
        /// </summary>
        /// <param name="shape">The shape to reshape.</param>
        /// <param name="axis">The axis to reshape along.</param>
        /// <param name="dimension">The new dimension along the <c>axis</c>.</param>
        /// <returns>
        /// The reshaped shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int[] Reshape(int[] shape, int axis, int dimension)
        {
            shape = shape?.ToArray() ?? throw new ArgumentNullException(nameof(shape));
            int rank = shape.Length;

            if (axis < 0)
            {
                throw new ArgumentException(Properties.Resources.E_NegativeAxisIndex, nameof(axis));
            }

            if (axis >= rank)
            {
                throw new ArgumentException("Invalid axis.", nameof(axis));
            }

            shape[axis] = dimension;

            return shape;
        }

        /// <summary>
        /// Initializes the shape after it has been constructed.
        /// </summary>
        /// <param name="shape">The shape dimensions.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void InitializeShape(int[] shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            int length = 1;

            int rank = shape.Length;
            int[] axes = this.Axes = new int[rank];
            int[] strides = this.Strides = new int[rank];

            for (int i = rank - 1; i >= 0; i--)
            {
                axes[i] = shape[i];
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
