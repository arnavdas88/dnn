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
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// The base neural layer class. This is an abstract class.
    /// </summary>
    [DebuggerDisplay("{Architecture}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Layer : ICloneable
    {
        private static Dictionary<Type, Regex> registeredLayers = new Dictionary<Type, Regex>();

        static Layer()
        {
            // activations
            Layer.RegisterLayer(typeof(ReLULayer), ReLULayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(SigmoidLayer), SigmoidLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(TanhLayer), TanhLayer.ArchitecturePattern);

            // pooling
            Layer.RegisterLayer(typeof(MaxPoolingLayer), MaxPoolingLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(AveragePoolingLayer), AveragePoolingLayer.ArchitecturePattern);

            // stochastic
            Layer.RegisterLayer(typeof(FullyConnectedLayer), FullyConnectedLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(ConvolutionLayer), ConvolutionLayer.ArchitecturePattern);

            // recurrent
            Layer.RegisterLayer(typeof(SRNCell), SRNCell.ArchitecturePattern);
            Layer.RegisterLayer(typeof(SRNLayer), SRNLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(LSTMCell), LSTMCell.ArchitecturePattern);
            Layer.RegisterLayer(typeof(LSTMLayer), LSTMLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(GRUCell), GRUCell.ArchitecturePattern);
            Layer.RegisterLayer(typeof(GRULayer), GRULayer.ArchitecturePattern);

            // loss
            Layer.RegisterLayer(typeof(SoftMaxLayer), SoftMaxLayer.ArchitecturePattern);

            // other
            Layer.RegisterLayer(typeof(InputLayer), InputLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(DropoutLayer), DropoutLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(ScaleLayer), ScaleLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(SplitLayer), SplitLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(ConcatLayer), ConcatLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(LRNLayer), LRNLayer.ArchitecturePattern);
            Layer.RegisterLayer(typeof(Map2SequenceLayer), Map2SequenceLayer.ArchitecturePattern);
        }

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
        /// The <see cref="string"/>that describes the layer architecture.
        /// </value>
        public abstract string Architecture { get; }

        /// <summary>
        /// Gets or sets the number of outputs of the layer.
        /// </summary>
        /// <value>
        /// The number of outputs.
        /// </value>
        [JsonProperty("NumberOfOutputs")]
        public int NumberOfOutputs { get; protected internal set; } = 1;

        /// <summary>
        /// Gets or sets the dimensions of the layer's output tensor.
        /// </summary>
        /// <value>
        /// The array that contains output tensor dimensions.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Need a fast access to the collection.")]
        [JsonProperty("Output")]
        public int[] OutputShape { get; protected internal set; }

        /// <summary>
        /// Registers a new type of layer.
        /// </summary>
        /// <param name="layerType">The layer type.</param>
        /// <param name="pattern">The regular expression pattern that matches layer architecture.</param>
        public static void RegisterLayer(Type layerType, string pattern)
        {
            if (layerType == null)
            {
                throw new ArgumentNullException(nameof(layerType));
            }

            if (!typeof(Layer).IsAssignableFrom(layerType))
            {
                throw new ArgumentException("The object must be of type layer.");
            }

            Layer.registeredLayers[layerType] = new Regex(pattern, RegexOptions.ECMAScript);
        }

        /// <summary>
        /// Makes an attempt to create a <see cref="Layer"/> from the specified architecture string.
        /// </summary>
        /// <param name="inputShape">The dimensions of the layer's input tensor.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        /// <returns>
        /// The <see cref="Layer"/> this method creates.
        /// </returns>
        public static Layer CreateFromArchitecture(int[] inputShape, string architecture, RandomNumberGenerator random)
        {
            foreach (KeyValuePair<Type, Regex> layer in Layer.registeredLayers)
            {
                if (layer.Value.IsMatch(architecture))
                {
                    return (Layer)Activator.CreateInstance(layer.Key, new object[] { inputShape, architecture, random });
                }
            }

            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnrecognizableLayerArchitecture, architecture),
                nameof(architecture));
        }

        /// <summary>
        /// Makes an attempt to create a <see cref="Layer"/> from the specified architecture string.
        /// </summary>
        /// <param name="inputShapes">The dimensions of the layer's input tensors.</param>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="random">The random numbers generator.</param>
        /// <returns>
        /// The <see cref="Layer"/> this method creates.
        /// </returns>
        public static Layer CreateFromArchitecture(IList<int[]> inputShapes, string architecture, RandomNumberGenerator random)
        {
            foreach (KeyValuePair<Type, Regex> layer in Layer.registeredLayers)
            {
                if (layer.Value.IsMatch(architecture))
                {
                    return (Layer)Activator.CreateInstance(layer.Key, new object[] { inputShapes, architecture, random });
                }
            }

            throw new ArgumentException(
                string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnrecognizableLayerArchitecture, architecture),
                nameof(architecture));
        }

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

        /// <summary>
        /// Parses layer architecture.
        /// </summary>
        /// <param name="architecture">The layer architecture.</param>
        /// <param name="pattern">The regular expression pattern that matches layer architecture.</param>
        /// <returns>The collection of regular expression groups.</returns>
        private protected static List<Group> ParseArchitechture(string architecture, string pattern)
        {
            if (architecture == null)
            {
                throw new ArgumentNullException(nameof(architecture));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            Match match = Regex.Match(architecture, pattern, RegexOptions.ECMAScript);
            if (match == null || !match.Success)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidLayerArchitecture, architecture),
                    nameof(architecture));
            }

            return match.Groups.OfType<Group>().ToList();
        }

        /// <summary>
        /// Parse convolution kernel.
        /// </summary>
        /// <param name="groups">The collection of regular expression groups.</param>
        /// <param name="startingGroup">The first group to start parsing.</param>
        /// <param name="defaultStride">The default value for stride parameter.</param>
        /// <param name="padding">Determines whether padding should be parsed.</param>
        /// <returns>The <see cref="Kernel"/> object this method creates.</returns>
        private protected static Kernel ParseKernel(IList<Group> groups, int startingGroup, int? defaultStride, bool padding)
        {
            int width = Convert.ToInt32(groups[startingGroup].Value, CultureInfo.InvariantCulture);
            int height = !string.IsNullOrEmpty(groups[startingGroup + 1].Value) ? Convert.ToInt32(groups[startingGroup + 1].Value, CultureInfo.InvariantCulture) : width;

            // strides
            int strideX = defaultStride.GetValueOrDefault(width);
            int strideY = defaultStride.GetValueOrDefault(height);
            if (!string.IsNullOrEmpty(groups[startingGroup + 2].Value))
            {
                strideX = Convert.ToInt32(groups[startingGroup + 2].Value, CultureInfo.InvariantCulture);
                strideY = !string.IsNullOrEmpty(groups[startingGroup + 3].Value) ? Convert.ToInt32(groups[startingGroup + 3].Value, CultureInfo.InvariantCulture) : strideX;
            }

            // padding
            int paddingX = 0;
            int paddingY = 0;
            if (padding)
            {
                if (!string.IsNullOrEmpty(groups[startingGroup + 4].Value))
                {
                    paddingX = Convert.ToInt32(groups[startingGroup + 4].Value, CultureInfo.InvariantCulture);
                    paddingY = !string.IsNullOrEmpty(groups[startingGroup + 5].Value) ? Convert.ToInt32(groups[startingGroup + 5].Value, CultureInfo.InvariantCulture) : paddingX;
                }
            }

            return new Kernel(width, height, strideX, strideY, paddingX, paddingY);
        }
    }
}
