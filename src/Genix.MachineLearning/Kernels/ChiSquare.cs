// -----------------------------------------------------------------------
// <copyright file="ChiSquare.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Kernels
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Represents the Chi-Square kernel comes from the Chi-Square distribution.
    /// </summary>
    public class ChiSquare
        : IKernel, ISparseKernel
    {
        private const float Eps = 1e-10f;

        /// <inheritdoc />
        public float Execute(int length, float[] x, int offx, float[] y, int offy)
        {
            return NativeMethods.chisquare_f32(length, x, offx, y, offy);
            /*float sum = 0.0f;

            for (int i = 0; i < length; i++)
            {
                float xi = x[offx + i];
                float yi = y[offy + i];

                float num = xi - yi;
                sum += (num * num) / (xi + yi + ChiSquare.Eps);
            }

            return 1.0f - (2.0f * sum);*/
        }

        /// <inheritdoc />
        public float Execute(int[] xidx, float[] x, float[] y, int offy)
        {
            float sum = 0.0f;

            for (int i = 0, ii = xidx.Length; i < ii; i++)
            {
                float xi = x[i];
                float yi = y[offy + xidx[i]];

                float num = xi - yi;
                sum += (num * num) / (xi + yi + ChiSquare.Eps);
            }

            return 1.0f - (2.0f * sum);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            private const string DllName = "Genix.MachineLearning.Native.dll";

            [DllImport(NativeMethods.DllName)]
            public static extern float chisquare_f32(int n, [In] float[] x, int offx, [In] float[] y, int offy);

            [DllImport(NativeMethods.DllName)]
            public static extern float sparse_chisquare_f32(int n, [In] int[] xidx, [In] float[] x, [In] float y, int offy);
        }
    }
}
