// -----------------------------------------------------------------------
// <copyright file="CompositeTransform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a composite transformation an <see cref="Image"/> that consists of several other transformations.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CompositeTransform : Transform
    {
        [JsonProperty("transforms")]
        private readonly List<Transform> transforms = new List<Transform>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTransform"/> class.
        /// </summary>
        /// <param name="transforms">The collection of transformations.</param>
        public CompositeTransform(IEnumerable<Transform> transforms)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            Transform last = null;
            Append(transforms);

            void Append(IEnumerable<Transform> collection)
            {
                foreach (Transform transform in transforms)
                {
                    if (transform is IdentityTransform)
                    {
                        continue;
                    }
                    else if (transform is MatrixTransform matrixTransform && last is MatrixTransform lastMatrixTransform)
                    {
                        this.transforms[this.transforms.Count - 1] = last = lastMatrixTransform.Append(matrixTransform);
                    }
                    else if (transform is CompositeTransform compositeTransform)
                    {
                        Append(compositeTransform.transforms);
                    }
                    else
                    {
                        last = transform;
                        this.transforms.Add(transform);
                    }
                }
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="CompositeTransform" /> class from being created.
        /// </summary>
        [JsonConstructor]
        private CompositeTransform()
        {
        }

        /// <summary>
        /// Gets a collection of transformations.
        /// </summary>
        /// <value>
        /// The collection of transformations.
        /// </value>
        public IReadOnlyCollection<Transform> Transforms => this.transforms;

        /// <inheritdoc />
        public override Point Convert(Point value)
        {
            foreach (Transform transform in this.transforms)
            {
                value = transform.Convert(value);
            }

            return value;
        }

        /// <inheritdoc />
        public override Rect Convert(Rect value)
        {
            foreach (Transform transform in this.transforms)
            {
                value = transform.Convert(value);
            }

            return value;
        }

        /// <inheritdoc />
        public override Transform Append(Transform transform)
        {
            if (transform is IdentityTransform)
            {
                return this;
            }

            if (transform is MatrixTransform matrixTransform && matrixTransform.IsIdentity)
            {
                return this;
            }

            return new CompositeTransform(this.transforms.Append(transform));
        }
    }
}
