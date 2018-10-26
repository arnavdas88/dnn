// -----------------------------------------------------------------------
// <copyright file="BitmapExtensions.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Genix.Core;
    using Genix.Drawing;

    /// <summary>
    /// Provides extension methods for the <see cref="Image"/> class that let you work with Windows <see cref="System.Drawing.Bitmap"/> class.
    /// </summary>
    public static class BitmapExtensions
    {
        private static readonly Dictionary<string, string> MetadataConversionTable = new Dictionary<string, string>()
        {
            // TIFF Metadata
            { "/ifd/{ushort=330}", "/ifd/thumb" },
            { "/ifd/{ushort=700}", "/ifd/xmp" },
            { "/ifd/{ushort=34665}", "/ifd/exif" },
            { "/ifd/{ushort=34853}", "/ifd/gps" },
            { "/ifd/exif/{ushort=40965}", "/ifd/exif/interop" },
            { "/ifd/{ushort=33723}", "/ifd/iptc" },

            // Jpeg Metadata
            { "/app0/{ushort=2}", "/ifd/{ushort=282}" },
            { "/app0/{ushort=3}", "/ifd/{ushort=283}" },
            { "/app1/ifd/exif", "/ifd/exif" },
            { "/app1/ifd", "/ifd" },
            { "/app1/{ushort=0}/{ushort=34665}", "/ifd/exif" },
            { "/app1/{ushort=0}", "/ifd" },
        };

        /// <summary>
        /// Creates an <see cref="Image"/> from an encapsulated GDI+ bitmap.
        /// </summary>
        /// <param name="bitmap">The GDI+ bitmap from which to create the <see cref="Image"/>.</param>
        /// <returns>
        /// The <see cref="Image"/> this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bitmap"/> is <b>null</b>.
        /// </exception>
        public static Image FromBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            Image image = new Image(
                bitmap.Width,
                bitmap.Height,
                BitmapExtensions.PixelFormatToBitsPerPixel(bitmap.PixelFormat),
                (int)(bitmap.HorizontalResolution + 0.5f),
                (int)(bitmap.VerticalResolution + 0.5f));

            BitmapData srcData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            unsafe
            {
                fixed (ulong* dst = image.Bits)
                {
                    Arrays.CopyStrides(
                        image.Height,
                        srcData.Scan0,
                        srcData.Stride,
                        new IntPtr(dst),
                        image.Stride8);
                }
            }

            if (image.BitsPerPixel < 8)
            {
                Vectors.SwapBits(image.Bits.Length, image.BitsPerPixel, image.Bits, 0);
            }

            bitmap.UnlockBits(srcData);

            return Image.OnLoaded(
                image,
                null,
                bitmap.Palette?.Entries?.Select(x => Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray());
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from a specified area of an encapsulated GDI+ bitmap.
        /// </summary>
        /// <param name="bitmap">The GDI+ bitmap from which to create the <see cref="Image"/>.</param>
        /// <param name="rect">Defines the portion of the <paramref name="bitmap"/> to copy. Coordinates are relative to <paramref name="bitmap"/>.</param>
        /// <returns>
        /// The <see cref="Image"/> this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bitmap"/> is <b>null</b>.
        /// </exception>
        public static Image FromBitmap(System.Drawing.Bitmap bitmap, Rectangle rect)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            using (System.Drawing.Bitmap clonedBitmap = bitmap.Clone(
                new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height),
                bitmap.PixelFormat))
            {
                return BitmapExtensions.FromBitmap(clonedBitmap);
            }
        }

        /// <summary>
        /// Converts this <see cref="Image"/> to a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> from which to create the GDI+ bitmap.</param>
        /// <returns>
        /// A <see cref="System.Drawing.Bitmap"/> that represents the converted <see cref="Image"/>.
        /// </returns>
        public static System.Drawing.Bitmap ToBitmap(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // select pixel format
            System.Drawing.Imaging.PixelFormat pixelFormat = BitmapExtensions.BitsPerPixelToPixelFormat(image.BitsPerPixel);

            // create new bitmap
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image.Width, image.Height, pixelFormat);
            try
            {
                bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                // copy bits
                BitmapData dstData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.WriteOnly,
                    pixelFormat);

                unsafe
                {
                    fixed (ulong* src = image.Bits)
                    {
                        Arrays.CopyStrides(
                            image.Height,
                            new IntPtr(src),
                            image.Stride8,
                            dstData.Scan0,
                            dstData.Stride);

                        if (image.BitsPerPixel < 8)
                        {
                            IntPtr dst = dstData.Scan0;
                            if (dstData.Stride < 0)
                            {
                                dst = IntPtr.Add(dst, (image.Height - 1) * dstData.Stride);
                            }

                            Vectors.SwapBits(
                                image.Height * Math.Abs(dstData.Stride) / sizeof(uint),
                                image.BitsPerPixel,
                                (uint*)dst.ToPointer());
                        }
                    }
                }

                bitmap.UnlockBits(dstData);

                // we need to set palette for binary and gray scale images
                // as our color map is different from default map used by System.Drawing.Bitmap
                if (image.BitsPerPixel <= 8 && bitmap.Palette != null)
                {
                    Color[] palette = Image.CreatePalette(image.BitsPerPixel);
                    ColorPalette dst = bitmap.Palette;
                    if (dst.Entries.Length == palette.Length)
                    {
                        for (int i = 0, ii = palette.Length; i < ii; i++)
                        {
                            dst.Entries[i] = System.Drawing.Color.FromArgb((int)palette[i].Argb);
                        }
                    }

                    // we need to reset the palette as the getter returns its clone
                    bitmap.Palette = dst;
                }

                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        internal static BitmapFrame ToBitmapFrame(this Image image)
        {
            // calculate pixel format
            System.Windows.Media.PixelFormat pixelFormat = PixelFormats.Indexed1;
            switch (image.BitsPerPixel)
            {
                case 1:
                    pixelFormat = PixelFormats.Indexed1;
                    break;
                case 2:
                    pixelFormat = PixelFormats.Indexed2;
                    break;
                case 4:
                    pixelFormat = PixelFormats.Indexed4;
                    break;
                case 8:
                    pixelFormat = PixelFormats.Indexed8;
                    break;
                case 32:
                    pixelFormat = PixelFormats.Bgr32;
                    break;

                default:
                    throw new NotImplementedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, image.BitsPerPixel));
            }

            // get palette
            BitmapPalette bitmapPalette = null;
            Color[] palette = Image.CreatePalette(image.BitsPerPixel);
            if (palette != null)
            {
                bitmapPalette = new BitmapPalette(palette.Select(x => System.Windows.Media.Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray());
            }

            WriteableBitmap bitmapSource = new WriteableBitmap(
                image.Width,
                image.Height,
                image.HorizontalResolution,
                image.VerticalResolution,
                pixelFormat,
                bitmapPalette);

            System.Windows.Int32Rect sourceRect = new System.Windows.Int32Rect(0, 0, image.Width, image.Height);
            if (image.BitsPerPixel < 8)
            {
                // swap bits to make storage big-endian
                ulong[] bits = new ulong[image.Bits.Length];
                Vectors.SwapBits(image.Bits.Length, image.Bits, 0, image.BitsPerPixel, bits, 0);
                bitmapSource.WritePixels(sourceRect, bits, image.Stride8, 0);
            }
            else
            {
                bitmapSource.WritePixels(sourceRect, image.Bits, image.Stride8, 0);
            }

            return BitmapFrame.Create(bitmapSource);
        }

        internal static (Image Image, ImageMetadata Metadata) FromBitmapFrame(this BitmapFrame bitmapFrame)
        {
            if (bitmapFrame == null)
            {
                throw new ArgumentNullException(nameof(bitmapFrame));
            }

            int? xres = null;
            int? yres = null;

            // load metadata
            ImageMetadata metadata = null;
            if (bitmapFrame.Metadata is BitmapMetadata bitmapMetadata)
            {
                metadata = BitmapExtensions.ReadBitmapMetadata(bitmapMetadata);

                // reset resolution to 200 dpi for images that do not have resolution tag
                if (bitmapFrame.Decoder is TiffBitmapDecoder)
                {
                    if (!metadata.HasPropertyItem((int)TIFFField.XResolution))
                    {
                        xres = 200;
                    }

                    if (!metadata.HasPropertyItem((int)TIFFField.YResolution))
                    {
                        yres = 200;
                    }
                }

                // remove standard tags
                metadata.RemovePropertyItems(x => x.Id == (int)TIFFField.ImageWidth ||
                                                       x.Id == (int)TIFFField.ImageLength ||
                                                       x.Id == (int)TIFFField.XResolution ||
                                                       x.Id == (int)TIFFField.YResolution ||
                                                       x.Id == (int)TIFFField.BitsPerSample);
            }

            // verify palette presence
            System.Windows.Media.PixelFormat pixelFormat = bitmapFrame.Format;
            if ((pixelFormat == PixelFormats.Indexed1 ||
                 pixelFormat == PixelFormats.Indexed2 ||
                 pixelFormat == PixelFormats.Indexed4 ||
                 pixelFormat == PixelFormats.Indexed8) &&
                bitmapFrame.Palette?.Colors == null)
            {
                throw new InvalidOperationException(Properties.Resources.E_UnsupportedImageFormat);
            }

            // load image
            Image image = new Image(
                bitmapFrame.PixelWidth,
                bitmapFrame.PixelHeight,
                pixelFormat.BitsPerPixel,
                xres.GetValueOrDefault((int)(bitmapFrame.DpiX + 0.5f)),
                yres.GetValueOrDefault((int)(bitmapFrame.DpiY + 0.5f)));

            int strideInBytes = (((image.Width * image.BitsPerPixel) + 31) & ~31) >> 3;
            uint[] bits = new uint[image.Height * strideInBytes / sizeof(uint)];
            bitmapFrame.CopyPixels(bits, strideInBytes, 0);

            unsafe
            {
                fixed (uint* src = bits)
                {
                    fixed (ulong* dst = image.Bits)
                    {
                        Arrays.CopyStrides(
                            image.Height,
                            new IntPtr(src),
                            strideInBytes,
                            new IntPtr(dst),
                            image.Stride8);
                    }
                }
            }

            if (image.BitsPerPixel < 8)
            {
                Vectors.SwapBits(image.Bits.Length, image.BitsPerPixel, image.Bits, 0);
            }

            // special case for BitmapFrame BlackWhite pixel format
            if (bitmapFrame.Format == PixelFormats.BlackWhite)
            {
                image.Not(image);
                metadata.RemovePropertyItem((int)TIFFField.PhotometricInterpretation);
            }

            return (
                Image.OnLoaded(
                    image,
                    metadata,
                    bitmapFrame.Palette?.Colors?.Select(x => Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray()),
                metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsGrayScalePalette(int bitsPerPixel, IList<Color> palette, out bool invertedPalette)
        {
            invertedPalette = false;

            // create a gray-scale palette to compare with
            Color[] testPalette = Image.CreatePalette(bitsPerPixel);
            if (testPalette == null)
            {
                return false;
            }

            // check palette sizes
            if (testPalette.Length != palette.Count())
            {
                return false;
            }

            // compare two palettes
            if (testPalette.SequenceEqual(palette, new ColorEqualityComparer()))
            {
                return true;
            }

            // compare two palettes in reverse order
            if (testPalette.SequenceEqual(palette.Reverse(), new ColorEqualityComparer()))
            {
                invertedPalette = true;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Image ApplyPalette(this Image image, IList<Color> palette)
        {
            Image dst = new Image(image.Width, image.Height, 32, image);

            int height = image.Height;
            int bitsPerPixel = image.BitsPerPixel;

            ulong[] srcbits = image.Bits;
            ulong[] dstbits = dst.Bits;
            int srcstride = image.Stride;
            int dststride = dst.Stride;

            int srcwidthbits = image.WidthBits;
            int srcwidthbits2 = image.WidthBits & ~1;
            int srcwidthbits64 = image.WidthBits & ~63;

            uint[] colormap = palette.Select(x => x.Argb).ToArray();

            for (int iy = 0, yoffsrc = 0, yoffdst = 0; iy < height; iy++, yoffsrc += srcstride, yoffdst += dststride)
            {
                int xoffsrc = yoffsrc;
                int xoffdst = yoffdst;
                int ix = 0;

                for (; ix < srcwidthbits64; ix += 64, xoffsrc++)
                {
                    ulong bits = srcbits[xoffsrc];
                    for (int i = 0; i < 64; i += 2 * bitsPerPixel, xoffdst++)
                    {
                        dstbits[xoffdst] =
                            (ulong)colormap[BitUtils.GetBits(bits, i, bitsPerPixel)] |
                            ((ulong)colormap[BitUtils.GetBits(bits, i + bitsPerPixel, bitsPerPixel)] << 32);
                    }
                }

                if (ix < srcwidthbits)
                {
                    ulong bits = srcbits[xoffsrc];

                    for (; ix < srcwidthbits2; ix += 2 * bitsPerPixel, xoffdst++)
                    {
                        dstbits[xoffdst] =
                            (ulong)colormap[BitUtils.GetBits(bits, ix & 63, bitsPerPixel)] |
                            ((ulong)colormap[BitUtils.GetBits(bits, (ix + bitsPerPixel) & 63, bitsPerPixel)] << 32);
                    }

                    if (ix < srcwidthbits)
                    {
                        dstbits[xoffdst] = (ulong)colormap[BitUtils.GetBits(bits, ix & 63, bitsPerPixel)];
                    }
                }
            }

            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int PixelFormatToBitsPerPixel(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return 1;

                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    return 4;

                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return 8;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return 24;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return 32;

                default:
                    throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Unknown pixel format: {0}.", pixelFormat));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.Drawing.Imaging.PixelFormat BitsPerPixelToPixelFormat(int bitsPerPixel)
        {
            switch (bitsPerPixel)
            {
                case 1:
                    return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;

                case 4:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;

                case 8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

                case 24:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;

                case 32:
                    return System.Drawing.Imaging.PixelFormat.Format32bppRgb;

                default:
                    throw new NotImplementedException(
                        string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_UnsupportedDepth, bitsPerPixel));
            }
        }

        /// <summary>
        /// Converts property items (pieces of metadata) extracted from the <see cref="BitmapFrame"/> into their internal representation.
        /// </summary>
        /// <param name="bitmapMetadata">The collection of property items (pieces of metadata) extracted from the <see cref="BitmapFrame"/>.</param>
        /// <returns>A collection of <see cref="PropertyItem"/> objects.</returns>
        /// <remarks>
        /// For description of Windows metadata format, see: http://msdn.microsoft.com/en-us/library/windows/desktop/ee719904(v=vs.85).aspx.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Do not allow metadata reading exceptions to pass to application layer.")]
        private static ImageMetadata ReadBitmapMetadata(BitmapMetadata bitmapMetadata)
        {
            List<(string, object)> metadata = new List<(string, object)>();
            ParseBitmapMetadata(bitmapMetadata, string.Empty);

            void ParseBitmapMetadata(BitmapMetadata parentMetadata, string parentQuery)
            {
                foreach (string name in parentMetadata)
                {
                    object query = null;
                    try
                    {
                        query = parentMetadata.GetQuery(name);
                    }
                    catch
                    {
                        // metadata read may throw exceptions on some images- ignore
                        continue;
                    }

                    BitmapMetadata subMetadata = query as BitmapMetadata;
                    if (query is BitmapMetadata childMetadata)
                    {
                        ParseBitmapMetadata(childMetadata, parentQuery + name);
                    }
                    else
                    {
                        metadata.Add((parentQuery + name, query));
                    }
                }
            }

            List<PropertyItem> items = new List<PropertyItem>();

            // convert extracted pieces of metadata
            List<(string, object)> unusedItems = new List<(string, object)>();
            foreach ((string key, object value) item in metadata)
            {
                string key = item.key;
                object value = item.value;

                // convert tags
                foreach (KeyValuePair<string, string> entry in BitmapExtensions.MetadataConversionTable)
                {
                    if (key.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        key = key.Replace(entry.Key, entry.Value);
                        break;
                    }
                }

                if (key.StartsWith("/ifd/", StringComparison.OrdinalIgnoreCase))
                {
                    Match match = Regex.Match(key.Substring(5), @"\d+");
                    if (match.Success)
                    {
                        if (int.TryParse(match.Value, out int propertyId))
                        {
                            items.Add(new PropertyItem(propertyId, value));
                            continue;
                        }
                    }
                }
                else if (key.StartsWith("/xmp/tiff:", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<TIFFField>(key.Substring(10), out TIFFField result))
                    {
                        items.Add(new PropertyItem((int)result, value));
                        continue;
                    }
                }
                else if (key.StartsWith("/xmp/exif:", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<EXIFField>(key.Substring(10), out EXIFField result))
                    {
                        items.Add(new PropertyItem((int)result, value));
                        continue;
                    }
                }

                unusedItems.Add(item);
            }

            // filter and sort the list - remove unused binary properties
            return new ImageMetadata(items.Distinct(new PropertyItemComparer()).OrderBy(x => x.Id));
        }

        /// <summary>
        /// Defines methods to support the comparison of property items for equality.
        /// </summary>
        private class PropertyItemComparer : IEqualityComparer<PropertyItem>
        {
            /// <summary>
            /// Determines whether the specified property items are equal.
            /// </summary>
            /// <param name="x">The first property item to compare.</param>
            /// <param name="y">The second property item to compare.</param>
            /// <returns><b>true</b> if the specified property items are equal; otherwise, <b>false</b>.</returns>
            public bool Equals(PropertyItem x, PropertyItem y)
            {
                return x.Id == y.Id;
            }

            /// <summary>
            /// Returns a hash code for the specified property item.
            /// </summary>
            /// <param name="propertyItem">The property item for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified property item.</returns>
            /// <exception cref="ArgumentNullException">
            /// The <paramref name="propertyItem"/> is <b>null</b>.
            /// </exception>
            public int GetHashCode(PropertyItem propertyItem)
            {
                return propertyItem.Id;
            }
        }

        /// <summary>
        /// Defines methods to support the comparison of colors for equality.
        /// </summary>
        private class ColorEqualityComparer : IEqualityComparer<Color>
        {
            /// <summary>
            /// Determines whether the specified colors are equal.
            /// </summary>
            /// <param name="x">The first color to compare.</param>
            /// <param name="y">The second color to compare.</param>
            /// <returns><b>true</b> if the specified colors are equal; otherwise, <b>false</b>.</returns>
            public bool Equals(Color x, Color y)
            {
                return x == y;
            }

            /// <summary>
            /// Returns a hash code for the specified color.
            /// </summary>
            /// <param name="color">The color for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified color.</returns>
            public int GetHashCode(Color color)
            {
                return color.GetHashCode();
            }
        }
    }
}
