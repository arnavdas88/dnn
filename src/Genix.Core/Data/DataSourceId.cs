// -----------------------------------------------------------------------
// <copyright file="DataSourceId.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Globalization;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the source of data.
    /// </summary>
    public class DataSourceId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceId"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the data.</param>
        /// <param name="name">The name of the data.</param>
        /// <param name="frameIndex">
        /// The zero-based index for this data if it belongs to an indexed source, such as multi-page file.
        /// <b>null</b> if this data does not belong to an indexed source.
        /// </param>
        public DataSourceId(string id, string name, int? frameIndex)
        {
            this.Id = id;
            this.Name = name;
            this.FrameIndex = frameIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceId"/> class,
        /// using the existing <see cref="DataSourceId"/> object.
        /// </summary>
        /// <param name="other">The <see cref="DataSourceId"/> to copy the data from.</param>
        public DataSourceId(DataSourceId other)
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
        /// Initializes a new instance of the <see cref="DataSourceId"/> class.
        /// </summary>
        private DataSourceId()
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
        /// Gets the zero-based index of a data if it belongs to an indexed source, such as multi-page file.
        /// <b>null</b> if this data does not belong to an indexed source.
        /// </summary>
        /// <value>
        /// The zero-based index for this data if it belongs to an indexed source, such as multi-page file.
        /// <b>null</b> if this data does not belong to an indexed source.
        /// </value>
        [JsonProperty("frameIndex")]
        public int? FrameIndex { get; private set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            DataSourceId other = obj as DataSourceId;
            if (other == null)
            {
                return false;
            }

            return string.Compare(this.Id, other.Id, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                this.FrameIndex == other.FrameIndex;
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.ToString().GetHashCode();

        /// <inheritdoc />
        public override string ToString() =>
            string.Join(
                ",",
                this.Id,
                this.Name,
                this.FrameIndex.HasValue ? this.FrameIndex.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);

        /// <summary>
        /// Constructs the file name for this <see cref="DataSourceId"/> that consist of file name and frame index.
        /// </summary>
        /// <returns>The file name for this <see cref="DataSourceId"/>.</returns>
        public string ToFileName()
        {
            if (this.FrameIndex.HasValue && this.FrameIndex != 0)
            {
                return string.Join(";", Path.GetFileName(this.Id), (this.FrameIndex + 1).ToString());
            }
            else
            {
                return Path.GetFileName(this.Id);
            }
        }
    }
}
