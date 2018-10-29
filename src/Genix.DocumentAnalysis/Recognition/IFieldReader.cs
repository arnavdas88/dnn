// -----------------------------------------------------------------------
// <copyright file="IFieldReader.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Recognition
{
    using System.Collections.Generic;
    using System.Threading;
    using Genix.Core;

    /// <summary>
    /// Defines the contract that provides text field recognition.
    /// This is an abstract class.
    /// </summary>
    /// <typeparam name="TSource">The type of data source this <see cref="IFieldReader{TSource}"/> can process.</typeparam>
    public interface IFieldReader<TSource>
        where TSource : DataSource
    {
        /// <summary>
        /// Recognizes a text within the field, and monitors cancellation requests.
        /// </summary>
        /// <param name="source">The data source that contains the text to recognize.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="IFieldReader{TSource}"/> that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Answer"/> that contains the field recognition answer.
        /// </returns>
        Answer Recognize(TSource source, CancellationToken cancellationToken);

        /// <summary>
        /// Recognizes a text within a batch of fields, and monitors cancellation requests.
        /// </summary>
        /// <param name="sources">The collection of data sources that contains the texts to recognize.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="IFieldReader{TSource}"/> that the operation should be canceled.</param>
        /// <returns>
        /// The collection of <see cref="Answer"/> objects that contains the field recognition answers.
        /// </returns>
        IList<Answer> Recognize(IList<TSource> sources, CancellationToken cancellationToken);
    }
}
