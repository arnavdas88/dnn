// -----------------------------------------------------------------------
// <copyright file="CompositeTransform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a composite transformation an <see cref="Image"/> that consists of several other transformations.
    /// </summary>
    public class CompositeTransform : TransformBase
    {
        [JsonProperty("transforms")]
        private readonly List<TransformBase> transforms = new List<TransformBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTransform"/> class.
        /// </summary>
        /// <param name="transforms">The collection of transformations.</param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 local functions.")]
        public CompositeTransform(IEnumerable<TransformBase> transforms)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            TransformBase last = null;
            append(transforms);

            void append(IEnumerable<TransformBase> collection)
            {
                foreach (TransformBase transform in transforms)
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
                        append(compositeTransform.transforms);
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
        public ReadOnlyCollection<TransformBase> Transforms => new ReadOnlyCollection<TransformBase>(this.transforms);

        /// <inheritdoc />
        public override Point Convert(Point value)
        {
            foreach (TransformBase transform in this.transforms)
            {
                value = transform.Convert(value);
            }

            return value;
        }

        /// <inheritdoc />
        public override Rect Convert(Rect value)
        {
            foreach (TransformBase transform in this.transforms)
            {
                value = transform.Convert(value);
            }

            return value;
        }

        /// <inheritdoc />
        public override TransformBase Append(TransformBase transform)
        {
            if (transform is IdentityTransform)
            {
                return this;
            }

            return new CompositeTransform(this.transforms.Append(transform));
        }
    }
}
