// -----------------------------------------------------------------------
// <copyright file="Pix.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using Genix.Core;

    /// <summary>
    /// Represents the Leptonica image.
    /// </summary>
    public sealed class Pix : DisposableObject
    {
        /// <summary>
        /// The handle reference for the object.
        /// </summary>
        private SafePixHandle handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pix"/> class.
        /// </summary>
        /// <param name="handle">The pointer to Leptonica's <see cref="Pix"/> object.</param>
        private Pix(SafePixHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// Gets the pointer to Leptonica's <see cref="Pix"/> object.
        /// </summary>
        /// <value>
        /// The pointer to Leptonica's <see cref="Pix"/> object.
        /// </value>
        public SafeHandle Handle => this.handle;

        /// <summary>
        /// Creates a new Leptonica's <see cref="Pix"/> object from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>
        /// The <see cref="Pix"/> object this method creates.
        /// </returns>
        public static Pix FromImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            SafePixHandle handle = NativeMethods.pixCreate(image.Width, image.Height, image.BitsPerPixel);
            try
            {
                NativeMethods.pixSetResolution(handle, image.HorizontalResolution, image.VerticalResolution);

                Pix.CopyBits(
                    image.Height,
                    image.Bits,
                    image.Stride8,
                    NativeMethods.pixGetData(handle),
                    NativeMethods.pixGetWpl(handle) * sizeof(uint),
                    image.BitsPerPixel);
            }
            catch
            {
                handle.Dispose();
                throw;
            }

            return new Pix(handle);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            this.handle?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, ulong[] src, int strideSrc, IntPtr dst, int strideDst, int bitsPerPixel)
        {
            unsafe
            {
                fixed (ulong* usrc = src)
                {
                    if (bitsPerPixel == 1)
                    {
                    }
                    else
                    {
                        Arrays.CopyStrides(height, new IntPtr(usrc), strideSrc, dst, strideDst);
                    }
                }
            }
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Leptonica.Native.dll";

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern SafePixHandle pixCreate(int width, int height, int depth);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void pixDestroy(ref IntPtr ppix);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int pixSetResolution(SafePixHandle pix, int xres, int yres);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr pixGetData(SafePixHandle pix);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int pixGetWpl(SafePixHandle pix);
        }

        /// <summary>
        /// Represents a wrapper class for the Leptonica's <see cref="Pix"/> object.
        /// </summary>
        private sealed class SafePixHandle : SafeHandle
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SafePixHandle"/> class.
            /// </summary>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafePixHandle()
                : this(IntPtr.Zero, true)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SafePixHandle"/> class.
            /// </summary>
            /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafePixHandle(IntPtr preexistingHandle)
                : this(preexistingHandle, true)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SafePixHandle"/> class.
            /// </summary>
            /// <param name="preexistingHandle">An object that represents the pre-existing handle to use.</param>
            /// <param name="ownsHandle"><b>true</b> to reliably release the handle during the finalization phase; <b>false</b> to prevent reliable release (not recommended).</param>
            [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            public SafePixHandle(IntPtr preexistingHandle, bool ownsHandle)
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
                NativeMethods.pixDestroy(ref this.handle);
                return true;
            }
        }
    }
}
