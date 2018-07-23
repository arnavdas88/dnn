// -----------------------------------------------------------------------
// <copyright file="Logical.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;

    /// <summary>
    /// Provides logical extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Logical
    {
         /// <summary>
        /// Performs logical AND operation on two images.
        /// </summary>
        /// <param name="a">The first <see cref="Image"/>.</param>
        /// <param name="b">The second <see cref="Image"/>.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><c>a</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>b</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image AND(this Image a, Image b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Width != b.Width ||
                a.Height != b.Height ||
                a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new NotSupportedException();
            }

            Image dst = new Image(a);
            BitUtils64.WordsAND(a.Bits.Length, a.Bits, 0, b.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on two images and puts the result into the first image.
        /// </summary>
        /// <param name="a">The first <see cref="Image"/>.</param>
        /// <param name="b">The second <see cref="Image"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><c>a</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>b</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AndIP(this Image a, Image b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Width != b.Width ||
                a.Height != b.Height ||
                a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new NotSupportedException();
            }

            BitUtils64.WordsAND(a.Bits.Length, b.Bits, 0, a.Bits, 0);
        }

        /// <summary>
        /// Performs logical OR operation on two images.
        /// </summary>
        /// <param name="a">The first <see cref="Image"/>.</param>
        /// <param name="b">The second <see cref="Image"/>.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><c>a</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>b</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image OR(this Image a, Image b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Width != b.Width ||
                a.Height != b.Height ||
                a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new NotSupportedException();
            }

            Image dst = new Image(a);
            BitUtils64.WordsOR(a.Bits.Length, a.Bits, 0, b.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Performs logical OR operation on two images and puts the result into the first image.
        /// </summary>
        /// <param name="a">The first <see cref="Image"/>.</param>
        /// <param name="b">The second <see cref="Image"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><c>a</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>b</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OrIP(this Image a, Image b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Width != b.Width ||
                a.Height != b.Height ||
                a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new NotSupportedException();
            }

            BitUtils64.WordsOR(a.Bits.Length, b.Bits, 0, a.Bits, 0);
        }

        /// <summary>
        /// Performs logical XOR operation on two images.
        /// </summary>
        /// <param name="a">The first <see cref="Image"/>.</param>
        /// <param name="b">The second <see cref="Image"/>.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><c>a</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>b</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image XOR(this Image a, Image b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Width != b.Width ||
                a.Height != b.Height ||
                a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new NotSupportedException();
            }

            Image dst = new Image(a);
            BitUtils64.WordsXOR(a.Bits.Length, a.Bits, 0, b.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Performs logical XOR operation on two images and puts the result into the first image.
        /// </summary>
        /// <param name="a">The first <see cref="Image"/>.</param>
        /// <param name="b">The second <see cref="Image"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><c>a</c> is <b>null</b></para>
        /// <para>-or-</para>
        /// <para><c>b</c> is <b>null</b></para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void XorIP(this Image a, Image b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Width != b.Width ||
                a.Height != b.Height ||
                a.BitsPerPixel != b.BitsPerPixel)
            {
                throw new NotSupportedException();
            }

            BitUtils64.WordsXOR(a.Bits.Length, b.Bits, 0, a.Bits, 0);
        }
    }
}
