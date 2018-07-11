// -----------------------------------------------------------------------
// <copyright file="TIFFFillOrder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the values for the FillOrder field.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This enumeration does not have zero value.")]
    public enum TIFFFillOrder
    {
        /// <summary>
        /// Pixels are arranged within a byte such that pixels with lower column values are stored in the higher-order bits of the byte.
        /// </summary>
        MSB2LSB = 1,

        /// <summary>
        /// Pixels are arranged within a byte such that pixels with lower column values are stored in the lower-order bits of the byte.
        /// </summary>
        LSB2MSB = 2,
    }
}
