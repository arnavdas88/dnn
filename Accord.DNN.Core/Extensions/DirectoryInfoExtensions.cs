// -----------------------------------------------------------------------
// <copyright file="DirectoryInfoExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides a set of extension methods for <see cref="DirectoryInfo"/> class.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Returns an enumerable collection of file information that has one of the specified extensions.
        /// </summary>
        /// <param name="directoryInfo">The <see cref="DirectoryInfo"/> class to extend.</param>
        /// <param name="extensions">The array of extensions to match.</param>
        /// <returns>An enumerable collection of files that have one of the specified extensions.</returns>
        public static IEnumerable<FileInfo> EnumerateFilesByExtensions(this DirectoryInfo directoryInfo, params string[] extensions)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException("directoryInfo");
            }

            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

            return directoryInfo.EnumerateFiles()
                                .Where(f => allowedExtensions.Any(ext => string.Compare(ext, f.Extension, StringComparison.OrdinalIgnoreCase) == 0));
        }
    }
}
