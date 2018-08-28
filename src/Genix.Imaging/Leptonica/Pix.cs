// -----------------------------------------------------------------------
// <copyright file="Pix.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Leptonica
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
        private readonly SafePixHandle handle;

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
                handle?.Dispose();
                throw;
            }

            return new Pix(handle);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> object from this Leptonica's <see cref="Pix"/> object.
        /// </summary>
        /// <returns>
        /// The <see cref="Image"/> object this method creates.
        /// </returns>
        public Image ToImage()
        {
            NativeMethods.pixGetDimensions(this.handle, out int w, out int h, out int d);
            NativeMethods.pixGetResolution(this.handle, out int xres, out int yres);

            Image dst = new Image(w, h, d, xres, yres);

            Pix.CopyBits(
                dst.Height,
                NativeMethods.pixGetData(this.handle),
                NativeMethods.pixGetWpl(this.handle) * sizeof(uint),
                dst.Bits,
                dst.Stride8,
                dst.BitsPerPixel);

            return dst;
        }

        /// <summary>
        /// Converts this <see cref="Pix"/> from gray scale to black-and-white using Otsu binarization algorithm.
        /// </summary>
        /// <param name="adaptiveThreshold">Indicates whether to use adaptive threshold algorithm.</param>
        /// <returns>
        /// A new binary <see cref="Pix"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pix BinarizeOtsu(bool adaptiveThreshold)
        {
            NativeMethods.pixGetDimensions(this.handle, out int w, out int h, out int d);

            /*if (w < 16 || h < 16)
            {
                adaptiveThreshold = false;
            }*/

            if (w < 32 || h < 32)
            {
                adaptiveThreshold = true;
            }

            int minSize = adaptiveThreshold ? 16 : 4;

            int sx = Math.Max(minSize, Math.Min(64, w / 5));        // tile size in pixels
            int sy = Math.Max(minSize, Math.Min(128, h / 5));
            const int Thresh = 100;                                 // threshold for determining foreground
            int mincount = sx * sy / 3;                             // min threshold on counts in a tile
            const int Smoothx = 2;                                  // half-width of block convolution kernel width
            const int Smoothy = 2;                                  // half-width of block convolution kernel height
            const float Scorefact = 0.0f;                           // fraction of the max Otsu score; typ. 0.1

            SafePixHandle pixd;
            if (adaptiveThreshold)
            {
                NativeMethods.pixOtsuAdaptiveThreshold(this.handle, sx, sy, Smoothx, Smoothy, Scorefact, out SafePixHandle pixth, out pixd);
                pixth?.Dispose();
            }
            else
            {
                pixd = NativeMethods.pixOtsuThreshOnBackgroundNorm(this.handle, null, sx, sy, Thresh, mincount, 255, Smoothx, Smoothy, Scorefact, out int thresh);
            }

            if (pixd == null || pixd.IsInvalid)
            {
                throw new InvalidOperationException();
            }

            return new Pix(pixd);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, IntPtr src, int strideSrc, ulong[] dst, int strideDst, int bitsPerPixel)
        {
            unsafe
            {
                fixed (ulong* udst = dst)
                {
                    if (bitsPerPixel == 1)
                    {
                    }
                    else
                    {
                        Arrays.CopyStrides(height, src, strideSrc, new IntPtr(udst), strideDst);
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
            public static extern int pixGetResolution(SafePixHandle pix, out int xres, out int yres);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr pixGetData(SafePixHandle pix);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int pixGetWpl(SafePixHandle pix);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int pixGetDimensions(SafePixHandle pix, out int w, out int h, out int d);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int pixOtsuAdaptiveThreshold(SafePixHandle pix, int sx, int sy, int smoothx, int smoothy, float scorefract, out SafePixHandle pixth, out SafePixHandle pixd);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern SafePixHandle pixOtsuThreshOnBackgroundNorm(SafePixHandle pix, SafePixHandle pixim, int sx, int sy, int thresh, int mincount, int bgval, int smoothx, int smoothy, float scorefract, out int pthresh);
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
