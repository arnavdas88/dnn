// -----------------------------------------------------------------------
// <copyright file="Nonlinearity.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides nonlinearity calculation methods.
    /// </summary>
    public static class Nonlinearity
    {
        /// <summary>
        /// Computes a rectified linear unit nonlinearity element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReLU(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.relu(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a rectified linear unit tangent nonlinearity element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offdx">The index in the <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="y">The array that contains <see cref="ReLU"/> method results.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReLUGradient(int length, float[] dx, int offdx, bool cleardx, float[] y, int offy, float[] dy, int offdy)
        {
            NativeMethods.relu_gradient2(length, dx, offdx, cleardx, y, offy, dy, offdy);
        }

        /// <summary>
        /// Computes a sigmoid nonlinearity of the specified angle. S(x) = 1 / (1 + e^-x).
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The sigmoid of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sigmoid(float value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }

        /// <summary>
        /// Computes a derivative of a sigmoid nonlinearity of the specified angle.
        /// The method takes the result of <see cref="MKL.Sigmoid(Single)"/> method as an argument.
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The derivative of a sigmoid of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SigmoidDerivative2(float value)
        {
            return value * (1.0f - value);
        }

        /// <summary>
        /// Computes a sigmoid nonlinearity element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sigmoid(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods.sigmoid(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a gradient of sigmoid nonlinearity element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offdx">The index in the <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="y">The array that contains <see cref="Sigmoid"/> method results.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SigmoidGradient(int length, float[] dx, int offdx, bool cleardx, float[] y, int offy, float[] dy, int offdy)
        {
            NativeMethods.sigmoid_gradient2(length, dx, offdx, cleardx, y, offy, dy, offdy);
        }

        /// <summary>
        /// Computes a hyperbolic tangent nonlinearity element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="x">The array that contains data used for computation.</param>
        /// <param name="offx">The index in the <c>x</c> at which computation begins.</param>
        /// <param name="y">The array that receives the computed data.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tanh(int length, float[] x, int offx, float[] y, int offy)
        {
            NativeMethods._tanh(length, x, offx, y, offy);
        }

        /// <summary>
        /// Computes a hyperbolic tangent of the specified value.
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The hyperbolic tangent of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tanh(float value)
        {
            return (float)Math.Tanh(value);
        }

        /// <summary>
        /// Computes a derivative of a hyperbolic tangent of the specified value.
        /// The method takes the result of <see cref="MKL.Tanh(Single)"/> method as an argument.
        /// </summary>
        /// <param name="value">The value to compute.</param>
        /// <returns>
        /// The derivative of a hyperbolic tangent of <c>value</c>. 
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float TanhDerivative2(float value)
        {
            return 1.0f - (value * value);
        }

        /// <summary>
        /// Computes a gradient of hyperbolic tangent nonlinearity element wise on one array and puts results into another array.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="dx">The array that receives the computed gradient.</param>
        /// <param name="offdx">The index in the <c>dx</c> at which computation begins.</param>
        /// <param name="cleardx">Specifies whether the <c>dx</c> should be cleared before operation.</param>
        /// <param name="y">The array that contains <see cref="MKL.Tanh"/> method results.</param>
        /// <param name="offy">The index in the <c>y</c> at which computation begins.</param>
        /// <param name="dy">The array that contains chain gradient from next level.</param>
        /// <param name="offdy">The index in the <c>dy</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TanhGradient(int length, float[] dx, int offdx, bool cleardx, float[] y, int offy, float[] dy, int offdy)
        {
            NativeMethods.tanh_gradient2(length, dx, offdx, cleardx, y, offy, dy, offdy);
        }

        /// <summary>
        /// Computes a gradient of hyperbolic tangent nonlinearity element wise on an array in place.
        /// </summary>
        /// <param name="length">The number of elements to compute.</param>
        /// <param name="dxy">The array that contains chain gradient from next level receives the computed gradient.</param>
        /// <param name="offdxy">The index in the <c>dx</c> at which computation begins.</param>
        /// <param name="y">The array that contains <see cref="MKL.Tanh"/> method results.</param>
        /// <param name="offy">The index in the <c>x</c> at which computation begins.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TanhGradientIP(int length, float[] dxy, int offdxy, float[] y, int offy)
        {
            NativeMethods.tanh_gradient2_ip(length, dxy, offdxy, y, offy);
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void relu(
                int n,
                [In] float[] x,
                int offx,
                [Out] float[] y,
                int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void relu_gradient2(
                int n,
                [Out] float[] dx,
                int offdx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                int offy,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sigmoid(
                int n,
                [In] float[] x,
                int offx,
                [Out] float[] y,
                int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void sigmoid_gradient2(
                int n,
                [Out] float[] dx,
                int offdx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                int offy,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void _tanh(
                int n,
                [In] float[] x,
                int offx,
                [Out] float[] y,
                int offy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void tanh_gradient2(
                int n,
                [Out] float[] dx,
                int offdx,
                [MarshalAs(UnmanagedType.Bool)] bool cleardx,
                [In] float[] y,
                int offy,
                [In] float[] dy,
                int offdy);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void tanh_gradient2_ip(
                int n,
                [In, Out] float[] dxy,
                int offdxy,
                [In] float[] y,
                int offy);
        }
    }
}
