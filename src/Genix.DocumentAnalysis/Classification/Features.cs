// -----------------------------------------------------------------------
// <copyright file="Features.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.Classification
{
    using System;
    using System.IO;
    using System.Text;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a set of features extracted from a data source.
    /// This is an abstract class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Features : DataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the data.</param>
        /// <param name="name">The name of the data.</param>
        /// <param name="frameIndex">
        /// The zero-based index for this data if it belongs to a multi-page file.
        /// <b>null</b> if this data belongs to a single-page file.
        /// </param>
        protected Features(string id, string name, int? frameIndex)
            : base(id, name, frameIndex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class,
        /// using the <see cref="DataSource"/>.
        /// </summary>
        /// <param name="source">The data source used to build these features.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <b>null</b>.
        /// </exception>
        protected Features(DataSource source)
            : base(source)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class,
        /// using the existing <see cref="Features"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Features"/> to copy the data from.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is <b>null</b>.
        /// </exception>
        protected Features(Features other)
            : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Features"/> class.
        /// </summary>
        protected Features()
        {
        }

        /// <summary>
        /// Saves the current <see cref="Features"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Features"/>.</param>
        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Saves the current <see cref="Features"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="Features"/>.</returns>
        public byte[] SaveToMemory()
        {
            return UTF8Encoding.UTF8.GetBytes(this.SaveToString());
        }

        /// <summary>
        /// Saves the current <see cref="Features"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="Features"/>.</returns>
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
