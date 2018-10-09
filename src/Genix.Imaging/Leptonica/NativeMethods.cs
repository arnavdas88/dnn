// -----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Leptonica
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    internal static class NativeMethods
    {
        private const string DllName = "Genix.Leptonica.Native.dll";

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lept_free(IntPtr ptr);

        // PIX methods
        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixHandle pixCreate(int width, int height, int depth);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void pixDestroy(ref IntPtr ppix);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixSetResolution(SafePixHandle pix, int xres, int yres);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixGetResolution(SafePixHandle pix, out int xres, out int yres);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr pixGetData(SafePixHandle pix);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixGetWpl(SafePixHandle pix);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixGetDimensions(SafePixHandle pix, out int w, out int h, out int d);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixOtsuAdaptiveThreshold(SafePixHandle pix, int sx, int sy, int smoothx, int smoothy, float scorefract, out SafePixHandle pixth, out SafePixHandle pixd);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixHandle pixOtsuThreshOnBackgroundNorm(SafePixHandle pix, SafePixHandle pixim, int sx, int sy, int thresh, int mincount, int bgval, int smoothx, int smoothy, float scorefract, out int pthresh);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixHandle pixBackgroundNorm(SafePixHandle pixs, SafePixHandle pixim, SafePixHandle pixg, int sx, int sy, int thresh, int mincount, int bgval, int smoothx, int smoothy);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixHandle pixBackgroundNormMorph(SafePixHandle pixs, SafePixHandle pixim, int reduction, int size, int bgval);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixWriteMemBmp(out IntPtr fdata, out IntPtr fsize, SafePixHandle pix);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafeBoxaHandle pixConnComp(SafePixHandle pix, out SafePixaHandle pixa, int connectivity);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixHandle pixDistanceFunction(SafePixHandle pix, int connectivity, int outdepth, int boundcond);

        // PIXA methods
        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixaHandle pixaCreate(int n);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int pixaGetCount(SafePixaHandle boxa);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafePixHandle pixaGetPix(SafePixaHandle pixa, int index, int accesstype);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void pixaDestroy(ref IntPtr ppixa);

        // BOXA methods
        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int boxaGetCount(SafeBoxaHandle boxa);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SafeBoxHandle boxaGetBox(SafeBoxaHandle boxa, int index, int accessflag);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void boxaDestroy(ref IntPtr pboxa);

        // BOX methods
        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int boxGetGeometry(SafeBoxHandle box, out int px, out int py, out int pw, out int ph);

        [DllImport(NativeMethods.DllName, CallingConvention = CallingConvention.Cdecl)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void boxDestroy(ref IntPtr pbox);
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
}
