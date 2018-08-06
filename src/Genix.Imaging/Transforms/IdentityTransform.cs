// -----------------------------------------------------------------------
// <copyright file="IdentityTransform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a horizontal and vertical shift of an <see cref="Image"/>.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class IdentityTransform : Transform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityTransform"/> class.
        /// </summary>
        public IdentityTransform()
        {
        }

        /// <inheritdoc />
        public override Point Convert(Point value)
        {
            return value;
        }

        /// <inheritdoc />
        public override Rect Convert(Rect value)
        {
            return value;
        }

        /// <inheritdoc />
        public override Transform Append(Transform transform)
        {
            return transform;
        }
    }
}
