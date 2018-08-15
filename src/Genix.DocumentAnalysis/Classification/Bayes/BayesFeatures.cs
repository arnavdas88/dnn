// -----------------------------------------------------------------------
// <copyright file="BayesFeatures.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a set of features for <see cref="BayesClassifier"/>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BayesFeatures : Features
    {
        [JsonProperty("words")]
        private readonly List<string> words = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BayesFeatures"/> class.
        /// </summary>
        /// <param name="id">The source of data.</param>
        /// <param name="words">The collection of words extracted from the data.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="id"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="words"/> is <b>null</b>.</para>
        /// </exception>
        public BayesFeatures(DataSourceId id, IEnumerable<string> words)
            : base(id)
        {
            this.words.AddRange(words);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BayesFeatures"/> class,
        /// using the existing <see cref="BayesFeatures"/> object.
        /// </summary>
        /// <param name="other">The <see cref="BayesFeatures"/> to copy the data from.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is <b>null</b>.
        /// </exception>
        public BayesFeatures(BayesFeatures other)
            : base(other)
        {
            this.words.AddRange(other.words);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BayesFeatures"/> class.
        /// </summary>
        [JsonConstructor]
        private BayesFeatures()
        {
        }

        /// <summary>
        /// Gets a collection of words extracted from the data.
        /// </summary>
        /// <value>
        /// The <see cref="ReadOnlyCollection{T}"/> that contains a collection of words.
        /// </value>
        public ReadOnlyCollection<string> Words => new ReadOnlyCollection<string>(this.words);
    }
}
