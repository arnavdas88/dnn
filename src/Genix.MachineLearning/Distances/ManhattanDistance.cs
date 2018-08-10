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
          IVectorDistance<float, float>,
          IVectorDistance<double, double>
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
            return Math32f.ManhattanDistance(x.Length, x, 0, y, 0);
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
            return Mathematics.ManhattanDistance(x.Length, x, 0, y, 0);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(int length, float[] x, int offx, float[] y, int offy)
        {
            return Math32f.ManhattanDistance(length, x, offx, y, offy);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Distance(int length, double[] x, int offx, double[] y, int offy)
        {
            return Mathematics.ManhattanDistance(length, x, offx, y, offy);
        }
    }
}
