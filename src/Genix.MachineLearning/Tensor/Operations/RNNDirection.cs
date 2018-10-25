// -----------------------------------------------------------------------
// <copyright file="RNNDirection.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    /// <summary>
    /// Defines the direction of recurrent neural net cell (forward-only or bi-directional).
    /// </summary>
    public enum RNNDirection
    {
        /// <summary>
        /// The cell is forward-only.
        /// </summary>
        ForwardOnly = 0,

        /// <summary>
        /// The cell is bi-directional.
        /// </summary>
        BiDirectional = 1,
    }
}
