// -----------------------------------------------------------------------
// <copyright file="TextReader.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Recognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Genix.DNN;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Imaging;
    using Genix.MachineLearning.LanguageModel;

    /// <summary>
    /// Represents a checkbox recognizer.
    /// </summary>
    public class TextReader : IFieldReader<ImageSource>
    {
        private readonly ClassificationNetwork network;

        /// <summary>
        /// The language model used for recognition.
        /// </summary>
        private string model;
        private Context context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextReader"/> class.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the reader's <see cref="ClassificationNetwork"/>.</param>
        public TextReader(string fileName)
        {
            this.network = ClassificationNetwork.FromFile(fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextReader"/> class.
        /// </summary>
        public TextReader()
            : this(null/*this.options.NetworkFileName*/)
        {
        }

        /// <summary>
        /// Gets or sets the regular expression that describes the language model used for recognition.
        /// </summary>
        /// <value>
        /// The <see cref="string"/> that contains a regular expression.
        /// </value>
        public string Model
        {
            get => this.model;
            set
            {
                if (this.model != value)
                {
                    this.model = value;
                    this.context = null;
                }
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para><see cref="Model"/> is <b>null</b> or <see cref="string.Empty"/>.</para>
        /// </exception>
        public Answer Recognize(ImageSource source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // initialize language model
            if (this.context == null)
            {
                if (string.IsNullOrEmpty(this.model))
                {
                    throw new InvalidOperationException("The language model is not set.");
                }
            }

            // create input tensor
            Tensor x = ImageExtensions.FromImage(
                source.Image,
                this.network.InputShape.GetAxis(Axis.X),
                this.network.InputShape.GetAxis(Axis.Y),
                "text",
                this.network.InputShape.Format);

            // recognize the image
            IList<(string Answer, float Probability)> result = this.network.ExecuteSequence(x, this.context).Answers;

            // create the answer
            return TextReader.CreateAnswer(source, result);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sources"/> is <b>null</b>.
        /// </exception>
        public IList<Answer> Recognize(IList<ImageSource> sources, CancellationToken cancellationToken)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            return sources
                .AsParallel()
                .AsOrdered()
                .WithCancellation(cancellationToken)
                .Select(x => this.Recognize(x, cancellationToken))
                .ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Answer CreateAnswer(ImageSource source, IList<(string Answer, float Probability)> result)
        {
            return new Answer(source.Id, result[0].Answer, result[0].Probability, source.Image.Bounds, result);
        }
    }
}
