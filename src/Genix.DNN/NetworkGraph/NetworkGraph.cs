// -----------------------------------------------------------------------
// <copyright file="NetworkGraph.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Genix.DNN.Layers;
    using Genix.Graph;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a bi-directional graph that holds network layers.
    /// </summary>
    [JsonConverter(typeof(NetworkGraphJsonConverter))]
    internal class NetworkGraph : BidirectionalGraph<Layer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGraph"/> class.
        /// </summary>
        public NetworkGraph()
            : base(false, -1, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGraph"/> class
        /// that is empty, has the specified initial capacity for vertices and the default initial capacity for edges.
        /// </summary>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        public NetworkGraph(int vertexCapacity)
            : base(true, vertexCapacity, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGraph"/> class
        /// that is empty and has the specified initial capacity for vertices and edges.
        /// </summary>
        /// <param name="vertexCapacity">The number of vertices that the new graph can initially store.</param>
        /// <param name="edgeCapacity">The number of edges that the new graph can initially store.</param>
        public NetworkGraph(int vertexCapacity, int edgeCapacity)
            : base(true, vertexCapacity, edgeCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkGraph"/> class
        /// using existing graph as a source.
        /// </summary>
        /// <param name="other">The existing <see cref="NetworkGraph"/> to create this graph from.</param>
        /// <param name="cloneLayers">The value indicating whether the layers should be cloned.</param>
        public NetworkGraph(NetworkGraph other, bool cloneLayers)
            : base(other, cloneLayers)
        {
        }

        /// <summary>
        /// Creates a graph from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="NetworkGraph"/>.</param>
        /// <returns>The <see cref="NetworkGraph"/> this method creates.</returns>
        public static NetworkGraph FromFile(string fileName)
        {
            return NetworkGraph.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a graph from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="NetworkGraph"/> from.</param>
        /// <returns>The <see cref="NetworkGraph"/> this method creates.</returns>
        public static NetworkGraph FromMemory(byte[] buffer)
        {
            return NetworkGraph.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a graph from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="NetworkGraph"/> from.</param>
        /// <returns>The <see cref="NetworkGraph"/> this method creates.</returns>
        public static NetworkGraph FromString(string value)
        {
            return JsonConvert.DeserializeObject<NetworkGraph>(value);
        }

        /// <summary>
        /// Saves the current <see cref="NetworkGraph"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="NetworkGraph"/>.</param>
        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Saves the current <see cref="NetworkGraph"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="NetworkGraph"/>.</returns>
        public byte[] SaveToMemory()
        {
            return UTF8Encoding.UTF8.GetBytes(this.SaveToString());
        }

        /// <summary>
        /// Saves the current <see cref="NetworkGraph"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="NetworkGraph"/>.</returns>
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <inheritdoc />
        public override BidirectionalGraph<Layer, Edge<Layer>> Clone(bool cloneLayers)
        {
            return new NetworkGraph(this, cloneLayers);
        }

        /// <summary>
        /// Adds a collection of edges to the graph.
        /// </summary>
        /// <param name="sources">The source layers.</param>
        /// <param name="target">The target layer.</param>
        /// <returns>The number of edges that were added.</returns>
        public int AddEdges(IEnumerable<Layer> sources, Layer target)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            return this.AddEdges(sources.Select(x => (x, target)));
        }

        /// <summary>
        /// Adds a collection of edges to the graph.
        /// </summary>
        /// <param name="source">The source layer.</param>
        /// <param name="targets">The target layers.</param>
        /// <returns>The number of edges that were added.</returns>
        public int AddEdges(Layer source, IEnumerable<Layer> targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return this.AddEdges(targets.Select(x => (source, x)));
        }

        /// <summary>
        /// Adds a collection of edges to the graph.
        /// </summary>
        /// <param name="sources">The source layers.</param>
        /// <param name="targets">The target layers.</param>
        /// <returns>The number of edges that were added.</returns>
        public int AddEdges(IEnumerable<Layer> sources, IEnumerable<Layer> targets)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            int count = 0;
            foreach (Layer source in sources)
            {
                count += this.AddEdges(targets.Select(x => (source, x)));
            }

            return count;
        }

        /// <summary>
        /// Adds a collection of edges that connects specified layers to the graph.
        /// </summary>
        /// <param name="layers">The layers to add.</param>
        /// <returns>The number of edges that were added.</returns>
        public int AddEdges(IEnumerable<Layer> layers)
        {
            if (layers == null)
            {
                throw new ArgumentNullException(nameof(layers));
            }

            Layer source = null;
            int count = 0;
            foreach (Layer layer in layers)
            {
                if (source == null)
                {
                    this.AddVertex(layer);
                }
                else if (this.AddEdge(source, layer))
                {
                    count++;
                }

                source = layer;
            }

            return count;
        }

        public Tensor Forward(Session session, Tensor x)
        {
            return this.Forward(
                session,
                this.Sources.First(),
                new[] { x },
                new Dictionary<Edge<Layer>, Tensor>(this.Size));
        }

        private Tensor Forward(Session session, Layer target, IList<Tensor> xs, Dictionary<Edge<Layer>, Tensor> tensorMap)
        {
            while (target != null)
            {
                // collect input tensors
                if (xs == null)
                {
                    IList<Edge<Layer>> inputEdges = this.InEdges(target);
                    int inputDegree = inputEdges.Count;

                    // all inputs have to be computed to continue
                    xs = new Tensor[inputDegree];
                    for (int i = 0; i < inputDegree; i++)
                    {
                        if (!tensorMap.TryGetValue(inputEdges[i], out Tensor x))
                        {
                            return null;
                        }

                        xs[i] = x;
                    }
                }

                // execute layer on created inputs
                IList<Tensor> ys = target.Forward(session, xs);

                // process output tensors
                IList<Edge<Layer>> outEdges = this.OutEdges(target);
                int outDegree = outEdges.Count;
                Debug.Assert(ys.Count == outDegree || (ys.Count == 1 && outDegree == 0), "The number of output tensors must match out degree.");
                Debug.Assert(ys.SelectMany(t => t.Weights).All(w => !float.IsNaN(w)), "Tensor contains invalid weight.");

                if (outDegree == 0)
                {
                    // return result
                    return ys[0];
                }
                else if (outDegree == 1)
                {
                    // save output tensor
                    tensorMap[outEdges[0]] = ys[0];

                    // advance further down the tree
                    target = outEdges[0].Target;
                    xs = null;
                }
                else if (outDegree > 1)
                {
                    // save output tensors
                    for (int i = 0; i < outDegree; i++)
                    {
                        tensorMap[outEdges[i]] = ys[i];
                    }

                    // multiple out edges - traverse each one separately
                    for (int i = 0; i < outDegree; i++)
                    {
                        Tensor y = this.Forward(session, outEdges[i].Target, null, tensorMap);
                        if (y != null)
                        {
                            // we have reached the end
                            return y;
                        }
                    }

                    // we are inside bigger loop
                    return null;
                }
            }

            Debug.Assert(false, "Something is wrong.");
            return null;
        }
    }
}
