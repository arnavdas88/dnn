// -----------------------------------------------------------------------
// <copyright file="RNNCellDirection.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    /// <summary>
    /// Defines the direction of <see cref="RNNCell"/> (forward-only or bi-directional).
    /// </summary>
    public enum RNNCellDirection
    {
        /// <summary>
        /// <see cref="RNNCell"/> is forward-only.
        /// </summary>
        ForwardOnly = 0,

        /// <summary>
        /// <see cref="RNNCell"/> is bi-directional.
        /// </summary>
        BiDirectional = 1,
    }
}
