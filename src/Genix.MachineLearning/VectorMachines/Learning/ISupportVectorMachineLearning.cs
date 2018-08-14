// -----------------------------------------------------------------------
// <copyright file="ISupportVectorMachineLearning.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.VectorMachines.Learning
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Defines the contract for Support Vector Machines (SVM) learning.
    /// </summary>
    public interface ISupportVectorMachineLearning
    {
        /// <summary>
        /// Learns a Support Vector Machines (SVM) that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="samples">
        /// The samples used for learning.
        /// Each sample consists of input vector <c>x</c>,
        /// expected binary output <c>y</c>,
        /// and the <c>weight</c> of importance (if supported by the learning algorithm).
        /// </param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the machine that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="SupportVectorMachine"/> learned by this method.
        /// A model that has learned how to produce <paramref name="samples" />.y given <paramref name="samples" />.x.
        /// </returns>
        SupportVectorMachine Learn(IList<(float[] x, bool y, float weight)> samples, CancellationToken cancellationToken);
    }
}
