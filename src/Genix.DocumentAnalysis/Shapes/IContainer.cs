// -----------------------------------------------------------------------
// <copyright file="IContainer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for a shape that contains other shapes.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Enumerates all shapes contained in this container and all its children.
        /// </summary>
        /// <returns>A sequence of <see cref="Shape"/> objects.</returns>
        IEnumerable<Shape> EnumAllShapes();

        /// <summary>
        /// Enumerates all shapes of the specific type contained in this container and all its children.
        /// </summary>
        /// <typeparam name="TShape">The type of the <see cref="Shape"/> object to return.</typeparam>
        /// <returns>A sequence of <see cref="Shape"/> objects.</returns>
        IEnumerable<TShape> EnumAllShapes<TShape>()
            where TShape : Shape;
    }
}