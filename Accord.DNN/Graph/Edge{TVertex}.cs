// -----------------------------------------------------------------------
// <copyright file="Edge{TVertex}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Newtonsoft.Json;

    /// <summary>
    /// The default graph edge implementation.
    /// </summary>
    /// <typeparam name="TVertex">The type of the vertex.</typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Source} -> {Target}")]
    internal class Edge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Edge{TVertex}"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        public Edge(TVertex source, TVertex target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge{TVertex}"/> class.
        /// </summary>
        /// <param name="other">The <see cref="Edge{TVertex}"/> to copy the data from.</param>
        /// <param name="verticesSubstitutionMap">The dictionary used to map vertices from the <c>other</c> edge.</param>
        public Edge(Edge<TVertex> other, IDictionary<TVertex, TVertex> verticesSubstitutionMap)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (verticesSubstitutionMap != null)
            {
                this.Source = verticesSubstitutionMap[other.Source];
                this.Target = verticesSubstitutionMap[other.Target];
            }
            else
            {
                this.Source = other.Source;
                this.Target = other.Target;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge{TVertex}"/> class.
        /// </summary>
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

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <param name="verticesSubstitutionMap">The dictionary used to map vertices from the cloned edge.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone(IDictionary<TVertex, TVertex> verticesSubstitutionMap)
        {
            return new Edge<TVertex>(this, verticesSubstitutionMap);
        }
    }
}
