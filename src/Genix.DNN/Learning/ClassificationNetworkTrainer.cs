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

    /// <summary>
    /// Represents a basic trainer for neural nets.
    /// </summary>
    public class ClassificationNetworkTrainer : NetworkTrainer<int[]>
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
#pragma warning disable SA1009 // Closing parenthesis must be spaced correctly
         public TrainingResult RunEpoch(
            int epoch,
            ClassificationNetwork net,
            IEnumerable<(Tensor Data, string[] Label)> input,
            ITrainingAlgorithm algorithm,
            ILoss<int[]> lossFunction,
            CancellationToken cancellationToken)
#pragma warning restore SA1009 // Closing parenthesis must be spaced correctly
        {
            if (net == null)
            {
                throw new ArgumentNullException(nameof(net));
            }

            return this.RunEpoch(
                net,
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
                input.Select(x => (x.Data, x.Label.Select(truth => net.Classes.IndexOf(truth)).ToArray())),
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
                epoch,
                algorithm,
                lossFunction,
                cancellationToken);
        }
    }
}
