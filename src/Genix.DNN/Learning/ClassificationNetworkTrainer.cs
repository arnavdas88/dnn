// -----------------------------------------------------------------------
// <copyright file="ClassificationNetworkTrainer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Learning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Learning;

    /// <summary>
    /// Represents a basic trainer for neural nets.
    /// </summary>
    public class ClassificationNetworkTrainer : Trainer<int[]>
    {
        /// <summary>
        /// Performs one epoch of SGD algorithm.
        /// </summary>
        /// <param name="epoch">The zero-based index of learning epoch.</param>
        /// <param name="net">The network to train.</param>
        /// <param name="input">The sequence of learning samples.</param>
        /// <param name="algorithm">The training algorithm.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <see cref="TrainingResult"/> object that contains training results for the epoch.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need a sequence of generic arguments here.")]
        public TrainingResult RunEpoch(
            int epoch,
            ClassificationNetwork net,
            IEnumerable<(Tensor Data, string[] Label)> input,
            ITrainingAlgorithm algorithm,
            ILoss<int[]> lossFunction,
            CancellationToken cancellationToken)
        {
            if (net == null)
            {
                throw new ArgumentNullException(nameof(net));
            }

            return this.RunEpoch(
                net,
                input.Select(x => (x.Data, x.Label.Select(truth => net.Classes.IndexOf(truth)).ToArray())),
                epoch,
                algorithm,
                lossFunction,
                cancellationToken);
        }
    }
}
