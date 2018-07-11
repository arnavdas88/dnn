// -----------------------------------------------------------------------
// <copyright file="TransformBase.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System.Windows;

    /// <summary>
    /// Represents a transformation of an image. This is an abstract class.
    /// </summary>
    public abstract class TransformBase
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
        /// Appends the specified <see cref="TransformBase"/> to this <see cref="TransformBase"/>.
        /// </summary>
        /// <param name="transform">The <see cref="TransformBase"/> to append.</param>
        /// <returns>The <see cref="TransformBase"/> that contains combined transformation.</returns>
        public abstract TransformBase Append(TransformBase transform);
    }
}
