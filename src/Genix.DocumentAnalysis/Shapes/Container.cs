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
    using System.Runtime.CompilerServices;
    using Genix.Geometry;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a shape that contains other shapes. This is an abstract class.
    /// </summary>
    /// <typeparam name="T">The type of the shapes in the container.</typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Container<T>
        : Shape, IContainer, IContainer<T>
        where T : Shape
    {
        [JsonProperty("shapes")]
        private readonly List<T> shapes = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Container{T}"/> class.
        /// </summary>
        /// <param name="bounds">The shape boundaries.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Container(Rectangle bounds)
        {
            this.Bounds = bounds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Container{T}"/> class.
        /// </summary>
        /// <param name="bounds">The shape boundaries.</param>
        /// <param name="shapes">The shapes to add to this container.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Container(Rectangle bounds, IEnumerable<T> shapes)
            : this(bounds)
        {
            this.shapes.AddRange(shapes);
        }

        /// <summary>
        /// Gets the shapes this container contains.
        /// </summary>
        /// <value>
        /// The collection of shapes.
        /// </value>
        public IReadOnlyCollection<T> Shapes => this.shapes;

        /// <inheritdoc />
        public IEnumerable<T> EnumShapes() => this.shapes;

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

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<LineShape> EnumAllLines() => this.EnumAllShapes<LineShape>();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddShape(T shape)
        {
            this.shapes.Add(shape);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddShapes(IEnumerable<T> shapes)
        {
            this.shapes.AddRange(shapes);
        }

        /// <inheritdoc />
        public override void Offset(int dx, int dy)
        {
            base.Offset(dx, dy);

            foreach (Shape shape in this.EnumAllShapes())
            {
                shape.Offset(dx, dy);
            }
        }

        /// <inheritdoc />
        public override void Offset(Point point)
        {
            base.Offset(point);

            foreach (Shape shape in this.EnumAllShapes())
            {
                shape.Offset(point);
            }
        }

        /// <inheritdoc />
        public override void Scale(int dx, int dy)
        {
            base.Scale(dx, dy);

            foreach (Shape shape in this.EnumAllShapes())
            {
                shape.Scale(dx, dy);
            }
        }

        /// <inheritdoc />
        public override void Scale(float dx, float dy)
        {
            base.Scale(dx, dy);

            foreach (Shape shape in this.EnumAllShapes())
            {
                shape.Scale(dx, dy);
            }
        }
    }
}