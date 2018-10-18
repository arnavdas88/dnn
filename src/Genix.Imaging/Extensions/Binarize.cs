// -----------------------------------------------------------------------
// <copyright file="Binarize.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using Genix.Core;
    ////using Leptonica;

    /// <content>
    /// Provides gray-to-binary conversion methods for the <see cref="Image"/> class.
    /// </content>
    public partial class Image
    {
        /// <summary>
        /// Converts this <see cref="Image"/> from gray scale to black-and-white.
        /// </summary>
        /// <param name="dst">The destination <see cref="Image"/>. Can be <b>null</b>.</param>
        /// <param name="sx">The tile width, in pixels. If 0, the method uses default value 16.</param>
        /// <param name="sy">The tile height, in pixels. If 0, the method uses default value 32.</param>
        /// <param name="smoothx">The half-width of tile convolution kernel width.</param>
        /// <param name="smoothy">The half-width of tile convolution kernel height.</param>
        /// <param name="normalizeBackground">Determines whether the image background should be normalized before binarization.</param>
        /// <param name="adaptiveThreshold">The threshold for determining foreground. If 0, the method uses default value 100.</param>
        /// <param name="mincount">The minimum number of foreground pixels in tile. If 0, the method uses default value (<paramref name="sx"/> * <paramref name="sy"/>) / 4.</param>
        /// <returns>
        /// The destination <see cref="Image"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <para>The depth of this <see cref="Image"/> is not 8 bits per pixel.</para>
        /// </exception>
        /// <remarks>
        /// <para>If <paramref name="dst"/> is <b>null</b> the method creates new destination <see cref="Image"/> with dimensions of this <see cref="Image"/>.</para>
        /// <para>If <paramref name="dst"/> equals this <see cref="Image"/>, the operation is performed in-place.</para>
        /// <para>Conversely, the <paramref name="dst"/> is reallocated to the dimensions of this <see cref="Image"/>.</para>
        /// </remarks>
        public Image Binarize(Image dst, int sx, int sy, int smoothx, int smoothy, bool normalizeBackground, byte adaptiveThreshold, int mincount)
        {
            if (this.BitsPerPixel != 8)
            {
                throw new ArgumentException(Properties.Resources.E_UnsupportedDepth_8bpp);
            }

            int width = this.Width;
            int height = this.Height;

            // initialize default variables
            if (sx == 0)
            {
                sx = 16;
            }

            if (sy == 0)
            {
                sy = 32;
            }

            sx = Math.Max(((sx + 7) / 8) * 8, 16); // horizontal tile size must be rounded to 8 pixels
            sy = Math.Max(sy, 16);

            if (mincount == 0)
            {
                mincount = (sx * sy) / 4;
            }

            // Calculate tile size
            int nx = Math.Max(1, width / sx);
            int ny = Math.Max(1, height / sy);

            // Allocate threshold map
            byte[] map = new byte[nx * ny];

            // Create destination image
            bool inplace = dst == this;
            dst = this.CreateTemplate(dst, 1);

            if (normalizeBackground)
            {
                // calculate adaptive threshold
                if (adaptiveThreshold == 0)
                {
                    // calculate the threshold
                    adaptiveThreshold = this.Otsu(0, 0, width, height);
                    adaptiveThreshold = (byte)(3 * adaptiveThreshold / 4);
                }

                // generate foreground mask
                // use destination as a temporary buffer
                this.Convert8To1(dst, adaptiveThreshold);
                dst.Dilate(dst, StructuringElement.Square(3), 1, BorderType.BorderConst, 0);

                // create mask that has all foreground pixels set to zero
                Image maskg = dst.Convert1To8(null);
                maskg.And(maskg, this);

                // calculate adaptive threshold map
                // dst currently holds background mask
                CalculateAdaptiveThresholds(dst, maskg);

                // apply thresholds
                // use gray mask as a temporary buffer for normalized image
                ApplyAdaptiveThresholds(this, maskg);

                //// !!!! TEMP
                maskg.Sub(maskg, maskg.MorphBlackHat(null, StructuringElement.Square(7), 1, BorderType.BorderRepl, 0), 0);
                //// !!!! TEMP

                // apply single otsu threshold to entire normalized image
                byte otsuThreshold = maskg.Otsu(0, 0, width, height);
                maskg.Convert8To1(dst, otsuThreshold);

                //// !!!! TEMP
                sx *= 2;
                sy *= 2;
                nx = Math.Max(1, width / sx);
                ny = Math.Max(1, height / sy);
                map = new byte[nx * ny];

                // calculate adaptive threshold map
                Image maskg2 = maskg & dst.Convert1To8(null);
                CalculateAdaptiveThresholds(dst, maskg2);

                // Apply the threshold
                Image temp1 = this.CreateTemplate(null, 1);
                unsafe
                {
                    fixed (ulong* bitsnorm = maskg.Bits, bitstemp1 = temp1.Bits)
                    {
                        for (int iy = 0, ty = 0, mapoff = 0; iy < ny; iy++, ty += sy, mapoff += nx)
                        {
                            int th = iy + 1 == ny ? height - ty : sy;
                            for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                            {
                                int tw = ix + 1 == nx ? width - tx : sx;

                                byte mapThreshold = map[mapoff + ix];
                                byte threshold;
                                if (mapThreshold <= otsuThreshold)
                                {
                                    threshold = otsuThreshold;
                                }
                                else
                                {
                                    threshold = (byte)((int)otsuThreshold + (2 * ((int)mapThreshold - (int)otsuThreshold) / 3));
                                }

                                NativeMethods._convert8to1(tx, ty, tw, th, (byte*)bitsnorm, maskg.Stride8, (byte*)bitstemp1, temp1.Stride8, threshold);
                            }
                        }
                    }
                }

                Image temp2 = dst.Dilate(null, StructuringElement.Square(7), 1, BorderType.BorderConst, 0);
                temp2.And(temp2, temp1);

                dst.FloodFill(dst, 8, temp2);

                /*                            Image temp1 = this.CreateTemplate(null, 1);
                                            fixed (ulong* bitstemp1 = temp1.Bits)
                                            {
                                                NativeMethods._convert8to1(0, 0, width, height, (byte*)bitsnorm, this.Stride8, (byte*)bitstemp1, temp1.Stride8, otsuThreshold + 50);
                                            }

                                            Image temp2 = dst.Dilate(null, StructuringElement.Square(5), 1, BorderType.BorderConst, 0);
                                            temp2.And(temp2, temp1);

                                            dst.Or(dst, temp2);*/

                //// !!!! TEMP

                void CalculateAdaptiveThresholds(Image bgmask, Image fgmask)
                {
                    for (int iy = 0, ty = 0, mapoff = 0; iy < ny; iy++, ty += sy, mapoff += nx)
                    {
                        int th = iy + 1 == ny ? height - ty : sy;
                        for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                        {
                            int tw = ix + 1 == nx ? width - tx : sx;
                            int count = (tw * th) - (int)bgmask.Power(tx, ty, tw, th);
                            if (count >= mincount)
                            {
                                int sum = (int)fgmask.Power(tx, ty, tw, th);
                                map[mapoff + ix] = (byte)(sum / count);
                            }
                            else
                            {
                                map[mapoff + ix] = 0;
                            }
                        }
                    }

                    // fill holes in map
                    // these are tiles that did not have enough foreground pixels to make an estimate
                    FillHoles();

                    // Optionally smooth the threshold map
                    SmoothMap();

                    void FillHoles()
                    {
                        bool needBackwardPass = false;
                        for (int iy = 0, mapoff = 0; iy < ny; iy++, mapoff += nx)
                        {
                            for (int ix = 0, prevoff = mapoff - nx; ix < nx; ix++)
                            {
                                if (map[mapoff + ix] == 0)
                                {
                                    int sum = 0;
                                    int div = 0;

                                    // left
                                    if (ix > 0)
                                    {
                                        byte val = map[mapoff + ix - 1];
                                        if (val != 0)
                                        {
                                            sum += val;
                                            div++;

                                            // right
                                            for (int iix = ix + 1; iix < nx; iix++)
                                            {
                                                if (map[mapoff + iix] != 0)
                                                {
                                                    int coeff = iix - ix;
                                                    sum += (coeff - 1) * val;
                                                    sum += map[mapoff + iix];
                                                    div += coeff;

                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    // top
                                    if (iy > 0)
                                    {
                                        byte val = map[prevoff + ix];
                                        if (val != 0)
                                        {
                                            sum += val;
                                            div++;

                                            // bottom
                                            for (int iiy = iy + 1, nextoff = mapoff + nx + ix; iiy < ny; iiy++, nextoff += nx)
                                            {
                                                if (map[nextoff] != 0)
                                                {
                                                    int coeff = iiy - iy;
                                                    sum += (coeff - 1) * val;
                                                    sum += map[nextoff];
                                                    div += coeff;

                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (div == 0)
                                    {
                                        needBackwardPass = true;
                                    }
                                    else
                                    {
                                        map[mapoff + ix] = (byte)(sum / div);
                                    }
                                }
                            }
                        }

                        if (needBackwardPass)
                        {
                            for (int iy = ny - 1, mapoff = iy * nx; iy >= 0; iy--, mapoff -= nx)
                            {
                                for (int ix = nx - 1, prevoff = mapoff + nx; ix >= 0; ix--)
                                {
                                    if (map[mapoff + ix] == 0)
                                    {
                                        int sum = 0;
                                        int div = 0;

                                        if (iy + 1 < ny)
                                        {
                                            byte val = map[prevoff + ix];
                                            if (val != 0)
                                            {
                                                sum += map[prevoff + ix];
                                                div++;
                                            }
                                        }

                                        if (ix + 1 < nx)
                                        {
                                            byte val = map[mapoff + ix + 1];
                                            if (val != 0)
                                            {
                                                sum += map[mapoff + ix + 1];
                                                div++;
                                            }
                                        }

                                        if (div != 0)
                                        {
                                            map[mapoff + ix] = (byte)(sum / div);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                void ApplyAdaptiveThresholds(Image source, Image destination)
                {
                    for (int iy = 0, ty = 0, mapoff = 0; iy < ny; iy++, ty += sy, mapoff += nx)
                    {
                        int th = iy + 1 == ny ? height - ty : sy;
                        for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                        {
                            int tw = ix + 1 == nx ? width - tx : sx;
                            destination.DivC(tx, ty, tw, th, source, map[mapoff + ix], -8);
                        }
                    }
                }
            }
            else
            {
                // Compute the thresholds for the tiles
                for (int iy = 0, ty = 0, mapoff = 0; iy < ny; iy++, ty += sy, mapoff += nx)
                {
                    int th = iy + 1 == ny ? height - ty : sy;
                    for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                    {
                        int tw = ix + 1 == nx ? width - tx : sx;
                        map[mapoff + ix] = this.Otsu(tx, ty, tw, th);
                    }
                }

                // Optionally smooth the threshold map
                SmoothMap();

                // Apply the threshold
                unsafe
                {
                    fixed (ulong* bitssrc = this.Bits, bitsdst = dst.Bits)
                    {
                        for (int iy = 0, ty = 0, mapoff = 0; iy < ny; iy++, ty += sy, mapoff += nx)
                        {
                            int th = iy + 1 == ny ? height - ty : sy;
                            for (int ix = 0, tx = 0; ix < nx; ix++, tx += sx)
                            {
                                int tw = ix + 1 == nx ? width - tx : sx;
                                NativeMethods._convert8to1(tx, ty, tw, th, (byte*)bitssrc, this.Stride8, (byte*)bitsdst, dst.Stride8, map[mapoff + ix]);
                            }
                        }
                    }
                }
            }

            void SmoothMap()
            {
                if (smoothx > 0 || smoothy > 0)
                {
                    // kernel too large; reducing!
                    if (nx < (2 * smoothx) + 1 || ny < (2 * smoothy) + 1)
                    {
                        smoothx = Math.Min(smoothx, (nx - 1) / 2);
                        smoothy = Math.Min(smoothy, (ny - 1) / 2);
                    }

                    if (smoothx > 0 || smoothy > 0)
                    {
                        unsafe
                        {
                            fixed (byte* ptr = map)
                            {
                                NativeMethods.filterBox(8, nx, ny, ptr, nx, ptr, nx, (2 * smoothx) + 1, (2 * smoothy) + 1, BorderType.BorderRepl, 0);
                            }
                        }
                    }
                }
            }

            if (inplace)
            {
                this.Attach(dst);
                return this;
            }

            return dst;

            /*try
            {
                using (Pix pixs = this.CreatePix())
                {
                    using (Pix pixd = pixs.pixOtsu(false))
                    {
                        return pixd.CreateImage(this.HorizontalResolution, this.VerticalResolution);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Cannot binarize the this.", e);
            }*/
        }

        private byte Otsu(int x, int y, int width, int height)
        {
            byte threshold;
            unsafe
            {
                // calculate the threshold
                fixed (ulong* bits = this.Bits)
                {
                    NativeMethods.otsu_threshold(x, y, width, height, (byte*)bits, this.Stride8, out threshold);
                }
            }

            return threshold;
        }

        [SuppressUnmanagedCodeSecurity]
        private static partial class NativeMethods
        {
            [DllImport(NativeMethods.DllName)]
            public static unsafe extern int otsu_threshold(
                int x,
                int y,
                int width,
                int height,
                byte* src,
                int stridesrc,
                out byte threshold);
        }
    }
}
