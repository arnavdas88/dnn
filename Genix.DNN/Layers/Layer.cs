// -----------------------------------------------------------------------
// <copyright file="Layer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// The base neural layer class. This is an abstract class.
    /// </summary>
    [DebuggerDisplay("{Architecture}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Layer : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Layer"/> class.
        /// </summary>
        /// <param name="numberOfOutputs">The number of output tensors.</param>
        /// <param name="outputShape">The dimensions of the layer's output tensor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Layer(int numberOfOutputs, int[] outputShape)
        {
            if (outputShape == null)
            {
                throw new ArgumentNullException(nameof(outputShape));
            }

            this.NumberOfOutputs = numberOfOutputs;
            this.OutputShape = outputShape;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer"/> class, using the existing <see cref="Layer"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Layer"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Layer(Layer other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.NumberOfOutputs = other.NumberOfOutputs;
            this.OutputShape = other.OutputShape;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Layer()
        {
        }

        /// <summary>
        /// Gets the layer architecture.
        /// </summary>
        /// <value>
        /// The <see cref="String"/>that describes the layer architecture.
        /// </value>
        public abstract string Architecture { get; }

        /// <summary>
        /// Gets the number of outputs of the layer.
        /// </summary>
        /// <value>
        /// The number of outputs.
        /// </value>
        [JsonProperty("NumberOfOutputs")]
        public int NumberOfOutputs { get; private set; }

        /// <summary>
        /// Gets the dimensions of the layer's output tensor.
        /// </summary>
        /// <value>
        /// The array that contains output tensor dimensions.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Need a fast access to the collection.")]
        [JsonProperty("Output")]
        public int[] OutputShape { get; private set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return this.Architecture;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public abstract object Clone();

        /// <summary>
        /// Saves the current <see cref="Layer"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="Layer"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Optimizes the <see cref="Layer"/> for learning.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OptimizeForLearning()
        {
        }

        /// <summary>
        /// Optimizes the <see cref="Layer"/> for testing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OptimizeForTesting()
        {
        }

        /// <summary>
        /// Computes output tensors for the layer.
        /// </summary>
        /// <param name="session">The graph that stores all operations performed on the tensors.</param>
        /// <param name="xs">The input tensors.</param>
        /// <returns>The output tensors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract IList<Tensor> Forward(Session session, IList<Tensor> xs);
    }
}
