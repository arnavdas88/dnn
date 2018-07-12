// -----------------------------------------------------------------------
// <copyright file="EXIFExposureProgram.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Specifies the values for the <see cref="EXIFField.ExposureProgram"/> field.
    /// </summary>
    public enum EXIFExposureProgram
    {
        /// <summary>
        /// The undefined exposure program.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// The manual exposure program.
        /// </summary>
        Manual = 1,

        /// <summary>
        /// The normal exposure program.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// The aperture priority exposure program.
        /// </summary>
        AperturePriority = 3,

        /// <summary>
        /// The shutter priority exposure program.
        /// </summary>
        ShutterPriority = 4,

        /// <summary>
        /// The creative program (biased toward depth of field) exposure program.
        /// </summary>
        CreativeProgram = 5,

        /// <summary>
        /// The action program (biased toward fast shutter speed) exposure program.
        /// </summary>
        ActionProgram = 6,

        /// <summary>
        /// The portrait mode (for close up photos with the background out of focus) exposure program.
        /// </summary>
        Portrait = 7,

        /// <summary>
        /// The landscape mode (for landscape photos with the background in focus) exposure program.
        /// </summary>
        Landscape = 8,
    }
}
