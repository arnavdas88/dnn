// -----------------------------------------------------------------------
// <copyright file="Shape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape on a document. This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Shape : IAlignedBoundedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        /// <param name="bounds">The shape boundaries.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Shape(Rectangle bounds)
        {
            this.Bounds = bounds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Shape()
        {
        }

        /// <inheritdoc />
        [JsonProperty("bounds")]
        public Rectangle Bounds { get; protected set; }

        /// <inheritdoc />
        [JsonProperty("halign")]
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.None;

        /// <inheritdoc />
        [JsonProperty("valign")]
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.None;

        /// <summary>
        /// Gets the shape text.
        /// </summary>
        /// <value>
        /// The <see cref="string"/> that contains the shape text.
        /// </value>
        public abstract string Text { get; }

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "{0}: {1}",
            this.GetType().Name,
            this.Bounds);
    }
}