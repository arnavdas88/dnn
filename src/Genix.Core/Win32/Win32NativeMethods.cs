// -----------------------------------------------------------------------
// <copyright file="Win32NativeMethods.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Win32
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;

    /// <summary>
    /// .NET interface for MSCORLIB.
    /// </summary>
    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static class Win32NativeMethods
    {
        private const string KERNEL32 = "kernel32.dll";

#pragma warning disable SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Determines the length of the specified string (not including the terminating null character).
        /// </summary>
        /// <param name="ptr">The null-terminated string to be checked.</param>
        /// <returns>
        /// The function returns the length of the string, in characters. If <paramref name="ptr"/> is <see cref="IntPtr.Zero"/>, the function returns 0.
        /// </returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible", Justification = "Provides access to public Win32 API.")]
        [DllImport(Win32NativeMethods.KERNEL32, CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "lstrlenA")]
        [ResourceExposure(ResourceScope.None)]
        public static extern int lstrlenA(IntPtr ptr);

        /// <summary>
        /// Copies bytes between buffers.
        /// </summary>
        /// <param name="dst">New buffer.</param>
        /// <param name="src">Buffer to copy from.</param>
        /// <param name="size">Number of characters to copy.</param>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible", Justification = "Provides access to public Win32 API.")]
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern void memcpy(IntPtr dst, IntPtr src, IntPtr size);

        /// <summary>
        /// Sets buffers to a specified character.
        /// </summary>
        /// <param name="dst">Pointer to destination.</param>
        /// <param name="value">Character to set.</param>
        /// <param name="size">Number of characters.</param>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible", Justification = "Provides access to public Win32 API.")]
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern void memset(IntPtr dst, int value, IntPtr size);

#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Multiplies two 32-bit integers and then divides the 64-bit result by a third 32-bit value.
        /// </summary>
        /// <param name="number">The multiplicand.</param>
        /// <param name="numerator">The multiplier.</param>
        /// <param name="denominator">The number by which the result of the multiplication operation is to be divided.</param>
        /// <returns>
        /// <para>
        /// If the function succeeds, the return value is the result of the multiplication and division, rounded to the nearest integer.
        /// If the result is a positive half integer (ends in .5), it is rounded up.
        /// If the result is a negative half integer, it is rounded down.
        /// </para>
        /// <para>
        /// If either an overflow occurred or <c>denominator</c> was 0, the return value is -1.
        /// </para>
        /// </returns>
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible", Justification = "Provides access to public Win32 API.")]
        [DllImport(Win32NativeMethods.KERNEL32)]
        public static extern int MulDiv(int number, int numerator, int denominator);
    }
}
