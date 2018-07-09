// -----------------------------------------------------------------------
// <copyright file="Shape.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.DocumentAnalysis
{
    using System;
    using System.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape on a document. This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Shape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        /// <param name="bounds">The shape position.</param>
        protected Shape(Rectangle bounds)
        {
            this.Bounds = bounds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        protected Shape()
        {
        }

        /// <summary>
        /// Gets the shape boundaries.
        /// </summary>
        /// <value>
        /// The <see cref="Rectangle"/> that contains shape boundaries.
        /// </value>
        [JsonProperty("bounds")]
        public Rectangle Bounds { get; protected set; }
    }
}