﻿// -----------------------------------------------------------------------
// <copyright file="NumericExtensions.tt" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 10/26/2018 12:37:04 PM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace System
{
    using System.Runtime.CompilerServices;
    using Genix.Win32;

    /// <summary>
    /// Provides extension methods for the numeric types.
    /// </summary>
    public static class NumericExtensions
    {

        /// <summary>
        /// Multiplies two 32-bit integers and then divides the 64-bit result by a third 32-bit value.
        /// </summary>
        /// <param name="value">The multiplicand.</param>
        /// <param name="numerator">The multiplier.</param>
        /// <param name="denominator">The number by which the result of the multiplication operation is to be divided.</param>
        /// <returns>
        /// <para>
        /// If the function succeeds, the return value is the result of the multiplication and division, rounded to the nearest integer.
        /// If the result is a positive half integer (ends in .5), it is rounded up.
        /// If the result is a negative half integer, it is rounded down.
        /// </para>
        /// <para>
        /// If either an overflow occurred or <c>denominator</c> was 0, the return value is -1.
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MulDiv(this int value, int numerator, int denominator)
        {
            return Win32NativeMethods.MulDiv(value, numerator, denominator);
        }

        /// <summary>
        /// Rounds the value up to a next specified multiple.
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="multiple">The multiple.</param>
        /// <returns>The rounded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundUp(this int value, int multiple)
        {
            return (value + multiple - 1) / multiple * multiple;
        }

        /// <summary>
        /// Determines whether this instance falls within the range of specified single-precision floating point numbers.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// <b>true</b> if this instance is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(this int value, int lowerBound, int upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }

        /// <summary>
        /// Clips this instance to a specified minimum and maximum value.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// A value that is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clip(this int value, int lowerBound, int upperBound)
        {
            if (value < lowerBound)
            {
                return lowerBound;
            }
            else if (value > upperBound)
            {
                return upperBound;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Determines whether this instance falls within the range of specified single-precision floating point numbers.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// <b>true</b> if this instance is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(this float value, float lowerBound, float upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }

        /// <summary>
        /// Clips this instance to a specified minimum and maximum value.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// A value that is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clip(this float value, float lowerBound, float upperBound)
        {
            if (value < lowerBound)
            {
                return lowerBound;
            }
            else if (value > upperBound)
            {
                return upperBound;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Determines whether this instance falls within the range of specified single-precision floating point numbers.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// <b>true</b> if this instance is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(this double value, double lowerBound, double upperBound)
        {
            return value >= lowerBound && value <= upperBound;
        }

        /// <summary>
        /// Clips this instance to a specified minimum and maximum value.
        /// </summary>
        /// <param name="value">This instance value.</param>
        /// <param name="lowerBound">The lower inclusive bound of the range.</param>
        /// <param name="upperBound">The upper inclusive bound of the range.</param>
        /// <returns>
        /// A value that is greater than or equal to <paramref name="lowerBound"/> and less than or equal to <paramref name="upperBound"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clip(this double value, double lowerBound, double upperBound)
        {
            if (value < lowerBound)
            {
                return lowerBound;
            }
            else if (value > upperBound)
            {
                return upperBound;
            }
            else
            {
                return value;
            }
        }
    }
}