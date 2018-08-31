// -----------------------------------------------------------------------
// <copyright file="Container.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Genix.Drawing;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains other shapes. This is an abstract class.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the container.</typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Container<T>
        : Shape, IContainer
        where T : Shape
    {
        [JsonProperty("shapes")]
        private readonly List<T> shapes = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Container{T}"/> class.
        /// </summary>
        /// <param name="shapes">The shapes contained in this container.</param>
        protected Container(IList<T> shapes)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            this.shapes.AddRange(shapes);
            this.Bounds = Rectangle.Union(shapes.Select(x => x.Bounds));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Container{T}"/> class.
        /// </summary>
        /// <param name="shapes">The shapes contained in this container.</param>
        /// <param name="bounds">The shape boundaries.</param>
        protected Container(IList<T> shapes, Rectangle bounds)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            this.shapes.AddRange(shapes);
            this.Bounds = bounds;
        }

        /// <summary>
        /// Gets the shapes this container contains.
        /// </summary>
        /// <value>
        /// The collection of shapes.
        /// </value>
        public IReadOnlyCollection<T> Shapes => this.shapes;

        /// <summary>
        /// Enumerates all shapes contained in this container.
        /// </summary>
        /// <returns>A sequence of <typeparamref name="T"/> objects.</returns>
        public IEnumerable<T> EnumShapes()
        {
            return this.shapes;
        }

        /// <inheritdoc />
        public IEnumerable<Shape> EnumAllShapes()
        {
            foreach (Shape shape in this.shapes)
            {
                yield return shape;

                if (shape is IContainer container)
                {
                    foreach (Shape shapeInContaner in container.EnumAllShapes())
                    {
                        yield return shapeInContaner;
                    }
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<TShape> EnumAllShapes<TShape>()
            where TShape : Shape
        {
            foreach (Shape shape in this.shapes)
            {
                if (shape is TShape tshape)
                {
                    yield return tshape;
                }

                if (shape is IContainer container)
                {
                    foreach (TShape shapeInContaner in container.EnumAllShapes<TShape>())
                    {
                        yield return shapeInContaner;
                    }
                }
            }
        }
    }
}