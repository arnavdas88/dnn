// -----------------------------------------------------------------------
// <copyright file="MatrixLayout.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    /// <summary>
    /// Defines the matrix layout (column major or row major).
    /// </summary>
    public enum MatrixLayout
    {
        /// <summary>
        /// The matrix layout is column major.
        /// </summary>
        ColumnMajor = 0,

        /// <summary>
        /// The matrix layout is row major.
        /// </summary>
        RowMajor = 1,
    }
}
