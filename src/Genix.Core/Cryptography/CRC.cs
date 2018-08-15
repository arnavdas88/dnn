// -----------------------------------------------------------------------
// <copyright file="CRC.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;

    /// <summary>
    /// Provides critical sum calculation methods.
    /// </summary>
    [CLSCompliant(false)]
    public static class CRC
    {
        /// <summary>
        /// Calculates a critical sum of the array of <see cref="byte"/> values.
        /// </summary>
        /// <param name="values">The array to calculate the critical sum of.</param>
        /// <returns>The calculated critical sum value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> is <b>null</b>.
        /// </exception>
        public static uint Calculate(byte[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            unsafe
            {
                fixed (byte* bytes = values)
                {
                    return CRC.Calculate(bytes, values.Length);
                }
            }
        }

        /// <summary>
        /// Calculates a critical sum of the array of <see cref="int"/> values.
        /// </summary>
        /// <param name="values">The array to calculate the critical sum of.</param>
        /// <returns>The calculated critical sum value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> is <b>null</b>.
        /// </exception>
        public static uint Calculate(int[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            unsafe
            {
                fixed (int* bytes = values)
                {
                    return Calculate((byte*)bytes, values.Length * sizeof(int));
                }
            }
        }

        /// <summary>
        /// Calculates a critical sum of the array of <see cref="ulong"/> values.
        /// </summary>
        /// <param name="values">The array to calculate the critical sum of.</param>
        /// <returns>The calculated critical sum value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> is <b>null</b>.
        /// </exception>
        public static uint Calculate(ulong[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            unsafe
            {
                fixed (ulong* bytes = values)
                {
                    return Calculate((byte*)bytes, values.Length * sizeof(ulong));
                }
            }
        }

        /// <summary>
        /// Calculates a critical sum of the array of <see cref="float"/> values.
        /// </summary>
        /// <param name="values">The array to calculate the critical sum of.</param>
        /// <returns>The calculated critical sum value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> is <b>null</b>.
        /// </exception>
        public static uint Calculate(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            unsafe
            {
                fixed (float* bytes = values)
                {
                    return Calculate((byte*)bytes, values.Length * sizeof(float));
                }
            }
        }

        /// <summary>
        /// Calculates a critical sum of the array of values of an unspecified type.
        /// </summary>
        /// <param name="values">The pointer to an unspecified type.</param>
        /// <param name="length">The number of bytes in array pointed by <paramref name="values"/>.</param>
        /// <returns>The calculated critical sum value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> is <b>null</b>.
        /// </exception>
        private static unsafe uint Calculate(void* values, int length)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (length == 0)
            {
                return 0;
            }

            byte* bytes = (byte*)values;
            uint hash = 3141592654;

            for (int i = 0, count = length / 4; i < count; i++, bytes += 4)
            {
                uint val = (uint)bytes[0] | ((uint)bytes[1]) << 8 | ((uint)bytes[2]) << 16 | ((uint)bytes[3]) << 24;

                hash = ((hash >> 3) ^ hash ^ (val >> 4)) & 0x0FFFFFFF | (hash << 28);
                uint cst = hash >> 24;
                hash = ((cst >> 3) ^ cst ^ val) & 0x0F | (hash << 4);
                if (hash == 0)
                {
                    hash = (uint)i;
                }
            }

            for (int i = 0, count = length % 4; i < count; i++)
            {
                uint val = bytes[i];

                uint cst = hash >> 20;
                hash = ((cst >> 3) ^ cst ^ val) & 0x0FF | (hash << 8);
                if (hash == 0)
                {
                    hash = (uint)i;
                }
            }

            return hash;
        }
    }
}
