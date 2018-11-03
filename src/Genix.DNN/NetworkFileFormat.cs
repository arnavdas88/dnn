// -----------------------------------------------------------------------
// <copyright file="NetworkFileFormat.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    /// <summary>
    /// Defines the file format for the <see cref="Network"/>.
    /// </summary>
    public enum NetworkFileFormat
    {
        /// <summary>
        /// JSON file format (default).
        /// </summary>
        JSON = 0,

        /// <summary>
        /// BSON file format.
        /// </summary>
        BSON = 1,
    }
}
