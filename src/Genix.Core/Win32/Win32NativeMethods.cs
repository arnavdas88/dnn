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

#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
