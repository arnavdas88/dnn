// -----------------------------------------------------------------------
// <copyright file="NetworkEdge.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Layers;
    using Newtonsoft.Json;

    /// <summary>
    /// The network graph edge implementation.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class NetworkEdge : Edge<Layer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkEdge"/> class.
        /// </summary>
        /// <param name="source">The source vertex.</param>
        /// <param name="target">The target vertex.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NetworkEdge(Layer source, Layer target) : base(source, target)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkEdge"/> class.
        /// </summary>
        /// <param name="other">The <see cref="NetworkEdge"/> to copy the data from.</param>
        /// <param name="verticesSubstitutionMap">The dictionary used to map vertices from the <c>other</c> edge.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NetworkEdge(NetworkEdge other, IDictionary<Layer, Layer> verticesSubstitutionMap)
            : base(other, verticesSubstitutionMap)
        {
            // do not copy working tensors
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NetworkEdge"/> class from being created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        private NetworkEdge()
        {
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Clone(IDictionary<Layer, Layer> verticesSubstitutionMap)
        {
            return new NetworkEdge(this, verticesSubstitutionMap);
        }
    }
}
