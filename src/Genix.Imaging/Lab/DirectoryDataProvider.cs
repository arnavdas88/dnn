// -----------------------------------------------------------------------
// <copyright file="DirectoryDataProvider.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Genix.Core;
    using Genix.Imaging;
    using Genix.Lab;

    /// <summary>
    /// Provides images stored in file system for machine learning.
    /// </summary>
    public class DirectoryDataProvider : TestImageProvider<string>
    {
        private readonly Random random = new Random(0);
        private readonly string[] truthFileNames = new string[] { "truth.new", "truth.txt" };
        private readonly string[] truthFieldNames = new string[] { "Class##Name", "Word" };

        ////private readonly List<Tuple<string, bool, Truth, string>> data = new List<Tuple<string, bool, Truth, string>>();

        private readonly List<(string Path, Truth Truth, string[] Labels)> samples = new List<(string, Truth, string[])>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryDataProvider"/> class.
        /// </summary>
        /// <param name="width">The sample width, in pixels.</param>
        /// <param name="height">The sample height, in pixels.</param>
        public DirectoryDataProvider(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Adds a directory used to load images.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="recursive"><b>true</b> to scan the directory recursively.</param>
        /// <param name="label">The label for all samples in the directory.</param>
        public void AddDirectory(string path, bool recursive, string label)
        {
            ////this.data.Add(Tuple.Create(path, recursive, (Truth)null, label));

            foreach (string s in DirectoryDataProvider.ListDirectory(path, recursive))
            {
                this.samples.Add((s, null, new string[] { label }));
            }
        }

        /// <summary>
        /// Adds a directory used to load images.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="recursive"><b>true</b> to scan the directory recursively.</param>
        /// <param name="truthFileName">The path to a truth file name.</param>
        /// <param name="truthFieldName">The name of a column that contains truth data.</param>
        public void AddDirectory(string path, bool recursive, string truthFileName, string truthFieldName)
        {
            if (string.IsNullOrEmpty(truthFileName))
            {
                string directory = File.Exists(path) ? Path.GetDirectoryName(path) : path;
                foreach (string fileName in this.truthFileNames)
                {
                    string fname = Path.Combine(directory, fileName);
                    if (File.Exists(fname))
                    {
                        truthFileName = fname;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(truthFileName))
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot find truth file in directory '{0}'.",
                        path));
                }
            }

            Truth truth = Truth.FromFile(truthFileName);

            if (string.IsNullOrEmpty(truthFieldName))
            {
                foreach (string fieldName in this.truthFieldNames)
                {
                    if (truth.FieldIndex(fieldName) >= 0)
                    {
                        truthFieldName = fieldName;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(truthFieldName) || truth.FieldIndex(truthFieldName) < 0)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Cannot find column '{0}' in truth file '{1}'.",
                    truthFieldName,
                    truthFileName));
            }

            ////this.data.Add(Tuple.Create(path, recursive, truth, truth != null ? truthFieldName : null));

            this.samples.Add((path, truth, null));

            /*if (File.Exists(path))
            {
                string extension = Path.GetExtension(path);
                if (Imaging.Image.SupportedFileExtensions.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    string fileName = Path.GetFileName(path);
                    foreach (string fn in truth.Files)
                    {
                        string fileNameWithoutFrameIndex = Truth.SplitFileName(fn, out int frameIndex);
                        if (fileNameWithoutFrameIndex.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            this.samples.Add((s, new string[] { label }));
                        }
                    }
                }
            }
            else
            {
                foreach (string s in DirectoryDataProvider.ListDirectory(path, recursive))
                {
                    string label = truth.GetTruth(s, null, truthFieldName);
                    if (!string.IsNullOrEmpty(label))
                    {
                        this.samples.Add((s, new string[] { label }));
                    }
                }
            }*/
        }

        /// <summary>
        /// Generates samples for network learning for specified labels.
        /// </summary>
        /// <param name="labels">The labels to generate samples for.</param>
        /// <returns>The sequence of samples. Each sample consist of image and a ground truth.</returns>
        public override IEnumerable<TestImage> Generate(ISet<string> labels)
        {
            /*Truth truth = Truth.FromFile(@"Z:\Test\Recognition\English\Numeric\MachinePrint\truth.txt");

            const string Path = @"Z:\Test\Recognition\English\Numeric\MachinePrint\000017.tif";
            ////foreach ((Bitmap image, int? frameIndex) in DirectoryDataProvider.LoadBitmap(Path, width, height))
            foreach ((Imaging.Image image, int? frameIndex) in DirectoryDataProvider.LoadImage(Path, width, height))
            {
                yield return new SampleImage(
                    Path,
                    frameIndex,
                    null,
                    null,
                    image,
                    truth.GetTruth(Path, frameIndex, "WORD")?.Select(x => new string(x, 1))?.ToArray());
            }*/

            ILookup<string, string> lookup = labels?.ToLookup(x => x);

            List<(string, Truth, string[])> completeSamples = new List<(string, Truth, string[])>(this.samples.Count);

            while (this.samples.Count > 0)
            {
                int index = this.random.Next(this.samples.Count);
                (string path, Truth truth, string[] label) sample = this.samples[index];

                ////(string path, Truth truth, string[] label) sample = (@"L:\CHARACTER\MP\mp1\BLIND\6\6_310024.tif", null, new string[] { "0" });

                if (lookup == null || lookup.Contains(string.Concat(sample.label)))
                {
                    foreach ((Genix.Imaging.Image image, int? frameIndex) in this.LoadImage(sample.path))
                    {
                        yield return new TestImage(
                            new DataSourceId(sample.path, Path.GetFileName(sample.path), frameIndex),
                            null,
                            null,
                            image,
                            sample.label);
                    }
                }

                this.samples.RemoveAt(index);
                completeSamples.Add(sample);
            }

            this.samples.Clear();
            this.samples.AddRange(completeSamples);
        }

        private static IEnumerable<string> ListDirectory(string path, bool recursive)
        {
            // process all files in the directory
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo fileInfo in di.EnumerateFilesByExtensions(Genix.Imaging.Image.SupportedFileExtensions.ToArray()))
            {
                yield return fileInfo.FullName;
            }

            // process sub-directories
            if (recursive)
            {
                foreach (DirectoryInfo sdi in di.GetDirectories())
                {
                    foreach (string s in DirectoryDataProvider.ListDirectory(sdi.FullName, true))
                    {
                        yield return s;
                    }
                }
            }
        }

        private IEnumerable<(Genix.Imaging.Image, int?)> LoadImage(string fileName)
        {
            foreach ((Genix.Imaging.Image image, int? frameIndex, _) in Genix.Imaging.Image.FromFile(fileName))
            {
                IList<Genix.Imaging.ConnectedComponent> components = image.FindConnectedComponents().ToList();
                if (components.Count > 1)
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        Genix.Imaging.ConnectedComponent component = components[i];
                        if (component.Power <= 24)
                        {
                            Rectangle position = component.Bounds;
                            double distance = components.Where((x, j) => j != i).Min(x => position.Distance(x.Bounds));
                            if (distance >= 3.0 * Math.Min(position.Width, position.Height))
                            {
                                image.RemoveConnectedComponent(component);
                                components.RemoveAt(i--);
                            }
                        }
                    }
                }

                Genix.Imaging.Image result = image.CropBlackArea(1, 1);
                result = result.FitToSize(this.Width > 0 ? this.Width : result.Width, this.Height, ScalingOptions.None);

                yield return (result, frameIndex);
            }
        }
    }
}
