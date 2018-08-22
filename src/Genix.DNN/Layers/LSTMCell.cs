﻿// -----------------------------------------------------------------------
// <copyright file="LSTMCell.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define TENSORFLOW

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Long short-term memory (LSTM) cell.
    /// </summary>
    public class LSTMCell : RNNCell
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(LSTMC)(?:\(([A-Za-z]+)=([0-9.]+)(?:,([A-Za-z]+)=([0-9.]+))*\))?$";

        ////(?:\(ForgetBias=((?:\d*\.)?\d+)\))?$";

        /// <summary>
        /// The default value for forget bias.
        /// </summary>
        public const float DefaultForgetBias = 1.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        public LSTMCell(
            int[] inputShape,
            RNNCellDirection direction,
            int numberOfNeurons)
            : this(inputShape, direction, numberOfNeurons, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="forgetBias">The bias added to forget gates.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public LSTMCell(
            int[] inputShape,
            RNNCellDirection direction,
            int numberOfNeurons,
            float forgetBias,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            this.Initialize(inputShape, direction, numberOfNeurons, matrixLayout, forgetBias, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public LSTMCell(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, LSTMCell.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            if (!Layer.TryParseArchitectureParameter(groups, "LSTMC", "Bi", out RNNCellDirection direction))
            {
                direction = RNNCellDirection.ForwardOnly;
            }

            if (!Layer.TryParseArchitectureParameter(groups, "LSTMC", "ForgetBias", out float forgetBias))
            {
                forgetBias = LSTMCell.DefaultForgetBias;
            }

            this.Initialize(
                inputShape,
                direction,
                numberOfNeurons,
                MatrixLayout.RowMajor,
                forgetBias,
                random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class, using the existing <see cref="LSTMCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="LSTMCell"/> to copy the data from.</param>
        public LSTMCell(LSTMCell other)
            : base(other)
        {
            this.ForgetBias = other.ForgetBias;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LSTMCell"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private LSTMCell()
        {
        }

        /// <summary>
        /// Gets the bias added to forget gates.
        /// </summary>
        /// <value>
        /// The bias added to forget gates. Default is 1.0f.
        /// </value>
        /// <remarks>
        /// The forget bias is added to the forget gate and helps reduce the scale of forgetting in the beginning of the training.
        /// </remarks>
        [JsonProperty("ForgetBias")]
        public float ForgetBias { get; private set; } = LSTMCell.DefaultForgetBias;

        /// <inheritdoc />
        public override string Architecture
        {
            get
            {
                List<string> prms = new List<string>();
                if (this.Direction != RNNCellDirection.ForwardOnly)
                {
                    prms.Add("Bi=1");
                }

                if (this.ForgetBias != LSTMCell.DefaultForgetBias)
                {
                    prms.Add(string.Format(CultureInfo.InvariantCulture, "ForgetBias={0}", this.ForgetBias));
                }

                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}LSTMC{1}",
                    this.NumberOfNeurons,
                    prms.Count > 0 ? "(" + string.Join(",", prms) + ")" : string.Empty);
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new LSTMCell(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // LSTM formula is:
            // i(t) = SIGMOID(Wi x x(t) + Ui x h(t-1) + bi)
            // j(t) = TANH(Ws x x(t) + Us x h(t-1) + bs)
            // f(t) = SIGMOID(Wf x x(t) + Uf x h(t-1) + bf)
            // o(t) = SIGMOID(Wo x x(t) + Uo x h(t-1) + bo)
            // c(t) = f(t) * c(t-1) + i(t) * j(t)
            // h(t) = o(t) * TANH(c(t))
            //
            // calculate gates = W * x + b
            Tensor g = base.Forward(session, xs)[0];

#if TENSORFLOW
            float forgetBias = this.ForgetBias;

            Tensor[] hs = session.Unstack(g, 0);
            Tensor state = null;

            hs[0] = this.Step(session, hs[0], null, ref state, forgetBias);

            for (int t = 1, T = hs.Length; t < T; t++)
            {
                hs[t] = this.Step(session, hs[t], hs[t - 1], ref state, forgetBias);
            }

            return new[] { session.Stack(hs, 0) };
#else

#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            int T = g.Axes[(int)Axis.B];                // number of vectors in time sequence
#pragma warning restore SA1312 // Variable names must begin with lower-case letter
            int ylen = this.NumberOfNeurons;            // number of neurons / size of output vector

            Tensor y = session.RunOperation(
                "lstm",
                () =>
                {
                    bool calculateGradient = session.CalculateGradients;

                    Tensor h = session.AllocateTensor("lstm", new[] { T, ylen }, calculateGradient);
                    Tensor s = session.AllocateTensor("lstm cell", h.Axes, calculateGradient);

                    NativeMethods.lstm(
                        T,
                        ylen,
                        this.U.Weights,
                        g.Weights,
                        s.Weights,
                        h.Weights,
                        this.ForgetBias,
                        true,
                        this.MatrixLayout == MatrixLayout.RowMajor);

                    if (calculateGradient)
                    {
                        session.Push(
                            "lstm",
                            () =>
                            {
                                NativeMethods.lstm_gradient(
                                    T,
                                    ylen,
                                    this.U.Weights,
                                    this.U.Gradient,
                                    g.Weights,
                                    g.Gradient,
                                    s.Weights,
                                    s.Gradient,
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
        /// <param name="h">The hidden vector.</param>
        /// <param name="state">The hidden state.</param>
        /// <param name="forgetBias">The forget gate bias.</param>
        /// <returns>The output vector <c>y</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Tensor Step(Session session, Tensor x, Tensor h, ref Tensor state, float forgetBias)
        {
            if (h == null)
            {
                Tensor[] gates = session.Split(x, 0, 4);
                Tensor igate = session.Sigmoid(gates[0]);
                Tensor jgate = session.Tanh(gates[1]);
                Tensor ogate = session.Sigmoid(gates[3]);

                state = session.Multiply(igate, jgate);
                return session.Multiply(ogate, session.Tanh(state));
            }
            else
            {
                // add hidden layer to the output tensor
                // y += U * y(t-1) (product of hidden weight matrix and hidden vector)
                x = session.Add(x, session.MxV(this.MatrixLayout, this.U, false, h, null));

                Tensor[] gates = session.Split(x, 0, 4);
                Tensor igate = session.Sigmoid(gates[0]);
                Tensor fgate = forgetBias == 0.0f ? session.Sigmoid(gates[2]) : session.Sigmoid(session.Add(gates[2], forgetBias));
                Tensor jgate = session.Tanh(gates[1]);
                Tensor ogate = session.Sigmoid(gates[3]);

                state = session.Add(session.Multiply(fgate, state), session.Multiply(igate, jgate));
                return session.Multiply(ogate, session.Tanh(state));
            }
        }
#endif

        /// <summary>
        /// Initializes the <see cref="LSTMCell"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="forgetBias">The bias added to forget gates.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            int[] inputShape,
            RNNCellDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            float forgetBias,
            RandomNumberGenerator<float> random)
        {
            // column-major matrix organization - each row contains all weights for one neuron
            // row-major matrix organization - each column contains all weights for one neuron
            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);
            int[] weightsShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { mbsize, 4 * numberOfNeurons } :
                new[] { 4 * numberOfNeurons, mbsize };

            int[] hiddenShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { numberOfNeurons, 4 * numberOfNeurons } :
                new[] { 4 * numberOfNeurons, numberOfNeurons };

            // keep all weights in single channel
            // allocate four matrices (one for each of three gates and one for the state)
            int[] biasesShape = new[] { 4 * numberOfNeurons };

            this.Initialize(
                direction,
                numberOfNeurons,
                matrixLayout,
                weightsShape,
                hiddenShape,
                biasesShape,
                random ?? new RandomRangeGenerator(-0.08f, 0.08f));

            this.ForgetBias = forgetBias;
            this.OutputShape = new[] { inputShape[(int)Axis.B], numberOfNeurons };
        }

#if !TENSORFLOW
        private static class NativeMethods
        {
            private const string DllName = "Genix.DNN.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
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
            [SuppressUnmanagedCodeSecurity]
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
        }
#endif
    }
}
