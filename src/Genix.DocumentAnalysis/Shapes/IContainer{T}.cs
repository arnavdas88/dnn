// -----------------------------------------------------------------------
// <copyright file="IContainer{T}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for a shape that contains other shapes of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the shapes in the container.</typeparam>
    public interface IContainer<T>
        where T : Shape
    {
        /// <summary>
        /// Enumerates all shapes contained in this container.
        /// </summary>
        /// <returns>A sequence of <typeparamref name="T"/> objects.</returns>
        IEnumerable<T> EnumShapes();

        /// <summary>
        /// Adds a shape to this container.
        /// </summary>
        /// <param name="shape">The shape to add.</param>
        void AddShape(T shape);

        /// <summary>
        /// Adds shapes from the specified collection to this container.
        /// </summary>
        /// <param name="shapes">The shapes to add.</param>
        void AddShapes(IEnumerable<T> shapes);
    }
}