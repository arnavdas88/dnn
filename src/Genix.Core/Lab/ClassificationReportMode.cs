// -----------------------------------------------------------------------
// <copyright file="ClassificationReportMode.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the classification report writing mode.
    /// </summary>
    [Flags]
    public enum ClassificationReportMode
    {
        /// <summary>
        /// None of the options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Writes report class summary.
        /// </summary>
        Summary = 1,

        /// <summary>
        /// Writes class confusion matrix.
        /// </summary>
        ConfusionMatrix = 2,

        /// <summary>
        /// Writes reject curves.
        /// </summary>
        RejectCurves = 4,

        /// <summary>
        /// Writes list of errors.
        /// </summary>
        Errors = 8,

        /// <summary>
        /// Writes list of answers.
        /// </summary>
        Answers = 16,

        /// <summary>
        /// All of the options.
        /// </summary>
        All = Summary | /*ConfusionMatrix |*/ RejectCurves | Errors /*| Answers*/,
    }
}
