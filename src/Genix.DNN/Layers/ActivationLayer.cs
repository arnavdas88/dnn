// -----------------------------------------------------------------------
// <copyright file="ActivationLayer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the layer of activation neurons. This is an abstract class.
    /// </summary>
    /// <seealso cref="ReLULayer"/>
    /// <seealso cref="SigmoidLayer"/>
    /// <seealso cref="TanhLayer"/>
    /// <seealso cref="MaxOutLayer"/>
    public abstract class ActivationLayer : Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationLayer"/> class.
        /// </summary>
        /// <param name="outputShape">The dimensions of the layer's output tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ActivationLayer(int[] outputShape)
            : base(1, outputShape)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationLayer"/> class, using the existing <see cref="ActivationLayer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ActivationLayer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ActivationLayer(ActivationLayer other) : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationLayer"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ActivationLayer()
        {
        }
    }
}
