// -----------------------------------------------------------------------
// <copyright file="Edit.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using Genix.Core;

    /// <content>
    /// Provides editing extension methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Gets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>
        /// The color of the specified pixel.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="x"/> is less than 0, or greater than or equal to <see cref="Image{T}.Width"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="y"/> is less than 0, or greater than or equal to <see cref="Image{T}.Height"/>.</para>
        /// </exception>
        [CLSCompliant(false)]
        public uint GetPixel(int x, int y)
        {
            this.ValidatePosition(x, y);

            int xpos = x * this.BitsPerPixel;
            int pos = (y * this.Stride) + (xpos >> 6);
            xpos &= 63;

            if (this.BitsPerPixel == 24 && xpos + 24 > 64)
            {
                int rem = 64 - xpos;
                return (uint)BitUtils.GetBits(this.Bits[pos], xpos, rem) |
                       (uint)(BitUtils.GetBits(this.Bits[pos + 1], 0, 24 - rem) << rem);
            }
            else
            {
                return (uint)BitUtils.GetBits(this.Bits[pos], xpos, this.BitsPerPixel);
            }
        }

        /// <summary>
        /// Sets the color of the specified pixel in this <see cref="Image"/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <param name="color">The color to assign to the specified pixel.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="x"/> is less than 0, or greater than or equal to <see cref="Image{T}.Width"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="y"/> is less than 0, or greater than or equal to <see cref="Image{T}.Height"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 1,
        /// <paramref name="color"/> > 0 sets the bit on.
        /// </para>
        /// <para>
        /// For <see cref="Image{T}.BitsPerPixel"/> == 2, 4, 8, and 16,
        /// <paramref name="color"/> > 0 is masked to the maximum allowable pixel value, and any(invalid) higher order bits are discarded.
        /// </para>
        /// </remarks>
        [CLSCompliant(false)]
        public void SetPixel(int x, int y, uint color)
        {
            this.ValidatePosition(x, y);

            int xpos = x * this.BitsPerPixel;
            int pos = (y * this.Stride) + (xpos >> 6);
            xpos &= 63;

            if (this.BitsPerPixel == 1)
            {
                if (color > 0)
                {
                    this.Bits[pos] = BitUtils.SetBit(this.Bits[pos], xpos);
                }
                else
                {
                    this.Bits[pos] = BitUtils.ResetBit(this.Bits[pos], xpos);
                }
            }
            else if (this.BitsPerPixel == 24 && xpos + 24 > 64)
            {
                int rem = 64 - xpos;
                this.Bits[pos] = BitUtils.CopyBits(this.Bits[pos], xpos, rem, color);
                this.Bits[pos + 1] = BitUtils.CopyBits(this.Bits[pos + 1], 0, 24 - rem, color >> rem);
            }
            else
            {
                this.Bits[pos] = BitUtils.CopyBits(this.Bits[pos], xpos, this.BitsPerPixel, color);
            }
        }
    }
}
