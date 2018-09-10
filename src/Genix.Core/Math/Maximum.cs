// -----------------------------------------------------------------------
// <copyright file="Maximum.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides methods to calculate minimums and maximums.
    /// </summary>
    public static class Maximum
    {
        /// <summary>
        /// Computes the derivative of the arguments of the <see cref="Vectors.Min(int, float[], int, float[], int, float[], int)"/> and <see cref="Vectors.Max(int, float[], int, float[], int, float[], int)"/> methods.
        /// </summary>
        /// <param name="length">The number of elements to calculate.</param>
        /// <param name="x">One of the <see cref="Vectors.Min(int, float[], int, float[], int, float[], int)"/> or
        /// <see cref="Vectors.Max(int, float[], int, float[], int, float[], int)"/> methods input arrays <c>a</c> or <c>b</c>.</param>
        /// <param name="dx">The array that contains calculated gradient for <paramref name="x"/>.</param>
        /// <param name="offx">The index in the <paramref name="x"/> and <paramref name="dx"/> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <paramref name="dx"/> should be cleared before operation.</param>
        /// <param name="y">The <see cref="Vectors.Min(int, float[], int, float[], int, float[], int)"/> or
        /// <see cref="Vectors.Max(int, float[], int, float[], int, float[], int)"/> methods output array <paramref name="y"/>.</param>
        /// <param name="dy">The array that contains gradient for <paramref name="y"/>.</param>
        /// <param name="offy">The index in the <paramref name="y"/> and <paramref name="dy"/> at which calculation begins.</param>
        /// <remarks>
        /// The method performs operation defined as <c>dx(offx + i) += x(offx + i) == y(offy + i) ? dy(offy + i) : 0</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMaxGradient(int length, float[] x, float[] dx, int offx, bool cleardx, float[] y, float[] dy, int offy)
        {
            NativeMethods.minmax_gradient_f32(length, x, dx, offx, cleardx, y, dy, offy);
        }

        /// <summary>
        /// Calculates softmax probabilities for values of array in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source and destination array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SoftMax(int length, float[] x, int offx)
        {
            // compute max activation
            float amax = Vectors.Max(length, x, offx);

            // compute exponentials (carefully to not blow up)
            float esum = 0.0f;
            for (int i = 0; i < length; i++)
            {
                float e = (float)Math.Exp(x[offx + i] - amax);
                esum += e;
                x[offx + i] = e;
            }

            // normalize and output to sum to one
            if (esum != 0.0f)
            {
                Vectors.DivC(length, esum, x, offx);
            }
        }

        /// <summary>
        /// Calculates softmax probabilities for values of array not-in-place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The source array.</param>
        /// <param name="offx">The starting position in <paramref name="x"/>.</param>
        /// <param name="y">The destination array.</param>
        /// <param name="offy">The starting position in <paramref name="y"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SoftMax(int length, float[] x, int offx, float[] y, int offy)
        {
            // compute max activation
            float amax = Vectors.Max(length, x, offx);

            // compute exponentials (carefully to not blow up)
            float esum = 0.0f;
            for (int i = 0; i < length; i++)
            {
                float e = (float)Math.Exp(x[offx + i] - amax);
                esum += e;
                y[offy + i] = e;
            }

            // normalize and output to sum to one
            if (esum != 0.0f)
            {
                Vectors.DivC(length, esum, y, offy);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern void minmax_gradient_f32(
                int n,
                [In] float[] x,
                [Out] float[] dx,
                int offx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                [In] float[] dy,
                int offy);
        }
    }
}
