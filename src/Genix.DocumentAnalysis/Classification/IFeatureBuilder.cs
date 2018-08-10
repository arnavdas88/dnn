// -----------------------------------------------------------------------
// <copyright file="IFeatureBuilder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System.Threading;
    using Genix.Core;

    /// <summary>
    /// Defines the contract that provides features extraction for further classification.
    /// This is an abstract class.
    /// </summary>
    /// <typeparam name="TSource">The type of data source this <see cref="IFeatureBuilder{TSource, TFeatures}"/> can extract features from.</typeparam>
    /// <typeparam name="TFeatures">The type of features this <see cref="IFeatureBuilder{TSource, TFeatures}"/> extracts.</typeparam>
    public interface IFeatureBuilder<TSource, TFeatures>
        where TSource : DataSource
        where TFeatures : Features
    {
        /// <summary>
        /// Extracts a set of features from a data source, and monitors cancellation requests.
        /// </summary>
        /// <param name="source">The data source to extract the set of features from.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="IFeatureBuilder{TSource, TFeatures}"/> that the operation should be canceled.</param>
        /// <returns>
        /// The <typeparamref name="TFeatures"/> object this method creates.
        /// </returns>
        TFeatures BuildFeatures(TSource source, CancellationToken cancellationToken);
    }
}
