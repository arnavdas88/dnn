// -----------------------------------------------------------------------
// <copyright file="Box.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Leptonica
{
    using System;
    using System.Runtime.InteropServices;
    using Genix.Geometry;

    /// <summary>
    /// Represents the Leptonica's Box object.
    /// </summary>
    public sealed partial class Box : DisposableObject
    {
        /// <summary>
        /// Gets the bounds of this <see cref="Box"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="Rectangle"/> that contains the <see cref="Box"/> bounds.
        /// </returns>
        public Rectangle GetBounds()
        {
            NativeMethods.boxGetGeometry(this.handle, out int x, out int y, out int w, out int h);
            return new Rectangle(x, y, w, h);
        }
    }
}
