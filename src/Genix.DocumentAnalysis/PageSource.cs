// -----------------------------------------------------------------------
// <copyright file="PageSource.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the <see cref="DataSource"/> that contains the page data.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PageSource : DataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageSource"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the data.</param>
        /// <param name="name">The name of the data.</param>
        /// <param name="frameIndex">
        /// The zero-based index for this data if it belongs to a multi-page file.
        /// <b>null</b> if this data belongs to a single-page file.
        /// </param>
        /// <param name="page">The <see cref="PageShape"/> object that contains the page data.</param>
        public PageSource(string id, string name, int? frameIndex, PageShape page)
            : base(id, name, frameIndex)
        {
            this.Page = page;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSource"/> class,
        /// using the existing <see cref="DataSource"/> object and the page data.
        /// </summary>
        /// <param name="other">The <see cref="DataSource"/> to copy the data from.</param>
        /// <param name="page">The <see cref="PageShape"/> object that contains the page data.</param>
        public PageSource(DataSource other, PageShape page)
            : base(other)
        {
            this.Page = page;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSource"/> class,
        /// using the existing <see cref="PageSource"/> object.
        /// </summary>
        /// <param name="other">The <see cref="PageSource"/> to copy the data from.</param>
        public PageSource(PageSource other)
            : base(other)
        {
            this.Page = other.Page;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSource"/> class.
        /// </summary>
        private PageSource()
        {
        }

        /// <summary>
        /// Gets the page data.
        /// </summary>
        /// <value>
        /// A <see cref="PageShape"/> object that contains the page data.
        /// </value>
        [JsonProperty("page")]
        public PageShape Page { get; private set; }
    }
}
