// -----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class NativeMethods
    {
        private const string DllName = "Genix.Tesseract.Native.dll";

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern TesseractHandle TessBaseAPICreate();

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessBaseAPIDelete(IntPtr handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int TessBaseAPIInit2(
            TesseractHandle handle,
            [MarshalAs(UnmanagedType.LPStr)] string datapath,
            [MarshalAs(UnmanagedType.LPStr)] string language,
            OcrEngineMode oem);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int TessBaseAPIInit3(
            TesseractHandle handle,
            [MarshalAs(UnmanagedType.LPStr)] string datapath,
            [MarshalAs(UnmanagedType.LPStr)] string language);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessBaseAPIClear(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessBaseAPIClearAdaptiveClassifier(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessBaseAPIClearPersistentCache(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessBaseAPISetImage2(TesseractHandle handle, SafeHandle pix);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessBaseAPISetPageSegMode(TesseractHandle handle, PageSegmentationMode mode);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern PageIteratorHandle TessBaseAPIAnalyseLayout(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessPageIteratorDelete(IntPtr handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessPageIteratorBegin(PageIteratorHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessPageIteratorNext(PageIteratorHandle handle, PageIteratorLevel level);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void TessPageIteratorOrientation(
            PageIteratorHandle handle,
            out Orientation orientation,
            out WritingDirection writingDirection,
            out TextLineOrder textLineOrder,
            out float deskewAngle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern PolyBlockType TessPageIteratorBlockType(PageIteratorHandle handle);
    }
}
