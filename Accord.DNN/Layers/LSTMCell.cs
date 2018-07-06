// -----------------------------------------------------------------------
// <copyright file="LSTMCell.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define TENSORFLOW

namespace Accord.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Long short-term memory (LSTM) cell.
    /// </summary>
    public class LSTMCell : RNNCell
    {
        /// <summary>
        /// The default value for forget bias.
        /// </summary>
        public const float DefaultForgetBias = 1.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LSTMCell(int[] inputShape, int numberOfNeurons)
            : this(inputShape, numberOfNeurons, LSTMCell.DefaultForgetBias, MatrixLayout.ColumnMajor, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="forgetBias">The bias added to forget gates.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LSTMCell(
            int[] inputShape,
            int numberOfNeurons,
            float forgetBias,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
            : base(
                  LSTMCell.CalculateOutputShape(inputShape, numberOfNeurons),
                numberOfNeurons,
                matrixLayout,
                LSTMCell.CalculateWeightsShape(inputShape, numberOfNeurons, matrixLayout),
                LSTMCell.CalculateHiddenWeightsShape(numberOfNeurons, matrixLayout),
                LSTMCell.CalculateBiasesShape(numberOfNeurons),
                random)
        {
            this.ForgetBias = forgetBias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSTMCell"/> class, using the existing <see cref="LSTMCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="LSTMCell"/> to copy the data from.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The parameter is validated by the base constructor.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LSTMCell(LSTMCell other) : base(other)
        {
            this.ForgetBias = other.ForgetBias;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LSTMCell"/> class from being created.
        /// </summary>
        [JsonConstructor]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}LSTMC", this.NumberOfNeurons);

                if (this.ForgetBias != LSTMCell.DefaultForgetBias)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "(ForgetBias={0})", this.ForgetBias);
                }

                return sb.ToString();
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new LSTMCell(this);

        /// <inheritdoc />
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Stands for length of time sequence.")]
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
            int T = g.Axes[(int)Axis.B];                // number of vectors in time sequence
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
        /// Computes the dimensions of the layer's destination tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <returns>
        /// The dimensions of the layer's destination tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateOutputShape(int[] inputShape, int numberOfNeurons)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            return new[] { inputShape[(int)Axis.B], numberOfNeurons };
        }

        /// <summary>
        /// Computes the dimensions of the layer's weights tensor.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the matrix is row-major or column-major.</param>
        /// <returns>
        /// The dimensions of the layer's weights tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateWeightsShape(int[] inputShape, int numberOfNeurons, MatrixLayout matrixLayout)
        {
            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);

            if (matrixLayout == MatrixLayout.ColumnMajor)
            {
                // column-major matrix organization - each row contains all weights for one neuron
                return new[] { mbsize, 4 * numberOfNeurons };
            }
            else
            {
                // row-major matrix organization - each column contains all weights for one neuron
                return new[] { 4 * numberOfNeurons, mbsize };
            }
        }

        /// <summary>
        /// Computes the dimensions of the layer's hidden weights tensor.
        /// </summary>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the matrix is row-major or column-major.</param>
        /// <returns>
        /// The dimensions of the layer's hidden weights tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateHiddenWeightsShape(int numberOfNeurons, MatrixLayout matrixLayout)
        {
            if (matrixLayout == MatrixLayout.ColumnMajor)
            {
                // column-major matrix organization - each row contains all weights for one neuron
                return new[] { numberOfNeurons, 4 * numberOfNeurons };
            }
            else
            {
                // row-major matrix organization - each column contains all weights for one neuron
                return new[] { 4 * numberOfNeurons, numberOfNeurons };
            }
        }

        /// <summary>
        /// Computes the dimensions of the layer's biases tensor.
        /// </summary>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <returns>
        /// The dimensions of the layer's biases tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] CalculateBiasesShape(int numberOfNeurons)
        {
            // keep all weights in single channel
            // allocate four matrices (one for each of three gates and one for the state)
            return new[] { 4 * numberOfNeurons };
        }

#if !TENSORFLOW
        private static class NativeMethods
        {
            [DllImport("Accord.DNN.CPP.dll")]
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

            [DllImport("Accord.DNN.CPP.dll")]
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
