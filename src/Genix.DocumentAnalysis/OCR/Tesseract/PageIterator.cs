// -----------------------------------------------------------------------
// <copyright file="PageIterator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Drawing;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <summary>
    /// Class to iterate over tesseract page structure,
    /// providing access to all levels of the page hierarchy,
    /// without including any tesseract headers or having to handle any tesseract structures.
    /// </summary>
    internal class PageIterator : SafeHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageIterator"/> class.
        /// </summary>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public PageIterator()
            : this(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageIterator"/> class.
        /// </summary>
        /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public PageIterator(IntPtr preexistingHandle)
            : this(preexistingHandle, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageIterator"/> class.
        /// </summary>
        /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
        /// <param name="ownsHandle"><b>true</b> to reliably release the handle during the finalization phase; <b>false</b> to prevent reliable release (not recommended).</param>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public PageIterator(IntPtr preexistingHandle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
            this.SetHandle(preexistingHandle);
        }

        /// <inheritdoc />
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Resets the iterator to point to the start of the page.
        /// </summary>
        public void Begin()
        {
            NativeMethods.TessPageIteratorBegin(this);
        }

        /// <summary>
        /// Moves to the start of the next object at the given level in the page hierarchy.
        /// </summary>
        /// <param name="level">The level of iteration.</param>
        /// <returns>
        /// <b>false</b> if the end of the page was reached; otherwise, <b>true</b>.
        /// </returns>
        public virtual bool Next(PageIteratorLevel level)
        {
            return NativeMethods.TessPageIteratorNext(this, level);
        }

        /// <summary>
        /// Determines whether the iterator is at the start of an object at the given level.
        /// Possible uses include determining if a call to Next(RIL_WORD) moved to the start of a RIL_PARA.
        /// </summary>
        /// <param name="level">The level to test.</param>
        /// <returns>
        /// <b>true</b> if the iterator is at the start of an object at the given level; otherwise, <b>false</b>.
        /// </returns>
        public bool IsAtBeginningOf(PageIteratorLevel level)
        {
            return NativeMethods.TessPageIteratorIsAtBeginningOf(this, level);
        }

        /// <summary>
        /// Determines whether the iterator is positioned at the last element in a given level.
        /// (e.g.the last word in a line, the last line in a block).
        /// </summary>
        /// <param name="level">The level to test.</param>
        /// <param name="element">The element to test.</param>
        /// <returns>
        /// <b>true</b> if the iterator is positioned at the last element in the given level; otherwise, <b>false</b>.
        /// </returns>
        public bool IsAtFinalElement(PageIteratorLevel level, PageIteratorLevel element)
        {
            return NativeMethods.TessPageIteratorIsAtFinalElement(this, level, element);
        }

        /// <summary>
        /// Returns orientation for the block the iterator points to.
        /// </summary>
        /// <param name="orientation">The block orientation.</param>
        /// <param name="writingDirection">The block writing direction.</param>
        /// <param name="textLineOrder">The block text order.</param>
        /// <param name="deskewAngle">The block deskew angle, in radians, counter-clockwise.</param>
        public void GetOrientation(out Orientation orientation, out WritingDirection writingDirection, out TextLineOrder textLineOrder, out float deskewAngle)
        {
            NativeMethods.TessPageIteratorOrientation(this, out orientation, out writingDirection, out textLineOrder, out deskewAngle);
        }

        /// <summary>
        /// Returns the type of the current block.
        /// </summary>
        /// <returns>
        /// The <see cref="PolyBlockType"/> enumeration value.
        /// </returns>
        public PolyBlockType GetBlockType()
        {
            return NativeMethods.TessPageIteratorBlockType(this);
        }

        /// <summary>
        /// Returns the bounding rectangle of the current object at the given level in coordinates of the original image.
        /// </summary>
        /// <param name="level">The iteration level.</param>
        /// <returns>
        /// The bounding rectangle of the current object at the given level in coordinates of the original image;
        /// <see cref="Rectangle.Empty"/> if there is no such object at the current position.
        /// </returns>
        public Rectangle GetBoundingBox(PageIteratorLevel level)
        {
            if (NativeMethods.TessPageIteratorBoundingBox(this, level, out int left, out int top, out int right, out int bottom) == 1)
            {
                return Rectangle.FromLTRB(left, top, right, bottom);
            }
            else
            {
                return Rectangle.Empty;
            }
        }

        /// <inheritdoc />
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            // If ReleaseHandle failed, it can be reported via the "releaseHandleFailed" managed debugging assistant (MDA).
            // This MDA is disabled by default, but can be enabled in a debugger or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover from the problem.
            NativeMethods.TessPageIteratorDelete(this.handle);
            return true;
        }

#if false

        /*internal PageIteratorHandle(ResultIterator resultIterator)
            : base(NativeMethods.TessResultIteratorGetPageIterator((HandleRef)resultIterator))
        { }*/

        public void ParagraphInfo(out ParagraphJustification justification, out int isListItem, out int isCrown, out int firstLineIndent)
        {
            NativeMethods.TessPageIteratorParagraphInfo(this.handle, out justification, out isListItem, out isCrown, out firstLineIndent);
        }

        public bool Baseline(PageIteratorLevel level, out int x1, out int y1, out int x2, out int y2)
        {
            return NativeMethods.TessPageIteratorBaseline(this.handle, level, out x1, out y1, out x2, out y2) == 1 ? true : false;
        }

        public Pix GetImage(PageIteratorLevel level, int padding, Pix originalImage, out int left, out int top)
        {
            var pointer = NativeMethods.TessPageIteratorGetImage(this.handle, level, padding, (HandleRef)originalImage, out left, out top);

            if (IntPtr.Zero != pointer)
            {
                return new Pix(pointer);
            }

            return null;
        }

        public Pix GetBinaryImage(PageIteratorLevel level)
        {
            var pointer = NativeMethods.TessPageIteratorGetBinaryImage(this.handle, level);
            if (IntPtr.Zero != pointer)
            {
                return new Pix(pointer);
            }

            return null;
        }
#endif
    }
}
