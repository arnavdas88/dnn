// -----------------------------------------------------------------------
// <copyright file="SRNCell.cs" company="Noname, Inc.">
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
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Simple recurrent network (SRN) cell.
    /// </summary>
    public class SRNCell : RNNCell
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(SRNC)(?:\(([A-Za-z]+)=([0-9.]+)(?:,([A-Za-z]+)=([0-9.]+))*\))?$";

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public SRNCell(
            Shape shape,
            RNNDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            this.Initialize(shape, direction, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class, using the specified architecture.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public SRNCell(Shape shape, string architecture, RandomNumberGenerator<float> random)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, SRNCell.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            if (!Layer.TryParseArchitectureParameter(groups, "SRNC", "Bi", out RNNDirection direction))
            {
                direction = RNNDirection.ForwardOnly;
            }

            this.Initialize(
                shape,
                direction,
                numberOfNeurons,
                MatrixLayout.RowMajor,
                random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class, using the existing <see cref="SRNCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SRNCell"/> to copy the data from.</param>
        public SRNCell(SRNCell other)
            : base(other)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SRNCell"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private SRNCell()
        {
        }

        /// <inheritdoc />
        public override string Architecture => string.Format(
            CultureInfo.InvariantCulture,
            "{0}SRNC{1}",
            this.NumberOfNeurons,
            this.Direction == RNNDirection.BiDirectional ? "(Bi=1)" : string.Empty);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new SRNCell(this);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override IList<Tensor> Forward(Session session, IList<Tensor> xs)
        {
            // SRN formula is:
            // h(t) = A(W * x(t) + U * h(t-1) + b)
            //
            // calculate y = W * x + b
            // pre-calculating matrix for all input vectors is approximately 30% faster
            // then using step-by-step x and h vectors concatenation and multiplication by joined W and U matrix
            // described in academic papers
#if TENSORFLOW
            Tensor y = base.Forward(session, xs)[0];

            int tt = y.Axes[(int)Axis.B];        // number of vectors in time sequence

            if (this.Direction == RNNDirection.BiDirectional)
            {
                int numberOfNeurons = this.NumberOfNeurons; // number of neurons / size of output vector

                Tensor[] u = session.Split(
                    this.U,
                    this.MatrixLayout == MatrixLayout.ColumnMajor ? 1 : 0,
                    new[] { numberOfNeurons / 2, numberOfNeurons / 2 });

                // forward pass
                Tensor[] fhs = session.Unstack(
                    session.Slice(y, new int[] { 0, 0 }, new int[] { tt, numberOfNeurons / 2 }), 0);

                for (int t = 0; t < tt; t++)
                {
                    fhs[t] = Step(fhs[t], t > 0 ? fhs[t - 1] : null, u[0]);
                }

                // backward pass
                Tensor[] bhs = session.Unstack(
                    session.Slice(y, new int[] { 0, numberOfNeurons / 2 }, new int[] { tt, -1 }), 0);

                for (int t = tt - 1; t >= 0; t--)
                {
                    bhs[t] = Step(bhs[t], t < tt - 1 ? bhs[t + 1] : null, u[1]);
                }

                // merge states
                for (int t = 0; t < tt; t++)
                {
                    fhs[t] = session.Concat(new Tensor[] { fhs[t], bhs[t] }, 0);
                }

                return new[] { session.Stack(fhs, 0) };
            }
            else
            {
                Tensor[] hs = session.Unstack(y, (int)Axis.B);

                for (int t = 0; t < tt; t++)
                {
                    hs[t] = Step(hs[t], t > 0 ? hs[t - 1] : null, this.U);
                }

                return new[] { session.Stack(hs, 0) };
            }

            // add hidden layer to the output tensor
            // y += U * y(t-1) (product of hidden weight matrix and hidden vector)
            Tensor Step(Tensor x, Tensor state, Tensor u)
            {
                if (state == null)
                {
                    return session.ReLU(x);
                }
                else
                {
                    Tensor h = session.Add(
                       x,
                       session.MxV(this.MatrixLayout, u, false, state, null));

                    return session.ReLU(h);
                }
            }
#else
            return new[] { session.SRN(xs[0], this.W, this.U, this.B, this.NumberOfNeurons, this.MatrixLayout) };
#endif
        }

        /// <summary>
        /// Initializes the <see cref="SRNCell"/>.
        /// </summary>
        /// <param name="shape">The shape of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            Shape shape,
            RNNDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            int[] axes = shape.Axes;

            // column-major matrix organization - each row contains all weights for one neuron
            // row-major matrix organization - each column contains all weights for one neuron
            int xlen = axes.Skip(1).Aggregate(1, (total, next) => total * next);
            int[] weightsShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { xlen, numberOfNeurons } :
                new[] { numberOfNeurons, xlen };

            int hlen = direction == RNNDirection.ForwardOnly ? numberOfNeurons : numberOfNeurons / 2;
            int[] hiddenShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { hlen, numberOfNeurons } :
                new[] { numberOfNeurons, hlen };

            int[] biasesShape = new[] { numberOfNeurons };

            this.Initialize(direction, numberOfNeurons, matrixLayout, weightsShape, hiddenShape, biasesShape, random);

            this.OutputShape = new Shape(axes[(int)Axis.B], numberOfNeurons);
        }
    }
}
