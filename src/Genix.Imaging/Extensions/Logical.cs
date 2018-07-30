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

    /// <content>
    /// Provides logical extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Inverts all pixels in this <see cref="Image"/> not-in-place.
        /// </summary>
        /// <returns>
        /// A new inverted <see cref="Image"/>.
        /// </returns>
        public Image NOT()
        {
            Image dst = this.Clone(false);
            BitUtils64.WordsNOT(this.Bits.Length, this.Bits, 0, dst.Bits, 0);
            return dst;
        }

        /// <summary>
        /// Inverts all pixels in this <see cref="Image"/> in-place.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NOTIP() => BitUtils64.WordsNOT(this.Bits.Length, this.Bits, 0);

        /// <summary>
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public Image AND(Image op)
        {
            Image dst = this.Clone(true);
            dst.ANDIP(op);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void ANDIP(Image op)
        {
            if (op == null)
            {
                throw new ArgumentNullException(nameof(op));
            }

            if (this.BitsPerPixel != op.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int minheight = Maximum.Min(this.Height, op.Height);
            if (this.Stride == op.Stride && this.Width <= op.Width)
            {
                Arrays.AND(this.Stride * minheight, op.Bits, 0, this.Bits, 0);
            }
            else
            {
                int minwidth = Maximum.Min(this.Width, op.Width);
                int stridesrc = op.Stride1;
                int stridedst = this.Stride1;
                for (int iy = 0, possrc = 0, posdst = 0; iy < minheight; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils64.BitsAND(minwidth, op.Bits, possrc, this.Bits, posdst);
                }
            }
        }

        /// <summary>
        /// Performs logical OR operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public Image OR(Image op)
        {
            Image dst = this.Clone(true);
            dst.ORIP(op);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void ORIP(Image op)
        {
            if (op == null)
            {
                throw new ArgumentNullException(nameof(op));
            }

            if (this.BitsPerPixel != op.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int minheight = Maximum.Min(this.Height, op.Height);
            if (this.Stride == op.Stride && this.Width <= op.Width)
            {
                Arrays.OR(this.Stride * minheight, op.Bits, 0, this.Bits, 0);
            }
            else
            {
                int minwidth = Maximum.Min(this.Width, op.Width);
                int stridesrc = op.Stride1;
                int stridedst = this.Stride1;
                for (int iy = 0, possrc = 0, posdst = 0; iy < minheight; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils64.OR(minwidth, op.Bits, possrc, this.Bits, posdst);
                }
            }
        }

        /// <summary>
        /// Performs logical XOR operation on this <see cref="Image"/> and the specified <see cref="Image"/> not-in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <returns>
        /// The <see cref="Image"/> that receives the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public Image XOR(Image op)
        {
            Image dst = this.Clone(true);
            dst.XORIP(op);
            return dst;
        }

        /// <summary>
        /// Performs logical AND operation on this <see cref="Image"/> and the specified <see cref="Image"/> in-place.
        /// </summary>
        /// <param name="op">The right-side operand of this operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="op"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The images have a different depth.
        /// The <see cref="Image{T}.BitsPerPixel"/> properties of <paramref name="op"/> and this <see cref="Image"/> are not the same.
        /// </exception>
        public void XORIP(Image op)
        {
            if (op == null)
            {
                throw new ArgumentNullException(nameof(op));
            }

            if (this.BitsPerPixel != op.BitsPerPixel)
            {
                throw new ArgumentException(Properties.Resources.E_DepthNotTheSame);
            }

            int minheight = Maximum.Min(this.Height, op.Height);
            if (this.Stride == op.Stride && this.Width <= op.Width)
            {
                Arrays.XOR(this.Stride * minheight, op.Bits, 0, this.Bits, 0);
            }
            else
            {
                int minwidth = Maximum.Min(this.Width, op.Width);
                int stridesrc = op.Stride1;
                int stridedst = this.Stride1;
                for (int iy = 0, possrc = 0, posdst = 0; iy < minheight; iy++, possrc += stridesrc, posdst += stridedst)
                {
                    BitUtils64.XOR(minwidth, op.Bits, possrc, this.Bits, posdst);
                }
            }
        }
    }
}
