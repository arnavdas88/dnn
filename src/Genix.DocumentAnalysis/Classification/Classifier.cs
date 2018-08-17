// -----------------------------------------------------------------------
// <copyright file="Classifier.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the contract that provides data classification.
    /// </summary>
    /// <typeparam name="TSource">The type of data source this <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> can classify.</typeparam>
    /// <typeparam name="TFeatures">The type of features this <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> can classify.</typeparam>
    /// <typeparam name="TFeatureBuilder">The type of feature builder this <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> uses.</typeparam>
    public abstract class Classifier<TSource, TFeatures, TFeatureBuilder>
        where TSource : DataSource
        where TFeatures : Features
        where TFeatureBuilder : IFeatureBuilder<TSource, TFeatures>, new()
    {
        private readonly TFeatureBuilder featureBuilder = new TFeatureBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> class.
        /// </summary>
        protected Classifier()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the classifier has been trained.
        /// </summary>
        /// <value>
        /// <b>true</b> if the classifier has been trained and is ready for use; otherwise, <b>false</b>.
        /// </value>
        public abstract bool IsLearned { get; }

        /// <summary>
        /// Gets a collection of classes the classifier is able to classify to.
        /// </summary>
        /// <value>
        /// The <see cref="IReadOnlyCollection{T}"/> that contains the class names.
        /// </value>
        public abstract IReadOnlyCollection<string> Classes { get; }

        /// <summary>
        /// Gets the feature builder used by this classifier.
        /// </summary>
        /// <value>
        /// The <see cref="IFeatureBuilder{TSource, TFeatures}"/> object.
        /// </value>
        public TFeatureBuilder FeatureBuilder => this.featureBuilder;

        /// <summary>
        /// Saves the current <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/>.</param>
        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Saves the current <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/>.</returns>
        public byte[] SaveToMemory()
        {
            return UTF8Encoding.UTF8.GetBytes(this.SaveToString());
        }

        /// <summary>
        /// Saves the current <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="Classifier{TSource, TFeatures, TFeatureBuilder}"/>.</returns>
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Extracts features from a <typeparamref name="TSource"/>,
        /// and monitors cancellation requests.
        /// </summary>
        /// <param name="source">The data to extract the features from.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The <typeparamref name="TFeatures"/> object this method creates.
        /// </returns>
        public TFeatures BuildFeatures(TSource source, CancellationToken cancellationToken)
        {
            return this.featureBuilder.BuildFeatures(source, cancellationToken);
        }

        /// <summary>
        /// Extracts features from a sequence of elements,
        /// provides progress information,
        /// and monitors cancellation requests.
        /// </summary>
        /// <typeparam name="T">The type of the elements to extract features from.</typeparam>
        /// <param name="sources">The elements to extract the features from.</param>
        /// <param name="selector">The selector that converts elements into <typeparamref name="TSource"/> objects.</param>
        /// <param name="progress">The provider used to report a progress change. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The sequence of <typeparamref name="TFeatures"/> objects this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="sources"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="selector"/> is <b>null</b>.</para>
        /// </exception>
        public IEnumerable<TFeatures> BuildFeatures<T>(
            IEnumerable<T> sources,
            Func<T, CancellationToken, TSource> selector,
            IClassifierProgress<T> progress,
            CancellationToken cancellationToken)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return sources
                .AsParallel()
                .AsOrdered()
                .WithCancellation(cancellationToken)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select((source, index) =>
                {
                    progress?.OnClassifying(source, index);

                    TFeatures features = null;
                    try
                    {
                        features = this.BuildFeatures(selector(source, cancellationToken), cancellationToken);
                        progress?.OnClassified(source, index, null, null);
                    }
                    catch (Exception e)
                    {
                        progress?.OnClassified(source, index, null, e);
                    }

                    return features;
                });
        }

        /// <summary>
        /// Classifies a data using the extracted <typeparamref name="TFeatures"/>,
        /// and monitors cancellation requests.
        /// </summary>
        /// <param name="features">The features extracted from the data.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Answer"/> that contains the classification result.
        /// </returns>
        public abstract Answer Classify(TFeatures features, CancellationToken cancellationToken);

        /// <summary>
        /// Classifies a data represented by <typeparamref name="TSource"/>,
        /// and monitors cancellation requests.
        /// </summary>
        /// <param name="source">The data to classify.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Answer"/> that contains the classification result.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        public Answer Classify(TSource source, CancellationToken cancellationToken)
        {
            return this.Classify(
                this.BuildFeatures(source, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        /// Classifies a sequence of elements using the features extracted from each element,
        /// and monitors cancellation requests.
        /// </summary>
        /// <typeparam name="T">The type of the elements to classify.</typeparam>
        /// <param name="sources">The elements to classify.</param>
        /// <param name="selector">The selector that converts elements into <typeparamref name="TFeatures"/> objects.</param>
        /// <param name="progress">The provider used to report a progress change. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The sequence of <see cref="Answer"/> objects that contains the classification results.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="sources"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="selector"/> is <b>null</b>.</para>
        /// </exception>
        public IEnumerable<Answer> Classify<T>(
            IEnumerable<T> sources,
            Func<T, CancellationToken, TFeatures> selector,
            IClassifierProgress<T> progress,
            CancellationToken cancellationToken)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return sources
                .AsParallel()
                .AsOrdered()
                .WithCancellation(cancellationToken)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select((source, index) =>
                {
                    Answer answer = null;

                    progress?.OnClassifying(source, index);

                    TFeatures features = selector(source, cancellationToken);
                    if (features != null)
                    {
                        try
                        {
                            answer = this.Classify(features, cancellationToken);
                            progress?.OnClassified(source, index, answer, null);
                        }
                        catch (Exception e)
                        {
                            progress?.OnClassified(source, index, null, e);
                        }
                    }

                    return answer;
                });
        }

        /// <summary>
        /// Classifies a sequence of elements, and monitors cancellation requests.
        /// </summary>
        /// <typeparam name="T">The type of the elements to classify.</typeparam>
        /// <param name="sources">The elements to classify.</param>
        /// <param name="selector">The selector that converts elements into <typeparamref name="TSource"/> objects.</param>
        /// <param name="progress">The provider used to report a progress change. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns>
        /// The sequence of <see cref="Answer"/> objects that contains the classification results.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="sources"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="selector"/> is <b>null</b>.</para>
        /// </exception>
        public IEnumerable<(T source, Answer answer)> Classify<T>(
            IEnumerable<T> sources,
            Func<T, CancellationToken, TSource> selector,
            IClassifierProgress<T> progress,
            CancellationToken cancellationToken)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return sources
                .AsParallel()
                .AsOrdered()
                .WithCancellation(cancellationToken)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Select((element, index) =>
                {
                    Answer answer = null;

                    progress?.OnClassifying(element, index);

                    TSource source = selector(element, cancellationToken);
                    if (source != null)
                    {
                        try
                        {
                            answer = this.Classify(source, cancellationToken);
                            progress?.OnClassified(element, index, answer, null);
                        }
                        catch (Exception e)
                        {
                            progress?.OnClassified(element, index, null, e);
                        }
                    }

                    return (element, answer);
                });
        }

        /*/// <summary>
        /// Trains the classifier on a sequence of features extracted from the elements,
        /// provides progress information,
        /// and monitors cancellation requests.
        /// </summary>
        /// <typeparam name="T">The type of the elements used for training.</typeparam>
        /// <param name="sources">The elements used for training.</param>
        /// <param name="selector">The selector that converts elements into <typeparamref name="TFeatures"/> objects.</param>
        /// <param name="truthselector">The selector that converts elements into ground truth.</param>
        /// <param name="progress">The provider used to report a progress change. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        public abstract void Train<T>(
            IEnumerable<T> sources,
            Func<T, CancellationToken, TFeatures> selector,
            Func<T, Features> truthselector,
            IClassifierProgress<T> progress,
            CancellationToken cancellationToken);*/

        /// <summary>
        /// Trains the classifier on a sequence of elements,
        /// provides progress information,
        /// and monitors cancellation requests.
        /// </summary>
        /// <typeparam name="T">The type of the elements used for training.</typeparam>
        /// <param name="sources">The elements used for training.</param>
        /// <param name="selector">The selector that converts elements into <typeparamref name="TSource"/> objects.</param>
        /// <param name="truthselector">The selector that converts elements into ground truth.</param>
        /// <param name="progress">The provider used to report a progress change. Can be <b>null</b>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="sources"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="selector"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="truthselector"/> is <b>null</b>.</para>
        /// </exception>
        public void Train<T>(
            IEnumerable<T> sources,
            Func<T, CancellationToken, TSource> selector,
            Func<T, string> truthselector,
            IClassifierProgress<T> progress,
            CancellationToken cancellationToken)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (truthselector == null)
            {
                throw new ArgumentNullException(nameof(truthselector));
            }

            this.BeginTraining(cancellationToken);

            try
            {
                int index = 0;
                int trainedCount = 0;
                sources
                    .AsParallel()
                    .AsOrdered()
                    .WithCancellation(cancellationToken)
                    .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                    .WithDegreeOfParallelism(1)
                    .ForAll(element =>
                    {
                        progress?.OnClassifying(element, index);

                        TSource source = selector(element, cancellationToken);
                        if (source != null)
                        {
                            string truth = truthselector(element);
                            if (!string.IsNullOrEmpty(truth))
                            {
                                try
                                {
                                    if (this.Train(this.BuildFeatures(source, cancellationToken), truth.ToUpperInvariant(), cancellationToken))
                                    {
                                        trainedCount++;
                                    }

                                    progress?.OnClassified(element, index, null, null);
                                }
                                catch (Exception e)
                                {
                                    progress?.OnClassified(element, index, null, e);
                                }
                            }
                        }

                        index = Interlocked.Increment(ref index);
                    });

                if (trainedCount < 1)
                {
                    throw new InvalidOperationException(Properties.Resources.E_Classifier_EmptyTrainset);
                }

                cancellationToken.ThrowIfCancellationRequested();
                this.FinishTraining(cancellationToken);
            }
            catch
            {
                // in case of exception reset to original state
                this.BeginTraining(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// This method is called when training starts.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        private protected abstract void BeginTraining(CancellationToken cancellationToken);

        /// <summary>
        /// This method is called when a new features should be added to a training set.
        /// </summary>
        /// <param name="features">The features to add to the training set.</param>
        /// <param name="truth">The ground truth that corresponds to <paramref name="features"/>.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        /// <returns><b>true</b> if the classifier was trained on these features; otherwise, <b>false</b>.</returns>
        private protected abstract bool Train(TFeatures features, string truth, CancellationToken cancellationToken);

        /// <summary>
        /// This method is called when training ends.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken token used to notify the classifier that the operation should be canceled.</param>
        private protected abstract void FinishTraining(CancellationToken cancellationToken);
    }
}
