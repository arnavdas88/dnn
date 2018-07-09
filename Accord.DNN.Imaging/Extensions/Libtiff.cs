// -----------------------------------------------------------------------
// <copyright file="Libtiff.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using BitMiracle.LibTiff.Classic;

    /// <summary>
    /// Provides extension methods that allows integration with Libtiff.NET library.
    /// </summary>
    internal static class Libtiff
    {
        private static readonly MyTiffErrorHandler TiffErrorHandler = new MyTiffErrorHandler();

#if false
        public static Image LoadFromTiff(Stream stream)
        {
            Tiff.SetErrorHandler(Libtiff.TiffErrorHandler);

            using (Tiff tiff = Tiff.ClientOpen("in-memory", "r", stream, new TiffStream()))
            {
                if (tiff == null)
                {
                    throw new System.IO.FileFormatException();
                }

                do
                {
                    /*Frame frame = ExtractFrame(tiff);

                    frames.Add(frame);*/
                }
                while (tiff.ReadDirectory());
            }

            return null;
        }

        public static void LoadFromTiff(Stream stream, Action<Image> action)
        {
            Tiff.SetErrorHandler(Libtiff.TiffErrorHandler);

            using (Tiff tiff = Tiff.ClientOpen("in-memory", "r", stream, new TiffStream()))
            {
                if (tiff == null)
                {
                    throw new System.IO.FileFormatException();
                }

                do
                {
                    /*Frame frame = ExtractFrame(tiff);

                    frames.Add(frame);*/
                }
                while (tiff.ReadDirectory());
            }
        }
#endif

        public static void SaveToTiff(this Image image, Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Tiff.SetErrorHandler(Libtiff.TiffErrorHandler);

                using (Tiff tiff = Tiff.ClientOpen("in-memory", "w", ms, new TiffStream()))
                {
                    Libtiff.Save(tiff, image);

                    stream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                }
            }
        }

        public static void SaveToTiff(this IEnumerable<Image> images, Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Tiff.SetErrorHandler(Libtiff.TiffErrorHandler);

                using (Tiff tiff = Tiff.ClientOpen("in-memory", "w", ms, new TiffStream()))
                {
                    foreach (Image image in images)
                    {
                        Libtiff.Save(tiff, image);
                    }

                    stream.Write(ms.GetBuffer(), 0, (int)ms.Length);
                }
            }
        }

        private static void Save(Tiff tiff, Image image)
        {
            Libtiff.SaveTags(tiff, image, null);

            byte[] buffer = new byte[image.Stride8];

            // raster stride MAY be bigger than TIFF stride (due to padding in raster bits)
            for (int i = 0, offset = 0; i < image.Height; i++, offset += image.Stride)
            {
                // copy and swap bytes
                NativeMethods.bytesswap_64(image.Stride, image.Bits, offset, buffer, 0);
                ////ConvertBGRToRGB(bytes, bitmap._width, bitmap._height, image.BitsPerPixel);

                if (!tiff.WriteScanline(buffer, 0, i, 0))
                {
                    throw new InvalidOperationException(Properties.Resources.E_CannotEncodeImage);
                }
            }

            if (!tiff.WriteDirectory())
            {
                throw new InvalidOperationException(Properties.Resources.E_CannotEncodeImage);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Do not rethrow Libtiff exceptions.")]
        private static void SaveTags(Tiff tiff, Image image, ImageMetadata metadata)
        {
            tiff.SetField(TiffTag.IMAGEWIDTH, image.Width);
            tiff.SetField(TiffTag.IMAGELENGTH, image.Height);
            tiff.SetField(TiffTag.XRESOLUTION, image.HorizontalResolution);
            tiff.SetField(TiffTag.YRESOLUTION, image.VerticalResolution);
            tiff.SetField(TiffTag.RESOLUTIONUNIT, 2 /* inches */);
            tiff.SetField(TiffTag.ROWSPERSTRIP, image.Height);
            tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            tiff.SetField(TiffTag.FILLORDER, FillOrder.LSB2MSB);

            switch (image.BitsPerPixel)
            {
                case 1:
                    tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISWHITE);
                    tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    break;

                case 2:
                case 4:
                case 8:
                case 16:
                    tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    tiff.SetField(TiffTag.BITSPERSAMPLE, image.BitsPerPixel);
                    tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    break;

                case 32:
                    tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                    tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
                    tiff.SetField(TiffTag.SAMPLESPERPIXEL, 4);
                    break;

                default:
                    throw new NotImplementedException(Properties.Resources.E_InvalidDepth);
            }

            // fill color tables
            /*BitmapPalette palette = frame.Palette;
            if (palette != null)
            {
                IList<System.Windows.Media.Color> colors = palette.Colors;
                if (colors != null)
                {
                    tiff.SetField(
                        TiffTag.COLORMAP,
                        colors.Select(x => (ushort)x.R).ToArray(),
                        colors.Select(x => (ushort)x.G).ToArray(),
                        colors.Select(x => (ushort)x.B).ToArray());
                }
            }*/

            SaveCompressionTag();

            void SaveCompressionTag()
            {
                // set compression; update compression scheme for most common situations
                Compression compression = Compression.NONE;

                object itemCompression = metadata?.GetPropertyItem((int)TiffTag.COMPRESSION);
                if (itemCompression != null)
                {
                    compression = (Compression)System.Convert.ToInt32(itemCompression, CultureInfo.InvariantCulture);

                    // 'old-style' JPEG compression is not supported
                    if (compression == Compression.OJPEG)
                    {
                        compression = Compression.JPEG;
                    }

                    if (image.BitsPerPixel == 1)
                    {
                        if (compression == Compression.JPEG)
                        {
                            // switch from JPEG compression for binary images
                            compression = Compression.CCITT_T6;
                        }
                    }
                    else
                    {
                        if (compression == Compression.CCITT_T4 ||
                            compression == Compression.CCITT_T6)
                        {
                            // switch from Group 3/4 compression for non-binary images
                            compression = image.BitsPerPixel <= 8 ? Compression.LZW : Compression.DEFLATE;
                        }
                        else if (compression == Compression.JPEG)
                        {
                            // set JPEG quality to 100% percent if not set (default is 75%)
                            if (metadata.GetPropertyItem((int)TiffTag.JPEGQUALITY) == null)
                            {
                                metadata.SetPropertyItem((int)TiffTag.JPEGQUALITY, 100);
                            }
                        }
                    }
                }
                else
                {
                    if (image.BitsPerPixel == 1)
                    {
                        // use default Group 4 compression for binary images
                        compression = Compression.CCITT_T6;
                    }
                    else
                    {
                        // use LZW/ZIP compression for gray scale and colored images
                        compression = image.BitsPerPixel <= 8 ? Compression.LZW : Compression.DEFLATE;
                    }
                }

                if (compression != Compression.NONE)
                {
                    tiff.SetField(TiffTag.COMPRESSION, compression);
                }
            }

            if (metadata != null)
            {
                // important!!! set custom tags after compression is set, because it initializes codec-specific tags
                foreach (PropertyItem item in metadata.PropertyItems.Where(x => CanSaveTiffTag(x)))
                {
#if !DEBUG
                try
                {
#endif
                    switch (item.Id)
                    {
                        case (int)TIFFField.PageNumber:
                        case (int)TIFFField.HalftoneHints:
                        case (int)TIFFField.YCbCrSubSampling:
                            {
                                if (item.Value is Array array && array.Length == 2)
                                {
                                    tiff.SetField((TiffTag)item.Id, array.GetValue(0), array.GetValue(1));
                                }
                            }

                            break;

                        // known Photoshop tag issue
                        case (int)TIFFField.Photoshop:
                            {
                                byte[] bytes = item.Value as byte[];
                                if (bytes == null)
                                {
                                    if (item.Value is System.Windows.Media.Imaging.BitmapMetadataBlob blob)
                                    {
                                        bytes = blob.GetBlobValue();
                                    }
                                }

                                if (bytes != null)
                                {
                                    tiff.SetField((TiffTag)item.Id, bytes.Length, bytes);
                                }
                            }

                            break;

                        default:
                            {
                                if (item.Value is System.Windows.Media.Imaging.BitmapMetadataBlob blob)
                                {
                                    byte[] blobValue = blob.GetBlobValue();
                                    SaveTiffTag((TiffTag)item.Id, blobValue);
                                }
                                else
                                {
                                    SaveTiffTag((TiffTag)item.Id, item.Value);
                                }
                            }

                            break;
                    }
#if !DEBUG
                }
                catch
                {
                    // catch all exception, do not allow bad tags to fail whole image save operation
                }
#endif
                }

                bool CanSaveTiffTag(PropertyItem item)
                {
                    if (item.Id == (int)TIFFField.ImageWidth ||
                        item.Id == (int)TIFFField.ImageLength ||
                        item.Id == (int)TIFFField.XResolution ||
                        item.Id == (int)TIFFField.YResolution ||
                        item.Id == (int)TIFFField.ResolutionUnit ||
                        item.Id == (int)TIFFField.RowsPerStrip ||
                        item.Id == (int)TIFFField.StripByteCounts ||
                        item.Id == (int)TIFFField.StripOffsets ||
                        item.Id == (int)TIFFField.TileByteCounts ||
                        item.Id == (int)TIFFField.TileOffsets ||
                        item.Id == (int)TIFFField.Compression ||
                        item.Id == (int)TIFFField.T4Options ||
                        item.Id == (int)TIFFField.T6Options ||
                        item.Id == (int)TIFFField.BitsPerSample ||
                        item.Id == (int)TIFFField.SamplesPerPixel ||
                        item.Id == (int)TIFFField.PhotometricInterpretation ||
                        item.Id == (int)TIFFField.FillOrder ||
                        item.Id == (int)TIFFField.PlanarConfiguration ||
                        item.Id == (int)TIFFField.ColorMap ||
                        item.Id == (int)TIFFField.JpegTables ||
                        (item.Id >= 512 && item.Id <= 521) /* old JPEG tags */)
                    {
                        return false;
                    }

                    // unsupported compression modes
                    if (item.Id == (int)TIFFField.Compression)
                    {
                        int compression = System.Convert.ToInt32(item.Value, CultureInfo.InvariantCulture);
                        if (compression == (int)TIFFCompression.JPEG)
                        {
                            return false;
                        }
                    }

                    // fix known issues
                    if (item.Value is Array)
                    {
                        // some files have incorrect tags
                        if (item.Id == (int)TIFFField.SampleFormat)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // typically missed parameter
                        if (item.Id == (int)TIFFField.ExtraSamples)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                void SaveTiffTag(TiffTag tag, object value)
                {
                    Array valueArray = value as Array;

                    TiffFieldInfo fip = tiff.FindFieldInfo(tag, TiffType.ANY);
                    if (fip != null)
                    {
                        if (!TryFixTagValue(fip, ref value))
                        {
                            return;
                        }
                    }

                    if (fip == null)
                    {
                        // calculate tag type
                        TiffType type = ValueToTiffType(value);
                        if (type == TiffType.UNDEFINED)
                        {
                            return;
                        }

                        // register new tag
                        short readWriteCount = 1;
                        if (valueArray != null)
                        {
                            readWriteCount = valueArray.Length > short.MaxValue ? TiffFieldInfo.Variable2 : TiffFieldInfo.Variable;
                        }

                        fip = new TiffFieldInfo(
                            tag,
                            readWriteCount,
                            readWriteCount,
                            type,
                            FieldBit.Custom,
                            true,
                            valueArray != null,
                            string.Format(CultureInfo.InvariantCulture, "Tag {0}", (int)tag));

                        tiff.MergeFieldInfo(new TiffFieldInfo[1] { fip }, 1);
                    }

                    if (valueArray != null)
                    {
                        tiff.SetField(tag, valueArray.Length, valueArray);
                    }
                    else
                    {
                        if (value is string s)
                        {
                            // fix interoperability IFD bug and be some other bugs as well
                            if (fip.Type == TiffType.SHORT ||
                                fip.Type == TiffType.LONG ||
                                fip.Type == TiffType.SSHORT ||
                                fip.Type == TiffType.SLONG ||
                                fip.Type == TiffType.FLOAT ||
                                fip.Type == TiffType.DOUBLE)
                            {
                                return;
                            }

                            if (fip.Type == TiffType.BYTE && fip.PassCount)
                            {
                                tiff.SetField(tag, s.Length, value);
                            }
                            else
                            {
                                tiff.SetField(tag, value);
                            }
                        }
                        else
                        {
                            tiff.SetField(tag, value);
                        }
                    }
                }

                bool TryFixTagValue(TiffFieldInfo fip, ref object value)
                {
                    try
                    {
                        if (!fip.PassCount)
                        {
                            switch (fip.Type)
                            {
                                case TiffType.SHORT:
                                    value = System.Convert.ToUInt16(value, CultureInfo.InvariantCulture);
                                    break;

                                case TiffType.SSHORT:
                                    value = System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
                                    break;

                                case TiffType.LONG:
                                    value = System.Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                                    break;

                                case TiffType.SLONG:
                                    value = System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
                                    break;

                                case TiffType.LONG8:
                                    value = System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                                    break;

                                case TiffType.SLONG8:
                                    value = System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                    }
                    catch (OverflowException)
                    {
                        return false;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                    catch (InvalidCastException)
                    {
                        return false;
                    }

                    return true;
                }

                TiffType ValueToTiffType(object value)
                {
                    if (value is byte || value is byte[])
                    {
                        return TiffType.BYTE;
                    }
                    else if (value is sbyte || value is sbyte[])
                    {
                        return TiffType.SBYTE;
                    }
                    else if (value is ushort || value is ushort[])
                    {
                        return TiffType.SHORT;
                    }
                    else if (value is short || value is short[])
                    {
                        return TiffType.SSHORT;
                    }
                    else if (value is uint || value is uint[])
                    {
                        return TiffType.LONG;
                    }
                    else if (value is int || value is int[])
                    {
                        return TiffType.SLONG;
                    }
                    else if (value is ulong || value is ulong[])
                    {
                        return TiffType.LONG8;
                    }
                    else if (value is long || value is long[])
                    {
                        return TiffType.SLONG8;
                    }
                    else if (value is float || value is float[])
                    {
                        return TiffType.FLOAT;
                    }
                    else if (value is double || value is double[])
                    {
                        return TiffType.DOUBLE;
                    }
                    else if (value is string)
                    {
                        return TiffType.ASCII;
                    }

                    return TiffType.UNDEFINED;
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport("Accord.DNN.CPP.dll")]
            [SuppressUnmanagedCodeSecurity]
            public static extern unsafe void bytesswap_64(int n, [In] ulong[] x, int offx, [Out] byte[] y, int offy);
        }

        private sealed class MyTiffErrorHandler : TiffErrorHandler
        {
            public override void ErrorHandler(Tiff tif, string method, string format, params object[] args)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, format, args));
            }

            public override void ErrorHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
            {
            }

            public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
            {
            }

            public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
            {
            }
        }
    }
}
