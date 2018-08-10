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
    using System.Text;
    using Genix.Core;
    using Genix.Imaging;
    using Genix.Lab;

    /// <summary>
    /// Provides images stored in file system for machine learning.
    /// </summary>
    public class DirectoryDataProvider : TestImageProvider<string>
    {
        private readonly Random random = new Random(0);
        private readonly string[] truthFileNames = new string[] { "truth.txt", "truth.new" };
        private readonly string[] truthFieldNames = new string[] { "#Class", "Word" };

        private readonly List<(string Path, bool Recursive, Truth Truth, string FieldName, string[] Labels)> data =
            new List<(string, bool, Truth, string, string[])>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryDataProvider"/> class.
        /// </summary>
        public DirectoryDataProvider()
        {
        }

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
            this.data.Add((path, recursive, null, null, new string[] { label }));
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

            // find default column if was not provided
            if (string.IsNullOrEmpty(truthFieldName))
            {
                truthFieldName = this.truthFieldNames.FirstOrDefault(x => truth.FieldIndex(x) >= 0);
            }

            if (string.IsNullOrEmpty(truthFieldName) || truth.FieldIndex(truthFieldName) < 0)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Cannot find column '{0}' in truth file '{1}'.",
                    truthFieldName,
                    truthFileName));
            }

            this.data.Add((path, recursive, truth, truthFieldName, null));
        }

        /// <summary>
        /// Generates samples for network learning for specified labels.
        /// </summary>
        /// <param name="requestedLabels">The labels to generate samples for.</param>
        /// <returns>The sequence of samples. Each sample consist of image and a ground truth.</returns>
        public override IEnumerable<TestImage> Generate(ISet<string> requestedLabels)
        {
            ILookup<string, string> lookup = requestedLabels?.ToLookup(x => x);

            return EnumSamples()
                .Where(x => lookup == null || lookup.Contains(string.Concat(x.Labels)))
                /*.Shuffle(10)*/;

            IEnumerable<TestImage> EnumSamples()
            {
                foreach ((string path, bool recursive, Truth truth, string fieldName, string[] labels) in this.data)
                {
                    // filter by provided label
                    if (lookup != null && labels != null && !lookup.Contains(string.Concat(labels)))
                    {
                        continue;
                    }

                    foreach ((string fileName, int startingFrame, int frameCount) in DirectoryDataProvider.ListPath(path, recursive))
                    {
                        foreach ((Imaging.Image image, int? frameIndex) in this.LoadImage(fileName, startingFrame, frameCount))
                        {
                            yield return new TestImage(
                                new DataSourceId(fileName, Path.GetFileName(fileName), frameIndex),
                                null,
                                null,
                                image,
                                ComputeLabels(fileName, frameIndex));
                        }
                    }

                    string[] ComputeLabels(string fileName, int? frameIndex)
                    {
                        return labels ?? new string[] { truth[fileName, frameIndex, fieldName] };
                    }
                }
            }
        }

        private static IEnumerable<(string, int, int)> ListPath(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                foreach (string fileName in ListDirectory(path))
                {
                    yield return (fileName, 0, -1);
                }
            }
            else if (File.Exists(path))
            {
                foreach (var file in ListFile(path))
                {
                    yield return file;
                }
            }

            IEnumerable<string> ListDirectory(string directoryName)
            {
                // process all files in the directory
                DirectoryInfo di = new DirectoryInfo(directoryName);
                foreach (FileInfo fileInfo in di.EnumerateFilesByExtensions(Imaging.Image.SupportedFileExtensions.ToArray()))
                {
                    yield return fileInfo.FullName;
                }

                // process sub-directories
                if (recursive)
                {
                    foreach (DirectoryInfo sdi in di.GetDirectories())
                    {
                        foreach (string s in ListDirectory(sdi.FullName))
                        {
                            yield return s;
                        }
                    }
                }
            }

            IEnumerable<(string, int, int)> ListFile(string listName)
            {
                using (StreamReader reader = new StreamReader(listName, Encoding.UTF8))
                {
                    string lastFileName = null;
                    int lastStartingFrame = 0;
                    int lastFrameCount = -1;

                    while (true)
                    {
                        string s = reader.ReadLine();
                        if (s == null)
                        {
                            break;
                        }

                        if (!string.IsNullOrEmpty(s))
                        {
                            string fileName = s.Unqualify('\"').ToUpperInvariant();
                            int? frameIndex = null;

                            // try to split multi-frame file name
                            int index = fileName.LastIndexOf(';');
                            if (index != -1)
                            {
                                if (int.TryParse(fileName.Substring(index + 1), NumberStyles.Number, CultureInfo.InvariantCulture, out int frame))
                                {
                                    if (frame > 0)
                                    {
                                        frameIndex = frame - 1;
                                    }

                                    fileName = fileName.Substring(0, index);
                                }
                            }

                            // merge with last file
                            if (fileName == lastFileName && frameIndex == lastStartingFrame + lastFrameCount)
                            {
                                lastFrameCount++;
                                continue;
                            }

                            // release last file
                            if (!string.IsNullOrEmpty(lastFileName))
                            {
                                yield return (lastFileName, lastStartingFrame, lastFrameCount);
                            }

                            // cache current file
                            lastFileName = fileName;
                            if (frameIndex.HasValue)
                            {
                                lastStartingFrame = frameIndex.Value;
                                lastFrameCount = 1;
                            }
                            else
                            {
                                lastStartingFrame = 0;
                                lastFrameCount = -1;
                            }
                        }
                    }

                    // release last file
                    if (!string.IsNullOrEmpty(lastFileName))
                    {
                        yield return (lastFileName, lastStartingFrame, lastFrameCount);
                    }
                }
            }
        }

        private IEnumerable<(Imaging.Image, int?)> LoadImage(string fileName, int startingFrame, int frameCount)
        {
            foreach ((Imaging.Image image, int? frameIndex, _) in Imaging.Image.FromFile(fileName, startingFrame, frameCount))
            {
                if (this.Width > 0 || this.Height > 0)
                {
                    yield return (PrepareImage(image), frameIndex);
                }
                else
                {
                    yield return (image, frameIndex);
                }
            }

            Imaging.Image PrepareImage(Imaging.Image image)
            {
                IList<ConnectedComponent> components = image.FindConnectedComponents().ToList();
                if (components.Count > 1)
                {
                    for (int i = 0; i < components.Count; i++)
                    {
                        ConnectedComponent component = components[i];
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

                image = image.CropBlackArea(1, 1);

                if (this.Width > 0 || this.Height > 0)
                {
                    image = image.FitToSize(
                        this.Width > 0 ? this.Width : image.Width,
                        this.Height > 0 ? this.Height : image.Height,
                        ScalingOptions.None);
                }

                return image;
            }
        }
    }
}
