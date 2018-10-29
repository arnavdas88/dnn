// -----------------------------------------------------------------------
// <copyright file="ImageExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Imaging
{
    using System;
    using System.Collections.Generic;
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
        /// <param name="xaxis">The destination tensor dimension along its x-axis.</param>
        /// <param name="yaxis">The destination tensor dimension along its y-axis.</param>
        /// <param name="name">The tensor name.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImage(Image image, int xaxis, int yaxis, string name)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            Tensor y = new Tensor(name, new[] { 1, xaxis, yaxis, image.BitsPerPixel > 8 ? 3 : 1 });
            FillTensor(y, 0, image);

            return y;
        }

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the collection of <see cref="Image"/> objects.
        /// </summary>
        /// <param name="images">The collection of <see cref="Image"/> objects to create a <see cref="Tensor"/> from.</param>
        /// <param name="xaxis">The destination tensor dimension along its x-axis.</param>
        /// <param name="yaxis">The destination tensor dimension along its y-axis.</param>
        /// <param name="name">The tensor name.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImages(IList<Image> images, int xaxis, int yaxis, string name)
        {
            if (images == null)
            {
                throw new ArgumentNullException(nameof(images));
            }

            if (images.Count == 0)
            {
                throw new ArgumentException("Cannot create tensor. The collection of images is empty.");
            }

            Tensor y = null;
            for (int i = 0, ii = images.Count; i < ii; i++)
            {
                Image image = images[i];
                if (i == 0)
                {
                    y = new Tensor(name, new[] { images.Count, xaxis, yaxis, image.BitsPerPixel > 8 ? 3 : 1 });
                }

                FillTensor(y, i, image);
            }

            return y;
        }

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the collection of <see cref="Image"/> objects.
        /// </summary>
        /// <typeparam name="T">The type of objects that contain the images.</typeparam>
        /// <param name="sources">The collection of <see cref="Image"/> objects to create a <see cref="Tensor"/> from.</param>
        /// <param name="selector">The selector that converts <typeparamref name="T"/> into <see cref="Image"/>.</param>
        /// <param name="xaxis">The destination tensor dimension along its x-axis.</param>
        /// <param name="yaxis">The destination tensor dimension along its y-axis.</param>
        /// <param name="name">The tensor name.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImages<T>(IList<T> sources, Func<T, Image> selector, int xaxis, int yaxis, string name)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (sources.Count == 0)
            {
                throw new ArgumentException("Cannot create tensor. The collection of images is empty.");
            }

            Tensor y = null;
            for (int i = 0, ii = sources.Count; i < ii; i++)
            {
                Image image = selector(sources[i]);
                if (i == 0)
                {
                    y = new Tensor(name, new[] { sources.Count, xaxis, yaxis, image.BitsPerPixel > 8 ? 3 : 1 });
                }

                FillTensor(y, i, image);
            }

            return y;
        }

        private static void FillTensor(Tensor y, int iy0, Image image)
        {
            int y1 = y.Axes[(int)Axis.X];
            int y2 = y.Axes[(int)Axis.Y];
            int y3 = y.Axes[(int)Axis.C];
            int ystride0 = y.Strides[(int)Axis.B];
            int ystride1 = y.Strides[(int)Axis.X];
            int ystride2 = y.Strides[(int)Axis.Y];
            float[] yw = y.Weights;
            int ypos0 = iy0 * ystride0;

            int bitsPerPixel = image.BitsPerPixel;
            if ((bitsPerPixel > 8 ? 3 : 1) != y3)
            {
                throw new ArgumentException("Cannot create tensor. Images in the collection have different bit depth.");
            }

            if (image.Width != y1 || image.Height != y2)
            {
                image = image.FitToSize(y1, y2, ScalingOptions.None);
            }

            int stride = image.Stride;
            ulong[] bits = image.Bits;

            switch (bitsPerPixel)
            {
                case 1:
                    Convert1bpp();
                    break;

                case 2:
                case 4:
                case 8:
                case 16:
                    Convert2to16bpp();
                    break;

                case 24:
                    Convert24bpp();
                    break;

                case 32:
                    Convert32bpp();
                    break;

                default:
                    throw new NotSupportedException();
            }

            void Convert1bpp()
            {
                for (int iy2 = 0, ypos2 = ypos0, xoff2 = 0; iy2 < y2; iy2++, xoff2 += stride, ypos2 += ystride2)
                {
                    for (int iy1 = 0, ypos1 = ypos2, xoff1 = xoff2; iy1 < y1; xoff1++)
                    {
                        ulong b = bits[xoff1];
                        if (b != 0)
                        {
                            for (ulong mask = 1; mask != 0 && iy1 < y1; iy1++, mask <<= 1, ypos1 += ystride1)
                            {
                                if ((b & mask) != 0)
                                {
                                    yw[ypos1] = 1.0f;
                                }
                            }
                        }
                        else
                        {
                            iy1 += 64;
                            ypos1 += 64 * ystride1;
                        }
                    }
                }
            }

            void Convert2to16bpp()
            {
                int pixelsPerSample = 64 / bitsPerPixel;
                ulong maxcolor = image.MaxColor;

                for (int iy2 = 0, ypos2 = ypos0, xoff2 = 0; iy2 < y2; iy2++, xoff2 += stride, ypos2 += ystride2)
                {
                    for (int iy1 = 0, ypos1 = ypos2, xoff1 = xoff2; iy1 < y1; xoff1++)
                    {
                        ulong b = ~bits[xoff1];
                        if (b != 0)
                        {
                            for (int shift = 0; shift < 64 && iy1 < y1; iy1++, shift += bitsPerPixel, ypos1 += ystride1)
                            {
                                yw[ypos1] = (b >> shift) & maxcolor;
                            }
                        }
                        else
                        {
                            iy1 += pixelsPerSample;
                            ypos1 += pixelsPerSample * ystride1;
                        }
                    }
                }

                // normalize tensor to 1
                Vectors.MulC(ystride0, 1.0f / maxcolor, y.Weights, ypos0);
            }

            void Convert24bpp()
            {
                unsafe
                {
                    fixed (ulong* ubits = bits)
                    {
                        fixed (float* wptr = &yw[ypos0])
                        {
                            byte* bitsptry = (byte*)ubits;
                            int bstride8 = image.Stride8;

                            float* wptry = wptr;

                            for (int iy2 = 0; iy2 < y2; iy2++, bitsptry += bstride8, wptry += ystride2)
                            {
                                byte* bitsptrx = bitsptry;
                                float* wptrx = wptry;

                                for (int iy1 = 0; iy1 < y1; iy1++, bitsptrx += 3, wptrx += ystride1)
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
                Vectors.SubCRev(ystride0, 255, y.Weights, ypos0);
                Vectors.MulC(ystride0, 1.0f / 255, y.Weights, ypos0);
            }

            void Convert32bpp()
            {
                for (int iy2 = 0, ypos2 = ypos0, xoff2 = 0; iy2 < y2; iy2++, xoff2 += stride, ypos2 += ystride2)
                {
                    for (int iy1 = 0, ypos1 = ypos2, xoff1 = xoff2; iy1 < y1; xoff1++)
                    {
                        ulong b = ~bits[xoff1];
                        if ((b & 0x00ff_ffff_00ff_fffful) != 0ul)
                        {
                            for (int shift = 0; shift < 64 && iy1 < y1; iy1++, shift += 32, ypos1 += ystride1)
                            {
                                ulong bvalue = b >> shift;

                                yw[ypos1 + 0] = (bvalue >> 0) & 0xff;
                                yw[ypos1 + 1] = (bvalue >> 8) & 0xff;
                                yw[ypos1 + 2] = (bvalue >> 16) & 0xff;
                            }
                        }
                        else
                        {
                            iy1 += 2;
                            ypos1 += 2 * ystride1;
                        }
                    }
                }

                // normalize tensor to 1
                Vectors.MulC(ystride0, 1.0f / 255, y.Weights, ypos0);
            }
        }
    }
}
