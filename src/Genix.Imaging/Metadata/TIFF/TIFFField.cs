// -----------------------------------------------------------------------
// <copyright file="TIFFField.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the field in the TIFF image.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This enumeration does not have zero value.")]
    public enum TIFFField
    {
        /*InteroperabilityIndex = 0x0001,
        InteroperabilityVersion = 0x0002,*/

        /// <summary>
        /// A general indication of the kind of data contained in this sub-file.
        /// </summary>
        [TIFFField(typeof(TIFFNewSubfileTypes))]
        NewSubfileType = 254,

        /// <summary>
        /// A general indication of the kind of data contained in this sub-file.
        /// </summary>
        [TIFFField(typeof(TIFFSubfileType))]
        SubfileType = 255,

        /// <summary>
        /// The number of columns in the image, i.e., the number of pixels per row.
        /// </summary>
        ImageWidth = 256,

        /// <summary>
        /// The number of rows of pixels in the image.
        /// </summary>
        ImageLength = 257,

        /// <summary>
        /// Number of bits per component.
        /// </summary>
        BitsPerSample = 258,

        /// <summary>
        /// Compression scheme used on the image data.
        /// </summary>
        [TIFFField(typeof(TIFFCompression))]
        Compression = 259,

        /// <summary>
        /// The color space of the image data.
        /// </summary>
        [TIFFField(typeof(TIFFPhotometricInterpretation))]
        PhotometricInterpretation = 262,

        /// <summary>
        /// For black and white TIFF files that represent shades of gray, the technique used to convert from gray to black and white pixels.
        /// </summary>
        Threshholding = 263,

        /// <summary>
        /// The width of the dithering or half-toning matrix used to create a dithered or half-toned bi-level file.
        /// </summary>
        CellWidth = 264,

        /// <summary>
        /// The length of the dithering or half-toning matrix used to create a dithered or half-toned bi-level file.
        /// </summary>
        CellLength = 265,

        /// <summary>
        /// The logical order of bits within a byte.
        /// </summary>
        [TIFFField(typeof(TIFFFillOrder))]
        FillOrder = 266,

        /// <summary>
        /// The name of the document from which this image was scanned.
        /// </summary>
        DocumentName = 269,

        /// <summary>
        /// A string that describes the subject of the image.
        /// </summary>
        ImageDescription = 270,

        /// <summary>
        /// The scanner manufacturer.
        /// </summary>
        /// <remarks>
        /// Manufacturer of the scanner, video digitizer, or other type of equipment used to generate the image.
        /// Synthetic images should not include this field.
        /// </remarks>
        Make = 271,

        /// <summary>
        /// The scanner model name or number.
        /// </summary>
        /// <remarks>
        /// The model name or number of the scanner, video digitizer, or other type of equipment used to generate the image.
        /// </remarks>
        Model = 272,

        /// <summary>
        /// For each strip, the byte offset of that strip.
        /// </summary>
        StripOffsets = 273,

        /// <summary>
        /// The orientation of the image with respect to the rows and columns.
        /// </summary>
        [TIFFField(typeof(TIFFOrientation))]
        Orientation = 274,

        /// <summary>
        /// The number of components per pixel.
        /// </summary>
        /// <remarks>
        /// <b>SamplesPerPixel</b> is usually 1 for bi-level, grayscale, and palette-color images.
        /// <b>SamplesPerPixel</b> is usually 3 for RGB images.
        /// If this value is higher, <see cref="ExtraSamples"/> should give an indication of the meaning of the additional channels.
        /// </remarks>
        SamplesPerPixel = 277,

        /// <summary>
        /// The number of rows per strip.
        /// </summary>
        RowsPerStrip = 278,

        /// <summary>
        /// For each strip, the number of bytes in the strip after compression.
        /// </summary>
        StripByteCounts = 279,

        /// <summary>
        /// The minimum component value used.
        /// </summary>
        MinSampleValue = 280,

        /// <summary>
        /// The maximum component value used.
        /// </summary>
        MaxSampleValue = 281,

        /// <summary>
        /// The number of pixels per <see cref="ResolutionUnit"/> in the <see cref="ImageWidth"/> direction.
        /// </summary>
        /// <remarks>
        /// It is not mandatory that the image be actually displayed or printed at the size implied by this parameter.
        /// It is up to the application to use this information as it wishes.
        /// </remarks>
        XResolution = 282,

        /// <summary>
        /// The number of pixels per <see cref="ResolutionUnit"/> in the <see cref="ImageLength"/> direction.
        /// </summary>
        /// <remarks>
        /// It is not mandatory that the image be actually displayed or printed at the size implied by this parameter.
        /// It is up to the application to use this information as it wishes.
        /// </remarks>
        YResolution = 283,

        /// <summary>
        /// How the components of each pixel are stored.
        /// </summary>
        [TIFFField(typeof(TIFFPlanarConfiguration))]
        PlanarConfiguration = 284,

        /// <summary>
        /// The name of the page from which this image was scanned.
        /// </summary>
        PageName = 285,

        /// <summary>
        /// X position of the image.
        /// </summary>
        /// <remarks>
        /// The X offset in ResolutionUnits of the left side of the image, with respect to the left side of the page.
        /// </remarks>
        XPosition = 286,

        /// <summary>
        /// Y position of the image.
        /// </summary>
        /// <remarks>
        /// The Y offset in ResolutionUnits of the top of the image, with respect to the top of the page.
        /// In the TIFF coordinate scheme, the positive Y direction is down, so that YPosition is always positive.
        /// </remarks>
        YPosition = 287,

        /// <summary>
        /// For each string of contiguous unused bytes in a TIFF file, the byte offset of the string.
        /// </summary>
        FreeOffsets = 288,

        /// <summary>
        /// For each string of contiguous unused bytes in a TIFF file, the number of bytes in the string.
        /// </summary>
        FreeByteCounts = 289,

        /// <summary>
        /// The precision of the information contained in the GrayResponseCurve.
        /// </summary>
        GrayResponseUnit = 290,

        /// <summary>
        /// For grayscale data, the optical density of each possible pixel value.
        /// </summary>
        GrayResponseCurve = 291,

        /// <summary>
        /// Options for Group 3 Fax compression.
        /// </summary>
        T4Options = 292,

        /// <summary>
        /// Options for Group 4 Fax compression.
        /// </summary>
        T6Options = 293,

        /// <summary>
        /// The unit of measurement for <see cref="XResolution"/> and <see cref="YResolution"/>.
        /// </summary>
        [TIFFField(typeof(TIFFResolutionUnit))]
        ResolutionUnit = 296,

        /// <summary>
        /// The page number of the page from which this image was scanned.
        /// </summary>
        PageNumber = 297,

        /// <summary>
        /// Describes a transfer function for the image in tabular style.
        /// </summary>
        /// <remarks>
        /// Pixel components can be gamma-compensated, companded, non-uniformly quantized, or coded in some other way.
        /// The TransferFunction maps the pixel components from a non-linear BitsPerSample (e.g. 8-bit)
        /// form into a 16-bit linear form without a perceptible loss of accuracy.
        /// </remarks>
        TransferFunction = 301,

        /// <summary>
        /// Name and version number of the software package(s) used to create the image.
        /// </summary>
        Software = 305,

        /// <summary>
        /// Date and time of image creation.
        /// </summary>
        /// <remarks>
        /// The format is: "YYYY:MM:DD HH:MM:SS", with hours like those on a 24-hour clock, and one space character between the date and the time.
        /// The length of the string, including the terminating NULL, is 20 bytes.
        /// </remarks>
        DateTime = 306,

        /// <summary>
        /// Person who created the image.
        /// </summary>
        /// <remarks>
        /// Note: some older TIFF files used this tag for storing Copyright information.
        /// </remarks>
        Artist = 315,

        /// <summary>
        /// The computer and/or operating system in use at the time of image creation.
        /// </summary>
        HostComputer = 316,

        /// <summary>
        /// A mathematical operator that is applied to the image data before an encoding scheme is applied.
        /// </summary>
        Predictor = 317,

        /// <summary>
        /// The chromaticity of the white point of the image.
        /// </summary>
        WhitePoint = 318,

        /// <summary>
        /// The chromaticities of the primaries of the image.
        /// </summary>
        PrimaryChromaticities = 319,

        /// <summary>
        /// A color map for palette color images.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This field defines a Red-Green-Blue color map (often called a lookup table) for palette-color images.
        /// In a palette-color image, a pixel value is used to index into an RGB lookup table.
        /// For example, a palette-color pixel having a value of 0 would be displayed according to the 0th Red, Green, Blue triplet.
        /// </para>
        /// <para>
        /// In a TIFF ColorMap, all the Red values come first, followed by the Green values, then the Blue values.
        /// The number of values for each color is 2**BitsPerSample.
        /// Therefore, the ColorMap field for an 8-bit palette-color image would have 3 * 256 values.
        /// The width of each value is 16 bits, as implied by the type of SHORT. 0 represents the minimum intensity,
        /// and 65535 represents the maximum intensity. Black is represented by 0,0,0, and white by 65535, 65535, 65535.
        /// </para>
        /// <para>
        /// ColorMap must be included in all palette-color images.
        /// </para>
        /// </remarks>
        ColorMap = 320,

        /// <summary>
        /// Conveys to the halftone function the range of gray levels within a colorimetrically-specified image that should retain tonal detail.
        /// </summary>
        HalftoneHints = 321,

        /// <summary>
        /// The tile width in pixels. This is the number of columns in each tile.
        /// </summary>
        TileWidth = 322,

        /// <summary>
        /// The tile length (height) in pixels. This is the number of rows in each tile.
        /// </summary>
        TileLength = 323,

        /// <summary>
        /// For each tile, the byte offset of that tile, as compressed and stored on disk.
        /// </summary>
        TileOffsets = 324,

        /// <summary>
        /// For each tile, the number of (compressed) bytes in that tile.
        /// </summary>
        TileByteCounts = 325,

        /*        // Tiled images
                // TIFF-EP
                SubIFDs = 0x014a,*/

        /// <summary>
        /// The set of inks used in a separated (PhotometricInterpretation=5) image.
        /// </summary>
        InkSet = 332,

        /// <summary>
        /// The name of each ink used in a separated image.
        /// </summary>
        /// <remarks>
        /// These names are written as a list of concatenated, NUL-terminated ASCII strings. The number of strings must be equal to <see cref="NumberOfInks"/>.
        /// </remarks>
        InkNames = 333,

        /// <summary>
        /// Usually equal to <see cref="SamplesPerPixel"/>, unless there are extra samples.
        /// </summary>
        NumberOfInks = 334,

        /// <summary>
        /// The component values that correspond to a 0% dot and 100% dot.
        /// </summary>
        /// <remarks>
        /// DotRange[0] corresponds to a 0% dot, and DotRange[1] corresponds to a 100% dot.
        /// </remarks>
        DotRange = 336,

        /// <summary>
        /// A description of the printing environment for which this separation is intended.
        /// </summary>
        TargetPrinter = 337,

        /// <summary>
        /// Description of extra components.
        /// </summary>
        ExtraSamples = 338,

        /// <summary>
        /// Specifies how to interpret each data sample in a pixel.
        /// </summary>
        SampleFormat = 339,

        /// <summary>
        /// Specifies the minimum sample value.
        /// </summary>
        SMinSampleValue = 340,

        /// <summary>
        /// Specifies the maximum sample value.
        /// </summary>
        SMaxSampleValue = 341,

        /// <summary>
        /// Expands the range of the <see cref="TransferFunction"/>.
        /// </summary>
        TransferRange = 342,

        /// <summary>
        /// Mirrors the essentials of PostScript's path creation functionality.
        /// </summary>
        ClipPath = 343,

        /// <summary>
        /// Jpeg quantization and/or Huffman tables for subsequent use by JPEG image segments.
        /// </summary>
        JpegTables = 347,

        /// <summary>
        /// Jpeg process used to produce the compressed data.
        /// </summary>
        JpegProcess = 512,

        /// <summary>
        /// Indicates whether a Jpeg interchange format bit stream is present in the TIFF file.
        /// </summary>
        JpegInterchangeFormat = 513,

        /// <summary>
        /// The length of the Jpeg stream pointed to by JpegInterchangeFormat tag.
        /// </summary>
        JpegInterchangeFormatLength = 514,

        /// <summary>
        /// Jpeg restart interval.
        /// </summary>
        JpegRestartInterval = 515,

        /// <summary>
        /// Jpeg lossless predictors.
        /// </summary>
        JpegLosslessPredictors = 517,

        /// <summary>
        /// Jpeg point transforms.
        /// </summary>
        JpegPointTransforms = 518,

        /// <summary>
        /// A list of offsets to the quantization tables, one per component.
        /// </summary>
        JpegQTables = 519,

        /// <summary>
        /// A list of offsets to the DC tables, one per component.
        /// </summary>
        JpegDCTables = 520,

        /// <summary>
        /// A list of offsets to the AC tables, one per component.
        /// </summary>
        JpegACTables = 521,

        /// <summary>
        /// The transformation from RGB to YCbCr image data.
        /// The transformation is specified as three rational values that represent the coefficients used to compute luminance, Y.
        /// </summary>
        YCbCrCoefficients = 529,

        /// <summary>
        /// Specifies the sub-sampling factors used for the chrominance components of a YCbCr image.
        /// </summary>
        YCbCrSubSampling = 530,

        /// <summary>
        /// Specifies the positioning of sub-sampled chrominance components relative to luminance samples.
        /// </summary>
        YCbCrPositioning = 531,

        /*JPEGTables = 0x015b, // TIFF-EP*/

        /*ReferenceBlackWhite = 0x0214,
        RelatedImageFileFormat = 0x1000,
        RelatedImageWidth = 0x1001,
        RelatedImageLength = 0x1002,
        CFARepeatPatternDim = 0x828d,
        CFAPattern = 0x828e,
        BatteryLevel = 0x828f,*/

        /// <summary>
        /// Copyright notice.
        /// </summary>
        Copyright = 33432,

        /*ExposureTime = 0x829a,
        FNumber = 0x829d,

        // These are from the NIFF spec and only really valid when the header begins with IIN1
        // see the NiffTag enum for the specification specific names
        Rotation = 0x82b9,
        NavyCompression = 0x82ba,
        TileIndex = 0x82bb,

        // end NIFF specific
        IPTCNAA = 0x83bb,*/

        /// <summary>
        /// Collection of Photoshop 'Image Resource Blocks'.
        /// </summary>
        Photoshop = 34377,

        /// <summary>
        /// X dimension.
        /// </summary>
        XDimension = 40962,

        /// <summary>
        /// Y dimension.
        /// </summary>
        YDimension = 40963,

        /*ExifIfdPointer = 0x8769,
        InterColorProfile = 0x8773,
        ExposureProgram = 0x8822,
        SpectralSensitivity = 0x8824,
        GPSInfoIfdPointer = 0x8825,
        ISOSpeedRatings = 0x8827,
        OECF = 0x8828,*/
    }
}
