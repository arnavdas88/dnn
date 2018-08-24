// -----------------------------------------------------------------------
// <copyright file="TesseractObject.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a base implementation of a Tesseract object. This is an abstract class.
    /// </summary>
    public abstract class TesseractObject : DisposableObject
    {
        /// <summary>
        /// The handle reference for the object.
        /// </summary>
        private readonly HandleRef handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="TesseractObject"/> class.
        /// </summary>
        /// <param name="handle">The handle reference for the object.</param>
        public TesseractObject(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            this.handle = new HandleRef(this, handle);
        }

        /// <summary>
        /// Converts this <see cref="TesseractObject"/> to <see cref="HandleRef"/>.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        public static explicit operator HandleRef(TesseractObject obj)
        {
            return obj?.handle ?? new HandleRef(null, IntPtr.Zero);
        }
    }
}
