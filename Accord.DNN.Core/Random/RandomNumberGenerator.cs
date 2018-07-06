// -----------------------------------------------------------------------
// <copyright file="RandomNumberGenerator.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines a random numbers generator.
    /// </summary>
    public abstract class RandomNumberGenerator
    {
        /// <summary>
        /// Generates a random observation from the current distribution.
        /// </summary>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract float Generate();

        /// <summary>
        /// Generates a random vector of observations from the current distribution.
        /// </summary>
        /// <param name="samples">The number of samples to generate.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Generate(int samples)
        {
            return this.Generate(samples, new float[samples]);
        }

        /// <summary>
        /// Generates a random vector of observations from the current distribution.
        /// </summary>
        /// <param name="samples">The number of samples to generate.</param>
        /// <param name="result">The location where to store the samples.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Generate(int samples, float[] result)
        {
            if (result == null)
            {
                result = new float[samples];
            }

            for (int i = 0; i < samples; i++)
            {
                result[i] = this.Generate();
            }

            return result;
        }

        /// <summary>
        /// Generates a random observation that is within a specified range from the current distribution.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <returns>A random observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Generate(float minValue, float maxValue)
        {
            return (this.Generate() * (maxValue - minValue)) + minValue;
        }

        /// <summary>
        /// Generates a random vector of observations that are within a specified range from the current distribution.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <param name="samples">The number of samples to generate.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Generate(float minValue, float maxValue, int samples)
        {
            return this.Generate(minValue, maxValue, samples, new float[samples]);
        }

        /// <summary>
        /// Generates a random vector of observations that are within a specified range from the current distribution.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        /// <param name="samples">The number of samples to generate.</param>
        /// <param name="result">The location where to store the samples.</param>
        /// <returns>A random vector of observations drawn from this distribution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] Generate(float minValue, float maxValue, int samples, float[] result)
        {
            if (result == null)
            {
                result = new float[samples];
            }

            for (int i = 0; i < samples; i++)
            {
                result[i] = this.Generate(minValue, maxValue);
            }

            return result;
        }
    }
}
