namespace Genix.Imaging.Test
{
    using System;

    internal static class ColorHelpers
    {
        public static uint MaxColor(uint color1, uint color2, int bitsPerPixel)
        {
            if (bitsPerPixel <= 16)
            {
                return Math.Max(color1, color2);
            }
            else
            {
                return Color.Max(Color.FromArgb(color1), Color.FromArgb(color2)).Argb;
            }
        }
        public static uint MinColor(uint color1, uint color2, int bitsPerPixel)
        {
            if (bitsPerPixel <= 16)
            {
                return Math.Min(color1, color2);
            }
            else
            {
                return Color.Min(Color.FromArgb(color1), Color.FromArgb(color2)).Argb;
            }
        }
    }
}
