// -----------------------------------------------------------------------
// <copyright file="Axis.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the axis along one of the tensor dimensions.
    /// </summary>
    public enum Axis
    {
        /// <summary>
        /// The mini-batch axis.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B", Justification = "Represents axis name.")]
        B = 0,

        /// <summary>
        /// The horizontal axis.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "Represents axis name.")]
        X = 1,

        /// <summary>
        /// The vertical axis.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Represents axis name.")]
        Y = 2,

        /// <summary>
        /// The channel axis.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "C", Justification = "Represents axis name.")]
        C = 3,
    }
}
