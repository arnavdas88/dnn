// -----------------------------------------------------------------------
// <copyright file="Tesseract.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.IO;
    using Genix.Imaging;

    /// <summary>
    /// Tesseract OCR.
    /// </summary>
    public class Tesseract : DisposableObject
    {
        /// <summary>
        /// The handle reference for the object.
        /// </summary>
        private readonly TesseractHandle handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tesseract"/> class.
        /// </summary>
        /// <param name="handle">The pointer to native Tesseract object.</param>
        private Tesseract(TesseractHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// Creates and initializes a new <see cref="Tesseract"/> object.
        /// </summary>
        /// <param name="dataPath">The path to Tesseract data directory.</param>
        /// <returns>
        /// The <see cref="Tesseract"/> object this method creates.
        /// </returns>
        public static Tesseract Create(string dataPath)
        {
            // ensure the data directory exist
            if (!Directory.Exists(dataPath))
            {
                throw new DirectoryNotFoundException("Datapath does not exist.");
            }

            TesseractHandle handle = NativeMethods.TessBaseAPICreate();
            try
            {
                if (NativeMethods.TessBaseAPIInit2(handle, dataPath, "eng", OcrEngineMode.LstmOnly) != 0)
                {
                    throw new InvalidOperationException("Cannot initialize Tesseract engine.");
                }
            }
            catch
            {
                handle?.Dispose();
                throw;
            }

            return new Tesseract(handle);
        }

        /// <summary>
        /// Processes the <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public void SetImage(Image image)
        {
            using (Pix pix = Pix.FromImage(image))
            {
                ////Clear();
                ////NativeMethods.TessBaseAPIClear(this.handle);

                ////ClearPersistentCache();
                ////NativeMethods.TessBaseAPIClearPersistentCache(this.handle);

                ////ClearAdaptiveClassifier();
                ////NativeMethods.TessBaseAPIClearAdaptiveClassifier(this.handle);

                NativeMethods.TessBaseAPISetImage2(this.handle, pix.Handle);

                ////NativeMethods.BaseAPISetPageSegMode(this.handle, actualPageSegmentMode);

                using (PageIteratorHandle pageIterator = NativeMethods.TessBaseAPIAnalyseLayout(this.handle))
                {
                    pageIterator.Begin();

                    /*if (it)
                    {
                        it->Orientation(&orientation, &direction, &order, &deskew_angle);
                        tprintf(
                            "Orientation: %d\nWritingDirection: %d\nTextlineOrder: %d\n"
                            "Deskew angle: %.4f\n",
                            orientation, direction, order, deskew_angle);
                    }
                    else
                    {
                        ret_val = EXIT_FAILURE;
                    }*/
                }
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!this.handle.IsInvalid)
            {
                ////Clear();
                NativeMethods.TessBaseAPIClear(this.handle);

                ////ClearPersistentCache();
                NativeMethods.TessBaseAPIClearPersistentCache(this.handle);

                ////ClearAdaptiveClassifier();
                NativeMethods.TessBaseAPIClearAdaptiveClassifier(this.handle);

                this.handle.Dispose();
            }
        }
    }
}
