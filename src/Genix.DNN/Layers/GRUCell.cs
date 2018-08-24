﻿// -----------------------------------------------------------------------
// <copyright file="GRUCell.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#define TENSORFLOW

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Gated recurrent unit (GRU) cell.
    /// </summary>
    public class GRUCell : RNNCell
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(GRUC)(?:\(([A-Za-z]+)=([0-9.]+)(?:,([A-Za-z]+)=([0-9.]+))*\))?$";

        /// <summary>
        /// Initializes a new instance of the <see cref="GRUCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public GRUCell(
            int[] inputShape,
            RNNCellDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            this.Initialize(inputShape, direction, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GRUCell"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public GRUCell(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, GRUCell.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            if (!Layer.TryParseArchitectureParameter(groups, "GRUC", "Bi", out RNNCellDirection direction))
            {
                direction = RNNCellDirection.ForwardOnly;
            }

            this.Initialize(
                inputShape,
                direction,
                numberOfNeurons,
                MatrixLayout.RowMajor,
                random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GRUCell"/> class, using the existing <see cref="GRUCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="GRUCell"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GRUCell(GRUCell other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="GRUCell"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private GRUCell()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "{0}GRUC{1}",
            this.NumberOfNeurons,
            this.Direction == RNNCellDirection.BiDirectional ? "(Bi=1)" : string.Empty);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new GRUCell(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // GRU formula is:
            // u(t) = SIGMOID(Wu x x(t) + Uu x h(t-1) + bu)
            // r(t) = SIGMOID(Wr x x(t) + Ur x h(t-1) + br)
            // c(t) = TANH(Wc x x(t) + Uc x (r(t) * h(t-1)) + bc)
            // h(t) = u(t) * h(t-1) + (1 - u(t)) * c(t)
            //
            // calculate gates = W * x + b
            Tensor g = base.Forward(session, xs)[0];

            int tt = g.Axes[(int)Axis.B];               // number of vectors in time sequence
            int numberOfNeurons = this.NumberOfNeurons; // number of neurons / size of output vector

#if TENSORFLOW
            if (this.Direction == RNNCellDirection.BiDirectional)
            {
                Tensor[] u = session.Split(
                    this.U,
                    this.MatrixLayout == MatrixLayout.ColumnMajor ? 1 : 0,
                    new[] { numberOfNeurons, numberOfNeurons / 2, numberOfNeurons, numberOfNeurons / 2 });

                // forward pass
                Tensor[] fstates = session.Unstack(
                    session.Slice(g, new int[] { 0, 0 }, new int[] { tt, 3 * numberOfNeurons / 2 }), 0);

                for (int t = 0; t < tt; t++)
                {
                    fstates[t] = this.Step(session, fstates[t], t > 0 ? fstates[t - 1] : null, u[0], u[1]);
                }

                // backward pass
                Tensor[] bstates = session.Unstack(
                    session.Slice(g, new int[] { 0, 3 * numberOfNeurons / 2 }, new int[] { tt, -1 }), 0);

                for (int t = tt - 1; t >= 0; t--)
                {
                    bstates[t] = this.Step(session, bstates[t], t < tt - 1 ? bstates[t + 1] : null, u[2], u[3]);
                }

                // merge states
                for (int t = 0; t < tt; t++)
                {
                    fstates[t] = session.Concat(new Tensor[] { fstates[t], bstates[t] }, 0);
                }

                return new[] { session.Stack(fstates, 0) };
            }
            else
            {
                Tensor[] u = session.Split(
                    this.U,
                    this.MatrixLayout == MatrixLayout.ColumnMajor ? 1 : 0,
                    new[] { 2 * numberOfNeurons, numberOfNeurons });

                Tensor[] states = session.Unstack(g, 0);

                for (int t = 0; t < tt; t++)
                {
                    states[t] = this.Step(session, states[t], t > 0 ? states[t - 1] : null, u[0], u[1]);
                }

                return new[] { session.Stack(states, 0) };
            }
#else
            const string ActionName = "split";

            Tensor y = session.RunOperation(
                "gru",
                () =>
                {
                    bool calculateGradient = session.CalculateGradients;

                    Tensor h = session.AllocateTensor(ActionName, new[] { tt, numberOfNeurons }, calculateGradient);

                    NativeMethods.gru(
                        tt,
                        numberOfNeurons,
                        this.U.Weights,
                        g.Weights,
                        h.Weights,
                        true,
                        this.MatrixLayout == MatrixLayout.RowMajor);

                    if (calculateGradient)
                    {
                        session.Push(
                            ActionName,
                            () =>
                            {
                                NativeMethods.gru_gradient(
                                    tt,
                                    numberOfNeurons,
                                    this.U.Weights,
                                    this.U.Gradient,
                                    g.Weights,
                                    g.Gradient,
                                    h.Weights,
                                    h.Gradient,
                                    true,
                                    this.MatrixLayout == MatrixLayout.RowMajor);
                            });
                    }

                    return h;
                });

            return new[] { y };
#endif
        }

#if TENSORFLOW
        /// <summary>
        /// Performs one step of recurrent network.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> that executes the layer.</param>
        /// <param name="x">The input vector (output of fully connected layer).</param>
        /// <param name="state">The hidden state.</param>
        /// <param name="u0">The hidden weights matrix that contains update and reset gate weights.</param>
        /// <param name="u1">The hidden weights matrix that contains candidate weights.</param>
        /// <returns>The output vector <c>y</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Tensor Step(Session session, Tensor x, Tensor state, Tensor u0, Tensor u1)
        {
            if (state == null)
            {
                Tensor[] gates = session.Split(x, 0, 3);
                Tensor ugate = session.Sigmoid(gates[0]);
                Tensor candidate = session.Tanh(gates[2]);

                return session.Multiply(ugate, candidate);
            }
            else
            {
                int ylen = x.Axes[0] / 3;
                Tensor[] gates = session.Split(x, 0, new[] { 2 * ylen, ylen });

                Tensor h = session.Sigmoid(
                    session.Add(
                        gates[0],
                        session.MxV(this.MatrixLayout, u0, false, state, null)));

                Tensor[] gatesUR = session.Split(h, 0, 2);

                Tensor rs = session.Multiply(gatesUR[1], state);
                Tensor candidate = session.Tanh(
                    session.Add(
                        gates[1],
                        session.MxV(this.MatrixLayout, u1, false, rs, null)));

                return session.Add(
                    state,
                    session.Multiply(gatesUR[0], session.Subtract(candidate, state)));
            }
        }
#endif

        /// <summary>
        /// Initializes the <see cref="GRUCell"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            int[] inputShape,
            RNNCellDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            if (random == null)
            {
                random = new RandomRangeGenerator(-0.08f, 0.08f);
            }

            // column-major matrix organization - each row contains all weights for one neuron
            // row-major matrix organization - each column contains all weights for one neuron
            int xlen = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);
            int[] weightsShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { xlen, 3 * numberOfNeurons } :
                new[] { 3 * numberOfNeurons, xlen };

            // keep all weights in single channel
            // allocate three matrices (one for each of two gates and one for the state)
            int[] biasesShape = new[] { 3 * numberOfNeurons };

            int hlen = direction == RNNCellDirection.ForwardOnly ? numberOfNeurons : numberOfNeurons / 2;
            int[] hiddenShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { hlen, 3 * numberOfNeurons } :
                new[] { 3 * numberOfNeurons, hlen };

            this.Initialize(
                direction,
                numberOfNeurons,
                matrixLayout,
                weightsShape,
                hiddenShape,
                biasesShape,
                random ?? new RandomRangeGenerator(-0.08f, 0.08f));

            this.OutputShape = new[] { inputShape[(int)Axis.B], numberOfNeurons };

            // initialize biases for update and reset gates only
            Array32f.Set(2 * numberOfNeurons, 1.0f, this.B.Weights, 0);
        }

#if !TENSORFLOW
        private static class NativeMethods
        {
            private const string DllName = "Genix.DNN.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void gru(
                int steps,
                int ylen,
                [In] float[] u,
                [In] float[] g,
                [Out] float[] y,
                [MarshalAs(UnmanagedType.Bool)] bool forward,
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void gru_gradient(
                int steps,
                int ylen,
                [In] float[] u,
                [Out] float[] du,
                [In] float[] g,
                [Out] float[] dg,
                [In] float[] y,
                [Out] float[] dy,
                [MarshalAs(UnmanagedType.Bool)] bool forward,
                [MarshalAs(UnmanagedType.Bool)] bool rowmajor);
        }
#endif
    }
}
