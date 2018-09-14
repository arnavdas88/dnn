// -----------------------------------------------------------------------
// <copyright file="Pixa.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Leptonica
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents the Leptonica's Pixa object.
    /// </summary>
    public sealed partial class Pixa : DisposableObject
    {
        /// <summary>
        /// Gets the number of all images.
        /// </summary>
        /// <value>
        /// The number of all images.
        /// </value>
        public int Count => NativeMethods.pixaGetCount(this.handle);

        /// <summary>
        /// Gets the <see cref="Pix"/> object at the specified index.
        /// </summary>
        /// <param name="index">The zero-based box index.</param>
        /// <returns>The <see cref="Pix"/> object at the specified index.</returns>
        public Pix this[int index] => new Pix(NativeMethods.pixaGetPix(this.handle, index, 2 /*L_CLONE*/));

        /// <summary>
        /// Creates a new Leptonica's Pixa object.
        /// </summary>
        /// <param name="capacity">The initial capacity of Pixa object.</param>
        /// <returns>
        /// The <see cref="Pixa"/> object this method creates.
        /// </returns>
        public static Pixa Create(int capacity)
        {
            return new Pixa(NativeMethods.pixaCreate(capacity));
        }
    }
}
