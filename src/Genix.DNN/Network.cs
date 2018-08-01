// -----------------------------------------------------------------------
// <copyright file="Network.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Genix.Core;
    using Genix.DNN.Layers;
    using Genix.DNN.Learning;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Learning;
    using Newtonsoft.Json;

    /// <summary>
    /// The base neural network class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Architecture}")]
    public class Network : ITrainableMachine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Network"/> class.
        /// </summary>
        /// <param name="graph">The network graph.</param>
        internal Network(NetworkGraph graph)
        {
            this.Graph = graph ?? throw new ArgumentNullException(nameof(graph));

            // the graph source must be input layer
            if (graph.Sources.Count() != 1 || !(graph.Sources.FirstOrDefault() is InputLayer))
            {
                throw new ArgumentException(Properties.Resources.E_InvalidNetArchitecture_MissingInputLayer);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Network"/> class, using the existing <see cref="Network"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Network"/> to copy the data from.</param>
        /// <param name="cloneLayers">The value indicating whether the network layers should be cloned.</param>
        protected Network(Network other, bool cloneLayers)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Graph = other.Graph.Clone(cloneLayers) as NetworkGraph;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Network"/> class.
        /// </summary>
        [JsonConstructor]
        protected Network()
        {
        }

        /// <summary>
        /// Gets the network architecture.
        /// </summary>
        /// <value>
        /// The <see cref="string"/>that describes the network architecture.
        /// </value>
        [JsonProperty("Architecture", Order = 0)]
        public string Architecture => this.Graph?.ToString();

        /// <summary>
        /// Gets the network input layout.
        /// </summary>
        /// <value>
        /// The network input layout.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Need a fast access to the collection.")]
        [JsonIgnore]
        public int[] InputShape => this.Graph?.Sources.OfType<InputLayer>().FirstOrDefault()?.Shape;

        /// <summary>
        /// Gets the network output layouts.
        /// </summary>
        /// <value>
        /// The network output layouts.
        /// </value>
        [JsonIgnore]
        public IList<int[]> OutputShapes => this.Graph?.Sinks.Select(x => x.OutputShape).ToList();

        /// <summary>
        /// Gets the graph representation of the network.
        /// </summary>
        /// <value>
        /// The <see cref="NetworkGraph"/> object.
        /// </value>
        [JsonProperty("Graph", Order = 3)]
        internal NetworkGraph Graph { get; private set; }

        /// <summary>
        /// Creates a classification neural network from a string that contains network architecture.
        /// </summary>
        /// <param name="architecture">The network architecture.</param>
        /// <returns>The <see cref="Network"/> object this method creates.</returns>
        public static Network FromArchitecture(string architecture)
        {
            NetworkGraph graph = NetworkGraphBuilder.CreateNetworkGraph(architecture, true, false);

            return new Network(graph);
        }

        /// <summary>
        /// Creates a classification neural network from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Network"/>.</param>
        /// <returns>The <see cref="Network"/> this method creates.</returns>
        public static Network FromFile(string fileName)
        {
            return Network.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a classification neural network from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Network"/> from.</param>
        /// <returns>The <see cref="Network"/> this method creates.</returns>
        public static Network FromMemory(byte[] buffer)
        {
            return Network.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a classification neural network from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="Network"/> from.</param>
        /// <returns>The <see cref="Network"/> this method creates.</returns>
        public static Network FromString(string value)
        {
            return JsonConvert.DeserializeObject<Network>(value);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <param name="cloneLayers">The value indicating whether the network layers should be cloned.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual Network Clone(bool cloneLayers) => new Network(this, cloneLayers);

        /// <summary>
        /// Saves the current <see cref="Network"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Network"/>.</param>
        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Saves the current <see cref="Network"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="Network"/>.</returns>
        public byte[] SaveToMemory()
        {
            return UTF8Encoding.UTF8.GetBytes(this.SaveToString());
        }

        /// <summary>
        /// Saves the current <see cref="Network"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="Network"/>.</returns>
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <inheritdoc />
        public IEnumerable<(Tensor w, float rateL1Multiplier, float rateL2Multiplier)> EnumWeights()
        {
            return this
                .Graph
                .Vertices
                .OfType<TrainableLayer>()
                .Where(x => x.IsTrainable)
                .SelectMany(x => x.EnumGradients());
        }

        /// <inheritdoc />
        public float Learn<TExpected>(IList<(Tensor x, TExpected expected)> samples, ILoss<TExpected> lossFunction)
        {
            Session session = new Session(true);

            float loss = 0.0f;
            for (int i = 0; i < samples.Count; i++)
            {
                loss += this.LearnOne(session, samples[i].x, samples[i].expected, lossFunction).Loss;
            }

            return loss;
        }

        /// <summary>
        /// Optimizes the <see cref="Network"/> for learning.
        /// </summary>
        internal virtual void OptimizeForLearning()
        {
            foreach (Layer layer in this.Graph.Vertices)
            {
                layer.OptimizeForLearning();
            }
        }

        /// <summary>
        /// Optimizes the <see cref="Network"/> for testing.
        /// </summary>
        internal virtual void OptimizeForTesting()
        {
            foreach (Layer layer in this.Graph.Vertices)
            {
                layer.OptimizeForTesting();
            }
        }

        /// <summary>
        /// Computes output tensor of the network.
        /// </summary>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor.</param>
        /// <returns>
        /// The <see cref="Tensor"/> that contains the computed output tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Tensor Forward(Session session, Tensor x)
        {
            session = session ?? new Session(false);

            x.CalculateGradient = false;
            Tensor y = this.Graph.Forward(session, x);
            session.Unroll();

            session.DetachTensor(y);
            session.EndSession();

            return y;
        }

        /// <summary>
        /// Performs one iteration of SGD algorithm on single input.
        /// </summary>
        /// <typeparam name="TExpected">The type for the expected value.</typeparam>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="x">The input tensor.</param>
        /// <param name="expected">The value that should have been predicted.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <returns>
        /// The tuple that contains computed output <see cref="Tensor"/> and calculated loss.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (Tensor Y, float Loss) LearnOne<TExpected>(Session session, Tensor x, TExpected expected, ILoss<TExpected> lossFunction)
        {
            session = session ?? new Session();

            x.CalculateGradient = false;
            Tensor y = this.Graph.Forward(session, x);

            float loss = lossFunction.Loss(y, expected, true);

            Mathematics.Sub(y.Length, y.Weights, 0, y.Gradient, 0, y.Gradient, 0);
            session.Unroll();

            session.DetachTensor(y);
            session.EndSession();

            return (y, Maximum.Min(loss, 100.0f));  // limit loss on top
        }

        /*/// <summary>
        /// Performs one iteration of SGD algorithm on multiple inputs.
        /// </summary>
        /// <typeparam name="TExpected">The type for the expected value.</typeparam>
        /// <param name="session">The scope that executes this operation.</param>
        /// <param name="xs">The input tensors.</param>
        /// <param name="expected">The values that should have been predicted.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <returns>
        /// The tuple that contains computed output <see cref="Tensor"/>s and calculated loss.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (Tensor Y, float Loss) LearnMany<TExpected>(Session session, IList<Tensor> xs, TExpected expected, ILoss<TExpected> lossFunction)
        {
            session = session ?? new Session();

            for (int i = 0, ii < )

            Tensor x = session.Stack(session.Squeeze(xs, (int)Axis.B);
            Tensor y = this.Graph.Forward(session, xs[0]);

            Tensor grad;
            float loss = lossFunction.Loss(y, expected, out grad);

            Tensor dy = session.GetGradient(y);
            MKL.Subtract(y.Length, y.Weights, 0, grad.Weights, 0, dy.Weights, 0);
            session.Unroll();

            return (y, MKL.Min(loss, 100.0f));  // limit loss on top
        }*/
    }
}
