// -----------------------------------------------------------------------
// <copyright file="DataSource.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
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
            this.Id = new DataSourceId(id, name, frameIndex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource"/> class.
        /// </summary>
        /// <param name="id">The source of data.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="id"/> is <b>null</b>.
        /// </exception>
        protected DataSource(DataSourceId id)
        {
            this.Id = id ?? throw new ArgumentNullException("id");
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSource"/> class.
        /// </summary>
        protected DataSource()
        {
        }

        /// <summary>
        /// Gets the source of data.
        /// </summary>
        /// <value>
        /// A <see cref="DataSourceId"/> that contains the information about result source.
        /// </value>
        [JsonProperty("id")]
        public DataSourceId Id { get; private set; }
    }
}
