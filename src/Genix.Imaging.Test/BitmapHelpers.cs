
namespace Genix.Imaging.Test
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using Genix.Core;
    using Genix.Win32;

    internal static class BitmapHelpers
    {
        public static Bitmap CreateBitmap(int width, int height, int bitsPerPixel, bool whiteOnBlack)
        {
            PixelFormat pixelFormat;
            switch (bitsPerPixel)
            {
                case 1:
                    pixelFormat = PixelFormat.Format1bppIndexed;
                    break;

                case 4:
                    pixelFormat = PixelFormat.Format4bppIndexed;
                    break;

                case 8:
                    pixelFormat = PixelFormat.Format8bppIndexed;
                    break;

                case 24:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;

                case 32:
                    pixelFormat = PixelFormat.Format32bppRgb;
                    break;

                default:
                    throw new NotImplementedException();
            }

            Bitmap bitmap = new Bitmap(width, height, pixelFormat);
            try
            {
                // we need to set palette for binary and gray scale images
                // as our color map is different from default map used by System.Drawing.Bitmap
                if (bitmap.Palette != null)
                {
                    if (bitsPerPixel == 1 && whiteOnBlack)
                    {
                        ColorPalette palette = bitmap.Palette;
                        palette.Entries[0] = Color.Black;
                        palette.Entries[1] = Color.White;
                        bitmap.Palette = palette;
                    }
                    else if (bitsPerPixel <= 8)
                    {
                        Genix.Imaging.Color[] palette = Genix.Imaging.Image.CreatePalette(bitsPerPixel);
                        ColorPalette dst = bitmap.Palette;
                        if (dst.Entries.Length == palette.Length)
                        {
                            for (int i = 0, ii = palette.Length; i < ii; i++)
                            {
                                dst.Entries[i] = Color.FromArgb(palette[i].Argb);
                            }
                        }

                        // we need to reset the palette as the getter returns its clone
                        bitmap.Palette = dst;
                    }
                }

                if (bitsPerPixel != 1 || whiteOnBlack)
                {
                    BitmapData data = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.WriteOnly,
                        bitmap.PixelFormat);

                    Win32NativeMethods.memset(data.Scan0, 0xff, new IntPtr(data.Stride * data.Height));

                    bitmap.UnlockBits(data);
                }

                return bitmap;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }

        public static void SetPixel(Bitmap bitmap, int x, int y, bool whiteOnBlack)
        {
            int bitsPerPixel;
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    bitsPerPixel = 1;
                    break;

                case PixelFormat.Format4bppIndexed:
                    bitsPerPixel = 4;
                    break;

                case PixelFormat.Format8bppIndexed:
                    bitsPerPixel = 8;
                    break;

                case PixelFormat.Format24bppRgb:
                    bitsPerPixel = 24;
                    break;

                case PixelFormat.Format32bppRgb:
                    bitsPerPixel = 32;
                    break;

                default:
                    throw new NotImplementedException();
            }

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            unsafe
            {
                uint* bits = (uint*)data.Scan0;
                int xpos = x * bitsPerPixel;
                int pos = (y * data.Stride / 4) + (xpos >> 5);

                xpos &= 31;

                // convert to big-endian
                if (bitsPerPixel < 8)
                {
                    xpos = ((xpos / 8) * 8) + (8 - bitsPerPixel) - (xpos & 7);
                }

                if (bitsPerPixel == 1)
                {
                    if (whiteOnBlack)
                    {
                        bits[pos] = BitUtils.ResetBit(bits[pos], xpos);
                    }
                    else
                    {
                        bits[pos] = BitUtils.SetBit(bits[pos], xpos);
                    }
                }
                else if (bitsPerPixel == 24 && xpos + bitsPerPixel > 32)
                {
                    bits[pos] = BitUtils.CopyBits(bits[pos], xpos, 32 - xpos, 0);
                    bits[pos + 1] = BitUtils.CopyBits(bits[pos + 1], 0, xpos + bitsPerPixel - 32, 0);
                }
                else
                {
                    bits[pos] = BitUtils.CopyBits(bits[pos], xpos, bitsPerPixel, 0);
                }
            }

            bitmap.UnlockBits(data);
        }
    }
}
