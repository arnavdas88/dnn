// -----------------------------------------------------------------------
// <copyright file="DataSource.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the contract that provides various representations of data.
    /// This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class DataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the data.</param>
        /// <param name="name">The name of the data.</param>
        /// <param name="frameIndex">
        /// The zero-based index for this data if it belongs to a multi-page file.
        /// <b>null</b> if this data belongs to a single-page file.
        /// </param>
        protected DataSource(string id, string name, int? frameIndex)
        {
            this.Id = id;
            this.Name = name;
            this.FrameIndex = frameIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource"/> class,
        /// using the existing <see cref="DataSource"/> object.
        /// </summary>
        /// <param name="other">The <see cref="DataSource"/> to copy the data from.</param>
        protected DataSource(DataSource other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Id = other.Id;
            this.Name = other.Name;
            this.FrameIndex = other.FrameIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource"/> class.
        /// </summary>
        protected DataSource()
        {
        }

        /// <summary>
        /// Gets the unique identifier of the data.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the unique identifier of the data.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the data.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that contains the name of the data.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the zero-based index of a data if it belongs to a multi-page file.
        /// <b>null</b> if this data belongs to a single-page file.
        /// </summary>
        /// <value>
        /// The zero-based index for this data if it belongs to a multi-page file.
        /// <b>null</b> if this data belongs to a single-page file.
        /// </value>
        [JsonProperty("frameIndex")]
        public int? FrameIndex { get; private set; }
    }
}
