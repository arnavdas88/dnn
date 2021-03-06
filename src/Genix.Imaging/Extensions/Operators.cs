﻿// -----------------------------------------------------------------------
// <copyright file="Operators.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;

    /// <content>
    /// Provides operators for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Performs a bitwise logical NOR operation on the image and returns an inverted image.
        /// </summary>
        /// <param name="right">The <see cref="Image"/> that is to the right of the NOR operator.</param>
        /// <returns>
        /// The inverted <see cref="Image"/>.
        /// </returns>
        public static Image operator ~(Image right) => right.Not(null);

        /// <summary>
        /// Performs a bitwise logical AND operation on two images with equal depth and returns a combined image.
        /// </summary>
        /// <param name="left">The <see cref="Image"/> that is to the left of the AND operator.</param>
        /// <param name="right">The <see cref="Image"/> that is to the right of the AND operator.</param>
        /// <returns>
        /// The combined <see cref="Image"/>.
        /// </returns>
        public static Image operator &(Image left, Image right) => left.And(null, right);

        /// <summary>
        /// Performs a bitwise logical OR operation on two images with equal depth and returns a combined image.
        /// </summary>
        /// <param name="left">The <see cref="Image"/> that is to the left of the OR operator.</param>
        /// <param name="right">The <see cref="Image"/> that is to the right of the OR operator.</param>
        /// <returns>
        /// The combined <see cref="Image"/>.
        /// </returns>
        public static Image operator |(Image left, Image right) => left.Or(null, right);

        /// <summary>
        /// Performs a bitwise logical XOR operation on two images with equal depth and returns a combined image.
        /// </summary>
        /// <param name="left">The <see cref="Image"/> that is to the left of the XOR operator.</param>
        /// <param name="right">The <see cref="Image"/> that is to the right of the XOR operator.</param>
        /// <returns>
        /// The combined <see cref="Image"/>.
        /// </returns>
        public static Image operator ^(Image left, Image right) => left.Xor(null, right);

        /// <summary>
        /// Adds pixel values of two images with equal depth and returns a combined image.
        /// </summary>
        /// <param name="left">The <see cref="Image"/> that is to the left of the + operator.</param>
        /// <param name="right">The <see cref="Image"/> that is to the right of the + operator.</param>
        /// <returns>
        /// The combined <see cref="Image"/>.
        /// </returns>
        public static Image operator +(Image left, Image right) => left.Add(null, right, 0);

        /// <summary>
        /// Subtracts pixel values of two images with equal depth and returns a combined image.
        /// </summary>
        /// <param name="left">The <see cref="Image"/> that is to the left of the + operator.</param>
        /// <param name="right">The <see cref="Image"/> that is to the right of the + operator.</param>
        /// <returns>
        /// The combined <see cref="Image"/>.
        /// </returns>
        public static Image operator -(Image left, Image right) => left.Sub(null, right, 0);
    }
}