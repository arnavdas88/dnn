// -----------------------------------------------------------------------
// <copyright file="Boxa.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Leptonica
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents the Leptonica's Boxa object.
    /// </summary>
    public sealed partial class Boxa : DisposableObject
    {
        /// <summary>
        /// Gets the number of all boxes.
        /// </summary>
        /// <value>
        /// The number of all boxes.
        /// </value>
        public int Count => NativeMethods.boxaGetCount(this.handle);

        /// <summary>
        /// Gets the <see cref="Box"/> object at the specified index.
        /// </summary>
        /// <param name="index">The zero-based box index.</param>
        /// <returns>The <see cref="Box"/> object at the specified index.</returns>
        public Box this[int index] => new Box(NativeMethods.boxaGetBox(this.handle, index, 2 /*L_CLONE*/));
    }
}
