// -----------------------------------------------------------------------
// <copyright file="ManhattanDistance.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Distances
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Measures the Manhattan distance between two points.
    /// </summary>
    public struct ManhattanDistance
        : IDistance<float, float, float>,
          IDistance<double, double, double>,
          IDistance<float[], float[], float>,
          IDistance<SparseVectorF, float[], float>,
          IDistance<double[], double[], double>,
          IVectorDistance<float, IVector<float>, float>,
          IVectorDistance<double, IVector<double>, double>
    {
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(float x, float y)
        {
            return Math.Abs(x - y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(double x, double y)
        {
            return Math.Abs(x - y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(float[] x, float[] y)
        {
            return Vectors.ManhattanDistance(x.Length, x, 0, y, 0);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(SparseVectorF x, float[] y)
        {
            return x.ManhattanDistance(y, 0);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(double[] x, double[] y)
        {
            return Vectors.ManhattanDistance(x.Length, x, 0, y, 0);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(IVector<float> x, float[] y, int offy)
        {
            return x.ManhattanDistance(y, offy);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(IVector<double> x, double[] y, int offy)
        {
            return x.ManhattanDistance(y, offy);
        }
    }
}
