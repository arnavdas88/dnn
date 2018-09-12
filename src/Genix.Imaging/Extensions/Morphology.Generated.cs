﻿
// -----------------------------------------------------------------------
// <copyright file="Morphology.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 9/11/2018 2:39:39 PM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Imaging
{
    public partial class Image
    {

        /// <summary>
        /// Dilates the <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <param name="kernel">The structuring element used in this operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <returns>
        /// The new <see cref="Image"/>.
        /// </returns>
        /// <seealso cref="Dilate(StructuringElement, int)"/>
        public static Image Dilate(Image image, StructuringElement kernel, int iterations)
        {
            Image dst = image.Copy();
            dst.Dilate(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Erodes the <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <param name="kernel">The structuring element used in this operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <returns>
        /// The new <see cref="Image"/>.
        /// </returns>
        /// <seealso cref="Erode(StructuringElement, int)"/>
        public static Image Erode(Image image, StructuringElement kernel, int iterations)
        {
            Image dst = image.Copy();
            dst.Erode(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Perform morphological opening operation on the <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <param name="kernel">The structuring element used in this operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <returns>
        /// The new <see cref="Image"/>.
        /// </returns>
        /// <seealso cref="Open(StructuringElement, int)"/>
        public static Image Open(Image image, StructuringElement kernel, int iterations)
        {
            Image dst = image.Copy();
            dst.Open(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Perform morphological closing operation on the <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <param name="kernel">The structuring element used in this operation.</param>
        /// <param name="iterations">The number of times the operation is applied.</param>
        /// <returns>
        /// The new <see cref="Image"/>.
        /// </returns>
        /// <seealso cref="Close(StructuringElement, int)"/>
        public static Image Close(Image image, StructuringElement kernel, int iterations)
        {
            Image dst = image.Copy();
            dst.Close(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Removes small isolated pixels from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <returns>
        /// The new <see cref="Image"/>.
        /// </returns>
        /// <seealso cref="Despeckle()"/>
        public static Image Despeckle(Image image)
        {
            Image dst = image.Copy();
            dst.Despeckle();
            return dst;
        }
	}
}