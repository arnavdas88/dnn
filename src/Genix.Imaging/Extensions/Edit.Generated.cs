﻿
// -----------------------------------------------------------------------
// <copyright file="Edit.Generated.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 9/21/2018 4:41:23 PM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.Drawing;

    public partial class Image
    {

        /// <summary>
        /// Sets all image pixels to maximum value (2^bpp - 1) not-in-place.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels set to maximum value (2^bpp - 1).
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        /// <seealso cref="SetToMaxIP()"/>
        public Image SetToMax()
        {
            Image dst = this.Clone(false);
            Vectors.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);

            return dst;
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to maximum value (2^bpp - 1) not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to maximum value (2^bpp - 1).
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetToMaxIP(int, int, int, int)"/>
        public Image SetToMax(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                return this.SetToMax();
            }
            else
            {
                Image dst = this.Copy();
                dst.SetToMaxIP(x, y, width, height);
                return dst;
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to maximum value (2^bpp - 1) not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to maximum value (2^bpp - 1).
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetToMaxIP(Rectangle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetToMax(Rectangle rect) => this.SetToMax(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to maximum value (2^bpp - 1) in-place.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetToMaxIP() =>
            Vectors.Set(this.Bits.Length, ulong.MaxValue, this.Bits, 0);

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to maximum value (2^bpp - 1) in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        public void SetToMaxIP(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                this.SetToMaxIP();
            }
            else
            {
                this.ValidateArea(x, y, width, height);
                int stride1 = this.Stride1;
                int count = width * this.BitsPerPixel;
                ulong[] bits = this.Bits;

                for (int i = 0, off = (y * stride1) + (x * this.BitsPerPixel); i < height; i++, off += stride1)
                {
                    BitUtils.SetBits(count, bits, off);
                }
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to maximum value (2^bpp - 1) in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetToMaxIP(Rectangle rect) => this.SetToMaxIP(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to minimum value (zero) not-in-place.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels set to minimum value (zero).
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        /// <seealso cref="SetToMinIP()"/>
        public Image SetToMin()
        {
            Image dst = this.Clone(false);

            return dst;
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to minimum value (zero) not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to minimum value (zero).
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetToMinIP(int, int, int, int)"/>
        public Image SetToMin(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                return this.SetToMin();
            }
            else
            {
                Image dst = this.Copy();
                dst.SetToMinIP(x, y, width, height);
                return dst;
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to minimum value (zero) not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to minimum value (zero).
        /// </returns>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetToMinIP(Rectangle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetToMin(Rectangle rect) => this.SetToMin(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to minimum value (zero) in-place.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetToMinIP() =>
            Vectors.Set(this.Bits.Length, 0ul, this.Bits, 0);

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to minimum value (zero) in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        public void SetToMinIP(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                this.SetToMinIP();
            }
            else
            {
                this.ValidateArea(x, y, width, height);
                int stride1 = this.Stride1;
                int count = width * this.BitsPerPixel;
                ulong[] bits = this.Bits;

                for (int i = 0, off = (y * stride1) + (x * this.BitsPerPixel); i < height; i++, off += stride1)
                {
                    BitUtils.ResetBits(count, bits, off);
                }
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to minimum value (zero) in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetToMinIP(Rectangle rect) => this.SetToMinIP(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to white color not-in-place.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels set to white color.
        /// </returns>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <seealso cref="SetWhiteIP()"/>
        public Image SetWhite()
        {
            Image dst = this.Clone(false);
            if (dst.BitsPerPixel != 1)
            {
                Vectors.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to white color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to white color.
        /// </returns>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetWhiteIP(int, int, int, int)"/>
        public Image SetWhite(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                return this.SetWhite();
            }
            else
            {
                Image dst = this.Copy();
                dst.SetWhiteIP(x, y, width, height);
                return dst;
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to white color not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to white color.
        /// </returns>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetWhiteIP(Rectangle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetWhite(Rectangle rect) => this.SetWhite(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to white color in-place.
        /// </summary>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWhiteIP() =>
           Vectors.Set(this.Bits.Length, this.BitsPerPixel == 1 ? 0ul : ulong.MaxValue, this.Bits, 0);

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to white color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        public void SetWhiteIP(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                this.SetWhiteIP();
            }
            else
            {
                this.ValidateArea(x, y, width, height);
                int stride1 = this.Stride1;
                int count = width * this.BitsPerPixel;
                ulong[] bits = this.Bits;

                for (int i = 0, off = (y * stride1) + (x * this.BitsPerPixel); i < height; i++, off += stride1)
                {
                    if (this.BitsPerPixel == 1)
                    {
                        BitUtils.ResetBits(count, bits, off);
                    }
                    else
                    {
                        BitUtils.SetBits(count, bits, off);
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to white color in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWhiteIP(Rectangle rect) => this.SetWhiteIP(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to black color not-in-place.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels set to black color.
        /// </returns>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <seealso cref="SetBlackIP()"/>
        public Image SetBlack()
        {
            Image dst = this.Clone(false);
            if (dst.BitsPerPixel == 1)
            {
                Vectors.Set(dst.Bits.Length, ulong.MaxValue, dst.Bits, 0);
            }

            return dst;
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to black color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to black color.
        /// </returns>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetBlackIP(int, int, int, int)"/>
        public Image SetBlack(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                return this.SetBlack();
            }
            else
            {
                Image dst = this.Copy();
                dst.SetBlackIP(x, y, width, height);
                return dst;
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to black color not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels in the specified rectangular area set to black color.
        /// </returns>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetBlackIP(Rectangle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetBlack(Rectangle rect) => this.SetBlack(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels to black color in-place.
        /// </summary>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlackIP() =>
           Vectors.Set(this.Bits.Length, this.BitsPerPixel == 1 ? ulong.MaxValue : 0ul, this.Bits, 0);

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to black color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        public void SetBlackIP(int x, int y, int width, int height)
        {
            if (x == 0 && y == 0 && width == this.Width && height == this.Height)
            {
                this.SetBlackIP();
            }
            else
            {
                this.ValidateArea(x, y, width, height);
                int stride1 = this.Stride1;
                int count = width * this.BitsPerPixel;
                ulong[] bits = this.Bits;

                for (int i = 0, off = (y * stride1) + (x * this.BitsPerPixel); i < height; i++, off += stride1)
                {
                    if (this.BitsPerPixel == 1)
                    {
                        BitUtils.SetBits(count, bits, off);
                    }
                    else
                    {
                        BitUtils.ResetBits(count, bits, off);
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels in the specified rectangular area to black color in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlackIP(Rectangle rect) => this.SetBlackIP(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to white color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels outside the specified rectangular area set to white color.
        /// </returns>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetWhiteBorderIP(int, int, int, int)"/>
        public Image SetWhiteBorder(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            Image dst = this.Clone(false);
            Image.CopyArea(this, x, y, width, height, dst, x, y);

            if (this.BitsPerPixel > 1)
            {
                dst.SetWhiteBorderIP(x, y, width, height);
            }

            return dst;
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to white color not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels outside the specified rectangular area set to white color.
        /// </returns>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetWhiteBorderIP(Rectangle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetWhiteBorder(Rectangle rect) => this.SetWhiteBorder(rect.X, rect.Y, rect.Width, rect.Height);


        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to white color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        public void SetWhiteBorderIP(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            ulong[] bits = this.Bits;
            int stride1 = this.Stride1;
            ulong color = this.BitsPerPixel == 1 ? 0ul : ulong.MaxValue;

            // set top
            if (y > 0)
            {
                Vectors.Set(y * this.Stride, color, bits, 0);
            }

            // set left
            if (x > 0)
            {
                SetVerticalBits(x * this.BitsPerPixel, 0);
            }

            // set right
            if (x + width < this.Width)
            {
                SetVerticalBits((this.Width - (x + width)) * this.BitsPerPixel, (x + width) * this.BitsPerPixel);
            }

            // set bottom
            if (y + height < this.Height)
            {
                Vectors.Set((this.Height - (y + height)) * this.Stride, color, bits, (y + height) * this.Stride);
            }

            void SetVerticalBits(int count, int pos)
            {
                pos += y * stride1;
                if (this.BitsPerPixel == 1)
                {
                    for (int i = 0; i < height; i++, pos += stride1)
                    {
                        BitUtils.ResetBits(count, bits, pos);
                    }
                }
                else
                {
                    for (int i = 0; i < height; i++, pos += stride1)
                    {
                        BitUtils.SetBits(count, bits, pos);
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to white color in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// For binary images, the white color is 0; otherwise, the white color is (2^bpp - 1).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWhiteBorderIP(Rectangle rect) => this.SetWhiteBorderIP(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to black color not-in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels outside the specified rectangular area set to black color.
        /// </returns>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetBlackBorderIP(int, int, int, int)"/>
        public Image SetBlackBorder(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            Image dst = this.Clone(false);
            Image.CopyArea(this, x, y, width, height, dst, x, y);

            if (this.BitsPerPixel == 1)
            {
                dst.SetBlackBorderIP(x, y, width, height);
            }

            return dst;
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to black color not-in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <returns>
        /// A new <see cref="Image"/> that has all its pixels outside the specified rectangular area set to black color.
        /// </returns>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of image bounds.
        /// </exception>
        /// <seealso cref="SetBlackBorderIP(Rectangle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Image SetBlackBorder(Rectangle rect) => this.SetBlackBorder(rect.X, rect.Y, rect.Width, rect.Height);


        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to black color in-place.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/> and <paramref name="height"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        public void SetBlackBorderIP(int x, int y, int width, int height)
        {
            this.ValidateArea(x, y, width, height);

            ulong[] bits = this.Bits;
            int stride1 = this.Stride1;
            ulong color = this.BitsPerPixel == 1 ? ulong.MaxValue : 0ul;

            // set top
            if (y > 0)
            {
                Vectors.Set(y * this.Stride, color, bits, 0);
            }

            // set left
            if (x > 0)
            {
                SetVerticalBits(x * this.BitsPerPixel, 0);
            }

            // set right
            if (x + width < this.Width)
            {
                SetVerticalBits((this.Width - (x + width)) * this.BitsPerPixel, (x + width) * this.BitsPerPixel);
            }

            // set bottom
            if (y + height < this.Height)
            {
                Vectors.Set((this.Height - (y + height)) * this.Stride, color, bits, (y + height) * this.Stride);
            }

            void SetVerticalBits(int count, int pos)
            {
                pos += y * stride1;
                if (this.BitsPerPixel == 1)
                {
                    for (int i = 0; i < height; i++, pos += stride1)
                    {
                        BitUtils.SetBits(count, bits, pos);
                    }
                }
                else
                {
                    for (int i = 0; i < height; i++, pos += stride1)
                    {
                        BitUtils.ResetBits(count, bits, pos);
                    }
                }
            }
        }

        /// <summary>
        /// Sets all image pixels outside the specified rectangular area to black color in-place.
        /// </summary>
        /// <param name="rect">The width, height, and location of the area.</param>
        /// <remarks>
        /// For binary images, the black color is 1; otherwise, the black color is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The rectangular area described by <paramref name="rect"/> is outside of this <see cref="Image"/> bounds.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlackBorderIP(Rectangle rect) => this.SetBlackBorderIP(rect.X, rect.Y, rect.Width, rect.Height);
	}
}

