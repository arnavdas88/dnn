// -----------------------------------------------------------------------
// <copyright file="MathOperations.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define NOLEARNING

namespace Genix.MachineLearning
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Represents matrix operations on tensors.
    /// </summary>
    public static class MathOperations
    {
        /// <summary>
        /// Adds elements of two tensors element-wise.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="a">The input tensor <c>a</c>.</param>
        /// <param name="b">The input tensor <c>b</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the sum of <c>a</c> and <c>b</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := a(i) + b(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Add(this Session session, Tensor a, Tensor b)
        {
            const string ActionName = "add";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    Debug.Assert(a.Length == b.Length, "Tensors lengths are not equal.");

                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient);

                    Tensor y = session.AllocateTensor(ActionName, a.Axes, calculateGradient);
                    Mathematics.Add(a.Length, a.Weights, 0, b.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                // da += dy
                                if (a.CalculateGradient)
                                {
                                    lock (a)
                                    {
                                        Mathematics.Add(y.Length, y.Gradient, 0, a.Gradient, 0);
                                    }

                                    a.Validate();
                                }

                                // db += dy
                                if (b.CalculateGradient)
                                {
                                    lock (b)
                                    {
                                        Mathematics.Add(y.Length, y.Gradient, 0, b.Gradient, 0);
                                    }

                                    b.Validate();
                                }
                            });
                    }
#endif

                    y.Validate();

                    return y;
                });
        }

        /// <summary>
        /// Adds a scalar value to all elements of a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <param name="alpha">The scalar to add.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := x(i) + alpha</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Add(this Session session, Tensor x, float alpha)
        {
            const string ActionName = "add";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.AddC(x.Length, x.Weights, 0, alpha, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // dx += dy
                        session.Push(ActionName, () => Mathematics.Add(y.Length, y.Gradient, 0, x.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Subtracts elements of two tensors element-wise.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="a">The input tensor <c>a</c>.</param>
        /// <param name="b">The input tensor <c>b</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the difference of <c>a</c> and <c>b</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := a(i) - b(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Subtract(this Session session, Tensor a, Tensor b)
        {
            const string ActionName = "subtract";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    Debug.Assert(a.Length == b.Length, "Tensors lengths are not equal.");

                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient);

                    Tensor y = session.AllocateTensor(ActionName, a.Axes, calculateGradient);
                    Mathematics.Sub(a.Length, a.Weights, 0, b.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                // da += dy
                                if (a.CalculateGradient)
                                {
                                    lock (a)
                                    {
                                        Mathematics.Add(y.Length, y.Gradient, 0, a.Gradient, 0);
                                    }
                                }

                                // db -= dy
                                if (b.CalculateGradient)
                                {
                                    lock (b)
                                    {
                                        Mathematics.Sub(y.Length, y.Gradient, 0, b.Gradient, 0);
                                    }
                                }
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Multiplies elements of two tensors element-wise.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="a">The input tensor <c>a</c>.</param>
        /// <param name="b">The input tensor <c>b</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the product of <c>a</c> and <c>b</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := a(i) * b(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Multiply(this Session session, Tensor a, Tensor b)
        {
            const string ActionName = "multiply";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    Debug.Assert(a.Length == b.Length, "Tensors lengths are not equal.");

                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient);

                    Tensor y = session.AllocateTensor(ActionName, a.Axes, calculateGradient);
                    Mathematics.Multiply(a.Length, a.Weights, 0, b.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                // da += b * dy
                                if (a.CalculateGradient)
                                {
                                    lock (a)
                                    {
                                        Mathematics.MultiplyAndAdd(y.Length, b.Weights, 0, y.Gradient, 0, a.Gradient, 0);
                                    }
                                }

                                // db += a * dy
                                if (b.CalculateGradient)
                                {
                                    lock (b)
                                    {
                                        Mathematics.MultiplyAndAdd(y.Length, a.Weights, 0, y.Gradient, 0, b.Gradient, 0);
                                    }
                                }
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Multiplies all elements of one tensor by a scalar.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor that contains the data to multiply.</param>
        /// <param name="alpha">The scalar.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := alpha * x(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Multiply(this Session session, Tensor x, float alpha)
        {
            const string ActionName = "multiply";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.MulC(x.Length, x.Weights, 0, alpha, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Mathematics.MultiplyAndAdd(y.Length, alpha, y.Gradient, 0, x.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Divides elements of two tensors element-wise.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="a">The input tensor <c>a</c>.</param>
        /// <param name="b">The input tensor <c>b</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the division of <c>a</c> and <c>b</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := a(i) / b(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Divide(this Session session, Tensor a, Tensor b)
        {
            const string ActionName = "divide";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    Debug.Assert(a.Length == b.Length, "Tensors lengths are not equal.");

                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient);

                    Tensor y = session.AllocateTensor(ActionName, a.Axes, calculateGradient);
                    Mathematics.Divide(a.Length, a.Weights, 0, b.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                // da += b^-1 * dy
                                if (a.CalculateGradient)
                                {
                                    lock (a)
                                    {
                                        // TODO:
                                        throw new NotImplementedException();
                                    }
                                }

                                // db += a * -b^-2 * dy
                                if (b.CalculateGradient)
                                {
                                    lock (b)
                                    {
                                        // TODO:
                                        throw new NotImplementedException();
                                    }
                                }
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Squares elements of a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := x(i) * x(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Square(this Session session, Tensor x)
        {
            const string ActionName = "square";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.Square(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // dx += 2 * x * dy
                        session.Push(ActionName, () => Mathematics.PowGradient(x.Length, x.Weights, x.Gradient, 0, false, 2.0f, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Raises elements of a tensor to the scalar power.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <param name="power">The constant value for power.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := x(i) ^ p</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Pow(this Session session, Tensor x, float power)
        {
            const string ActionName = "pow";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.Pow(x.Length, x.Weights, 0, power, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // dx += p * x ^ (p - 1) * dy
                        session.Push(ActionName, () => Mathematics.PowGradient(x.Length, x.Weights, x.Gradient, 0, false, power, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a square root of elements of a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := sqrt(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Sqrt(this Session session, Tensor x)
        {
            const string ActionName = "sqrt";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.Sqrt(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        // dx += 1 / (2 * sqrt(x)) * dy
                        session.Push(ActionName, () => Mathematics.PowGradient(x.Length, x.Weights, x.Gradient, 0, false, 0.5f, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes absolute value of elements of a tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := abs(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Abs(this Session session, Tensor x)
        {
            const string ActionName = "abs";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.Abs(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Mathematics.AbsGradient(x.Length, x.Weights, x.Gradient, 0, false, y.Weights, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Calculates a larger of each pair of elements of the two tensors.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="a">The input tensor <c>a</c>.</param>
        /// <param name="b">The input tensor <c>b</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := max(a(i), b(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Max(this Session session, Tensor a, Tensor b)
        {
            const string ActionName = "max";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    Debug.Assert(a.Length == b.Length, "Tensors lengths are not equal.");

                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient);

                    Tensor y = session.AllocateTensor(ActionName, a.Axes, calculateGradient);
                    Maximum.Max(a.Length, a.Weights, 0, b.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                if (a.CalculateGradient)
                                {
                                    Maximum.MinMaxGradient(a.Length, a.Weights, a.Gradient, 0, false, y.Weights, y.Gradient, 0);
                                }

                                if (b.CalculateGradient)
                                {
                                    Maximum.MinMaxGradient(b.Length, b.Weights, b.Gradient, 0, false, y.Weights, y.Gradient, 0);
                                }
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Calculates a smaller of each pair of elements of the two tensors.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="a">The input tensor <c>a</c>.</param>
        /// <param name="b">The input tensor <c>b</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := min(a(i), b(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Min(this Session session, Tensor a, Tensor b)
        {
            const string ActionName = "min";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    Debug.Assert(a.Length == b.Length, "Tensors lengths are not equal.");

                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient);

                    Tensor y = session.AllocateTensor(ActionName, a.Axes, calculateGradient);
                    Maximum.Min(a.Length, a.Weights, 0, b.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                if (a.CalculateGradient)
                                {
                                    Maximum.MinMaxGradient(a.Length, a.Weights, a.Gradient, 0, false, y.Weights, y.Gradient, 0);
                                }

                                if (b.CalculateGradient)
                                {
                                    Maximum.MinMaxGradient(b.Length, b.Weights, b.Gradient, 0, false, y.Weights, y.Gradient, 0);
                                }
                            });
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a rectified linear unit nonlinearity element wise on one tensor and puts results into another tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := max(x(i), 0)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor ReLU(this Session session, Tensor x)
        {
            const string ActionName = "relu";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Nonlinearity.ReLU(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Nonlinearity.ReLUGradient(x.Length, x.Gradient, 0, false, y.Weights, 0, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a sigmoid nonlinearity element wise on one tensor and puts results into another tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := sigmoid(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Sigmoid(this Session session, Tensor x)
        {
            const string ActionName = "sigmoid";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Nonlinearity.Sigmoid(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Nonlinearity.SigmoidGradient(x.Length, x.Gradient, 0, false, y.Weights, 0, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a hyperbolic tangent nonlinearity element wise on one tensor and puts results into another tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := tanh(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Tanh(this Session session, Tensor x)
        {
            const string ActionName = "tanh";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Nonlinearity.Tanh(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Nonlinearity.TanhGradient(x.Length, x.Gradient, 0, false, y.Weights, 0, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a hyperbolic tangent nonlinearity element wise on a tensor in place.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The tensor <c>x</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>x(i) := tanh(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TanhIP(this Session session, Tensor x)
        {
            const string ActionName = "tanh";

            session.RunOperation(
                ActionName,
                () =>
                {
                    Nonlinearity.Tanh(x.Length, x.Weights, 0, x.Weights, 0);

#if !NOLEARNING
                    if (session.CalculateGradients && x.CalculateGradient)
                    {
                        session.Push(ActionName, () => Nonlinearity.TanhGradientIP(x.Length, x.Gradient, 0, x.Weights, 0));
                    }
#endif

                    return (Tensor)null;    // we have to return something
                });
        }

        /// <summary>
        /// Computes a sines element wise on one tensor and puts results into another tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := sin(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Sin(this Session session, Tensor x)
        {
            const string ActionName = "sin";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.Sin(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Mathematics.SinGradient(x.Length, x.Weights, x.Gradient, 0, false, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a cosines element wise on one tensor and puts results into another tensor.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor <c>x</c>.</param>
        /// <returns>
        /// The output <see cref="Tensor"/> <c>y</c>.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as <c>y(i) := cos(x(i))</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Cos(this Session session, Tensor x)
        {
            const string ActionName = "sin";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && x.CalculateGradient;

                    Tensor y = session.AllocateTensor(ActionName, x.Axes, calculateGradient);
                    Mathematics.Cos(x.Length, x.Weights, 0, y.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(ActionName, () => Mathematics.CosGradient(x.Length, x.Weights, x.Gradient, 0, false, y.Gradient, 0));
                    }
#endif

                    return y;
                });
        }

        /// <summary>
        /// Computes a matrix-matrix product.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="matrixLayout">Specifies whether the matrices A, B, and C are row-major or column-major.</param>
        /// <param name="a">The tensor that contains the matrix A.</param>
        /// <param name="transa">Specifies whether the matrix A should be transposed before computation.</param>
        /// <param name="b">The tensor that contains the matrix B.</param>
        /// <param name="transb">Specifies whether the matrix B should be transposed before computation.</param>
        /// <param name="bias">The tensor that contains the bias vector <c>b</c> to add to each column of matrix A. Can be null.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the destination matrix C.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as C := op(A)*op(B) + b.
        /// </remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Unroll code for better performance. Unit tests for all execution paths are provided.")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor MxM(this Session session, MatrixLayout matrixLayout, Tensor a, bool transa, Tensor b, bool transb, Tensor bias)
        {
            const string ActionName = "MxM";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || b.CalculateGradient || (bias?.CalculateGradient ?? false));

                    // get dimensions of matrix a
                    if (a.Rank < 2)
                    {
                        throw new ArgumentException("The matrix A must have a rank of at least 2.");
                    }

                    int m = a.Strides[0];
                    int k = a.Axes[0];
                    if (matrixLayout == MatrixLayout.RowMajor ^ transa)
                    {
                        Swapping.Swap(ref m, ref k);
                    }

                    // get dimensions of matrix b
                    if (b.Rank < 2)
                    {
                        throw new ArgumentException("The matrix B must have a rank of at least 2.");
                    }

                    int k2 = b.Strides[0];
                    int n = b.Axes[0];
                    if (matrixLayout == MatrixLayout.RowMajor ^ transb)
                    {
                        Swapping.Swap(ref n, ref k2);
                    }

                    // inner matrix dimensions must match
                    if (k != k2)
                    {
                        throw new ArgumentException("The number of columns in matrix op(A) must match the number of rows in matrix op(B).");
                    }

                    // calculate matrix C dimensions
                    int ccols = matrixLayout == MatrixLayout.RowMajor ? m : n;
                    int crows = matrixLayout == MatrixLayout.RowMajor ? n : m;
                    Tensor c = session.AllocateTensor(ActionName, new[] { ccols, crows }, calculateGradient);

                    bool cleary = true;
                    if (bias != null)
                    {
                        if (m != bias.Length)
                        {
                            throw new ArgumentException("The number of rows in matrix op(A) must match the length of bias vector.");
                        }

                        Arrays.Tile(m, n, bias.Weights, 0, c.Weights, 0);
                        cleary = false;
                    }

                    if (n == 1)
                    {
                        if (transa)
                        {
                            // MxV uses matrix A dimensions, while MxM uses op(A) dimensions
                            Swapping.Swap(ref m, ref k);
                        }

                        Matrix.MxV(matrixLayout, m, k, a.Weights, 0, transa, b.Weights, 0, c.Weights, 0, cleary);

#if !NOLEARNING
                        if (calculateGradient)
                        {
                            session.Push(
                                ActionName,
                                () =>
                                {
                                    if (a.CalculateGradient)
                                    {
                                        lock (a)
                                        {
                                            if (!transa)
                                            {
                                                // dA += dC * B
                                                Matrix.VxV(matrixLayout, m, k, c.Gradient, 0, b.Weights, 0, a.Gradient, 0);
                                            }
                                            else
                                            {
                                                // dA += B * dC
                                                Matrix.VxV(matrixLayout, m, k, b.Weights, 0, c.Gradient, 0, a.Gradient, 0);
                                            }
                                        }
                                    }

                                    if (b.CalculateGradient)
                                    {
                                        lock (b)
                                        {
                                            // dB += !transa ? A' * dC : A * dC
                                            Matrix.MxV(matrixLayout, m, k, a.Weights, 0, !transa, c.Gradient, 0, b.Gradient, 0, false);
                                        }
                                    }

                                    if (bias?.CalculateGradient ?? false)
                                    {
                                        lock (bias)
                                        {
                                            Mathematics.Add(c.Length, c.Gradient, 0, bias.Gradient, 0);
                                        }

                                        bias.Validate();
                                    }
                                });
                        }
#endif
                    }
                    else
                    {
                        Matrix.MxM(matrixLayout, m, k, n, a.Weights, 0, transa, b.Weights, 0, transb, c.Weights, 0, cleary);

#if !NOLEARNING
                        if (calculateGradient)
                        {
                            session.Push(
                                ActionName,
                                () =>
                                {
                                    if (a.CalculateGradient)
                                    {
                                        lock (a)
                                        {
                                            if (!transa)
                                            {
                                                // dA += !transb ? dC * B' : dC * B
                                                Matrix.MxM(matrixLayout, m, n, k, c.Gradient, 0, false, b.Weights, 0, !transb, a.Gradient, 0, false);
                                            }
                                            else
                                            {
                                                // dA += !transb ? B * dC' : B' * dC'
                                                Matrix.MxM(matrixLayout, k, n, m, b.Weights, 0, transb, c.Gradient, 0, true, a.Gradient, 0, false);
                                            }
                                        }
                                    }

                                    if (b.CalculateGradient)
                                    {
                                        lock (b)
                                        {
                                            if (!transb)
                                            {
                                                // dB += !transa ? A' * dC : A * dC
                                                Matrix.MxM(matrixLayout, k, m, n, a.Weights, 0, !transa, c.Gradient, 0, false, b.Gradient, 0, false);
                                            }
                                            else
                                            {
                                                // dB += !transa ? dC' * A : dC' * A'
                                                Matrix.MxM(matrixLayout, n, m, k, c.Gradient, 0, true, a.Weights, 0, transa, b.Gradient, 0, false);
                                            }
                                        }
                                    }

                                    if (bias?.CalculateGradient ?? false)
                                    {
                                        float[] ones = new float[n];
                                        Arrays.Set(n, 1.0f, ones, 0);

                                        lock (bias)
                                        {
                                            Matrix.MxV(matrixLayout, m, n, c.Gradient, 0, false, ones, 0, bias.Gradient, 0, false);
                                        }

                                        bias.Validate();
                                    }
                                });
                        }
#endif
                    }

                    return c;
                });
        }

        /// <summary>
        /// Computes a matrix-vector product.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="matrixLayout">Specifies whether the matrix A is row-major or column-major.</param>
        /// <param name="a">The tensor that contains the matrix A.</param>
        /// <param name="transa">Specifies whether the matrix A should be transposed before computation.</param>
        /// <param name="x">The tensor that contains the vector x.</param>
        /// <param name="bias">The tensor that contains the bias vector <c>b</c>. Can be null.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the destination vector y.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as y := op(A)*x + b.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor MxV(this Session session, MatrixLayout matrixLayout, Tensor a, bool transa, Tensor x, Tensor bias)
        {
            const string ActionName = "MxV";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && (a.CalculateGradient || x.CalculateGradient || (bias?.CalculateGradient ?? false));

                    // get dimensions of matrix a
                    if (a.Rank < 2)
                    {
                        throw new ArgumentException("The matrix A must have a rank of at least 2.");
                    }

                    int m = a.Strides[0];
                    int n = a.Axes[0];
                    if (matrixLayout == MatrixLayout.RowMajor)
                    {
                        Swapping.Swap(ref m, ref n);
                    }

                    // calculate x and y vectors lengths
                    int xlen = transa ? m : n;
                    int ylen = transa ? n : m;
                    if (xlen != x.Length)
                    {
                        throw new ArgumentException("The number of columns in matrix op(A) must match the length of vector X.");
                    }

                    Tensor y = session.AllocateTensor(ActionName, new[] { ylen }, calculateGradient);

                    bool cleary = true;
                    if (bias != null)
                    {
                        if (ylen != bias.Length)
                        {
                            throw new ArgumentException("The number of rows in matrix op(A) must match the length of bias vector.");
                        }

                        Arrays.Copy(ylen, bias.Weights, 0, y.Weights, 0);
                        cleary = false;
                    }

                    Matrix.MxV(matrixLayout, m, n, a.Weights, 0, transa, x.Weights, 0, y.Weights, 0, cleary);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                // dA += op(dy * x'), transa == false => dy * x', transa == true => x * dy'
                                if (a.CalculateGradient)
                                {
                                    lock (a)
                                    {
                                        if (transa)
                                        {
                                            Matrix.VxV(matrixLayout, xlen, ylen, x.Weights, 0, y.Gradient, 0, a.Gradient, 0);
                                        }
                                        else
                                        {
                                            Matrix.VxV(matrixLayout, ylen, xlen, y.Gradient, 0, x.Weights, 0, a.Gradient, 0);
                                        }
                                    }

                                    a.Validate();
                                }

                                // dx += op(A') * dy
                                if (x.CalculateGradient)
                                {
                                    lock (x)
                                    {
                                        Matrix.MxV(matrixLayout, m, n, a.Weights, 0, !transa, y.Gradient, 0, x.Gradient, 0, false);
                                    }

                                    x.Validate();
                                }

                                if (bias?.CalculateGradient ?? false)
                                {
                                    lock (bias)
                                    {
                                        Mathematics.Add(ylen, y.Gradient, 0, bias.Gradient, 0);
                                    }

                                    bias.Validate();
                                }
                            });
                    }
#endif

                    y.Validate();

                    return y;
                });
        }

        /// <summary>
        /// Performs a rank-1 update of a general matrix.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="matrixLayout">Specifies whether the matrix A is row-major or column-major.</param>
        /// <param name="x">The tensor that contains the vector <c>x</c>.</param>
        /// <param name="y">The tensor that contains the vector <c>y</c>.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the destination matrix A.
        /// </returns>
        /// <remarks>
        /// The method performs operation defined as A := x*y'.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor VxV(this Session session, MatrixLayout matrixLayout, Tensor x, Tensor y)
        {
            const string ActionName = "VxV";

            return session.RunOperation(
                ActionName,
                () =>
                {
                    bool calculateGradient = session.CalculateGradients && (x.CalculateGradient || y.CalculateGradient);

                    int m = x.Length;
                    int n = y.Length;

                    int[] axes = matrixLayout == MatrixLayout.ColumnMajor ? new[] { n, m } : new[] { m, n };
                    Tensor a = session.AllocateTensor(ActionName, axes, calculateGradient);

                    Matrix.VxV(matrixLayout, m, n, x.Weights, 0, y.Weights, 0, a.Weights, 0);

#if !NOLEARNING
                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                // dx += dA * y
                                if (x.CalculateGradient)
                                {
                                    lock (x)
                                    {
                                        Matrix.MxV(matrixLayout, m, n, a.Gradient, 0, false, y.Weights, 0, x.Gradient, 0, false);
                                    }
                                }

                                // dy += dA' * x
                                if (y.CalculateGradient)
                                {
                                    lock (y)
                                    {
                                        Matrix.MxV(matrixLayout, m, n, a.Gradient, 0, true, x.Weights, 0, y.Gradient, 0, false);
                                    }
                                }
                            });
                    }
#endif

                    return a;
                });
        }
    }
}