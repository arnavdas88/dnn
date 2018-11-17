// -----------------------------------------------------------------------
// <copyright file="NetworkGraphBuilder.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Genix.Core;
    using Genix.DNN.Layers;
    using Genix.Graph;
    using Genix.MachineLearning;

    internal class NetworkGraphBuilder
    {
        private const char Splitter = '~';
        private const char Delimiter = ',';
        private const char StartQualifier = '{';
        private const char EndQualifier = '}';

        public static NetworkGraph CreateNetworkGraph(string architecture, TensorShape shape, bool addActivationLayers, bool addLossLayer)
        {
            if (architecture == null)
            {
                throw new ArgumentNullException(nameof(architecture));
            }

            if (string.IsNullOrEmpty(architecture))
            {
                throw new ArgumentException(Properties.Resources.E_InvalidNetArchitecture_NoLayers, nameof(architecture));
            }

            // 1. parse architecture string and build preliminary graph
            ComponentGraph componentGraph = NetworkGraphBuilder.ParseArchitecture(architecture, false);

            // 2. create layers in the preliminary graph
            RandomNumberGenerator<float> random = null; //// new Random(0);
            foreach (ComponentVertex sink in componentGraph.Sinks)
            {
                NetworkGraphBuilder.CreateLayerInGraph(shape, componentGraph, sink, random);
            }

            // 3. convert to network graph
            NetworkGraph graph = new NetworkGraph();
            foreach (Edge<ComponentVertex> edge in componentGraph.Edges)
            {
                /*NetworkGraph sourceGraph = (edge.Source.Layer as RNNLayer)?.Graph;
                NetworkGraph targetGraph = (edge.Target.Layer as RNNLayer)?.Graph;

                if (sourceGraph != null)
                {
                    graph.AddGraph(sourceGraph);
                    if (targetGraph != null)
                    {
                        graph.AddEdges(sourceGraph.Sinks, targetGraph.Sources);
                        graph.AddGraph(targetGraph);
                    }
                    else
                    {
                        graph.AddEdges(sourceGraph.Sinks, edge.Target.Layer);
                    }
                }
                else if (targetGraph != null)
                {
                    graph.AddEdges(edge.Source.Layer, targetGraph.Sources);
                    graph.AddGraph(targetGraph);
                }
                else*/
                {
                    graph.AddEdge(edge.Source.Layer, edge.Target.Layer);
                }
            }

            // 4. add missing loss layers
            if (addLossLayer)
            {
                NetworkGraphBuilder.AddLossLayers(graph);
            }

            // 5. add missing activation layers
            if (addActivationLayers)
            {
                NetworkGraphBuilder.AddActivationLayers(graph);
            }

            // 6. initialize stochastic biases with ReLU activations
            NetworkGraphBuilder.InitializeReLUs(graph);

            return graph;
        }

        private static ComponentGraph ParseArchitecture(string architecture, bool isNested)
        {
            // split architecture into elements that include layers, modules, and edges
            IList<string> components = ComponentGraph.SplitArchitecture(architecture, NetworkGraphBuilder.Delimiter);

            // extract elements that defines edges and modules (in A=... format)
            Dictionary<string, string> elements;
            Dictionary<string, ComponentGraph> modules;
            NetworkGraphBuilder.ParseComponents(components, out elements, out modules);

            // components must now contain edges only (in A-B-... format)
            ComponentGraph graph = ComponentGraph.FromComponents(components, elements, modules);

            // process nested graphs
            if (isNested)
            {
                // nested graphs must start with a single split layer
                IList<ComponentVertex> sources = graph.Sources.ToList();
                if (sources.Count > 1)
                {
                    ComponentVertex split = new ComponentVertex(
                        Guid.NewGuid().ToString(),
                        string.Format(CultureInfo.InvariantCulture, "SP{0}", sources.Count));

                    graph.AddEdges(sources.Select(x => new Edge<ComponentVertex>(split, x)));
                }

                // nested graphs must end with a single concat layer
                IList<ComponentVertex> sinks = graph.Sinks.ToList();
                if (sinks.Count > 1)
                {
                    ComponentVertex concat = new ComponentVertex(Guid.NewGuid().ToString(), "CONCAT");

                    graph.AddEdges(sinks.Select(x => new Edge<ComponentVertex>(x, concat)));
                }
            }

            return graph;
        }

        private static void ParseComponents(IList<string> components, out Dictionary<string, string> elements, out Dictionary<string, ComponentGraph> modules)
        {
            Regex regex = new Regex(@"^(\w+)\s*=\s*(.+)$", RegexOptions.ECMAScript);

            elements = new Dictionary<string, string>();
            modules = new Dictionary<string, ComponentGraph>();

            for (int i = 0; i < components.Count; i++)
            {
                string component = components[i];
                Match match = regex.Match(component);
                if (match != null && match.Success && match.Groups.Count == 3)
                {
                    string key = match.Groups[1].Value.Trim();
                    string value = match.Groups[2].Value.Trim();

                    if (elements.ContainsKey(key))
                    {
                        throw new ArgumentException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidNetArchitecture_DuplicateVertex, component));
                    }

                    if (value.First() == NetworkGraphBuilder.StartQualifier && value.Last() == NetworkGraphBuilder.EndQualifier)
                    {
                        modules.Add(key, NetworkGraphBuilder.ParseArchitecture(value.Substring(1, value.Length - 2), true));
                    }
                    else
                    {
                        elements.Add(key, value);
                    }

                    components.RemoveAt(i--);
                }
            }
        }

        private static void CreateLayerInGraph(TensorShape shape, ComponentGraph graph, ComponentVertex vertex, RandomNumberGenerator<float> random)
        {
            if (graph.InDegree(vertex) == 0)
            {
                // use some arbitrary layout to start
                // source layer must be input layer that overrides it
                vertex.Layer = Layer.CreateFromArchitecture(shape, new[] { -1, 100, 100, 100 }, vertex.Architecture, random);
            }
            else
            {
                IList<int[]> axes = new List<int[]>();
                foreach (Edge<ComponentVertex> edge in graph.InEdges(vertex))
                {
                    if (edge.Source.Layer == null)
                    {
                        NetworkGraphBuilder.CreateLayerInGraph(graph, edge.Source, random);
                    }

                    axes.Add(edge.Source.Layer.OutputAxes);
                }

                vertex.Layer = axes.Count == 1 ?
                    Layer.CreateFromArchitecture(axes[0], vertex.Architecture, random) :
                    Layer.CreateFromArchitecture(axes, vertex.Architecture, random);
            }
        }

        private static void AddActivationLayers(NetworkGraph graph)
        {
            foreach (Layer layer in graph.Vertices.Where(x => ((x as TrainableLayer)?.NeedsActivation).GetValueOrDefault()).ToList())
            {
                Layer source = layer;
                if (graph.OutDegree(source) == 1)
                {
                    // optimization - add activation layer after max pooling layer that follows stochastic
                    NetworkEdge edge = graph.OutEdges(source)[0];
                    if (edge.Target is MaxPoolingLayer)
                    {
                        source = edge.Target;
                    }
                }

                if (graph.OutDegree(source) == 1)
                {
                    NetworkEdge edge = graph.OutEdges(source)[0];
                    if (!(edge.Target is ActivationLayer) && !(edge.Target is LossLayer))
                    {
                        Layer activationLayer = new TanhLayer(edge.Source.OutputShape);
                        graph.AddVertex(activationLayer);

                        NetworkEdge newEdge = new NetworkEdge(edge.Source, activationLayer);
                        graph.OutEdges(source)[0] = newEdge;
                        graph.InEdges(activationLayer).Add(newEdge);

                        if (edge.Target != null)
                        {
                            IList<NetworkEdge> inedges = graph.InEdges(edge.Target);
                            int index = inedges.IndexOf(edge);
                            newEdge = new NetworkEdge(activationLayer, edge.Target);
                            inedges[index] = newEdge;
                            graph.OutEdges(activationLayer).Add(newEdge);
                        }
                    }
                }
            }
        }

        private static void AddLossLayers(NetworkGraph graph)
        {
            foreach (Layer layer in graph.Sinks.ToList())
            {
                if (!(layer is LossLayer))
                {
                    graph.AddEdge(layer, new SoftMaxLayer(layer.OutputShape));
                }
            }
        }

        private static void InitializeReLUs(NetworkGraph graph)
        {
            // relus like a bit of positive bias to get gradients early
            // otherwise it's technically possible that a relu unit will never turn on (by chance)
            // and will never get any gradient and never contribute any computation. Dead relu.
            foreach (Layer layer in graph.Vertices.Where(x => x is ReLULayer).ToList())
            {
                Layer target = layer;
                if (graph.InDegree(target) == 1)
                {
                    NetworkEdge edge = graph.InEdges(target)[0];
                    if (edge.Target is MaxPoolingLayer)
                    {
                        target = edge.Source;
                    }
                }

                if (graph.InDegree(target) == 1)
                {
                    NetworkEdge edge = graph.InEdges(target)[0];
                    if (edge.Source is StochasticLayer stochasticLayer)
                    {
                        stochasticLayer.B.Set(0.1f);
                    }
                }
            }
        }

        private class ComponentGraph : BidirectionalGraph<ComponentVertex>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentGraph"/> class.
            /// </summary>
            public ComponentGraph()
                : base(true, -1, -1)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComponentGraph"/> class
            /// using existing graph as a source.
            /// </summary>
            /// <param name="other">The existing <see cref="ComponentGraph"/> to create this graph from.</param>
            /// <param name="cloneVertices">The value indicating whether the graph vertices should be cloned.</param>
            public ComponentGraph(ComponentGraph other, bool cloneVertices)
                : base(other, cloneVertices)
            {
            }

            public static ComponentGraph FromComponents(IEnumerable<string> components, IDictionary<string, string> elements, IDictionary<string, ComponentGraph> modules)
            {
                ComponentGraph graph = new ComponentGraph();
                Dictionary<string, ComponentVertex> vertices = new Dictionary<string, ComponentVertex>();

                foreach (string component in components)
                {
                    IList<string> parts = ComponentGraph.SplitArchitecture(component, NetworkGraphBuilder.Splitter);
                    if (parts.Count >= 2 && parts.All(x => !string.IsNullOrEmpty(x)))
                    {
                        ComponentVertex sourceVertex = null;
                        ComponentGraph sourceGraph = null;

                        for (int i = 0, ii = parts.Count; i < ii; i++)
                        {
                            string key = parts[i];
                            ComponentGraph targetGraph = null;

                            if (key.First() == NetworkGraphBuilder.StartQualifier && key.Last() == NetworkGraphBuilder.EndQualifier)
                            {
                                targetGraph = NetworkGraphBuilder.ParseArchitecture(key.Substring(1, key.Length - 2), true);
                            }
                            else
                            {
                                ComponentGraph moduleGraph;
                                if (modules.TryGetValue(key, out moduleGraph))
                                {
                                    targetGraph = moduleGraph.Clone(true) as ComponentGraph;
                                }
                            }

                            if (targetGraph != null)
                            {
                                if (i > 0)
                                {
                                    bool result = sourceVertex != null ? ComponentGraph.AddEdge(graph, sourceVertex, targetGraph) : ComponentGraph.AddEdge(graph, sourceGraph, targetGraph);
                                    if (!result)
                                    {
                                        throw new ArgumentException(
                                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidNetArchitecture_DuplicateEdge, parts[i - 1], parts[i]));
                                    }
                                }

                                sourceVertex = null;
                                sourceGraph = targetGraph;
                            }
                            else
                            {
                                ComponentVertex targetVertex;

                                string arch;
                                if (elements.TryGetValue(key, out arch))
                                {
                                    if (!vertices.TryGetValue(key, out targetVertex))
                                    {
                                        vertices[key] = targetVertex = new ComponentVertex(key, arch);
                                    }
                                }
                                else
                                {
                                    targetVertex = new ComponentVertex(Guid.NewGuid().ToString(), key);
                                }

                                if (i > 0)
                                {
                                    bool result = sourceVertex != null ? ComponentGraph.AddEdge(graph, sourceVertex, targetVertex) : ComponentGraph.AddEdge(graph, sourceGraph, targetVertex);
                                    if (!result)
                                    {
                                        throw new ArgumentException(
                                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidNetArchitecture_DuplicateEdge, parts[i - 1], parts[i]));
                                    }
                                }

                                sourceVertex = targetVertex;
                                sourceGraph = null;
                            }
                        }
                    }
                    else if (parts.Count == 1 && !string.IsNullOrEmpty(parts[0]))
                    {
                        graph.AddVertex(new ComponentVertex(Guid.NewGuid().ToString(), parts[0]));
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidNetArchitecture, component));
                    }
                }

                // recreate vertices keys, for the embedded graphs to have unique layers
                foreach (ComponentVertex vertex in graph.Vertices)
                {
                    vertex.Key = Guid.NewGuid().ToString();
                }

                return graph;
            }

            public static IList<string> SplitArchitecture(string s, char delimiter)
            {
                List<string> result = new List<string>();
                for (int i = 0, ii = s.Length; i < ii; i++)
                {
                    int iorig = i;

                    start:
                    int pos = s.IndexOfAny(new char[] { delimiter, NetworkGraphBuilder.StartQualifier }, i);
                    if (pos == -1)
                    {
                        result.Add(s.Substring(iorig).Trim());
                        break;
                    }
                    else if (s[pos] == delimiter)
                    {
                        result.Add(s.Substring(iorig, pos - iorig).Trim());
                        i = pos;
                    }
                    else if (s[pos] == NetworkGraphBuilder.StartQualifier)
                    {
                        int qualcount = 1;
                        do
                        {
                            pos = s.IndexOfAny(new char[] { NetworkGraphBuilder.StartQualifier, NetworkGraphBuilder.EndQualifier }, pos + 1);
                            if (pos == -1)
                            {
                                throw new ArgumentException(
                                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_InvalidNetArchitecture, s));
                            }

                            if (s[pos] == NetworkGraphBuilder.StartQualifier)
                            {
                                qualcount++;
                            }

                            if (s[pos] == NetworkGraphBuilder.EndQualifier)
                            {
                                qualcount--;
                            }
                        }
                        while (qualcount > 0);

                        i = pos + 1;
                        goto start;
                    }
                }

                return result;
            }

            /// <summary>
            /// Creates a new object that is a copy of the current instance.
            /// </summary>
            /// <param name="cloneVertices">The value indicating whether the graph vertices should be cloned.</param>
            /// <returns>
            /// A new object that is a copy of this instance.
            /// </returns>
            public override BidirectionalGraph<ComponentVertex, Edge<ComponentVertex>> Clone(bool cloneVertices)
            {
                return new ComponentGraph(this, cloneVertices);
            }

            private static bool AddEdge(ComponentGraph graph, ComponentVertex sourceVertex, ComponentVertex targetVertex)
            {
                return graph.AddEdge(new Edge<ComponentVertex>(sourceVertex, targetVertex));
            }

            private static bool AddEdge(ComponentGraph graph, ComponentVertex sourceVertex, ComponentGraph targetGraph)
            {
                if (graph.AddEdges(targetGraph.Sources.Select(x => new Edge<ComponentVertex>(sourceVertex, x))) != targetGraph.Sinks.Count())
                {
                    return false;
                }

                return graph.AddEdges(targetGraph.Edges) == targetGraph.Size;
            }

            private static bool AddEdge(ComponentGraph graph, ComponentGraph sourceGraph, ComponentVertex targetVertex)
            {
                return graph.AddEdges(sourceGraph.Sinks.Select(x => new Edge<ComponentVertex>(x, targetVertex))) == sourceGraph.Sinks.Count();
            }

            private static bool AddEdge(ComponentGraph graph, ComponentGraph sourceGraph, ComponentGraph targetGraph)
            {
                graph.AddEdges(sourceGraph.Sinks.Zip(targetGraph.Sources, (s, t) => new Edge<ComponentVertex>(s, t)));

                return graph.AddEdges(targetGraph.Edges) == targetGraph.Size;
            }
        }

        private class ComponentVertex : ICloneable
        {
            public ComponentVertex(string key, string architecture)
            {
                this.Key = key;
                this.Architecture = architecture;
            }

            public string Key { get; set; }

            public string Architecture { get; }

            public Layer Layer { get; set; }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                ComponentVertex other = obj as ComponentVertex;
                if (other == null)
                {
                    return false;
                }

                return this.Key == other.Key &&
                    this.Architecture == other.Architecture &&
                    object.ReferenceEquals(this.Layer, other.Layer);
            }

            /// <inheritdoc />
            public override int GetHashCode() => this.Architecture.GetHashCode();

            /// <inheritdoc />
            public override string ToString() => this.Layer?.ToString() ?? this.Architecture;

            /// <summary>
            /// Creates a new object that is a copy of the current instance.
            /// </summary>
            /// <returns>
            /// A new object that is a copy of this instance.
            /// </returns>
            public virtual object Clone() => new ComponentVertex(this.Key, this.Architecture)
            {
                Layer = this.Layer?.Clone() as Layer,
            };
        }
    }
}
