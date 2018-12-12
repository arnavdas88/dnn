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
        /// Converts the <see cref="Tensor"/> to a <see cref="Image"/>.
        /// </summary>
        /// <param name="tensor">The <see cref="Tensor"/> to convert.</param>
        /// <returns>
        /// The <see cref="Image"/> this method creates.
        /// </returns>
        public static Image ToImage(this Tensor tensor)
        {
            int w, h, c;
            int stridew, strideh, stridec;

            if (tensor.Rank == 2)
            {
                w = tensor.Axes[0];
                h = tensor.Axes[1];
                c = 1;

                stridew = tensor.Strides[0];
                strideh = tensor.Strides[1];
                stridec = 1;
            }
            else if (tensor.Rank == 4)
            {
                w = tensor.Shape.GetAxis(Axis.X);
                h = tensor.Shape.GetAxis(Axis.Y);
                c = tensor.Shape.GetAxis(Axis.C);

                stridew = tensor.Shape.GetStride(Axis.X);
                strideh = tensor.Shape.GetStride(Axis.Y);
                stridec = tensor.Shape.GetStride(Axis.C);
            }
            else
            {
                throw new NotSupportedException();
            }

            if (c != 1 && c != 3)
            {
                throw new ArgumentException("Only tensorts with one or three channels can be converted to image. To convert other tensors, slice them across C axis, and convert resulting tensors individually.");
            }

            tensor.MinMax(out float min, out float max);

            float[] weights = tensor.Weights;

            // create new bitmap
            Image image = new Image(w, h, c == 3 ? 24 : 8, 200, 200);
            int stride8 = image.Stride8;

            unsafe
            {
                fixed (ulong* bits = image.Bits)
                {
                    byte* bits8 = (byte*)bits;

                    for (int y = 0, posY = 0, offY = 0; y < h; y++, posY += strideh, offY += stride8)
                    {
                        for (int x = 0, posX = posY, offX = offY; x < w; x++, posX += stridew, offX += c)
                        {
                            for (int ic = 0, posC = posX, offC = offX; ic < c; ic++, posC += stridec, offC++)
                            {
                                float weight = weights[posC];
                                int color = ((weight - min) / (max - min) * 255).Round().Clip(0, 255);
                                bits8[offC] = (byte)(255 - color);
                            }
                        }
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to create a <see cref="Tensor"/> from.</param>
        /// <param name="name">The tensor name.</param>
        /// <param name="format">The format of the destination.</param>
        /// <param name="w">The destination tensor dimension along its x-axis.</param>
        /// <param name="h">The destination tensor dimension along its y-axis.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImage(Image image, string name, string format, int w, int h)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // calculate tensor width
            if (w == -1)
            {
                w = image.Width.MulDiv(h, image.Height);
            }

            Shape shape = new Shape(format, 1, w, h, image.BitsPerPixel > 8 ? 3 : 1);
            Tensor y = new Tensor(name, shape);

            FillTensor(y, 0, image);

            return y;
        }

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the collection of <see cref="Image"/> objects.
        /// </summary>
        /// <param name="images">The collection of <see cref="Image"/> objects to create a <see cref="Tensor"/> from.</param>
        /// <param name="name">The tensor name.</param>
        /// <param name="format">The format of the destination.</param>
        /// <param name="w">The destination tensor dimension along its x-axis.</param>
        /// <param name="h">The destination tensor dimension along its y-axis.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImages(IList<Image> images, string name, string format, int w, int h)
        {
            if (images == null)
            {
                throw new ArgumentNullException(nameof(images));
            }

            if (images.Count == 0)
            {
                throw new ArgumentException("Cannot create tensor. The collection of images is empty.");
            }

            // calculate tensor width
            if (w == -1)
            {
                for (int i = 0, ii = images.Count; i < ii; i++)
                {
                    Image image = images[i];
                    w = Math.Max(w, image.Width.MulDiv(h, image.Height));
                }
            }

            Shape shape = new Shape(format, images.Count, w, h, images[0].BitsPerPixel > 8 ? 3 : 1);
            Tensor y = new Tensor(name, shape);

            for (int i = 0, ii = images.Count; i < ii; i++)
            {
                FillTensor(y, i, images[i]);
            }

            return y;
        }

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the collection of <see cref="Image"/> objects.
        /// </summary>
        /// <typeparam name="T">The type of objects that contain the images.</typeparam>
        /// <param name="sources">The collection of <see cref="Image"/> objects to create a <see cref="Tensor"/> from.</param>
        /// <param name="selector">The selector that converts <typeparamref name="T"/> into <see cref="Image"/>.</param>
        /// <param name="name">The tensor name.</param>
        /// <param name="format">The format of the destination.</param>
        /// <param name="w">The destination tensor dimension along its x-axis.</param>
        /// <param name="h">The destination tensor dimension along its y-axis.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImages<T>(IList<T> sources, Func<T, Image> selector, string name, string format, int w, int h)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            if (sources.Count == 0)
            {
                throw new ArgumentException("Cannot create tensor. The collection of images is empty.");
            }

            // calculate tensor width
            if (w == -1)
            {
                for (int i = 0, ii = sources.Count; i < ii; i++)
                {
                    Image image = selector(sources[i]);
                    w = Math.Max(w, image.Width.MulDiv(h, image.Height));
                }
            }

            Tensor y = null;
            for (int i = 0, ii = sources.Count; i < ii; i++)
            {
                Image image = selector(sources[i]);
                if (i == 0)
                {
                    Shape shape = new Shape(format, sources.Count, w, h, image.BitsPerPixel > 8 ? 3 : 1);
                    y = new Tensor(name, shape);
                }

                FillTensor(y, i, image);
            }

            return y;
        }

        private static void FillTensor(Tensor dst, int b, Image image)
        {
            int w = dst.Shape.GetAxis(Axis.X);
            int h = dst.Shape.GetAxis(Axis.Y);
            int c = dst.Shape.GetAxis(Axis.C);
            int strideB = dst.Shape.GetStride(Axis.B);
            int strideW = dst.Shape.GetStride(Axis.X);
            int strideH = dst.Shape.GetStride(Axis.Y);
            int strideC = dst.Shape.GetStride(Axis.C);

            float[] dstw = dst.Weights;
            int posB = b * strideB;

            int bpp = image.BitsPerPixel;
            if ((bpp > 8 ? 3 : 1) != c)
            {
                throw new ArgumentException("Cannot create tensor. Images in the collection have different bit depth.");
            }

            if (image.Width != w || image.Height != h)
            {
                image = image.FitToSize(w, h, new ScalingOptions());
            }

            int stride = image.Stride;
            ulong[] bits = image.Bits;

            switch (bpp)
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
                case 32:
                    Convert24to32bpp();
                    break;

                default:
                    throw new NotSupportedException();
            }

            void Convert1bpp()
            {
                for (int y = 0, posY = posB, offY = 0; y < h; y++, posY += strideH, offY += stride)
                {
                    for (int x = 0, posX = posY, offX = offY; x < w; offX++)
                    {
                        ulong ubits = bits[offX];
                        if (ubits != 0)
                        {
                            for (ulong mask = 1; mask != 0 && x < w; x++, mask <<= 1, posX += strideW)
                            {
                                if ((ubits & mask) != 0)
                                {
                                    dstw[posX] = 1.0f;
                                }
                            }
                        }
                        else
                        {
                            x += 64;
                            posX += 64 * strideW;
                        }
                    }
                }
            }

            void Convert2to16bpp()
            {
                int pixelsPerSample = 64 / bpp;
                ulong maxcolor = image.MaxColor;

                for (int y = 0, posY = posB, offY = 0; y < h; y++, posY += strideH, offY += stride)
                {
                    for (int x = 0, posX = posY, offX = offY; x < w; offX++)
                    {
                        ulong ubits = ~bits[offX];
                        if (ubits != 0)
                        {
                            for (int shift = 0; shift < 64 && x < w; x++, shift += bpp, posX += strideW)
                            {
                                dstw[posX] = (ubits >> shift) & maxcolor;
                            }
                        }
                        else
                        {
                            x += pixelsPerSample;
                            posX += pixelsPerSample * strideW;
                        }
                    }
                }

                // normalize tensor to 1
                Mathematics.DivC(strideB, maxcolor, dstw, posB);
            }

            void Convert24to32bpp()
            {
                int xinc = bpp == 24 ? 3 : 4;

                unsafe
                {
                    fixed (ulong* ubits = bits)
                    {
                        byte* src = (byte*)ubits;
                        int stride8 = image.Stride8;

                        for (int y = 0, posY = posB, offY = 0; y < h; y++, posY += strideH, offY += stride8)
                        {
                            for (int x = 0, posX = posY, offX = offY; x < w; x++, posX += strideW, offX += xinc)
                            {
                                dstw[posX + (strideC * 0)] = src[offX + 0];
                                dstw[posX + (strideC * 1)] = src[offX + 1];
                                dstw[posX + (strideC * 2)] = src[offX + 2];
                            }
                        }
                    }
                }

                // normalize tensor to 1
                Mathematics.SubCRev(strideB, 255, dstw, posB);
                Mathematics.DivC(strideB, 255, dstw, posB);
            }
        }
    }
}
