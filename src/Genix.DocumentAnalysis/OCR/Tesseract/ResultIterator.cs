// -----------------------------------------------------------------------
// <copyright file="ResultIterator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Security.Permissions;

    /// <summary>
    /// Class to iterate over tesseract results,
    /// providing access to all levels of the page hierarchy,
    /// without including any tesseract headers or having to handle any tesseract structures.
    /// </summary>
    internal class ResultIterator : PageIterator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultIterator"/> class.
        /// </summary>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public ResultIterator()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultIterator"/> class.
        /// </summary>
        /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public ResultIterator(IntPtr preexistingHandle)
            : base(preexistingHandle)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultIterator"/> class.
        /// </summary>
        /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
        /// <param name="ownsHandle"><b>true</b> to reliably release the handle during the finalization phase; <b>false</b> to prevent reliable release (not recommended).</param>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public ResultIterator(IntPtr preexistingHandle, bool ownsHandle)
            : base(preexistingHandle, ownsHandle)
        {
        }

        /// <inheritdoc />
        public override bool Next(PageIteratorLevel level)
        {
            return NativeMethods.TessResultIteratorNext(this, level);
        }

        /// <summary>
        /// Returns the text string for the current object at the given level.
        /// </summary>
        /// <param name="level">The level to return the text for.</param>
        /// <returns>
        /// The text string for the current object at the given level.
        /// </returns>
        public string GetUTF8Text(PageIteratorLevel level)
        {
            IntPtr ptr = NativeMethods.TessResultIteratorGetUTF8Text(this, level);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return NativeMethods.PtrToStringUTF8(ptr);
            }
            finally
            {
                NativeMethods.TessDeleteText(ptr);
            }
        }

        /// <summary>
        /// Returns the mean confidence of the current object at the given level.
        /// </summary>
        /// <param name="level">The level to return the confidence for.</param>
        /// <returns>
        /// The number that should be interpreted as a percent probability. (0.0f-100.0f).
        /// </returns>
        public float GetConfidence(PageIteratorLevel level)
        {
            return NativeMethods.TessResultIteratorConfidence(this, level);
        }
    }
}
