// -----------------------------------------------------------------------
// <copyright file="SRNCell.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define TENSORFLOW

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
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public SRNCell(
            int[] inputShape,
            RNNDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            this.Initialize(inputShape, direction, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public SRNCell(int[] inputShape, string architecture, RandomNumberGenerator<float> random)
        {
            GroupCollection groups = Layer.ParseArchitecture(architecture, SRNCell.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            if (!Layer.TryParseArchitectureParameter(groups, "SRNC", "Bi", out RNNDirection direction))
            {
                direction = RNNDirection.ForwardOnly;
            }

            this.Initialize(
                inputShape,
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
            Tensor[] hs = session.Unstack(y, (int)Axis.B);
            hs[0] = session.ReLU(hs[0]);

            // add hidden layer to the output tensor
            // y += U * y(t-1) (product of hidden weight matrix and hidden vector)
            for (int t = 1; t < tt; t++)
            {
                Tensor h = session.Add(
                    hs[t],
                    session.MxV(this.MatrixLayout, this.U, false, hs[t - 1], null));

                hs[t] = session.ReLU(h);
            }

            return new[] { session.Stack(hs, 0) };
#else
            return new[] { session.SRN(xs[0], this.W, this.U, this.B, this.NumberOfNeurons, this.MatrixLayout) };
#endif
        }

        /// <summary>
        /// Initializes the <see cref="SRNCell"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="direction">The cell direction (forward-only or bi-directional).</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            int[] inputShape,
            RNNDirection direction,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator<float> random)
        {
            if (inputShape == null)
            {
                throw new ArgumentNullException(nameof(inputShape));
            }

            // column-major matrix organization - each row contains all weights for one neuron
            // row-major matrix organization - each column contains all weights for one neuron
            int mbsize = inputShape.Skip(1).Aggregate(1, (total, next) => total * next);
            int[] weightsShape = matrixLayout == MatrixLayout.ColumnMajor ?
                new[] { mbsize, numberOfNeurons } :
                new[] { numberOfNeurons, mbsize };

            int[] hiddenShape = new[] { numberOfNeurons, numberOfNeurons };

            int[] biasesShape = new[] { numberOfNeurons };

            this.Initialize(direction, numberOfNeurons, matrixLayout, weightsShape, hiddenShape, biasesShape, random);
            this.OutputShape = new[] { inputShape[(int)Axis.B], numberOfNeurons };
        }
    }
}
