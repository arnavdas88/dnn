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
        /// <param name="shape">The tensor shape.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImage(Image image, int xaxis, int yaxis, string name, TensorShape shape)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // calculate tensor width
            if (xaxis == -1)
            {
                xaxis = image.Width.MulDiv(yaxis, image.Height);
            }

            Tensor y = ImageExtensions.CreateTensor(image, 1, xaxis, yaxis, name, shape);
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
        /// <param name="shape">The tensor shape.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImages(IList<Image> images, int xaxis, int yaxis, string name, TensorShape shape)
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
            if (xaxis == -1)
            {
                for (int i = 0, ii = images.Count; i < ii; i++)
                {
                    Image image = images[i];
                    xaxis = Math.Max(xaxis, image.Width.MulDiv(yaxis, image.Height));
                }
            }

            Tensor y = ImageExtensions.CreateTensor(images[0], images.Count, xaxis, yaxis, name, shape);
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
        /// <param name="xaxis">The destination tensor dimension along its x-axis.</param>
        /// <param name="yaxis">The destination tensor dimension along its y-axis.</param>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromImages<T>(IList<T> sources, Func<T, Image> selector, int xaxis, int yaxis, string name, TensorShape shape)
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
            if (xaxis == -1)
            {
                for (int i = 0, ii = sources.Count; i < ii; i++)
                {
                    Image image = selector(sources[i]);
                    xaxis = Math.Max(xaxis, image.Width.MulDiv(yaxis, image.Height));
                }
            }

            Tensor y = null;
            for (int i = 0, ii = sources.Count; i < ii; i++)
            {
                Image image = selector(sources[i]);
                if (i == 0)
                {
                    y = ImageExtensions.CreateTensor(image, sources.Count, xaxis, yaxis, name, shape);
                }

                FillTensor(y, i, image);
            }

            return y;
        }

        private static Tensor CreateTensor(Image image, int b, int x, int y, string name, TensorShape shape)
        {
            int c = image.BitsPerPixel > 8 ? 3 : 1;
            switch (shape)
            {
                case TensorShape.BWHC:
                    return new Tensor(name, shape, new[] { b, x, y, c });

                case TensorShape.BHWC:
                    return new Tensor(name, shape, new[] { b, y, x, c });

                case TensorShape.BCHW:
                    return new Tensor(name, shape, new[] { b, c, y, x });

                default:
                    throw new NotSupportedException("The tensor shape is not supported by this operation.");
            }
        }

        private static void FillTensor(Tensor y, int b, Image image)
        {
            int yX = y.Axes[(int)Axis.X];
            int yY = y.Axes[(int)Axis.Y];
            int yC = y.Axes[(int)Axis.C];
            int ystrideB = y.Strides[(int)Axis.B];
            int ystrideX = y.Strides[(int)Axis.X];
            int ystrideY = y.Strides[(int)Axis.Y];
            int ystrideC = y.Strides[(int)Axis.C];

            float[] yw = y.Weights;
            int posB = b * ystrideB;

            int bpp = image.BitsPerPixel;
            if ((bpp > 8 ? 3 : 1) != yC)
            {
                throw new ArgumentException("Cannot create tensor. Images in the collection have different bit depth.");
            }

            if (image.Width != yX || image.Height != yY)
            {
                image = image.FitToSize(yX, yY, ScalingOptions.None);
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
                for (int iy = 0, posY = posB, offY = 0; iy < yY; iy++, posY += ystrideY, offY += stride)
                {
                    for (int ix = 0, posX = posY, offX = offY; ix < yX; offX++)
                    {
                        ulong ubits = bits[offX];
                        if (ubits != 0)
                        {
                            for (ulong mask = 1; mask != 0 && ix < yX; ix++, mask <<= 1, posX += ystrideX)
                            {
                                if ((ubits & mask) != 0)
                                {
                                    yw[posX] = 1.0f;
                                }
                            }
                        }
                        else
                        {
                            ix += 64;
                            posX += 64 * ystrideX;
                        }
                    }
                }
            }

            void Convert2to16bpp()
            {
                int pixelsPerSample = 64 / bpp;
                ulong maxcolor = image.MaxColor;

                for (int iy = 0, posY = posB, offY = 0; iy < yY; iy++, posY += ystrideY, offY += stride)
                {
                    for (int ix = 0, posX = posY, offX = offY; ix < yX; offX++)
                    {
                        ulong ubits = ~bits[offX];
                        if (ubits != 0)
                        {
                            for (int shift = 0; shift < 64 && ix < yX; ix++, shift += bpp, posX += ystrideX)
                            {
                                yw[posX] = (ubits >> shift) & maxcolor;
                            }
                        }
                        else
                        {
                            ix += pixelsPerSample;
                            posX += pixelsPerSample * ystrideX;
                        }
                    }
                }

                // normalize tensor to 1
                Vectors.DivC(ystrideB, maxcolor, y.Weights, posB);
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

                        for (int iy = 0, posY = posB, offY = 0; iy < yY; iy++, posY += ystrideY, offY += stride8)
                        {
                            for (int ix = 0, posX = posY, offX = offY; ix < yX; posX += ystrideX, offX += xinc)
                            {
                                yw[posX + (ystrideC * 0)] = src[offX + 0];
                                yw[posX + (ystrideC * 1)] = src[offX + 1];
                                yw[posX + (ystrideC * 2)] = src[offX + 2];
                            }
                        }
                    }
                }

                // normalize tensor to 1
                Vectors.SubCRev(ystrideB, 255, y.Weights, posB);
                Vectors.DivC(ystrideB, 255, y.Weights, posB);
            }
        }
    }
}
