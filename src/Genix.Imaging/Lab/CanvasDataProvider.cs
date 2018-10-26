// -----------------------------------------------------------------------
// <copyright file="CanvasDataProvider.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using Genix.Core;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides images of machine-printed characters to train neural networks.
    /// </summary>
    public class CanvasDataProvider : TestImageProvider<string>
    {
        private readonly Random random = new Random(0);

        [JsonProperty("classes", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        private readonly List<string> classes = new List<string>();

        /// <summary>
        /// The canvas used to draw samples.
        /// </summary>
        private readonly Canvas canvas;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasDataProvider"/> class.
        /// </summary>
        /// <param name="width">The sample width, in pixels.</param>
        /// <param name="height">The sample height, in pixels.</param>
        /// <param name="classes">The list of classes to generate.</param>
        /// <param name="blankClass">The blank class.</param>
        public CanvasDataProvider(int width, int height, IEnumerable<string> classes, string blankClass)
        {
            foreach (string cls in classes)
            {
                if (string.IsNullOrEmpty(cls))
                {
                    throw new ArgumentException("Null or empty class.", nameof(classes));
                }

                if (cls != blankClass)
                {
                    this.classes.Add(cls);
                }
            }

            this.Fonts.Add("Arial");
            this.Fonts.Add("Arial Black");
            this.Fonts.Add("Arial Narrow");
            this.Fonts.Add("Arial Unicode MS");
            this.Fonts.Add("Bahnschrift");
            this.Fonts.Add("Book Antiqua");
            this.Fonts.Add("Bookman Old Style");
            this.Fonts.Add("Calibri");
            this.Fonts.Add("Calibri Light");
            this.Fonts.Add("Candara");
            this.Fonts.Add("Cambria");
            this.Fonts.Add("Century Gothic");
            this.Fonts.Add("Consolas");
            this.Fonts.Add("Corbel");
            this.Fonts.Add("Courier New");
            this.Fonts.Add("Garamond");
            this.Fonts.Add("Georgia");
            this.Fonts.Add("Lucida Bright");
            this.Fonts.Add("Lucida Console");
            this.Fonts.Add("Lucida Fax");
            this.Fonts.Add("Lucida Sans Unicode");
            this.Fonts.Add("Microsoft Sans Serif");
            this.Fonts.Add("Segoe Print");
            this.Fonts.Add("Segoe Script");
            this.Fonts.Add("Segoe UI");
            this.Fonts.Add("Segoe UI Light");
            this.Fonts.Add("Segoe UI Semibold");
            this.Fonts.Add("Tahoma");
            this.Fonts.Add("Times New Roman");
            this.Fonts.Add("Trebuchet MS");
            this.Fonts.Add("Verdana");

            this.Width = width;
            this.Height = height;
            this.canvas = new Canvas(width, height);
        }

        /// <summary>
        /// Gets the fonts used in generation.
        /// </summary>
        /// <value>
        /// The <see cref="IList{T}"/> collection that contains font names.
        /// </value>
        [JsonProperty("fonts", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public IList<string> Fonts { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether regular font styles should be included in generation.
        /// </summary>
        /// <value>
        /// <b>true</b> to generate regular font styles; otherwise, <b>false</b>.
        /// </value>
        [JsonProperty("isRegular")]
        public bool IsRegular { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether bold font styles should be included in generation.
        /// </summary>
        /// <value>
        /// <b>true</b> to generate bold font styles; otherwise, <b>false</b>.
        /// </value>
        [JsonProperty("isBold")]
        public bool IsBold { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether italic font styles should be included in generation.
        /// </summary>
        /// <value>
        /// <b>true</b> to generate italic font styles; otherwise, <b>false</b>.
        /// </value>
        [JsonProperty("isItalic")]
        public bool IsItalic { get; set; } = true;

        /// <summary>
        /// Gets the classes used for text generation.
        /// </summary>
        /// <value>
        /// The <see cref="ReadOnlyCollection{T}"/> collection that contains the classes.
        /// </value>
        public ReadOnlyCollection<string> Classes => new ReadOnlyCollection<string>(this.classes);

        /// <summary>
        /// Gets or sets the minimum number of elements in generated string.
        /// </summary>
        /// <value>
        /// The minimum number of elements in generated string.
        /// </value>
        [JsonProperty("minLength")]
        public int MinLength { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of elements in generated string.
        /// </summary>
        /// <value>
        /// The maximum number of elements in generated string.
        /// </value>
        [JsonProperty("maxLength")]
        public int MaxLength { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of samples to generate for each label.
        /// </summary>
        /// <value>
        /// The number of samples to generate for each label.
        /// </value>
        [JsonProperty("numberOfSamples")]
        public int NumberOfSamples { get; set; } = 1;

        /// <summary>
        /// Generates samples for network learning for specified labels.
        /// </summary>
        /// <param name="labels">The labels to generate samples for.</param>
        /// <returns>The sequence of samples. Each sample consist of image and a ground truth.</returns>
        public override IEnumerable<TestImage> Generate(ISet<string> labels)
        {
            for (int i = 0; i < this.NumberOfSamples; i++)
            {
                foreach (string font in this.Fonts)
                {
                    if (this.IsRegular)
                    {
                        foreach (TestImage sample in this.DrawBitmaps(font, System.Drawing.FontStyle.Regular, this.GenerateLabels(labels)))
                        {
                            yield return sample;
                        }
                    }

                    if (this.IsBold)
                    {
                        foreach (TestImage sample in this.DrawBitmaps(font, System.Drawing.FontStyle.Bold, this.GenerateLabels(labels)))
                        {
                            yield return sample;
                        }
                    }

                    if (this.IsItalic)
                    {
                        foreach (TestImage sample in this.DrawBitmaps(font, System.Drawing.FontStyle.Italic, this.GenerateLabels(labels)))
                        {
                            yield return sample;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Dispose is called.")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.canvas?.Dispose();
            }
        }

        private IEnumerable<string[]> GenerateLabels(ISet<string> lookup)
        {
            if (this.MaxLength == 1)
            {
                for (int i = 0, ii = this.classes.Count; i < ii; i++)
                {
                    string s = this.classes[i];
                    if (lookup == null || lookup.Contains(s))
                    {
                        yield return new string[] { s };
                    }
                }
            }
            else
            {
                int length = this.random.Next(this.MinLength, this.MaxLength + 1);

                string[] s = new string[length];
                for (int i = 0; i < length; i++)
                {
                    s[i] = this.classes[this.random.Next(0, this.classes.Count)];
                }

                if (lookup == null || lookup.Contains(string.Concat(s)))
                {
                    yield return s;
                }
            }
        }

        private IEnumerable<TestImage> DrawBitmaps(string font, System.Drawing.FontStyle fontStyle, IEnumerable<string[]> labels)
        {
            this.canvas.SelectFont(font, this.Height, fontStyle, System.Drawing.GraphicsUnit.Pixel);

            foreach (string[] label in labels)
            {
                string id = string.Join("_", string.Concat(label), font, fontStyle).Replace(' ', '_');

                this.canvas.Clear();
                Rectangle position = this.canvas.DrawText(
                    string.Concat(label),
                    new Rectangle(0, 0, this.Width, this.Height),
                    HorizontalAlignment.Center);

                yield return new TestImage(
                    new DataSourceId(id, null, null),
                    font,
                    fontStyle,
                    this.canvas.ToImage(this.Width <= 0 ? new Rectangle(0, 0, position.Right + (this.Height / 4), this.Height) : Rectangle.Empty),
                    label);
            }
        }
    }
}