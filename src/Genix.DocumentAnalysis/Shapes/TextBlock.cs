// -----------------------------------------------------------------------
// <copyright file="TextBlock.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis
{
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a block of text.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TextBlock : Container<TextLine>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlock"/> class.
        /// </summary>
        /// <param name="lines">The collection of <see cref="TextLine"/> contained in this container.</param>
        protected TextBlock(IList<TextLine> lines) : base(lines)
        {
        }

        /// <inheritdoc />
        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "Text block: {0}",
            this.Bounds);
    }
}