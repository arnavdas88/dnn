// -----------------------------------------------------------------------
// <copyright file="GradientAccumulators.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class GradientAccumulators<T> : ConcurrentDictionary<float[], T>
    {
        public GradientAccumulators()
            : base(ReferenceEqualityComparer.Default)
        {
        }

        public T GetAccumulator(float[] gradient, Func<float[], T> factory)
        {
            T value;
            if (!this.TryGetValue(gradient, out value))
            {
                value = factory(gradient);
                this.TryAdd(gradient, value);
            }

            return value;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Default = new ReferenceEqualityComparer();

            private ReferenceEqualityComparer()
            {
            }

            public new bool Equals(object x, object y)
            {
                return x == y; // This is reference equality! (See explanation below.)
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}