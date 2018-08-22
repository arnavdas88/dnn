// -----------------------------------------------------------------------
// <copyright file="GraphContext.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.LanguageModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using Genix.Graph;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a language model.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GraphContext : Context
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="GraphContext"/> class.
        /// </summary>
        [JsonConstructor]
        public GraphContext()
        {
            this.Graph = new ContextGraph(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphContext"/> class, using the existing <see cref="GraphContext"/> object.
        /// </summary>
        /// <param name="other">The <see cref="GraphContext"/> to copy the data from.</param>
        private GraphContext(GraphContext other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Graph = new ContextGraph(this, other.Graph);
        }

        /// <inheritdoc />
        public override State InitialState
        {
            get
            {
                List<State> states = new List<State>();
                foreach (Context source in this.Graph.Sources)
                {
                    State state = source.InitialState;
                    if (state != null)
                    {
                        states.Add(state);
                    }
                }

                switch (states.Count)
                {
                    case 0:
                        return null;

                    case 1:
                        return states[0];

                    default:
                        return new CompositeState(states);
                }
            }
        }

        /// <summary>
        /// Gets the graph representation of the model.
        /// </summary>
        [JsonProperty("Graph")]
        internal ContextGraph Graph { get; private set; }

        /// <summary>
        /// Creates a <see cref="GraphContext"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="GraphContext"/>.</param>
        /// <returns>The <see cref="GraphContext"/> this method creates.</returns>
        public static GraphContext FromFile(string fileName)
        {
            return GraphContext.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a <see cref="GraphContext"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="GraphContext"/> from.</param>
        /// <returns>The <see cref="GraphContext"/> this method creates.</returns>
        public static GraphContext FromMemory(byte[] buffer)
        {
            return GraphContext.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a graph from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="GraphContext"/> from.</param>
        /// <returns>The <see cref="GraphContext"/> this method creates.</returns>
        public static GraphContext FromString(string value)
        {
            return JsonConvert.DeserializeObject<GraphContext>(value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GraphContext);
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode(); //// ^ (this.characters ?? string.Empty).GetHashCode();

        /// <inheritdoc />
        public override string ToString() => this.Graph?.ToString();

        /// <inheritdoc />
        public override object Clone() => new GraphContext(this);

        /// <summary>
        /// Returns the initial state for the specified <see cref="Context"/> that is a part of this graph.
        /// </summary>
        /// <param name="context">The <see cref="Context"/> to return the initial state for.</param>
        /// <returns>
        /// The <see cref="State"/> object that contains the initial state for the specified <see cref="Context"/>.
        /// </returns>
        public State GetInitialState(Context context)
        {
            List<State> states = new List<State>();
            foreach (Edge<Context> edge in this.Graph.OutEdges(context))
            {
                State state = edge.Target.InitialState;
                if (state != null)
                {
                    states.Add(state);
                }
            }

            switch (states.Count)
            {
                case 0:
                    return null;

                case 1:
                    return states[0];

                default:
                    return new CompositeState(states);
            }
        }

        [JsonConverter(typeof(GraphJsonConverter))]
        internal class ContextGraph : BidirectionalGraph<Context>
        {
            public ContextGraph(GraphContext parent)
            {
                this.Parent = parent;
            }

            public ContextGraph(GraphContext parent, ContextGraph other)
                : base(other, true)
            {
                this.Parent = parent;
            }

            private ContextGraph()
            {
            }

            public GraphContext Parent { get; private set; }

            /// <inheritdoc />
            protected override void OnVertexAdded(Context vertex)
            {
                vertex.Parent = this.Parent;
            }

            /// <inheritdoc />
            protected override void OnVertexRemoved(Context vertex)
            {
                vertex.Parent = null;
            }

            /// <inheritdoc />
            protected override void OnEdgeAdded(Edge<Context> edge)
            {
                edge.Source.IsTail = false;
                edge.Target.IsTail = this.OutDegree(edge.Target) == 0;
            }

            /// <inheritdoc />
            protected override void OnEdgeRemoved(Edge<Context> edge)
            {
                if (this.ContainsVertex(edge.Source))
                {
                    edge.Source.IsTail = this.OutDegree(edge.Source) == 0;
                }
            }
        }

        private class GraphJsonConverter : BidirectionalGraphJsonConverter<ContextGraph, Context>
        {
        }
    }
}
