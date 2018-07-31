// -----------------------------------------------------------------------
// <copyright file="ImageSource.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using Genix.Imaging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the <see cref="DataSource"/> that contains the image data.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ImageSource : DataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSource"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the data.</param>
        /// <param name="name">The name of the data.</param>
        /// <param name="frameIndex">
        /// The zero-based index for this page if it belongs to a multi-page file.
        /// <b>null</b> if this page belongs to a single-page file.
        /// </param>
        /// <param name="image">The <see cref="PageShape"/> object that contains the page data.</param>
        protected ImageSource(string id, string name, int? frameIndex, Image image)
            : base(id, name, frameIndex)
        {
            this.Image = image;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSource"/> class,
        /// using the existing <see cref="ImageSource"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ImageSource"/> to copy the data from.</param>
        protected ImageSource(ImageSource other)
            : base(other)
        {
            this.Image = other.Image;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSource"/> class.
        /// </summary>
        private ImageSource()
        {
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>
        /// A <see cref="Image"/> object that contains the image.
        /// </value>
        [JsonProperty("image")]
        public Image Image { get; private set; }
    }
}
