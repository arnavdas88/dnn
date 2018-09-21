// -----------------------------------------------------------------------
// <copyright file="BorderType.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Defines how values of border pixels are calculated.
    /// </summary>
    public enum BorderType
    {
        /// <summary>
        /// All border pixels are set to the constant value.
        /// </summary>
        BorderConst = 0,

        /// <summary>
        /// Border pixels are replicated from the edge pixels.
        /// </summary>
        BorderRepl,
    }
}
