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
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Genix.Core;

    /// <summary>
    /// Provides extension methods for the <see cref="Image"/> class that let you work with Windows <see cref="Bitmap"/> class.
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
        /// <c>bitmap</c> is <b>null</b>.
        /// </exception>
        public static Image FromBitmap(Bitmap bitmap)
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
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            BitmapExtensions.CopyBits(
                image.Height,
                srcData.Scan0,
                Math.Abs(srcData.Stride) / sizeof(uint),
                image.Bits,
                image.Stride,
                srcData.Stride < 0);

            bitmap.UnlockBits(srcData);

            return Image.OnLoaded(image, null, bitmap.Palette?.Entries);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from a specified area of an encapsulated GDI+ bitmap.
        /// </summary>
        /// <param name="bitmap">The GDI+ bitmap from which to create the <see cref="Image"/>.</param>
        /// <param name="rect">Defines the portion of the <c>bitmap</c> to copy. Coordinates are relative to <c>bitmap</c>.</param>
        /// <returns>
        /// The <see cref="Image"/> this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>bitmap</c> is <b>null</b>.
        /// </exception>
        public static Image FromBitmap(Bitmap bitmap, Rectangle rect)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            using (Bitmap clonedBitmap = bitmap.Clone(rect, bitmap.PixelFormat))
            {
                return BitmapExtensions.FromBitmap(clonedBitmap);
            }
        }

        /// <summary>
        /// Converts this <see cref="Image"/> to a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> from which to create the GDI+ bitmap.</param>
        /// <returns>
        /// A <see cref="Bitmap"/> that represents the converted <see cref="Image"/>.
        /// </returns>
        public static Bitmap ToBitmap(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // select pixel format
            System.Drawing.Imaging.PixelFormat pixelFormat = BitmapExtensions.BitsPerPixelToPixelFormat(image.BitsPerPixel);

            // create new bitmap
            Bitmap bitmap = new Bitmap(image.Width, image.Height, pixelFormat);
            try
            {
                bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                // copy bits
                BitmapData dstData = bitmap.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.WriteOnly,
                    pixelFormat);

                BitmapExtensions.CopyBits(
                    image.Height,
                    image.Bits,
                    image.Stride,
                    dstData.Scan0,
                    Math.Abs(dstData.Stride) / sizeof(uint),
                    dstData.Stride < 0);

                bitmap.UnlockBits(dstData);

                // we need to set palette for binary and gray scale images
                // as our color map is different from default map used by System.Drawing.Bitmap
                if (image.BitsPerPixel <= 8 && bitmap.Palette != null)
                {
                    System.Drawing.Color[] palette = BitmapExtensions.CreatePalette(image.BitsPerPixel);
                    ColorPalette dst = bitmap.Palette;
                    if (dst.Entries.Length == palette.Length)
                    {
                        for (int i = 0, ii = palette.Length; i < ii; i++)
                        {
                            dst.Entries[i] = palette[i];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            System.Drawing.Color[] palette = BitmapExtensions.CreatePalette(image.BitsPerPixel);
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

            // swap bytes to make storage little-endian
            ulong[] bits = new ulong[image.Bits.Length];
            BitUtils64.BiteSwap(image.Bits.Length, image.Bits, 0, bits, 0);

            bitmapSource.WritePixels(
                new System.Windows.Int32Rect(0, 0, image.Width, image.Height),
                bits,
                image.Stride8,
                0);

            return BitmapFrame.Create(bitmapSource);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            int strideSrc = strideInBytes / sizeof(uint);
            int strideDst = image.Stride;
            int offsrc = 0;
            int offdst = 0;
            for (int i = 0, ii = image.Height; i < ii; i++)
            {
                BitUtils64.Copy32To64(strideSrc, bits, offsrc, image.Bits, offdst, true);
                offsrc += strideSrc;
                offdst += strideDst;
            }

            // special case for BitmapFrame BlackWhite pixel format
            if (bitmapFrame.Format == PixelFormats.BlackWhite)
            {
                image.InvertIP();
                metadata.RemovePropertyItem((int)TIFFField.PhotometricInterpretation);
            }

            return (
                Image.OnLoaded(
                    image,
                    metadata,
                    bitmapFrame.Palette?.Colors?.Select(x => System.Drawing.Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray()),
                metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsGrayScalePalette(int bitsPerPixel, IList<System.Drawing.Color> palette, out bool invertedPalette)
        {
            invertedPalette = false;

            // create a gray-scale palette to compare with
            System.Drawing.Color[] testPalette = BitmapExtensions.CreatePalette(bitsPerPixel);
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
        internal static Image ApplyPalette(this Image image, IList<System.Drawing.Color> palette)
        {
            Image dst = new Image(
                image.Width,
                image.Height,
                32,
                image.HorizontalResolution,
                image.VerticalResolution);

            int height = image.Height;
            int bitsPerPixel = image.BitsPerPixel;

            ulong[] srcbits = image.Bits;
            ulong[] dstbits = dst.Bits;
            int srcstride = image.Stride;
            int dststride = dst.Stride;

            int srcwidth32 = image.WidthBits / srcstride;
            int srctail = image.WidthBits - (srcwidth32 * 32);

            uint[] colormap = palette
                .Select(x => ((uint)x.R << 24) | ((uint)x.G << 16) | ((uint)x.B << 8) | ((uint)x.A << 0))
                .ToArray();

            for (int row = 0, offsrc = 0, offdst = 0; row < height; row++, offsrc = srcstride, offdst = dststride)
            {
                int offs = offsrc;
                int offd = offdst;

                for (int col = 0; col < srcwidth32; col++, offs++)
                {
                    for (int i = 0; i < 32; i += bitsPerPixel, offd++)
                    {
                        dstbits[offd] = colormap[BitUtils64.GetBits(srcbits[offs], i, bitsPerPixel)];
                    }
                }

                for (int i = 0; i < srctail; i += bitsPerPixel, offd++)
                {
                    dstbits[offd] = colormap[BitUtils64.GetBits(srcbits[offs], i, bitsPerPixel)];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.Drawing.Color[] CreatePalette(int bitsPerPixel)
        {
            switch (bitsPerPixel)
            {
                case 1:
                    return new System.Drawing.Color[2]
                    {
                        System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff),
                        System.Drawing.Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                    };

                case 2:
                    return new System.Drawing.Color[4]
                    {
                        System.Drawing.Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                        System.Drawing.Color.FromArgb(0xff, 0x55, 0x55, 0x55),
                        System.Drawing.Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa),
                        System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff),
                    };

                case 4:
                    return new System.Drawing.Color[16]
                    {
                        System.Drawing.Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                        System.Drawing.Color.FromArgb(0xff, 0x14, 0x14, 0x14),
                        System.Drawing.Color.FromArgb(0xff, 0x20, 0x20, 0x20),
                        System.Drawing.Color.FromArgb(0xff, 0x2c, 0x2c, 0x2c),
                        System.Drawing.Color.FromArgb(0xff, 0x38, 0x38, 0x38),
                        System.Drawing.Color.FromArgb(0xff, 0x45, 0x45, 0x45),
                        System.Drawing.Color.FromArgb(0xff, 0x51, 0x51, 0x51),
                        System.Drawing.Color.FromArgb(0xff, 0x61, 0x61, 0x61),
                        System.Drawing.Color.FromArgb(0xff, 0x71, 0x71, 0x71),
                        System.Drawing.Color.FromArgb(0xff, 0x82, 0x82, 0x82),
                        System.Drawing.Color.FromArgb(0xff, 0x92, 0x92, 0x92),
                        System.Drawing.Color.FromArgb(0xff, 0xa2, 0xa2, 0xa2),
                        System.Drawing.Color.FromArgb(0xff, 0xb6, 0xb6, 0xb6),
                        System.Drawing.Color.FromArgb(0xff, 0xcb, 0xcb, 0xcb),
                        System.Drawing.Color.FromArgb(0xff, 0xe3, 0xe3, 0xe3),
                        System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff),
                    };

                case 8:
                    System.Drawing.Color[] colors = new System.Drawing.Color[256];
                    for (int i = 0; i < 256; i++)
                    {
                        byte c = (byte)i;
                        colors[i] = System.Drawing.Color.FromArgb(0xff, c, c, c);
                    }

                    return colors;

                default:
                    return null;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, ulong[] src, int strideSrc, IntPtr dst, int strideDst, bool isUpsideDown)
        {
            unsafe
            {
                uint* udst = (uint*)dst;

                if (isUpsideDown)
                {
                    int offsrc = (height - 1) * strideSrc;
                    int offdst = (height - 1) * strideDst;
                    for (int i = 0; i < height; i++)
                    {
                        NativeMethods.bits_copy_be64to32(strideDst, src, offsrc, udst, offdst, true);
                        offsrc -= strideSrc;
                        offdst -= strideDst;
                    }
                }
                else if (2 * strideDst == strideSrc)
                {
                    NativeMethods.bits_copy_be64to32(height * strideDst, src, 0, udst, 0, true);
                }
                else
                {
                    int offsrc = 0;
                    int offdst = 0;
                    for (int i = 0; i < height; i++)
                    {
                        NativeMethods.bits_copy_be64to32(strideDst, src, offsrc, udst, offdst, true);
                        offsrc += strideSrc;
                        offdst += strideDst;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyBits(int height, IntPtr src, int strideSrc, ulong[] dst, int strideDst, bool isUpsideDown)
        {
            unsafe
            {
                uint* usrc = (uint*)src;

                if (isUpsideDown)
                {
                    int offsrc = (height - 1) * strideSrc;
                    int offdst = (height - 1) * strideDst;
                    for (int i = 0; i < height; i++)
                    {
                        NativeMethods.bits_copy_be32to64(strideSrc, usrc, offsrc, dst, offdst, true);
                        offsrc -= strideSrc;
                        offdst -= strideDst;
                    }
                }
                else if (strideDst == 2 * strideSrc)
                {
                    NativeMethods.bits_copy_be32to64(height * strideSrc, usrc, 0, dst, 0, true);
                }
                else
                {
                    int offsrc = 0;
                    int offdst = 0;
                    for (int i = 0; i < height; i++)
                    {
                        NativeMethods.bits_copy_be32to64(strideSrc, usrc, offsrc, dst, offdst, true);
                        offsrc += strideSrc;
                        offdst += strideDst;
                    }
                }
            }
        }

        private static class NativeMethods
        {
            private const string DllName = "Genix.Core.Native.dll";

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void bits_copy_be64to32(int count, [In] ulong[] src, int offsrc, [Out] uint* dst, int offdst, [MarshalAs(UnmanagedType.Bool)] bool swapBytes);

            [DllImport(NativeMethods.DllName)]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void bits_copy_be32to64(int count, [In] uint* src, int offsrc, [Out] ulong[] dst, int offdst, [MarshalAs(UnmanagedType.Bool)] bool swapBytes);
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
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "This is a private method.")]
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
            /// The <c>propertyItem</c> is <b>null</b>.
            /// </exception>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "This is a private method.")]
            public int GetHashCode(PropertyItem propertyItem)
            {
                return propertyItem.Id;
            }
        }

        /// <summary>
        /// Defines methods to support the comparison of colors for equality.
        /// </summary>
        private class ColorEqualityComparer : IEqualityComparer<System.Drawing.Color>
        {
            /// <summary>
            /// Determines whether the specified colors are equal.
            /// </summary>
            /// <param name="x">The first color to compare.</param>
            /// <param name="y">The second color to compare.</param>
            /// <returns><b>true</b> if the specified colors are equal; otherwise, <b>false</b>.</returns>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "This is a private method.")]
            public bool Equals(System.Drawing.Color x, System.Drawing.Color y)
            {
                return x.A == y.A && x.R == y.R && x.G == y.G && x.B == y.B;
            }

            /// <summary>
            /// Returns a hash code for the specified color.
            /// </summary>
            /// <param name="color">The color for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified color.</returns>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "This is a private method.")]
            public int GetHashCode(System.Drawing.Color color)
            {
                return color.GetHashCode();
            }
        }
    }
}
