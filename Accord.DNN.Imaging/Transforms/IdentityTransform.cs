// -----------------------------------------------------------------------
// <copyright file="IdentityTransform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System.Windows;

    /// <summary>
    /// Represents a horizontal and vertical shift of an <see cref="Image"/>.
    /// </summary>
    public class IdentityTransform : TransformBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityTransform"/> class.
        /// </summary>
        public IdentityTransform()
        {
        }

        /// <inheritdoc />
        public override Point Convert(Point value)
        {
            return value;
        }

        /// <inheritdoc />
        public override Rect Convert(Rect value)
        {
            return value;
        }

        /// <inheritdoc />
        public override TransformBase Append(TransformBase transform)
        {
            return transform;
        }
    }
}
