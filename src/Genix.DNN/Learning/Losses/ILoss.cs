// -----------------------------------------------------------------------
// <copyright file="ILoss.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Genix.MachineLearning;

namespace Genix.DNN.Learning
{
    /// <summary>
    /// Common interface for loss functions, such as <see cref="SquareLoss"/> and <see cref="LogLikelihoodLoss"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In mathematical optimization, statistics, decision theory and machine learning, a loss
    /// function or cost function is a function that maps an event or values of one or more
    /// variables onto a real number intuitively representing some "cost" associated with the
    /// event. An optimization problem seeks to minimize a loss function. An objective function
    /// is either a loss function or its negative (sometimes called a reward function, a profit
    /// function, a utility function, a fitness function, etc.), in which case it is to be
    /// maximized.</para>
    /// <para>
    /// References:
    /// <list type="bullet">
    /// <item><description><a href="https://en.wikipedia.org/wiki/Loss_function">
    /// Wikipedia contributors. "Loss function." Wikipedia, The Free Encyclopedia.
    /// Wikipedia, The Free Encyclopedia, 18 Mar. 2016. Web.</a></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <typeparam name="TExpected">The type for the expected values.</typeparam>
    public interface ILoss<TExpected>
    {
        /// <summary>
        /// Computes the loss between the expected value (ground truth) and the given actual value that have been predicted.
        /// </summary>
        /// <param name="y">The value that have been predicted.</param>
        /// <param name="expected">The expected value that should have been predicted.</param>
        /// <param name="calculateGradient">Determines whether the gradient for <c>y</c> should be calculated.</param>
        /// <returns>The loss value between the expected value and the actual predicted value.</returns>
        float Loss(Tensor y, TExpected expected, bool calculateGradient);
    }
}
