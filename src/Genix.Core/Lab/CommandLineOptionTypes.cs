// -----------------------------------------------------------------------
// <copyright file="CommandLineOptionTypes.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;

    /// <summary>
    /// Specifies the types for the <see cref="CommandLineOption"/>.
    /// </summary>
    [Flags]
    public enum CommandLineOptionTypes
    {
        /// <summary>
        /// None of the options.
        /// </summary>
        None = 0,

        /// <summary>
        /// A value that indicates whether the parameter is required.
        /// </summary>
        Required = 1,

        /// <summary>
        /// A value that indicates whether the path specified by parameter must exist.
        /// </summary>
        PathMustExist = 2,

        /// <summary>
        /// A value that indicates whether the file specified by parameter must exist.
        /// </summary>
        FileMustExist = 4,

        /// <summary>
        /// A value that indicates whether the parameter can be parsed as an integer value.
        /// </summary>
        Integer = 8,

        /// <summary>
        /// A value that indicates whether the parameter can be parsed as an <see cref="DateTime"/> value.
        /// </summary>
        DateTime = 16,
    }
}
