// -----------------------------------------------------------------------
// <copyright file="LocatorBase.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;
    using System.Threading;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Defines the contract for a shape locator. This is an abstract class.
    /// </summary>
    public abstract class LocatorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocatorBase"/> class.
        /// </summary>
        protected LocatorBase()
        {
        }

        /// <summary>
        /// Locates shapes on the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="page">The <see cref="PageShape"/> that contains shapes previously found on the image and receives new shapes found by this locator..</param>
        /// <param name="image">The <see cref="Image"/> to locate shapes on. This image can be changed by locator.</param>
        /// <param name="originalImage">The <see cref="Image"/> to locate shapes on. This image is the copy of original image and remains unchanged.</param>
        /// <param name="areas">The areas on the <paramref name="image"/> to locate shapes in.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the locator that operation should be canceled.</param>
        /// <remarks>
        /// Use this method to locate shapes of different types (i.e. lines, pictures, tables).
        /// </remarks>
        public abstract void Locate(PageShape page, Image image, Image originalImage, IList<Rectangle> areas, CancellationToken cancellationToken);
    }
}
