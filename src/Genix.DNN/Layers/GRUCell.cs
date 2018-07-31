// -----------------------------------------------------------------------
// <copyright file="GRUCell.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define TENSORFLOW

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        public const string ArchitecturePattern = @"^(\d+)(GRUC)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="GRUCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public GRUCell(int[] inputShape, int numberOfNeurons, MatrixLayout matrixLayout, RandomNumberGenerator random)
        {
            this.Initialize(inputShape, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GRUCell"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public GRUCell(int[] inputShape, string architecture, RandomNumberGenerator random)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, GRUCell.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            this.Initialize(inputShape, numberOfNeurons, MatrixLayout.RowMajor, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GRUCell"/> class, using the existing <see cref="GRUCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="GRUCell"/> to copy the data from.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The parameter is validated by the base constructor.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GRUCell(GRUCell other)
            : base(other)
        {
            this.UC = other.UC?.Clone() as Tensor;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="GRUCell"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private GRUCell()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "{0}GRUC", this.NumberOfNeurons);

        /// <summary>
        /// Gets the candidate weights.
        /// </summary>
        /// <value>
        /// The tensor that contains candidate weights.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "UW", Justification = "Stands for hidden weights matrix according to DNN notation.")]
        [JsonProperty("UC")]
        public Tensor UC { get; private set; }

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

#if TENSORFLOW
            Tensor[] states = session.Unstack(g, 0);

            states[0] = this.Step(session, states[0], null);

            for (int t = 1, T = states.Length; t < T; t++)
            {
                states[t] = this.Step(session, states[t], states[t - 1]);
            }

            return new[] { session.Stack(states, 0) };
#else
#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            int T = g.Axes[(int)Axis.B];            // number of vectors in time sequence
#pragma warning restore SA1312 // Variable names must begin with lower-case letter
            int ylen = this.NumberOfNeurons;        // number of neurons / size of output vector

            Tensor y = session.RunOperation(
                "gru",
                () =>
                {
                    bool calculateGradient = session.CalculateGradients;

                    Tensor h = session.AllocateTensor("gru", new[] { T, ylen }, calculateGradient);

                    NativeMethods.gru(
                        T,
                        ylen,
                        this.U.Weights,
                        this.UC.Weights,
                        g.Weights,
                        h.Weights,
                        true,
                        this.MatrixLayout == MatrixLayout.RowMajor);

                    if (calculateGradient)
                    {
                        session.Push(
                            "gru",
                            () =>
                            {
                                NativeMethods.gru_gradient(
                                    T,
                                    ylen,
                                    this.U.Weights,
                                    this.U.Gradient,
                                    this.UC.Weights,
                                    this.UC.Gradient,
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

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IEnumerable<(Tensor, float, float)> EnumGradients()
        {
            return base.EnumGradients().Append((this.UC, 1.0f, 1.0f));
        }

#if TENSORFLOW
        /// <summary>
        /// Performs one step of recurrent network.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> that executes the layer.</param>
        /// <param name="x">The input vector (output of fully connected layer).</param>
        /// <param name="state">The hidden state.</param>
        /// <returns>The output vector <c>y</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Tensor Step(Session session, Tensor x, Tensor state)
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
                Tensor[] gates = session.Split(x, 0, new[] { 2 * this.NumberOfNeurons, this.NumberOfNeurons });

                Tensor h = session.Sigmoid(
                    session.Add(
                        gates[0],
                        session.MxV(this.MatrixLayout, this.U, false, state, null)));

                Tensor[] gatesUR = session.Split(h, 0, 2);

                Tensor rs = session.Multiply(gatesUR[1], state);
                Tensor candidate = session.Tanh(
                    session.Add(
                        gates[1],
                        session.MxV(this.MatrixLayout, this.UC, false, rs, null)));

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
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(int[] inputShape, int numberOfNeurons, MatrixLayout matrixLayout, RandomNumberGenerator random)
        {
            if (random == null)
            {
                random = new RandomRangeGenerator(-0.08f, 0.08f);
            }

            // column-major matrix organization - each row contains all weights for one neuron
            // row-major matrix organization - each column contains all weights for one neuron
            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);
            int[] weightsShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { mbsize, 3 * numberOfNeurons } :
                new[] { 3 * numberOfNeurons, mbsize };

            int[] hiddenShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { numberOfNeurons, 2 * numberOfNeurons } :
                new[] { 2 * numberOfNeurons, numberOfNeurons };

            // keep all weights in single channel
            // allocate four matrices (one for each of three gates and one for the state)
            int[] biasesShape = new[] { 3 * numberOfNeurons };

            this.Initialize(
                numberOfNeurons,
                matrixLayout,
                weightsShape,
                hiddenShape,
                biasesShape,
                random ?? new RandomRangeGenerator(-0.08f, 0.08f));

            this.OutputShape = new[] { inputShape[(int)Axis.B], numberOfNeurons };

            this.UC = new Tensor("hidden weights (2)", new[] { numberOfNeurons, numberOfNeurons });
            this.UC.Randomize(random);

            // initialize biases for update and reset gates only
            Arrays.Set(numberOfNeurons << 1, 1.0f, this.B.Weights, 0);
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
                [In] float[] uw,
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
                [In] float[] uc,
                [Out] float[] duc,
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
