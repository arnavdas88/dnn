// -----------------------------------------------------------------------
// <copyright file="TrainingResult.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System.Globalization;

    /// <summary>
    /// Represents a result of network training.
    /// </summary>
    public class TrainingResult
    {
        /// <summary>
        /// Gets or sets L1 regularization loss.
        /// </summary>
        /// <value>
        /// The L1 regularization loss.
        /// </value>
        public float L1Loss { get; set; }

        /// <summary>
        /// Gets or sets L2 regularization loss.
        /// </summary>
        /// <value>
        /// the L2 regularization loss.
        /// </value>
        public float L2Loss { get; set; }

        /// <summary>
        /// Gets or sets cost loss calculated by the loss function.
        /// </summary>
        /// <value>
        /// The cost loss.
        /// </value>
        public float CostLoss { get; set; }

        /// <summary>
        /// Gets the overall loss.
        /// </summary>
        /// <value>
        /// The sum of <see cref="L1Loss"/>, <see cref="L2Loss"/> and <see cref="CostLoss"/>.
        /// </value>
        public float Loss => this.CostLoss + this.L1Loss + this.L2Loss;

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "L1Loss: {0}, L2Loss: {1}, CostLoss: {2}, Loss: {3}",
            this.L1Loss,
            this.L2Loss,
            this.CostLoss,
            this.Loss);
    }
}
