// -----------------------------------------------------------------------
// <copyright file="RotateFlip.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Specifies the direction of an image's rotation and the axis used to flip the image.
    /// </summary>
    public enum RotateFlip
    {
        /// <summary>
        /// No rotation and no flipping.
        /// </summary>
        RotateNoneFlipNone = 0,

        /// <summary>
        /// 90-degree counter clockwise rotation without flipping.
        /// </summary>
        Rotate90FlipNone,

        /// <summary>
        /// 180-degree counter clockwise rotation without flipping.
        /// </summary>
        Rotate180FlipNone,

        /// <summary>
        /// 270-degree counter clockwise rotation without flipping.
        /// </summary>
        Rotate270FlipNone,

        /// <summary>
        /// No rotation followed by a horizontal flip.
        /// </summary>
        RotateNoneFlipX,

        /// <summary>
        /// 90-degree counter clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate90FlipX,

        /// <summary>
        /// 180-degree counter clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate180FlipX,

        /// <summary>
        /// 270-degree counter clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate270FlipX,

        /// <summary>
        /// No rotation followed by a vertical flip.
        /// </summary>
        RotateNoneFlipY,

        /// <summary>
        /// 90-degree counter clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate90FlipY,

        /// <summary>
        /// 180-degree counter clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate180FlipY,

        /// <summary>
        /// 270-degree counter clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate270FlipY,
    }
}
