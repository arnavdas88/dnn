// -----------------------------------------------------------------------
// <copyright file="Tesseract.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Tesseract OCR.
    /// </summary>
    public class Tesseract : TesseractObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tesseract"/> class.
        /// </summary>
        public Tesseract()
            : base(NativeMethods.TessBaseAPICreate())
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            HandleRef handle = (HandleRef)this;
            if (handle.Handle != IntPtr.Zero)
            {
                ////Clear();
                NativeMethods.TessBaseAPIClear(handle);

                ////ClearAdaptiveClassifier();
                NativeMethods.TessBaseAPIClearAdaptiveClassifier(handle);

                ////End();
                NativeMethods.TessBaseAPIEnd(handle);

                ////ClearPersistentCache();
                NativeMethods.TessBaseAPIClearPersistentCache(handle);

                NativeMethods.TessBaseAPIDelete(handle);
            }
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Tesseract.Native.dll";

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern IntPtr TessBaseAPICreate();

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void TessBaseAPIDelete(HandleRef handle);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void TessBaseAPIClear(HandleRef handle);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void TessBaseAPIClearAdaptiveClassifier(HandleRef handle);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void TessBaseAPIClearPersistentCache(HandleRef handle);

            [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void TessBaseAPIEnd(HandleRef handle);
        }
    }
}
