﻿// -----------------------------------------------------------------------
// <copyright file="PointsOfInterestFeatures.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a set of features for <see cref="PointsOfInterestClassifier"/>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PointsOfInterestFeatures : Features
    {
        /// <summary>
        /// The features (points of interest) extracted from an <see cref="Imaging.Image"/>.
        /// </summary>
        [JsonProperty("features")]
        private readonly FeatureDetectors.Features features = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsOfInterestFeatures"/> class.
        /// </summary>
        /// <param name="features">The features (points of interest) extracted from an <see cref="Imaging.Image"/>.</param>
        /// <param name="id">The source of data.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="id"/> is <b>null</b>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="features"/> is <b>null</b>.</para>
        /// </exception>
        public PointsOfInterestFeatures(DataSourceId id, FeatureDetectors.Features features)
            : base(id)
        {
            this.features = features ?? throw new ArgumentNullException(nameof(features));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsOfInterestFeatures"/> class.
        /// </summary>
        [JsonConstructor]
        private PointsOfInterestFeatures()
        {
        }

        /// <summary>
        /// Gets the features (points of interest) extracted from an <see cref="Imaging.Image"/>.
        /// </summary>
        /// <value>
        /// The features (points of interest) extracted from an <see cref="Imaging.Image"/>.
        /// </value>
        public FeatureDetectors.Features Features => this.features;
    }
}
