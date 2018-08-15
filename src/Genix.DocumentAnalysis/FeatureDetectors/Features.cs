// <copyright file="Features.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.FeatureDetectors
{
    using System.Collections.Generic;
    using Genix.Core;
    using Genix.Imaging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents features (points of interest) extracted from an <see cref="Image"/>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Features : DenseVectorPackF
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class.
        /// </summary>
        /// <param name="vectors">The feature vectors.</param>
        public Features(DenseVectorPackF vectors)
            : base(vectors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class.
        /// </summary>
        [JsonConstructor]
        private Features()
        {
        }
    }
}
