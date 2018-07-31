// -----------------------------------------------------------------------
// <copyright file="BayesClassifier.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Bayes classifier.
    /// </summary>
    public class BayesClassifier
        : Classifier<PageSource, BayesFeatures, BayesFeatureBuilder>
    {
        [JsonProperty("classes")]
        private readonly Dictionary<string, ClassData> classes = new Dictionary<string, ClassData>();

        /// <summary>
        /// Gets a number of unique words from all pages the classifier was trained on.
        /// </summary>
        /// <value>
        /// The <see cref="int"/> that contains unique word count.
        /// </value>
        [JsonProperty("uniqueWordCount")]
        public int UniqueWordCount { get; private set; }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="features"/> is <b>null</b>.
        /// </exception>
        public override Answer Classify(BayesFeatures features, CancellationToken cancellationToken)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            // calculate candidates scores
            if (features.Words.Count > 0)
            {
                List<(string className, double score)> candidates = new List<(string, double)>(this.classes.Count);
                foreach (KeyValuePair<string, ClassData> kvp in this.classes)
                {
                    double wordCount = kvp.Value.WordCount + this.UniqueWordCount;
                    double zeroFrequencyWeight = Math.Log(1.0 / wordCount);

                    double weight = features.Words.Sum(x =>
                        kvp.Value.Words.TryGetValue(x, out int count) ? Math.Log((1.0 + count) / wordCount) : zeroFrequencyWeight);
                    double score = weight / features.Words.Count;

                    candidates.Add((kvp.Key, score));
                }

                // calculate confidence
                if (candidates.Any())
                {
                    candidates.Sort((x, y) => y.score.CompareTo(x.score));

                    // select best candidate
                    (string className, double score) bestCandidate = candidates[0];
                    string answer = bestCandidate.className;

                    // calculate confidence
                    double normalizer = this.classes[answer].Normalizer;
                    double confidence = Math.Exp(bestCandidate.score - normalizer);

                    // scale answer confidence
                    confidence = ScaleConfidence(confidence);
                    confidence = Convert.ToInt32(Math.Round(100.0 * confidence)); ////.PutInRange(0, 100);

                    // scale confidences for the candidates
                    return new Answer(
                        features,
                        answer,
                        confidence,
                        candidates.Select(x => (x.className, ScaleConfidence(Math.Exp(x.score - normalizer)))));
                }
            }

            // return blank answer if could not classify
            return new Answer(features);

            double ScaleConfidence(double inputConf)
            {
                double[][] grid = new double[][]
                {
                    new double[] { 0, 0 },
                    new double[] { 0.1, 0.01 },
                    new double[] { 0.2, 0.025 },
                    new double[] { 0.3, 0.07 },
                    new double[] { 0.4, 0.3 },
                    new double[] { 0.5, 0.55 },
                    new double[] { 0.6, 0.8 },
                    new double[] { 0.7, 0.88 },
                    new double[] { 0.8, 0.93 },
                    new double[] { 0.9, 0.965 },
                    new double[] { 1, 1 },
                };

                for (int i = 0, ii = grid.GetLength(0) - 1; i < ii; i++)
                {
                    if (grid[i + 1][0] > inputConf)
                    {
                        return grid[i][1] + ((inputConf - grid[i][0]) * ((grid[i + 1][1] - grid[i][1]) / (grid[i + 1][0] - grid[i][0])));
                    }
                }

                return 1.0;
            }
        }

        /// <inheritdoc />
        public override void Train<T>(
            IEnumerable<T> sources,
            System.Func<T,
            CancellationToken,
            BayesFeatures> selector,
            System.Func<T, Features> truthselector,
            IClassifierProgress<T> progress,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        private protected override void BeginTraining(CancellationToken cancellationToken)
        {
            this.classes.Clear();

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <inheritdoc />
        private protected override void FinishTraining(CancellationToken cancellationToken)
        {
            // calculate the total number of unique words
            HashSet<string> uniqueWords = new HashSet<string>();
            foreach (ClassData data in this.classes.Values)
            {
                uniqueWords.UnionWith(data.Words.Keys);
            }

            this.UniqueWordCount = uniqueWords.Count;

            // calculate normalizers for the classes
            foreach (ClassData data in this.classes.Values)
            {
                data.Commit(this.UniqueWordCount);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <inheritdoc />
        private protected override bool Train(BayesFeatures features, string truth, CancellationToken cancellationToken)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            if (!this.classes.TryGetValue(truth, out ClassData data))
            {
                this.classes[truth] = data = new ClassData();
            }

            data.Add(features.Words);

            cancellationToken.ThrowIfCancellationRequested();
            return true;
        }

        /// <summary>
        /// Represents a part of the trained context that relates to a specific class.
        /// </summary>
        private sealed class ClassData
        {
            [JsonProperty("words")]
            private readonly SortedDictionary<string, int> words = new SortedDictionary<string, int>();

            [JsonProperty("wordCount")]
            private int wordCount = 0;

            [JsonProperty("normalizer")]
            private double normalizer = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClassData"/> class.
            /// </summary>
            public ClassData()
            {
            }

            /// <summary>
            /// Gets a total number of words from all pages the classifier was trained on.
            /// </summary>
            /// <value>
            /// The <see cref="int"/> that contains word count.
            /// </value>
            public int WordCount => this.wordCount;

            /// <summary>
            /// Gets a collection of words from all pages the classifier was trained on mapped into their number of appearances.
            /// </summary>
            /// <value>
            /// The <see cref="IDictionary{TKey, TValue}"/> that contains the word collection.
            /// </value>
            public IDictionary<string, int> Words => this.words;

            public double Normalizer => this.normalizer;

            public void Add(string word, int count)
            {
                if (this.words.TryGetValue(word, out int oldCount))
                {
                    this.words[word] = count + oldCount;
                }
                else
                {
                    this.words[word] = count;
                }

                this.wordCount += count;
            }

            public void Add(IEnumerable<string> addwords)
            {
                foreach (string word in addwords)
                {
                    this.Add(word, 1);
                }
            }

            public void Commit(int uniqueWordCount)
            {
                if (this.wordCount > 0 && uniqueWordCount > 0)
                {
                    double weight = this.words.Values.Sum(x =>
                    {
                        double frequency = (1.0 + x) / (this.wordCount + uniqueWordCount);
                        return Math.Log(frequency) * x;
                    });

                    this.normalizer = weight / this.wordCount;
                }
            }
        }
    }
}
