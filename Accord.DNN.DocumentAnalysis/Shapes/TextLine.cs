// -----------------------------------------------------------------------
// <copyright file="TextLine.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.DocumentAnalysis
{
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a horizontal text line.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextLine : Container<Text>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLine"/> class.
        /// </summary>
        /// <param name="texts">The collection of <see cref="Text"/> contained in this container.</param>
        protected TextLine(IList<Text> texts) : base(texts)
        {
        }

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text line: {0}",
            this.Bounds);
    }
}