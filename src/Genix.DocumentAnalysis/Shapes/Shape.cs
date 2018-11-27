// -----------------------------------------------------------------------
// <copyright file="Shape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Genix.Geometry;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape on a document. This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Shape : IAlignedBoundedObject
    {
        [JsonProperty("bounds")]
        private Rectangle bounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        /// <param name="bounds">The shape boundaries.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Shape(Rectangle bounds)
        {
            this.bounds = bounds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Shape()
        {
        }

        /// <inheritdoc />
        public Rectangle Bounds
        {
            get => this.bounds;
            private protected set => this.bounds = value;
        }

        /// <inheritdoc />
        [JsonProperty("halign", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.None;

        /// <inheritdoc />
        [JsonProperty("valign", DefaultValueHandling = DefaultValueHandling.Ignore)]
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
            this.bounds);

        /// <summary>
        /// Translates the position of this <see cref="Shape"/> by the specified amount.
        /// </summary>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Offset(int dx, int dy) => this.bounds.Offset(dx, dy);

        /// <summary>
        /// Translates the position of this <see cref="Shape"/> by the specified <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> that contains the offset.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Offset(Point point) => this.bounds.Offset(point);

        /// <summary>
        /// Scales the location and the dimensions of this <see cref="Shape"/>.
        /// </summary>
        /// <param name="dx">The amount by which to scale the left position and the width of the shape.</param>
        /// <param name="dy">The amount by which to scale the top position and the height of the shape.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Scale(int dx, int dy) => this.bounds.Scale(dx, dy);

        /// <summary>
        /// Scales the location and the dimensions of this <see cref="Shape"/>.
        /// </summary>
        /// <param name="dx">The amount by which to scale the left position and the width of the shape.</param>
        /// <param name="dy">The amount by which to scale the top position and the height of the shape.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Scale(float dx, float dy) => this.bounds.Scale(dx, dy);
    }
}