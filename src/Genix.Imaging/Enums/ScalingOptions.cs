// -----------------------------------------------------------------------
// <copyright file="ScalingOptions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;

    /// <summary>
    /// Represents the options for scaling methods.
    /// </summary>
    public class ScalingOptions
    {
        /// <summary>
        /// Gets or sets the interpolation type.
        /// </summary>
        /// <value>
        /// The <see cref="Imaging.InterpolationType"/> enumeration value.
        /// </value>
        public InterpolationType InterpolationType { get; set; } = InterpolationType.NearestNeighbor;

        /// <summary>
        /// Gets or sets a value indicating whether to use anti aliasing.
        /// </summary>
        /// <value>
        /// <b>true</b> to use anti aliasing; otherwise, <b>false</b>.
        /// </value>
        /// <remarks>
        /// <para>Use anti aliasing to reduce the image size with minimization of moire artifacts.</para>
        /// <para>
        /// For more information about the implemented algorithm, refer to
        /// <c>Dale A. Schumacher. General Filtered Image Rescaling, Graphic Gems III, Academic Press, 1992.</c>.
        /// </para>
        /// </remarks>
        public bool Antialiasing { get; set; } = false;

        /// <summary>
        /// Gets or sets the first parameter of cubic filter.
        /// </summary>
        /// <value>
        /// The first parameter of cubic filter.
        /// </value>
        /// <remarks>
        /// <para>This parameter is used only when <see cref="InterpolationType"/> is <see cref="Imaging.InterpolationType.Cubic"/>.</para>
        /// </remarks>
        public float ValueB { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the second parameter of cubic filter.
        /// </summary>
        /// <value>
        /// The second parameter of cubic filter.
        /// </value>
        /// <remarks>
        /// <para>This parameter is used only when <see cref="InterpolationType"/> is <see cref="Imaging.InterpolationType.Cubic"/>.</para>
        /// </remarks>
        public float ValueC { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the number of lobes in Lanczos filter.
        /// </summary>
        /// <value>
        /// The number of lobes in Lanczos filter. Possible values are 2 or 3.
        /// </value>
        /// <remarks>
        /// <para>This parameter is used only when <see cref="InterpolationType"/> is <see cref="Imaging.InterpolationType.Lanczos"/>.</para>
        /// </remarks>
        public int Lobes { get; set; } = 2;
    }
}
