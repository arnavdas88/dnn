// -----------------------------------------------------------------------
// <copyright file="Morphology.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Drawing;
    using Genix.Core;

    /// <summary>
    /// Provides morphology extension methods for the <see cref="Image"/> class.
    /// </summary>
    public static class Morphology
    {
        /// <summary>
        /// Dilates an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to dilate.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The dilated <see cref="Image"/>.
        /// </returns>
        public static Image Dilate(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.DilateIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Dilates an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to dilate.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void DilateIP(this Image image, StructuringElement kernel, int iterations)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            // create mask
            ulong[] mask = new ulong[image.Bits.Length];
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.BuildORMask(image, kernel, null, mask, iteration > 0);

                // process image
                BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);
            }
        }

        /// <summary>
        /// Erodes an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to erode.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The eroded <see cref="Image"/>.
        /// </returns>
        public static Image Erode(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.ErodeIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Erodes an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to erode.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void ErodeIP(this Image image, StructuringElement kernel, int iterations)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }

            // create mask
            ulong[] mask = new ulong[image.Bits.Length];
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.BuildANDMask(image, kernel, null, mask, true);

                // process image
                BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);
            }
        }

        /// <summary>
        /// Perform morphological opening operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The opened <see cref="Image"/>.
        /// </returns>
        public static Image Open(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.OpenIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Perform morphological opening operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void OpenIP(this Image image, StructuringElement kernel, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.ErodeIP(image, kernel, 1);
                Morphology.DilateIP(image, kernel, 1);
            }
        }

        /// <summary>
        /// Perform morphological closing operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        /// <returns>
        /// The closed <see cref="Image"/>.
        /// </returns>
        public static Image Close(this Image image, StructuringElement kernel, int iterations)
        {
            Image dst = CopyCrop.Copy(image);
            dst.CloseIP(kernel, iterations);
            return dst;
        }

        /// <summary>
        /// Perform morphological closing operation an <see cref="Image"/> by using the specified structuring element.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to open.</param>
        /// <param name="kernel">The structuring element used for dilation.</param>
        /// <param name="iterations">The number of times dilation is applied.</param>
        public static void CloseIP(this Image image, StructuringElement kernel, int iterations)
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Morphology.DilateIP(image, kernel, 1);
                Morphology.ErodeIP(image, kernel, 1);
            }
        }

        /// <summary>
        /// Removes small isolated pixels from this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to cleaned.</param>
        /// <returns>
        /// The cleaned <see cref="Image"/>.
        /// </returns>
        public static Image Despeckle(this Image image)
        {
            Image dst = CopyCrop.Copy(image);
            dst.DespeckleIP();
            return dst;
        }

        /// <summary>
        /// Removes small isolated pixels from this <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to cleaned.</param>
        public static void DespeckleIP(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // create masks
            ulong[] mask = new ulong[image.Bits.Length];
            ulong[] notbits = new ulong[image.Bits.Length];
            BitUtils64.WordsNOT(image.Bits.Length, image.Bits, 0, notbits, 0);

            // remove isolated pixels
            Morphology.BuildORMask(image, StructuringElement.Square(3), null, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // 0 0 0
            // 0 x 0
            // x x x
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 1)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 1, new Point(1, -1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // x x x
            // 0 x 0
            // 0 0 0
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 0)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(3, 1, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // x 0 0
            // x x 0
            // x 0 0
            Morphology.BuildORMask(image, StructuringElement.Rectangle(2, 3, new Point(0, 1)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(1, 3, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // 0 0 x
            // 0 x x
            // 0 0 x
            Morphology.BuildORMask(image, StructuringElement.Rectangle(2, 3, new Point(1, 1)), null, mask, true);
            Morphology.BuildORMask(image, StructuringElement.Rectangle(1, 3, new Point(-1, 1)), notbits, mask, false);
            BitUtils64.WordsAND(mask.Length, mask, 0, image.Bits, 0);

            // fill isolated gaps
            Morphology.BuildANDMask(image, StructuringElement.Cross(3, 3), null, mask, true);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // x x x
            // x 0 x
            // 0 0 0
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 1)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 1, new Point(1, -1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // 0 0 0
            // x 0 x
            // x x x
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 2, new Point(1, 0)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(3, 1, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // 0 x x
            // 0 0 x
            // 0 x x
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(2, 3, new Point(0, 1)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(1, 3, new Point(1, 1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);

            // x x 0
            // x 0 0
            // x x 0
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(2, 3, new Point(1, 1)), null, mask, true);
            Morphology.BuildANDMask(image, StructuringElement.Rectangle(1, 3, new Point(-1, 1)), notbits, mask, false);
            BitUtils64.WordsOR(mask.Length, mask, 0, image.Bits, 0);
        }

        private static void BuildORMask(Image image, StructuringElement kernel, ulong[] bits, ulong[] mask, bool cleanMask)
        {
            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            if (cleanMask)
            {
                MKL.Set(mask.Length, 0, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point.X == 0)
                {
                    BitUtils64.WordsOR(
                        (height - Math.Abs(point.Y)) * stride,
                        bits,
                        Math.Max(point.Y, 0) * stride,
                        mask,
                        Math.Max(-point.Y, 0) * stride);
                }
                else
                {
                    int count = width - Math.Abs(point.X);
                    int offx = (Math.Max(point.Y, 0) * stride1) + Math.Max(point.X, 0);
                    int offy = (Math.Max(-point.Y, 0) * stride1) + Math.Max(-point.X, 0);
                    for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                    {
                        BitUtils64.BitsOR(count, bits, offx, mask, offy);
                    }
                }
            }
        }

        private static void BuildANDMask(Image image, StructuringElement kernel, ulong[] bits, ulong[] mask, bool cleanMask)
        {
            int width = image.Width;
            int height = image.Height;
            int stride1 = image.Stride1;
            int stride = image.Stride;

            if (cleanMask)
            {
                MKL.Set(mask.Length, ulong.MaxValue, mask, 0);
            }

            if (bits == null)
            {
                bits = image.Bits;
            }

            foreach (Point point in kernel.GetElements())
            {
                if (point.X == 0)
                {
                    BitUtils64.WordsAND(
                        (height - Math.Abs(point.Y)) * stride,
                        bits,
                        Math.Max(point.Y, 0) * stride,
                        mask,
                        Math.Max(-point.Y, 0) * stride);
                }
                else
                {
                    int count = width - Math.Abs(point.X);
                    int offx = (Math.Max(point.Y, 0) * stride1) + Math.Max(point.X, 0);
                    int offy = (Math.Max(-point.Y, 0) * stride1) + Math.Max(-point.X, 0);
                    for (int i = 0, ii = height - Math.Abs(point.Y); i < ii; i++, offx += stride1, offy += stride1)
                    {
                        BitUtils64.BitsAND(count, bits, offx, mask, offy);
                    }
                }
            }
        }
    }
}
