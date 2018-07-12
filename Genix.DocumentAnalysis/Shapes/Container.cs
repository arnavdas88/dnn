// -----------------------------------------------------------------------
// <copyright file="Container.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains other shapes. This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Container<T> : Shape where T : Shape
    {
        [JsonProperty("shapes")]
        private readonly List<T> shapes = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="shapes">The shapes contained in this container.</param>
        protected Container(IList<T> shapes)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            this.shapes.AddRange(shapes);
            this.Bounds = shapes.Select(x => x.Bounds).Union();
        }

        /// <summary>
        /// Gets the shapes this container contains.
        /// </summary>
        /// <value>
        /// The collection of shapes.
        /// </value>
        public ReadOnlyCollection<T> Shapes => new ReadOnlyCollection<T>(this.shapes);
    }
}