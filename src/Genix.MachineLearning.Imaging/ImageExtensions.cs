﻿// -----------------------------------------------------------------------
// <copyright file="ImageExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Genix.Core;
    using Genix.Imaging;

    /// <summary>
    /// Provides machine learning extensions for <see cref="Image"/> class.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Creates a <see cref="Tensor"/> from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to create a <see cref="Tensor"/> from.</param>
        /// <param name="name">The tensor name.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor ToTensor(this Image image, string name)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            int bitsPerPixel = image.BitsPerPixel;
            int width = image.Width;
            int height = image.Height;

            switch (bitsPerPixel)
            {
                case 1:
                    return Convert1bpp();

                case 2:
                case 4:
                case 8:
                case 16:
                    return Convert2to16bpp();

                case 24:
                    return Convert24bpp();

                case 32:
                    return Convert32bpp();

                default:
                    throw new NotSupportedException();
            }

            Tensor Convert1bpp()
            {
                Tensor tensor = new Tensor(name, new[] { 1, width, height, 1 });

                int xstride = tensor.Strides[(int)Axis.X];
                int ystride = tensor.Strides[(int)Axis.Y];
                float[] w = tensor.Weights;

                ulong[] bits = image.Bits;
                int bstride = image.Stride;

                for (int y = 0, offy = 0, offby = 0; y < height; y++, offby += bstride, offy += ystride)
                {
                    for (int x = 0, offx = offy, offbx = offby; x < width; offbx++)
                    {
                        ulong b = bits[offbx];
                        if (b != 0)
                        {
                            for (ulong mask = 1; mask != 0 && x < width; x++, mask <<= 1, offx += xstride)
                            {
                                if ((b & mask) != 0)
                                {
                                    w[offx] = 1.0f;
                                }
                            }
                        }
                        else
                        {
                            x += 64;
                            offx += 64 * xstride;
                        }
                    }
                }

                return tensor;
            }

            Tensor Convert2to16bpp()
            {
                Tensor tensor = new Tensor(name, new[] { 1, width, height, 1 });

                int xstride = tensor.Strides[(int)Axis.X];
                int ystride = tensor.Strides[(int)Axis.Y];
                float[] w = tensor.Weights;

                ulong[] bits = image.Bits;
                int bstride = image.Stride;
                int pixelsPerSample = 64 / bitsPerPixel;
                ulong maxcolor = image.MaxColor;

                for (int y = 0, offy = 0, offby = 0; y < height; y++, offby += bstride, offy += ystride)
                {
                    for (int x = 0, offx = offy, offbx = offby; x < width; offbx++)
                    {
                        ulong b = ~bits[offbx];
                        if (b != 0)
                        {
                            for (int shift = 0; shift < 64 && x < width; x++, shift += bitsPerPixel, offx += xstride)
                            {
                                w[offx] = (b >> shift) & maxcolor;
                            }
                        }
                        else
                        {
                            x += pixelsPerSample;
                            offx += pixelsPerSample * xstride;
                        }
                    }
                }

                // normalize tensor to 1
                tensor.MulC(1.0f / maxcolor);

                return tensor;
            }

            Tensor Convert24bpp()
            {
                Tensor tensor = new Tensor(name, new[] { 1, width, height, 3 });

                int xstride = tensor.Strides[(int)Axis.X];
                int ystride = tensor.Strides[(int)Axis.Y];

                unsafe
                {
                    fixed (ulong* bits = image.Bits)
                    {
                        fixed (float* w = tensor.Weights)
                        {
                            byte* bitsptry = (byte*)bits;
                            int bstride8 = image.Stride8;

                            float* wptry = w;

                            for (int y = 0; y < height; y++, bitsptry += bstride8, wptry += ystride)
                            {
                                byte* bitsptrx = bitsptry;
                                float* wptrx = wptry;

                                for (int x = 0; x < width; x++, bitsptrx += 3, wptrx += xstride)
                                {
                                    wptrx[0] = bitsptrx[0];
                                    wptrx[1] = bitsptrx[1];
                                    wptrx[2] = bitsptrx[2];
                                }
                            }
                        }
                    }
                }

                // normalize tensor to 1
                tensor.SubCRev(255);
                tensor.MulC(1.0f / 255);

                return tensor;
            }

            Tensor Convert32bpp()
            {
                Tensor tensor = new Tensor(name, new[] { 1, width, height, 3 });

                int xstride = tensor.Strides[(int)Axis.X];
                int ystride = tensor.Strides[(int)Axis.Y];
                float[] w = tensor.Weights;

                ulong[] bits = image.Bits;
                int bstride = image.Stride;

                for (int y = 0, offy = 0, offby = 0; y < height; y++, offby += bstride, offy += ystride)
                {
                    for (int x = 0, offx = offy, offbx = offby; x < width; offbx++)
                    {
                        ulong b = ~bits[offbx];
                        if ((b & 0x00ff_ffff_00ff_fffful) != 0ul)
                        {
                            for (int shift = 0; shift < 64 && x < width; x++, shift += 32, offx += xstride)
                            {
                                ulong bvalue = b >> shift;

                                w[offx + 0] = (bvalue >> 0) & 0xff;
                                w[offx + 1] = (bvalue >> 8) & 0xff;
                                w[offx + 2] = (bvalue >> 16) & 0xff;
                            }
                        }
                        else
                        {
                            x += 2;
                            offx += 2 * xstride;
                        }
                    }
                }

                // normalize tensor to 1
                tensor.MulC(1.0f / 255);

                return tensor;
            }
        }

        /// <summary>
        /// Converts the <see cref="Tensor"/> to a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="tensor">The <see cref="Tensor"/> to convert.</param>
        /// <returns>
        /// The <see cref="System.Drawing.Bitmap"/> this method creates.
        /// </returns>
        public static System.Drawing.Bitmap ToBitmap(this Tensor tensor)
        {
            int width;
            int height;
            ////int depth;

            int xstride;
            int ystride;

            if (tensor.Axes.Length == 2)
            {
                width = tensor.Axes[0];
                height = tensor.Axes[1];
                ////depth = 1;

                xstride = tensor.Strides[0];
                ystride = tensor.Strides[1];
            }
            else if (tensor.Axes.Length == 4)
            {
                width = tensor.Axes[(int)Axis.X];
                height = tensor.Axes[(int)Axis.Y];
                ////depth = this.Axes[(int)Axis.C];

                xstride = tensor.Strides[(int)Axis.X];
                ystride = tensor.Strides[(int)Axis.Y];
            }
            else
            {
                throw new NotSupportedException();
            }

            tensor.MinMax(out float min, out float max);

            float[] w = tensor.Weights;

            // create new bitmap
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(
                width,
                height,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            try
            {
                System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    bitmap.PixelFormat);

                unsafe
                {
                    byte* p = (byte*)data.Scan0;
                    for (int ix = 0, offx = 0; ix < width; ix++, offx += xstride)
                    {
                        for (int iy = 0, offy = offx; iy < height; iy++, offy += ystride)
                        {
                            float weight = w[offy];
                            int color = (int)((weight - min) / (max - min) * 255);
                            color = MinMax.Max(color, 0);
                            color = MinMax.Min(color, 255);

                            p[(iy * data.Stride) + ix] = (byte)(255 - color);
                        }
                    }
                }

                bitmap.UnlockBits(data);
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }

            return bitmap;
        }
    }
}
