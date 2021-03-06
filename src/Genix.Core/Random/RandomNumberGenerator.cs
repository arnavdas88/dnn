﻿// -----------------------------------------------------------------------
// <copyright file="RandomNumberGenerator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines a random numbers generator.
    /// </summary>
    /// <typeparam name="T">The type of numbers this generator creates.</typeparam>
    public abstract class RandomNumberGenerator<T>
    {
        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T Generate();

        /// <summary>
        /// Generates a random vector of observations from the current distribution.
        /// </summary>
        /// <param name="samples">The number of samples to generate.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Generate(int samples)
        {
            return this.Generate(samples, new T[samples]);
        }

        /// <summary>
        /// Generates a random vector of observations from the current distribution.
        /// </summary>
        /// <param name="samples">The number of samples to generate.</param>
        /// <param name="result">The location where to store the samples.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Generate(int samples, T[] result)
        {
            if (result == null)
            {
                result = new T[samples];
            }

            for (int i = 0; i < samples; i++)
            {
                result[i] = this.Generate();
            }

            return result;
        }
    }
}
