// -----------------------------------------------------------------------
// <copyright file="Tesseract.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.OCR.Tesseract
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Genix.Core;
    using Genix.Drawing;
    using Genix.Imaging.Leptonica;

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
        /// <param name="dataDirectory">The path to Tesseract data directory.</param>
        /// <returns>
        /// The <see cref="Tesseract"/> object this method creates.
        /// </returns>
        public static Tesseract Create(string dataDirectory)
        {
            if (string.IsNullOrEmpty(dataDirectory))
            {
                dataDirectory = Globals.LookupDataDirectory("Tesseract");
            }

            // ensure the data directory exist
            if (!Directory.Exists(dataDirectory))
            {
                throw new DirectoryNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Genix.Core.Properties.Resources.E_DataDirectoryNotFound,
                        dataDirectory));
            }

            TesseractHandle handle = NativeMethods.TessBaseAPICreate();
            try
            {
                if (NativeMethods.TessBaseAPIInit2(handle, dataDirectory, "eng", OcrEngineMode.LstmOnly) != 0)
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
        /// Processes the <see cref="Imaging.Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Imaging.Image"/> to process.</param>
        /// <param name="pageSegmentationMode">The page segmentation mode.</param>
        /// <returns>
        /// The <see cref="PageShape"/> object this method creates.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is <b>null</b>.
        /// </exception>
        public PageShape Recognize(Imaging.Image image, PageSegmentationMode pageSegmentationMode)
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
                NativeMethods.TessBaseAPISetPageSegMode(this.handle, pageSegmentationMode);

                NativeMethods.TessBaseAPIRecognize(this.handle, IntPtr.Zero);

                return this.ExtractResults(image.Bounds);
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

        private PageShape ExtractResults(Rectangle pageBounds)
        {
            List<TextBlockShape> textBlocks = new List<TextBlockShape>();

            using (ResultIterator iterator = NativeMethods.TessBaseAPIGetIterator(this.handle))
            {
                TextBlockShape textBlock = ExtractTextBlock();
                if (textBlock != null)
                {
                    textBlocks.Add(textBlock);
                }

                TextBlockShape ExtractTextBlock()
                {
                    Rectangle bounds = iterator.GetBoundingBox(PageIteratorLevel.TextBlock);
                    if (bounds.IsEmpty)
                    {
                        return null;
                    }

                    List<Shape> shapes = new List<Shape>();
                    do
                    {
                        PolyBlockType type = iterator.GetBlockType();
                        switch (type)
                        {
                            case PolyBlockType.HorizontalLine:
                            case PolyBlockType.VerticalLine:
                                bounds = iterator.GetBoundingBox(PageIteratorLevel.TextBlock);
                                if (!bounds.IsEmpty)
                                {
                                    shapes.Add(new LineShape(
                                        bounds,
                                        1,
                                        type == PolyBlockType.HorizontalLine ? LineTypes.Horizontal : LineTypes.Vertical));
                                }

                                break;

                            default:
                                do
                                {
                                    ParagraphShape shape = ExtractParagraph();
                                    if (shape != null)
                                    {
                                        shapes.Add(shape);
                                    }
                                }
                                while (!iterator.IsAtFinalElement(PageIteratorLevel.TextBlock, PageIteratorLevel.Paragraph) &&
                                        iterator.Next(PageIteratorLevel.Paragraph));
                                break;
                        }
                    }
                    while (iterator.Next(PageIteratorLevel.TextBlock));

                    return shapes.Count > 0 ? new TextBlockShape(shapes, bounds) : null;
                }

                ParagraphShape ExtractParagraph()
                {
                    Rectangle bounds = iterator.GetBoundingBox(PageIteratorLevel.Paragraph);
                    if (bounds.IsEmpty)
                    {
                        return null;
                    }

                    List<TextLineShape> shapes = new List<TextLineShape>();
                    do
                    {
                        TextLineShape shape = ExtractTextLine();
                        if (shape != null)
                        {
                            shapes.Add(shape);
                        }
                    }
                    while (!iterator.IsAtFinalElement(PageIteratorLevel.Paragraph, PageIteratorLevel.TextLine) &&
                            iterator.Next(PageIteratorLevel.TextLine));

                    return shapes.Count > 0 ? new ParagraphShape(shapes, bounds) : null;
                }

                TextLineShape ExtractTextLine()
                {
                    Rectangle bounds = iterator.GetBoundingBox(PageIteratorLevel.TextLine);
                    if (bounds.IsEmpty)
                    {
                        return null;
                    }

                    List<WordShape> shapes = new List<WordShape>();
                    do
                    {
                        WordShape shape = ExtractWord();
                        if (shape != null)
                        {
                            shapes.Add(shape);
                        }
                    }
                    while (!iterator.IsAtFinalElement(PageIteratorLevel.TextLine, PageIteratorLevel.Word) &&
                            iterator.Next(PageIteratorLevel.Word));

                    return shapes.Count > 0 ? new TextLineShape(shapes, bounds) : null;
                }

                WordShape ExtractWord()
                {
                    Rectangle bounds = iterator.GetBoundingBox(PageIteratorLevel.Word);
                    if (bounds.IsEmpty)
                    {
                        return null;
                    }

                    /*List<CharacterAnswer> ^ characters = gcnew List<CharacterAnswer>();
                    do
                    {
                        CharacterAnswer character;
                        if (ExtractCharacter(iterator, character))
                        {
                            characters->Add(character);
                        }

                        if (iterator->IsAtFinalElement(level, RIL_SYMBOL))
                        {
                            break;
                        }
                    }
                    while (iterator->Next(RIL_SYMBOL));

                    if (!Enumerable::Any(characters))
                    {
                        return nullptr;
                    }

                    int confidence = MakeConfidence(iterator->Confidence(level));*/

                    string text = iterator.GetUTF8Text(PageIteratorLevel.Word);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return null;
                    }

                    float confidence = iterator.GetConfidence(PageIteratorLevel.Word) / 100.0f;

                    return new WordShape(bounds, text, confidence);
                }
            }

            return new PageShape(textBlocks, pageBounds);
        }
    }
}
