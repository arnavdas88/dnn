// -----------------------------------------------------------------------
// <copyright file="TesseractHandle.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <summary>
    /// Represents a wrapper class for the Tesseract object.
    /// </summary>
    internal sealed class TesseractHandle : SafeHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TesseractHandle"/> class.
        /// </summary>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public TesseractHandle()
            : this(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TesseractHandle"/> class.
        /// </summary>
        /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public TesseractHandle(IntPtr preexistingHandle)
            : this(preexistingHandle, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TesseractHandle"/> class.
        /// </summary>
        /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
        /// <param name="ownsHandle"><b>true</b> to reliably release the handle during the finalization phase; <b>false</b> to prevent reliable release (not recommended).</param>
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public TesseractHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
            this.SetHandle(preexistingHandle);
        }

        /// <inheritdoc />
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <inheritdoc />
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            // If ReleaseHandle failed, it can be reported via the "releaseHandleFailed" managed debugging assistant (MDA).
            // This MDA is disabled by default, but can be enabled in a debugger or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover from the problem.
            NativeMethods.TessBaseAPIDelete(this.handle);
            return true;
        }
    }
}
