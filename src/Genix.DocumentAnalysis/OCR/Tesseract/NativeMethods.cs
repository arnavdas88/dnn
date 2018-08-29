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
    using System.Text;
    using Genix.Win32;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        private const string DllName = "Genix.Tesseract.Native.dll";

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessDeleteText(IntPtr text);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TesseractHandle TessBaseAPICreate();

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void TessBaseAPIDelete(IntPtr handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int TessBaseAPIInit2(
            TesseractHandle handle,
            [MarshalAs(UnmanagedType.LPStr)] string datapath,
            [MarshalAs(UnmanagedType.LPStr)] string language,
            OcrEngineMode oem);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int TessBaseAPIInit3(
            TesseractHandle handle,
            [MarshalAs(UnmanagedType.LPStr)] string datapath,
            [MarshalAs(UnmanagedType.LPStr)] string language);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessBaseAPIClear(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessBaseAPIClearAdaptiveClassifier(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessBaseAPIClearPersistentCache(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessBaseAPISetImage2(TesseractHandle handle, SafeHandle pix);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessBaseAPISetPageSegMode(TesseractHandle handle, PageSegmentationMode mode);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern PageIterator TessBaseAPIAnalyseLayout(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TessBaseAPIRecognize(TesseractHandle handle, IntPtr monitor);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void TessPageIteratorDelete(IntPtr handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessPageIteratorBegin(PageIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessPageIteratorNext(PageIterator handle, PageIteratorLevel level);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessPageIteratorIsAtBeginningOf(PageIterator handle, PageIteratorLevel level);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessPageIteratorIsAtFinalElement(
            PageIterator handle,
            PageIteratorLevel level,
            PageIteratorLevel element);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TessPageIteratorOrientation(
            PageIterator handle,
            out Orientation orientation,
            out WritingDirection writingDirection,
            out TextLineOrder textLineOrder,
            out float deskewAngle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern PolyBlockType TessPageIteratorBlockType(PageIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TessPageIteratorBoundingBox(
            PageIterator handle,
            PageIteratorLevel level,
            out int left,
            out int top,
            out int right,
            out int bottom);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultIterator TessBaseAPIGetIterator(TesseractHandle handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessResultIteratorNext(ResultIterator handle, PageIteratorLevel level);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr TessResultIteratorGetUTF8Text(ResultIterator handle, PageIteratorLevel level);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float TessResultIteratorConfidence(ResultIterator handle, PageIteratorLevel level);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string TessResultIteratorWordRecognitionLanguage(ResultIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string TessResultIteratorWordFontAttributes(
            ResultIterator handle,
            [MarshalAs(UnmanagedType.Bool)] out bool bold,
            [MarshalAs(UnmanagedType.Bool)] out bool italic,
            [MarshalAs(UnmanagedType.Bool)] out bool underlined,
            [MarshalAs(UnmanagedType.Bool)] out bool monospace,
            [MarshalAs(UnmanagedType.Bool)] out bool serif,
            [MarshalAs(UnmanagedType.Bool)] out bool smallcaps,
            out int pointsize,
            out int fontId);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessResultIteratorWordIsFromDictionary(ResultIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessResultIteratorWordIsNumeric(ResultIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessResultIteratorSymbolIsSuperscript(ResultIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessResultIteratorSymbolIsSubscript(ResultIterator handle);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TessResultIteratorSymbolIsDropcap(ResultIterator handle);

        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            int nb = Win32NativeMethods.lstrlenA(ptr);
            if (nb == 0)
            {
                return null;
            }

            byte[] buffer = new byte[nb];
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
