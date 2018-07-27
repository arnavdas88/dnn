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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Simple recurrent network (SRN) cell.
    /// </summary>
    public class SRNCell : RNNCell
    {
        /// <summary>
        /// The regular expression pattern that matches layer architecture.
        /// </summary>
        public const string ArchitecturePattern = @"^(\d+)(SRNC)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        public SRNCell(
            int[] inputShape,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
        {
            this.Initialize(inputShape, numberOfNeurons, matrixLayout, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class, using the specified architecture.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        public SRNCell(int[] inputShape, string architecture, RandomNumberGenerator random)
        {
            List<Group> groups = Layer.ParseArchitechture(architecture, SRNCell.ArchitecturePattern);
            int numberOfNeurons = Convert.ToInt32(groups[1].Value, CultureInfo.InvariantCulture);

            this.Initialize(inputShape, numberOfNeurons, MatrixLayout.RowMajor, random);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SRNCell"/> class, using the existing <see cref="SRNCell"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SRNCell"/> to copy the data from.</param>
        public SRNCell(SRNCell other) : base(other)
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
        public override string Architecture => string.Format(CultureInfo.InvariantCulture, "{0}SRNC", this.NumberOfNeurons);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone() => new SRNCell(this);

        /// <inheritdoc />
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Stands for length of time sequence.")]
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
            IList<Tensor> ys = base.Forward(session, xs);

            Tensor x = xs[0];
            Tensor y = ys[0];
            int T = x.Axes[(int)Axis.B];        // number of vectors in time sequence*/

#if TENSORFLOW
            Tensor[] hs = session.Unstack(y, 0);

            hs[0] = session.ReLU(hs[0]);

            // add hidden layer to the output tensor
            // y += U * y(t-1) (product of hidden weight matrix and hidden vector)
            for (int t = 1; t < T; t++)
            {
                Tensor h = session.Add(
                    hs[t],
                    session.MxV(this.MatrixLayout, this.U, false, hs[t - 1], null));

                hs[t] = session.ReLU(h);
            }

            return new[] { session.Stack(hs, 0) };
#else
            int ylen = this.NumberOfNeurons;        // number of output neurons / size of output vector
            float[] uw = this.U.Weights;
            float[] yw = y.Weights;

            // add hidden layer to the output tensor
            // y += U * y(t-1) (product of hidden weight matrix and hidden vector)
            Nonlinearity.ReLU(ylen, yw, 0, yw, 0);

            for (int t = 1, yi = ylen; t < T; t++, yi += ylen)
            {
                Matrix.MxV(this.MatrixLayout, ylen, ylen, uw, 0, false, yw, yi - ylen, yw, yi, false);

                // TODO: customize activation function
                Nonlinearity.ReLU(ylen, yw, yi, yw, yi);
            }

            if (session.CalculateGradients)
            {
                session.Push(
                    "srn",
                    () =>
                    {
                        float[] duw = this.U.Gradient;
                        float[] dyw = y.Gradient;

                        for (int t = T - 1, yi = t * ylen; t > 0; t--, yi -= ylen)
                        {
                            Nonlinearity.ReLUGradient(ylen, dyw, yi, true, yw, yi, dyw, yi);

                            // dA += dy * x'
                            lock (this.U)
                            {
                                Matrix.VxV(this.MatrixLayout, ylen, ylen, dyw, yi, yw, yi - ylen, duw, 0);
                            }

                            // dx += A' * dy
                            Matrix.MxV(this.MatrixLayout, ylen, ylen, uw, 0, true, dyw, yi, dyw, yi - ylen, false);
                        }

                        Nonlinearity.ReLUGradient(ylen, dyw, 0, true, yw, 0, dyw, 0);
                    });
            }

            return ys;
#endif
        }

        /// <summary>
        /// Initializes the <see cref="SRNCell"/>.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="numberOfNeurons">The number of neurons in the layer.</param>
        /// <param name="matrixLayout">Specifies whether the weight matrices are row-major or column-major.</param>
        /// <param name="random">The random numbers generator.</param>
        private void Initialize(
            int[] inputShape,
            int numberOfNeurons,
            MatrixLayout matrixLayout,
            RandomNumberGenerator random)
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

            this.Initialize(numberOfNeurons, matrixLayout, weightsShape, hiddenShape, biasesShape, random);
            this.OutputShape = new[] { inputShape[(int)Axis.B], numberOfNeurons };
        }
    }
}
