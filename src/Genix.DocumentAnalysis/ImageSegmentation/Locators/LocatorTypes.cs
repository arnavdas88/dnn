// -----------------------------------------------------------------------
// <copyright file="LocatorTypes.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;

    /// <summary>
    /// Specifies the type(types) of a shape locator.
    /// </summary>
    [Flags]
    public enum LocatorTypes
    {
        /// <summary>
        /// None of shape locators.
        /// </summary>
        None = 0,

        /// <summary>
        /// The line locator.
        /// </summary>
        LineLocator = 1,

        /// <summary>
        /// The machine-printed text locator.
        /// </summary>
        TextLocator = 2,

        /// <summary>
        /// All available locators.
        /// </summary>
        All = LineLocator | TextLocator,
    }
}
