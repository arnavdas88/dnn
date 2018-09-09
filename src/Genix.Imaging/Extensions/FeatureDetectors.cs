// -----------------------------------------------------------------------
// <copyright file="FeatureDetectors.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;

    /// <content>
    /// Provides feature detection methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Suppresses the lines on the <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="Image"/> with lines suppressed.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1 or 8.
        /// </exception>
        public Image SuppressLines()
        {
            if (this.BitsPerPixel != 1 && this.BitsPerPixel != 8)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, this.BitsPerPixel));
            }

            // IPP does not support 1bpp images - convert to 8bpp
            Image src;
            bool convert1bpp = false;
            if (this.BitsPerPixel == 1)
            {
                src = this.Convert1To8();
                convert1bpp = true;
            }
            else
            {
                src = this;
            }

            Image dst = src.Clone(false);

            if (NativeMethods.supresslines(
                src.Width,
                src.Height,
                src.Stride,
                src.Bits,
                dst.Bits) != 0)
            {
                throw new OutOfMemoryException();
            }

            // convert back to 1bpp
            if (convert1bpp)
            {
                dst = dst.Convert8To1(1);
            }

            return dst;
        }

        /// <summary>
        /// Calculates the histogram of oriented gradients (HOG) on the <see cref="Image"/>.
        /// </summary>
        /// <param name="cellSize">The cell size, in pixels.</param>
        /// <param name="blockSize">The block size, in number of <paramref name="cellSize"/>.</param>
        /// <param name="blockStride">The block stride size, in number of <paramref name="cellSize"/>.</param>
        /// <param name="numberOfBins">The number of bins (orientations) in the histogram.</param>
        /// <param name="threshold">
        /// The threshold value to apply after normalization.
        /// Bins that are less than the threshold, are set to zero.
        /// </param>
        /// <returns>
        /// The <see cref="DenseVectorPackF"/> that contains packed points of interest.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="Image{T}.BitsPerPixel"/> is not 1, 8, 24, or 32.
        /// </exception>
        public DenseVectorPackF HOG(
            int cellSize,
            int blockSize,
            int blockStride,
            int numberOfBins,
            float threshold)
        {
            // convert image to float
            ImageF srcf = PrepareImage();
            int width = srcf.Width;
            int height = srcf.Height;
            int stride = srcf.Stride;
            float[] bits = srcf.Bits;

            /*if (NativeMethods.hog(
                srcf.BitsPerPixel,
                srcf.Width,
                srcf.Height,
                srcf.Stride,
                srcf.Bits) != 0)
            {
                throw new OutOfMemoryException();
            }*/

            // calculate gradient vectors magnitude and direction using Prewitt operator (-1 0 1)
            float[] mag = new float[bits.Length];
            float[] ang = new float[bits.Length];
            NativeMethods.gradientVectorPrewitt_f32(width, height, bits, stride, mag, stride, ang, stride);

            // convert angles to bins
            Vectors.DivC(ang.Length, (float)(Math.PI / numberOfBins), ang, 0);
            Vectors.Abs(ang.Length, ang, 0);

            // calculate histograms
            int cellCountX = width / cellSize;
            int cellCountY = height / cellSize;

            int blockCountX = ComputeBlockCount(cellCountX);
            int blockCountY = ComputeBlockCount(cellCountY);
            int blockCount = blockCountX * blockCountY;
            int blockSizeInBins = blockSize * blockSize * numberOfBins;
            float[] blocks = new float[blockCount * blockSizeInBins];
            int offblock = 0;   // running block offset

            // to save memory we allocate histograms needed for one row of blocks only
            // when we move throw the rows, the first histogram (not needed anymore) becomes last (working)
            List<float[]> hist = CreateRotatingHist();

            for (int iy = 0, offy = 0; iy < cellCountY; iy++, offy += stride * cellSize)
            {
                float[] h = hist[Core.MinMax.Min(iy, blockSize - 1)];
                ComputeHistLine(offy, h);

                if (iy + 1 >= blockSize)
                {
                    // calculate blocks - we are at the last block line
                    if (((iy + 1 - blockSize) % blockStride) == 0)
                    {
                        ComputeBlockLine();
                    }

                    // rotate histograms
                    if (blockSize > 1 && iy + 1 < cellCountY)
                    {
                        RotateHist(hist);
                    }
                }
            }

            Debug.Assert(offblock == blocks.Length, "We must have processed all blocks by now.");

            // normalize blocks
            const float Eps = 1e-10f;
            for (int i = 0, off = 0; i < blockCount; i++, off += blockSizeInBins)
            {
                float norm = Vectors.L2Norm(blockSizeInBins, blocks, off);
                Vectors.DivC(blockSizeInBins, norm + Eps, blocks, off);
            }

            // apply threshold
            if (threshold > 0.0f)
            {
                Array32f.ThresholdLT(blocks.Length, threshold, 0.0f, blocks, 0);
            }

            /*DenseVectorProxyF[] dense = new DenseVectorProxyF[blockCount];
            for (int i = 0, off = 0; i < blockCount; i++, off += blockSizeInBins)
            {
                dense[i] = new DenseVectorProxyF(blockSizeInBins, blocks, off);
            }*/

            /*SparseVectorF[] sparse = new SparseVectorF[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                sparse[i] = SparseVectorF.FromDense(blockSizeInBins, vectors[i].X, 0);
            }*/

            return new DenseVectorPackF(blockCount, blockSizeInBins, blocks);

            ImageF PrepareImage()
            {
                // convert image to 8bpp, then float
                return this
                    .ConvertTo(8)
                    .Convert8To32f(
                        ComputeImageSize(this.Width),
                        ComputeImageSize(this.Height),
                        BorderType.BorderRepl,
                        0);

                int ComputeImageSize(int size)
                {
                    int kernelSize = cellSize * blockSize;
                    int kernelStride = cellSize * blockStride;
                    return Mathematics.RoundUp(Core.MinMax.Max(size - kernelSize, 0), kernelStride) + kernelSize;
                }
            }

            int ComputeBlockCount(int cellCount)
            {
                return (Core.MinMax.Max(cellCount - blockSize, 0) / blockStride) + 1;
            }

            List<float[]> CreateRotatingHist()
            {
                List<float[]> h = new List<float[]>(blockSize);
                for (int i = 0; i < blockSize; i++)
                {
                    h.Add(new float[cellCountX * numberOfBins]);
                }

                return h;
            }

            void RotateHist(List<float[]> h)
            {
                float[] temp = h[0];
                h.RemoveAt(0);
                h.Add(temp);
                Vectors.Set(temp.Length, 0, temp, 0);
            }

            void ComputeHistLine(int offy, float[] h)
            {
                for (int ix = 0, offx = offy, offh = 0; ix < cellCountX; ix++, offx += cellSize, offh += numberOfBins)
                {
                    for (int iyc = 0, offyc = offx; iyc < cellSize; iyc++, offyc += stride)
                    {
                        for (int ixc = 0, offxc = offyc; ixc < cellSize; ixc++, offxc++)
                        {
                            float value = mag[offxc];
                            if (value != 0.0f)
                            {
                                ////int bin = (int)ang[offxc] % numberOfBins;
                                ////h[offh + bin] += mag[offxc];

                                float angle = ang[offxc];

                                int bin = (int)angle;
                                float value1 = value * (angle - bin);

                                h[offh + (bin % numberOfBins)] += value1;
                                h[offh + ((bin + 1) % numberOfBins)] += value - value1;
                            }
                        }
                    }
                }
            }

            void ComputeBlockLine()
            {
                int blockLengthInBins = blockSize * numberOfBins;
                int blockStrideInBins = blockStride * numberOfBins;
                for (int ix = 0, offh = 0; ix < blockCountX; ix++, offh += blockStrideInBins)
                {
                    for (int iyc = 0; iyc < blockSize; iyc++)
                    {
                        Vectors.Copy(blockLengthInBins, hist[iyc], offh, blocks, offblock);
                        offblock += blockLengthInBins;
                    }
                }
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static extern int supresslines(
               int width,
               int height,
               int stride,
               [In] ulong[] src,
               [Out] ulong[] dst);

            [DllImport(NativeMethods.DllName)]
            public static extern int hog(
                int bitsPerPixel,
                int width,
                int height,
                int stride,
                [In] float[] src);

            [DllImport(NativeMethods.DllName)]
            public static extern int cart2polar(
                int n,
                [In] float[] re,
                [In] float[] im,
                [Out] float[] magnitude,
                [Out] float[] phase);

            [DllImport(NativeMethods.DllName)]
            public static extern int gradientVectorPrewitt_f32(
                int width,
                int height,
                [In] float[] src,
                int stride,
                [Out] float[] magnitude,
                int magnitudeStride,
                [Out] float[] angle,
                int angleStride);
        }
    }
}
