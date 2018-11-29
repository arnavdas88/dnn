// -----------------------------------------------------------------------
// <copyright file="Edge{TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;

    /// <summary>
    /// The default graph edge implementation.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertex.</typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Source} -> {Target}")]
    public class Edge<TVertex> : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Edge{TVertex}"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public Edge(TVertex source, TVertex target)
        {
            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge{TVertex}"/> class.
        /// </summary>
        [JsonConstructor]
        protected Edge()
        {
        }

        /// <summary>
        /// Gets or sets the source vertex.
        /// </summary>
        /// <value>The source vertex.</value>
        public TVertex Source { get; set; }

        /// <summary>
        /// Gets or sets the target vertex.
        /// </summary>
        /// <value>The target vertex.</value>
        public TVertex Target { get; set; }

        /// <inheritdoc />
        public object Clone() => new Edge<TVertex>(this.Source, this.Target);
    }
}
