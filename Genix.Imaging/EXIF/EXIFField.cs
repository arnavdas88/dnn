// -----------------------------------------------------------------------
// <copyright file="EXIFField.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the fields according to EXIF specification.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This enumeration does not have zero value.")]
    public enum EXIFField
    {
        /// <summary>
        /// Exposure time, given in seconds.
        /// </summary>
        ExposureTime = 33434,

        /// <summary>
        /// The F number.
        /// </summary>
        FNumber = 33437,

        /// <summary>
        /// The class of the program used by the camera to set exposure when the picture is taken.
        /// </summary>
        [EXIFField(typeof(EXIFExposureProgram))]
        ExposureProgram = 34850,

        /// <summary>
        /// Indicates the spectral sensitivity of each channel of the camera used.
        /// </summary>
        /// <remarks>
        /// The tag value is an ASCII string compatible with the standard developed by the ASTM Technical committee.
        /// </remarks>
        SpectralSensitivity = 34852,

        /// <summary>
        /// Indicates the ISO Speed and ISO Latitude of the camera or input device as specified in ISO 12232.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "TIFF specifications.")]
        ISOSpeedRatings = 34855,

        /// <summary>
        /// Indicates the Opto-Electric Conversion Function (OECF) specified in ISO 14524.
        /// </summary>
        /// <remarks>
        /// OECF is the relationship between the camera optical input and the image values.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "TIFF specifications.")]
        OECF = 34856,

        /// <summary>
        /// The version of the supported EXIF standard.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "TIFF specifications.")]
        ExifVersion = 36864,

        /// <summary>
        /// The date and time when the original image data was generated.
        /// </summary>
        /// <remarks>
        /// For a digital still camera, this is the date and time the picture was taken or recorded.
        /// The format is "YYYY:MM:DD HH:MM:SS" with time shown in 24-hour format,
        /// and the date and time separated by one blank character (hex 20).
        /// </remarks>
        DateTimeOriginal = 36867,

        /// <summary>
        /// The date and time when the image was stored as digital data.
        /// </summary>
        /// <remarks>
        /// If, for example, an image was captured by a digital still camera,
        /// and at the same time the file was recorded,
        /// then the <see cref="DateTimeOriginal"/> and <b>DateTimeDigitized</b> will have the same contents.
        /// The format is "YYYY:MM:DD HH:MM:SS" with time shown in 24-hour format,
        /// and the date and time separated by one blank character (hex 20).
        /// </remarks>
        DateTimeDigitized = 36868,

        /// <summary>
        /// Specific to compressed data; specifies the channels and complements <see cref="TIFFField.PhotometricInterpretation"/>.
        /// </summary>
        ComponentsConfiguration = 37121,

        /// <summary>
        /// Specific to compressed data; states the compressed bits per pixel.
        /// </summary>
        /// <remarks>
        /// The compression mode used for a compressed image is indicated in unit bits per pixel.
        /// </remarks>
        CompressedBitsPerPixel = 37122,

        /// <summary>
        /// Shutter speed.
        /// </summary>
        /// <remarks>
        /// The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </remarks>
        ShutterSpeedValue = 37377,

        /// <summary>
        /// The lens aperture.
        /// </summary>
        /// <remarks>
        /// The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </remarks>
        ApertureValue = 37378,

        /// <summary>
        /// The lens aperture.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </para>
        /// <para>
        /// Ordinarily it is given in the range of -99.99 to 99.99.
        /// Note that if the numerator of the recorded value is hex FFFFFFFF, Unknown shall be indicated.
        /// </para>
        /// </remarks>
        BrightnessValue = 37379,

        /// <summary>
        /// The exposure bias.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </para>
        /// <para>
        /// Ordinarily it is given in the range of -99.99 to 99.99.
        /// </para>
        /// </remarks>
        ExposureBiasValue = 37380,

        /// <summary>
        /// The smallest F number of the lens.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </para>
        /// <para>
        /// Ordinarily it is given in the range of 00.00 to 99.99, but it is not limited to this range.
        /// </para>
        /// </remarks>
        MaxApertureValue = 37381,

        /// <summary>
        /// The distance to the subject, given in meters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that if the numerator of the recorded value is hex FFFFFFFF,
        /// Infinity shall be indicated; and if the numerator is 0, Distance unknown shall be indicated.
        /// </para>
        /// </remarks>
        SubjectDistance = 37382,

        /// <summary>
        /// The metering mode.
        /// </summary>
        [EXIFField(typeof(EXIFMeteringMode))]
        MeteringMode = 37383,

        /// <summary>
        /// The kind of light source.
        /// </summary>
        [EXIFField(typeof(EXIFLightSource))]
        LightSource = 37384,

        /// <summary>
        /// Indicates the status of flash when the image was shot.
        /// </summary>
        Flash = 37385,

        /// <summary>
        /// The actual focal length of the lens, in mm.
        /// </summary>
        /// <remarks>
        /// Conversion is not made to the focal length of a 35 mm film camera.
        /// </remarks>
        FocalLength = 37386,

        /*FlashEnergy_TIFFEP = 0x920b, //// TIFF-EP 
        SpacialFrequencyResponse = 0x920c, //// TIFF-EP 
        Noise = 0x920d, //// TIFF-EP 
        FocalPlaneXResolution_TIFFEP = 0x920e, //// TIFF-EP 
        FocalPlaneYResolution_TIFFEP = 0x920f, //// TIFF-EP 
        FocalPlaneResolutionUnit_TIFFEP = 0x9210, //// TIFF-EP 
        ImageName = 0x9211, //// TIFF-EP 
        SecurityClassification = 0x9212, //// TIFF-EP 

        ImageHistory = 0x9213, // TIFF-EP null separated list*/

        /// <summary>
        /// Indicates the location and area of the main subject in the overall scene.
        /// </summary>
        SubjectArea = 37396,

/*        ExposureIndex_TIFFEP = 0x9215, // TIFF-EP
        TIFFEPStandardID = 0x9216, // TIFF-EP
        SensingMethod_TIFFEP = 0x9217, // TIFF-EP*/

        /// <summary>
        /// Manufacturer specific information.
        /// </summary>
        /// <remarks>
        /// A tag for manufacturers of Exif writers to record any desired information.
        /// The contents are up to the manufacturer, but this tag should not be used for any other than its intended purpose.
        /// </remarks>
        MakerNote = 37500,

        /// <summary>
        /// Keywords or comments on the image; complements <see cref="TIFFField.ImageDescription"/>.
        /// </summary>
        UserComment = 37510,

        /// <summary>
        /// A tag used to record fractions of seconds for the <see cref="TIFFField.DateTime"/> tag.
        /// </summary>
        SubSecDateTime = 37520,

        /// <summary>
        /// A tag used to record fractions of seconds for the <see cref="DateTimeOriginal"/> tag.
        /// </summary>
        SubSecTimeOriginal = 37521,

        /// <summary>
        /// A tag used to record fractions of seconds for the <see cref="DateTimeDigitized"/> tag.
        /// </summary>
        SubSecTimeDigitized = 37522,

        /// <summary>
        /// The Flashpix format version supported by a FPXR file.
        /// </summary>
        FlashPixVersion = 0xa000,

        /// <summary>
        /// The color space information tag is always recorded as the color space specifier.
        /// </summary>
        ColorSpace = 0xa001,

        /// <summary>
        /// Specific to compressed data; the valid width of the meaningful image.
        /// </summary>
        /// <remarks>
        /// When a compressed file is recorded, the valid width of the meaningful image shall be recorded in this tag,
        /// whether or not there is padding data or a restart marker.
        /// This tag should not exist in an uncompressed file.
        /// </remarks>
        PixelXDimension = 0xa002,

        /// <summary>
        /// Specific to compressed data; the valid height of the meaningful image.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a compressed file is recorded, the valid height of the meaningful image shall be recorded in this tag,
        /// whether or not there is padding data or a restart marker.
        /// This tag should not exist in an uncompressed file.
        /// </para>
        /// <para>
        /// Since data padding is unnecessary in the vertical direction, the number of lines recorded in this valid image height tag will in fact be the same as that recorded in the SOF.
        /// </para>
        /// </remarks>
        PixelYDimension = 0xa003,

        /// <summary>
        /// Used to record the name of an audio file related to the image data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This tag is used to record the name of an audio file related to the image data.
        /// The only relational information recorded here is the Exif audio file name and extension
        /// (an ASCII string consisting of 8 characters + '.' + 3 characters).
        /// The path is not recorded.
        /// </para>
        /// </remarks>
        RelatedSoundFile = 0xa004,

        /// <summary>
        /// A pointer to the Exif-related Interoperability IFD.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "TIFF specifications.")]
        InteroperabilityIFDPointer = 0xa005,

        /// <summary>
        /// Indicates the strobe energy at the time the image is captured, as measured in Beam Candle Power Seconds.
        /// </summary>
        FlashEnergy = 0xa20b,

        /// <summary>
        /// Records the camera or input device spatial frequency table and SFR values in the direction of image width, image height, and diagonal direction,
        /// as specified in ISO 12233.
        /// </summary>
        SpatialFrequencyResponse = 0xa20c,

        /// <summary>
        /// Indicates the number of pixels in the image width (X) direction per <see cref="FocalPlaneResolutionUnit"/> on the camera focal plane.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The FocalPlaneResolution tags record the actual focal plane resolutions of the main image
        /// which is written as a file after processing instead of the pixel resolution of the image sensor in the camera.
        /// It should be noted carefully that the data from the image sensor is re-sampled.
        /// </para>
        /// <para>
        /// These tags are used at the same time as a <see cref="FocalLength"/> tag
        /// when the angle of field of the recorded image is to be calculated precisely.
        /// </para>
        /// </remarks>
        FocalPlaneXResolution = 0xa20e,

        /// <summary>
        /// Indicates the number of pixels in the image height (Y) direction per <see cref="FocalPlaneResolutionUnit"/> on the camera focal plane.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The FocalPlaneResolution tags record the actual focal plane resolutions of the main image
        /// which is written as a file after processing instead of the pixel resolution of the image sensor in the camera.
        /// It should be noted carefully that the data from the image sensor is re-sampled.
        /// </para>
        /// <para>
        /// These tags are used at the same time as a <see cref="FocalLength"/> tag
        /// when the angle of field of the recorded image is to be calculated precisely.
        /// </para>
        /// </remarks>
        FocalPlaneYResolution = 0xa20f,

        /// <summary>
        /// Indicates the unit for measuring <see cref="FocalPlaneXResolution"/> and <see cref="FocalPlaneYResolution"/>.
        /// </summary>
        FocalPlaneResolutionUnit = 0xa210,

        /// <summary>
        /// Indicates the location of the main subject in the scene.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this tag represents the pixel at the center of the main subject relative to the left edge,
        /// prior to rotation processing as per the Rotation tag.
        /// The first value indicates the X column number and second indicates the Y row number.
        /// </para>
        /// <para>
        /// When a camera records the main subject location, it is recommended that the SubjectArea tag be used instead of this tag.
        /// </para>
        /// </remarks>
        SubjectLocation = 0xa214,

        /// <summary>
        /// Indicates the exposure index selected on the camera or input device at the time the image is captured.
        /// </summary>
        ExposureIndex = 0xa215,

        /// <summary>
        /// Indicates the image sensor type on the camera or input device.
        /// </summary>
        SensingMethod = 0xa217,

        /// <summary>
        /// Indicates the image source.
        /// </summary>
        /// <remarks>
        /// If a DSC (Digital Still Camera) recorded the image, this tag will always be set to 3, indicating that the image was recorded on a DSC.
        /// </remarks>
        FileSource = 0xa300,

        /// <summary>
        /// Indicates the type of scene.
        /// </summary>
        /// <remarks>
        /// If a DSC recorded the image, this tag value shall always be set to 1, indicating that the image was directly photographed.
        /// </remarks>
        SceneType = 0xa301,

        /// <summary>
        /// Indicates the color filter array (CFA) geometric pattern of the image sensor when a one-chip color area sensor is used.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "TIFF specifications.")]
        CFAPattern = 0xa302,

        /// <summary>
        /// Indicates the use of special processing on image data, such as rendering geared to output.
        /// </summary>
        CustomRendered = 0xa401,

        /// <summary>
        /// Indicates the exposure mode set when the image was shot.
        /// </summary>
        ExposureMode = 0xa402,

        /// <summary>
        /// Indicates the white balance mode set when the image was shot.
        /// </summary>
        WhiteBalance = 0xa403,

        /// <summary>
        /// Indicates the digital zoom ratio when the image was shot.
        /// </summary>
        /// <remarks>
        /// If the numerator of the recorded value is 0, this indicates that digital zoom was not used.
        /// </remarks>
        DigitalZoomRatio = 0xa404,

        /// <summary>
        /// Indicates the equivalent focal length assuming a 35mm film camera, in mm.
        /// </summary>
        /// <remarks>
        /// A value of 0 means the focal length is unknown. Note that this tag differs from the <see cref="FocalLength"/> tag.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "TIFF specifications.")]
        FocalLengthIn35mmFilm = 0xa405,

        /// <summary>
        /// Indicates the type of scene that was shot.
        /// </summary>
        SceneCaptureType = 0xa406,

        /// <summary>
        /// Indicates the degree of overall image gain adjustment.
        /// </summary>
        GainControl = 0xa407,

        /// <summary>
        /// Indicates the direction of contrast processing applied by the camera when the image was shot.
        /// </summary>
        Contrast = 0xa408,

        /// <summary>
        /// Indicates the direction of saturation processing applied by the camera when the image was shot.
        /// </summary>
        Saturation = 0xa409,

        /// <summary>
        /// Indicates the direction of sharpness processing applied by the camera when the image was shot.
        /// </summary>
        Sharpness = 0xa40a,

        /// <summary>
        /// This tag indicates information on the picture-taking conditions of a particular camera model.
        /// </summary>
        /// <remarks>
        /// The tag is used only to indicate the picture-taking conditions in the reader.
        /// </remarks>
        DeviceSettingDescription = 0xa40b,

        /// <summary>
        /// Indicates the distance to the subject.
        /// </summary>
        SubjectDistanceRange = 0xa40c,

        /// <summary>
        /// Indicates an identifier assigned uniquely to each image.
        /// </summary>
        /// <remarks>
        /// It is recorded as an ASCII string equivalent to hexadecimal notation and 128-bit fixed length.
        /// </remarks>
        ImageUniqueId = 0xa420,

        // The Following IDs are not described the EXIF spec
        /*Gamma = 0xa500,

        // The XMP spec declares that XMP data should live 0x2bc when
        // embedded in tiff images.
        XMP = 0x02bc,

        // from the dng spec
        DNGVersion = 0xc612, // Ifd0
        DNGBackwardVersion = 0xc613, // Ifd0
        UniqueCameraModel = 0xc614, // Ifd0
        LocalizedCameraModel = 0xc615, // Ifd0
        CFAPlaneColor = 0xc616, // RawIfd
        CFALayout = 0xc617, // RawIfd
        LinearizationTable = 0xc618, // RawIfd
        BlackLevelRepeatDim = 0xc619, // RawIfd
        BlackLevel = 0xc61a, // RawIfd
        BlackLevelDeltaH = 0xc61b, // RawIfd
        BlackLevelDeltaV = 0xc61c, // RawIfd
        WhiteLevel = 0xc61d, // RawIfd
        DefaultScale = 0xc61e, // RawIfd
        DefaultCropOrigin = 0xc61f, // RawIfd
        DefaultCropSize = 0xc620, // RawIfd
        ColorMatrix1 = 0xc621, // Ifd0
        ColorMatrix2 = 0xc622, // Ifd0
        CameraCalibration1 = 0xc623, // Ifd0
        CameraCalibration2 = 0xc624, // Ifd0
        ReductionMatrix1 = 0xc625, // Ifd0
        ReductionMatrix2 = 0xc626, // Ifd0
        AnalogBalance = 0xc627, // Ifd0
        AsShotNetural = 0xc628, // Ifd0
        AsShotWhiteXY = 0xc629, // Ifd0
        BaselineExposure = 0xc62a, // Ifd0
        BaselineNoise = 0xc62b, // Ifd0
        BaselineSharpness = 0xc62c, // Ifd0
        BayerGreeSpit = 0xc62d, // Ifd0
        LinearResponseLimit = 0xc62e, // Ifd0
        CameraSerialNumber = 0xc62f, // Ifd0
        LensInfo = 0xc630, // Ifd0
        ChromaBlurRadius = 0xc631, // RawIfd
        AntiAliasStrength = 0xc632, // RawIfd
        DNGPrivateData = 0xc634, // Ifd0

        MakerNoteSafety = 0xc635, // Ifd0

        // The Spec says BestQualityScale is 0xc635 but it appears to be wrong
        ////BestQualityScale                = 0xc635, // RawIfd 
        BestQualityScale = 0xc63c, // RawIfd  this looks like the correct value

        CalibrationIlluminant1 = 0xc65a, // Ifd0
        CalibrationIlluminant2 = 0xc65b, // Ifd0

        // Print Image Matching data
        PimIfdPointer = 0xc4a5*/
    }
}
