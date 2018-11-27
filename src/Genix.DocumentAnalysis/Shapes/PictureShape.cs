// -----------------------------------------------------------------------
// <copyright file="PictureShape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using Genix.Geometry;
    using Genix.Imaging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains a picture.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PictureShape : Shape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureShape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        /// <param name="image">The image of the picture.</param>
        protected PictureShape(Rectangle bounds, Image image)
            : base(bounds)
        {
            this.Image = image;
        }

        /// <summary>
        /// Gets the image of the picture.
        /// </summary>
        /// <value>
        /// The <see cref="Image"/> object.
        /// </value>
        public Image Image { get; private set; }

        /// <inheritdoc />
        public override string Text => null;
    }
}