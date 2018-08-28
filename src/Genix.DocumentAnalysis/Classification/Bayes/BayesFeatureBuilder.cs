// -----------------------------------------------------------------------
// <copyright file="BayesFeatureBuilder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Genix.DocumentAnalysis.OCR;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides features extraction for <see cref="BayesClassifier"/>.
    /// </summary>
    public class BayesFeatureBuilder
        : IFeatureBuilder<PageSource, BayesFeatures>,
          IFeatureBuilder<ImageSource, BayesFeatures>
    {
        /// <summary>
        /// The words that should be excluded from extracted features.
        /// </summary>
        [JsonProperty("ignoreWords")]
        private readonly HashSet<string> ignoreWords = new HashSet<string>();

        private readonly OCR ocr = null;

        /// <summary>
        /// Gets the words that should be excluded from extracted features.
        /// </summary>
        /// <value>
        /// The <see cref="HashSet{T}"/> that contains words to exclude.
        /// </value>
        public HashSet<string> IgnoreWords => this.ignoreWords;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        public BayesFeatures BuildFeatures(PageSource source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            IList<string> words = GetWordsPage();
            return new BayesFeatures(source.Id, words);

            IList<string> GetWordsPage()
            {
                return source
                    .Page
                    .EnumAllShapes<WordShape>()
                    .Select(x => PrepareString(x.Text))
                    .Where(x => !string.IsNullOrEmpty(x) && !this.ignoreWords.Contains(x))
                    .ToList();

                string PrepareString(string s)
                {
                    if (string.IsNullOrEmpty(s) || s.HasDigits())
                    {
                        return null;
                    }

                    s = s.RemovePunctuation();
                    return s.Length > 2 && s.HasLetters() ? s.ToUpper(CultureInfo.InvariantCulture) : null;
                }
            }
        }

        /// <inheritdoc />
        public BayesFeatures BuildFeatures(ImageSource source, CancellationToken cancellationToken)
        {
            PageShape page = this.ocr.Recognize(source.Image);
            return this.BuildFeatures(new PageSource(source.Id, page), cancellationToken);
        }
    }
}
