// -----------------------------------------------------------------------
// <copyright file="BitmapExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Imaging
{
    using System;

    /// <summary>
    /// Provides machine learning extensions for <see cref="System.Drawing.Bitmap"/> class.
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts the <see cref="Tensor"/> to a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="tensor">The <see cref="Tensor"/> to convert.</param>
        /// <returns>
        /// The <see cref="System.Drawing.Bitmap"/> this method creates.
        /// </returns>
        public static System.Drawing.Bitmap ToBitmap(this Tensor tensor)
        {
            int x1;
            int x2;
            int x3;

            int xstride1;
            int xstride2;

            if (tensor.Rank == 2)
            {
                x1 = tensor.Axes[0];
                x2 = tensor.Axes[1];
                x3 = 1;

                xstride1 = tensor.Strides[0];
                xstride2 = tensor.Strides[1];
            }
            else if (tensor.Rank == 4)
            {
                x1 = tensor.Shape.GetAxis(Axis.X);
                x2 = tensor.Shape.GetAxis(Axis.Y);
                x3 = tensor.Shape.GetAxis(Axis.C);

                xstride1 = tensor.Shape.GetStride(Axis.X);
                xstride2 = tensor.Shape.GetStride(Axis.Y);
            }
            else
            {
                throw new NotSupportedException();
            }

            tensor.MinMax(out float min, out float max);

            float[] xw = tensor.Weights;

            // create new bitmap
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(x1, x2, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            try
            {
                System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    bitmap.PixelFormat);
                int stride = data.Stride;

                unsafe
                {
                    byte* p = (byte*)data.Scan0;
                    for (int ix1 = 0, xpos1 = 0, yoff1 = 0; ix1 < x1; ix1++, xpos1 += xstride1, yoff1++)
                    {
                        for (int ix2 = 0, xpos2 = xpos1, yoff2 = yoff1; ix2 < x2; ix2++, xpos2 += xstride2, yoff2 += stride)
                        {
                            int color = ((xw[xpos2] - min) / (max - min) * 255).Round().Clip(0, 255);
                            p[yoff2] = (byte)(255 - color);
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
