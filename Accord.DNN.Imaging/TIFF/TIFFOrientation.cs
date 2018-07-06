// -----------------------------------------------------------------------
// <copyright file="TIFFOrientation.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the values for the Orientation field.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This enumeration does not have zero value.")]
    public enum TIFFOrientation
    {
        /// <summary>
        /// The 0th row represents the visual top of the image, and the 0th column represents the visual left-hand side.
        /// </summary>
        TopLeft = 1,

        /// <summary>
        /// The 0th row represents the visual top of the image, and the 0th column represents the visual right-hand side.
        /// </summary>
        TopRight = 2,

        /// <summary>
        /// The 0th row represents the visual bottom of the image, and the 0th column represents the visual right-hand side.
        /// </summary>
        BottomRight = 3,

        /// <summary>
        /// The 0th row represents the visual bottom of the image, and the 0th column represents the visual left-hand side.
        /// </summary>
        BottomLeft = 4,

        /// <summary>
        /// The 0th row represents the visual left-hand side of the image, and the 0th column represents the visual top.
        /// </summary>
        LeftTop = 5,

        /// <summary>
        /// The 0th row represents the visual right-hand side of the image, and the 0th column represents the visual top.
        /// </summary>
        RightTop = 6,

        /// <summary>
        /// The 0th row represents the visual right-hand side of the image, and the 0th column represents the visual bottom.
        /// </summary>
        RightBottom = 7,

        /// <summary>
        /// The 0th row represents the visual left-hand side of the image, and the 0th column represents the visual bottom.
        /// </summary>
        LeftBottom = 8
    }
}
