// -----------------------------------------------------------------------
// <copyright file="EXIFLightSource.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    /// <summary>
    /// Specifies the values for the <see cref="EXIFField.LightSource"/> field.
    /// </summary>
    public enum EXIFLightSource
    {
        /// <summary>
        /// The unknown light source.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The daylight light source.
        /// </summary>
        Daylight = 1,

        /// <summary>
        /// The fluorescent source.
        /// </summary>
        Fluorescent = 2,

        /// <summary>
        /// The tungsten (incandescent light) light source.
        /// </summary>
        Tungsten = 3,

        /// <summary>
        /// The flash light source.
        /// </summary>
        Flash = 4,

        /// <summary>
        /// The fine weather light source.
        /// </summary>
        FineWeather = 9,

        /// <summary>
        /// The cloudy weather light source.
        /// </summary>
        CloudyWeather = 10,

        /// <summary>
        /// The shade light source.
        /// </summary>
        Shade = 11,

        /// <summary>
        /// The daylight fluorescent (D 5700 - 7100K) light source.
        /// </summary>
        DaylightFluorescent = 12,

        /// <summary>
        /// The day white fluorescent (N 4600 - 5400K) light source.
        /// </summary>
        DayWhiteFluorescent = 13,

        /// <summary>
        /// The cool white fluorescent (W 3900 - 4500K) light source.
        /// </summary>
        CoolWhiteFluorescent = 14,

        /// <summary>
        /// The white fluorescent (WW 3200 - 3700K) light source.
        /// </summary>
        WhiteFluorescent = 15,

        /// <summary>
        /// The standard light A light source.
        /// </summary>
        StandardLightA = 17,

        /// <summary>
        /// The standard light B light source.
        /// </summary>
        StandardLightB = 18,

        /// <summary>
        /// The standard light C light source.
        /// </summary>
        StandardLightC = 19,

        /// <summary>
        /// The D55 source.
        /// </summary>
        D55 = 20,

        /// <summary>
        /// The D65 source.
        /// </summary>
        D65 = 21,

        /// <summary>
        /// The D75 source.
        /// </summary>
        D75 = 22,

        /// <summary>
        /// The D50 source.
        /// </summary>
        D50 = 23,

        /// <summary>
        /// The ISO studio tungsten source.
        /// </summary>
        ISOStudioTungsten = 24,

        /// <summary>
        /// The other source.
        /// </summary>
        Other = 255,
    }
}
