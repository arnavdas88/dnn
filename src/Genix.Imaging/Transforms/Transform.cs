// -----------------------------------------------------------------------
// <copyright file="Transform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a transformation of an image. This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Transform
    {
        /// <summary>
        /// Converts coordinates represented by <see cref="Point"/> to coordinates on the original image.
        /// </summary>
        /// <param name="value">The coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public abstract Point Convert(Point value);

        /// <summary>
        /// Converts coordinates represented by <see cref="Rect"/> to coordinates on the original image.
        /// </summary>
        /// <param name="value">The coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public abstract Rect Convert(Rect value);

        /// <summary>
        /// Appends the specified <see cref="Transform"/> to this <see cref="Transform"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> to append.</param>
        /// <returns>The <see cref="Transform"/> that contains combined transformation.</returns>
        public abstract Transform Append(Transform transform);
    }
}
