// -----------------------------------------------------------------------
// <copyright file="CheckboxReader.cs" company="Noname, Inc.">
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

    /// <summary>
    /// Represents a checkbox recognizer.
    /// </summary>
    public class CheckboxReader : IFieldReader<ImageSource>
    {
        private readonly ClassificationNetwork network;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxReader"/> class.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the reader's <see cref="ClassificationNetwork"/>.</param>
        public CheckboxReader(string fileName)
        {
            this.network = ClassificationNetwork.FromFile(fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxReader"/> class.
        /// </summary>
        public CheckboxReader()
            : this(null/*this.options.NetworkFileName*/)
        {
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        public Answer Recognize(ImageSource source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // create input tensor
            Tensor x = ImageExtensions.FromImage(
                source.Image,
                "checkbox",
                this.network.InputShape.Format,
                this.network.InputShape.GetAxis(Axis.X),
                this.network.InputShape.GetAxis(Axis.Y));

            // recognize the image
            IList<(string Answer, float Probability)> result = this.network.Execute(x).Answers[0];

            // create the answer
            return CheckboxReader.CreateAnswer(source, result);
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

            // create input tensor
            Tensor x = ImageExtensions.FromImages(
                sources,
                source => source.Image,
                "checkbox",
                this.network.InputShape.Format,
                this.network.InputShape.GetAxis(Axis.X),
                this.network.InputShape.GetAxis(Axis.Y));

            // recognize the image
            IList<IList<(string Answer, float Probability)>> results = this.network.Execute(x).Answers;

            // create the answers
            List<Answer> answers = new List<Answer>(sources.Count);
            for (int i = 0, ii = results.Count; i < ii; i++)
            {
                answers.Add(CheckboxReader.CreateAnswer(sources[i], results[i]));
            }

            return answers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Answer CreateAnswer(ImageSource source, IList<(string Answer, float Probability)> result)
        {
            return new Answer(source.Id, result[0].Answer, result[0].Probability, source.Image.Bounds, result);
        }
    }
}
