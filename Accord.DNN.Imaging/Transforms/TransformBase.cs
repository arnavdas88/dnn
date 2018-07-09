// -----------------------------------------------------------------------
// <copyright file="TransformBase.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System.Drawing;

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
        /// Converts coordinates represented by <see cref="Rectangle"/> to coordinates on the original image.
        /// </summary>
        /// <param name="value">The coordinates to convert.</param>
        /// <returns>The converted coordinates.</returns>
        public abstract Rectangle Convert(Rectangle value);
    }
}
