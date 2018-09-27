// -----------------------------------------------------------------------
// <copyright file="Globals.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Provides framework-wide methods and utilities.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Looks for a specified data directory in a framework directory tree.
        /// </summary>
        /// <param name="directoryName">The name of the directory to find.</param>
        /// <returns>If the directory exists, a full path to the specified directory; otherwise, throws <see cref="DirectoryNotFoundException"/>.</returns>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified directory does not exist.
        /// </exception>
        public static string LookupDataDirectory(string directoryName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string baseDirectory = Path.GetDirectoryName(assembly.Location);
            string pathName = Path.Combine(baseDirectory, "Data", directoryName);

            // for Debug builds lookup the data directory in parent folders
            while (!Directory.Exists(pathName))
            {
                baseDirectory = Path.GetDirectoryName(baseDirectory);
                if (baseDirectory == null)
                {
                    throw new DirectoryNotFoundException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Properties.Resources.E_DirectoryNotFound,
                            Path.Combine("Data", directoryName)));
                }

                pathName = Path.Combine(baseDirectory, "Data", directoryName);
            }

            return pathName;
        }
    }
}
