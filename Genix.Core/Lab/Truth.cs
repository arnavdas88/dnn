// -----------------------------------------------------------------------
// <copyright file="Truth.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Declares an object that contains named truth information for a list of files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Truth"/> is serialized to a comma-separated text file, where each line represents a file and each column a field.
    /// The first column contains file names; the first row is a header that contains field names.
    /// </para>
    /// <para>
    /// Some field names are reserved to represent results of classification.
    /// To access such fields use constants declared in <see cref="Truth"/> object.
    /// </para>
    /// </remarks>
    public sealed class Truth
    {
        /// <summary>
        /// A collection of fields.
        /// </summary>
        private readonly List<string> fields = new List<string>();

        /// <summary>
        /// A collection of files.
        /// </summary>
        private readonly Dictionary<FileKey, FileData> files = new Dictionary<FileKey, FileData>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Truth"/> class.
        /// </summary>
        public Truth()
        {
        }

        /// <summary>
        /// Gets a collection of files for the current truth.
        /// </summary>
        /// <value>
        /// A collection of <see cref="String"/> objects that represents the names of files in the truth.
        /// </value>
        public ReadOnlyCollection<string> FileNames => new ReadOnlyCollection<string>(this.files.Keys.Select(x => x.FileName).ToList());

        /// <summary>
        /// Gets a collection of file paths for the current truth.
        /// </summary>
        /// <value>
        /// A collection of <see cref="String"/> objects that represents the names of file paths in the truth.
        /// </value>
        public ReadOnlyCollection<string> FilePaths => new ReadOnlyCollection<string>(this.files.Keys.Select(x => x.FilePath).ToList());

        /// <summary>
        /// Gets a collection of files for the current truth.
        /// </summary>
        /// <value>
        /// A collection of <see cref="String"/> objects that represents the names of fields in the truth.
        /// </value>
        public ReadOnlyCollection<string> Fields => new ReadOnlyCollection<string>(this.fields);

        /// <summary>
        /// Gets or sets truth data for a field, specified by file name and field name.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="fieldName">A field name.</param>
        /// <returns>The truth data.</returns>
        public string this[string fileName, string fieldName]
        {
            get => this[fileName, null, fieldName];
            set => this[fileName, null, fieldName] = value;
        }

        /// <summary>
        /// Gets or sets truth data for a field, specified by file name and field index.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="fieldIndex">A zero-based field index in the fields collection.</param>
        /// <returns>The truth data.</returns>
        public string this[string fileName, int fieldIndex]
        {
            get => this[fileName, null, fieldIndex];
            set => this[fileName, null, fieldIndex] = value;
        }

        /// <summary>
        /// Gets or sets truth data for a field, specified by file name, frame index, and field name.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="frameIndex">A zero-based index of this image in the frame file.</param>
        /// <param name="fieldName">A field name.</param>
        /// <returns>The truth data.</returns>
        public string this[string fileName, int? frameIndex, string fieldName]
        {
            get => this[fileName, frameIndex, this.FieldIndex(fieldName)];
            set => this[fileName, frameIndex, this.FieldIndex(fieldName)] = value;
        }

        /// <summary>
        /// Gets or sets truth data for a field, specified by file name, TIFF index, and field index.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="frameIndex">A zero-based index of this image in the TIFF file.</param>
        /// <param name="fieldIndex">A zero-based field index in the fields collection.</param>
        /// <returns>The truth data.</returns>
        public string this[string fileName, int? frameIndex, int fieldIndex]
        {
            get
            {
                if (this.files.TryGetValue(new FileKey(fileName, frameIndex), out FileData file))
                {
                    file.GetField(fieldIndex);
                }

                return null;
            }

            set
            {
                FileKey key = new FileKey(fileName, frameIndex);
                if (!this.files.TryGetValue(key, out FileData file))
                {
                    file = this.files[key] = new FileData();
                }

                file.SetField(fieldIndex, value, this.fields.Count);
            }
        }

        /// <summary>
        /// Gets or sets truth data for a field, specified by file name, field name, row index, and column index.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="fieldName">A field name.</param>
        /// <param name="rowIndex">The zero-based row index; <b>null</b> if the field is not a table field.</param>
        /// <param name="columnIndex">The zero-based column index; <b>null</b> if the field is not a table field.</param>
        /// <returns>The truth data.</returns>
        public string this[string fileName, string fieldName, int? rowIndex, int? columnIndex]
        {
            get => this[fileName, null, fieldName, rowIndex, columnIndex];
            set => this[fileName, null, fieldName, rowIndex, columnIndex] = value;
        }

        /// <summary>
        /// Gets or sets truth data for a field, specified by file name, TIFF index, field name, row index, and column index.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="frameIndex">A zero-based index of this image in the TIFF file.</param>
        /// <param name="fieldName">A field name.</param>
        /// <param name="rowIndex">The zero-based row index; <b>null</b> if the field is not a table field.</param>
        /// <param name="columnIndex">The zero-based column index; <b>null</b> if the field is not a table field.</param>
        /// <returns>The truth data.</returns>
        public string this[string fileName, int? frameIndex, string fieldName, int? rowIndex, int? columnIndex]
        {
            get => this[fileName, frameIndex, Truth.MakeTableFieldName(fieldName, rowIndex, columnIndex)];
            set => this[fileName, frameIndex, Truth.MakeTableFieldName(fieldName, rowIndex, columnIndex)] = value;
        }

        /// <summary>
        /// Initialize a new instance of Truth object and loads the truth data from a file.
        /// </summary>
        /// <param name="fileName">The name of the truth data file.</param>
        /// <returns>The Truth object this method creates.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is safe to dispose FileStream twice.")]
        public static Truth FromFile(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    Truth truth = new Truth();

                    bool firstLine = true;
                    while (true)
                    {
                        string s = reader.ReadLine();
                        if (s == null)
                        {
                            break;
                        }

                        if (!string.IsNullOrEmpty(s))
                        {
                            string[] split = s.ToUpper(CultureInfo.InvariantCulture).SplitQualified(',');
                            if (split.Any() && !string.IsNullOrEmpty(split.First()))
                            {
                                if (firstLine)
                                {
                                    // parse field names
                                    firstLine = false;
                                    truth.fields.AddRange(split.Skip(1));
                                }
                                else
                                {
                                    // file name is the first column
                                    int frameIndex;
                                    string fname = Truth.SplitFileName(split[0], out frameIndex);

                                    // add new file
                                    truth.files[new FileKey(fname, frameIndex)] = new FileData(split.Skip(1), truth.fields.Count);
                                }
                            }
                        }
                    }

                    return truth;
                }
            }
        }

        /// <summary>
        /// Produces a string that contains table field name parameterized with its row and column indexes.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <param name="rowIndex">The zero-based row index.</param>
        /// <param name="columnIndex">The zero-based column index.</param>
        /// <returns>A string that contains table field name parameterized with its row and column indexes.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", Justification = "Do not expect overflow.")]
        public static string MakeTableFieldName(string fieldName, int? rowIndex, int? columnIndex)
        {
            if (rowIndex.HasValue || columnIndex.HasValue)
            {
                return fieldName + string.Format(
                    CultureInfo.InvariantCulture,
                    "[{0}-{1}]",
                    rowIndex.GetValueOrDefault() + 1,
                    columnIndex.GetValueOrDefault() + 1);
            }
            else
            {
                return fieldName;
            }
        }

        /// <summary>
        /// Constructs file representation in the truth that consist of file name and frame index.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="frameIndex">The zero-based index of recognized image in a multi-page image file. <b>null</b> if the recognized image belongs to a single-page image file.</param>
        /// <returns>The file representation in the truth.</returns>
        public static string MakeFileName(string fileName, int? frameIndex)
        {
            if (frameIndex.HasValue && frameIndex != 0)
            {
                return string.Join(";", Path.GetFileName(fileName), (frameIndex + 1).ToString());
            }
            else
            {
                return Path.GetFileName(fileName);
            }
        }

        /// <summary>
        /// Extracts file name and the frame index from the file representation in the truth.
        /// </summary>
        /// <param name="fileName">The file representation in the truth.</param>
        /// <param name="frameIndex">The zero-based index of recognized image in a multi-page image file.</param>
        /// <returns>The file name.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Need to return two values.")]
        public static string SplitFileName(string fileName, out int frameIndex)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            frameIndex = 0;

            // try to multi-frame split file name
            int semicolonIndex = fileName.LastIndexOf(';');
            if (semicolonIndex != -1)
            {
                if (int.TryParse(fileName.Substring(semicolonIndex + 1), NumberStyles.Number, CultureInfo.InvariantCulture, out frameIndex))
                {
                    fileName = fileName.Substring(0, semicolonIndex);

                    if (frameIndex > 0)
                    {
                        // indexes in truth are one-based
                        frameIndex--;
                    }
                }
            }

            return fileName;
        }

        /// <summary>
        /// Deletes all field and file information from this Truth object.
        /// </summary>
        public void Clear()
        {
            if (this.fields.Any() || this.files.Any())
            {
                this.fields.Clear();
                this.files.Clear();
            }
        }

        /// <summary>
        /// Merges the specified <see cref="Truth"/> with the current <see cref="Truth"/>.
        /// </summary>
        /// <param name="truth">The <see cref="Truth"/> to merge into the current truth.</param>
        /// <remarks>
        /// The method overrides the current <see cref="Truth"/> with non-blank values from the <c>truth</c>.
        /// Values that are present in the current <see cref="Truth"/> but are absent in <c>truth</c> remain unchanged.
        /// </remarks>
        public void MergeWith(Truth truth)
        {
            if (truth == null)
            {
                throw new ArgumentNullException(nameof(truth));
            }

            // add fields first
            foreach (string fieldName in truth.fields)
            {
                this.AddField(fieldName);
            }

            // map field names to indexes
            int[] fieldIndexes = truth.fields.Select(x => this.FieldIndex(x)).ToArray();

            int fieldCount = truth.fields.Count;
            foreach (KeyValuePair<FileKey, FileData> kvp in truth.files)
            {
                FileKey key = new FileKey(kvp.Key.FilePath, kvp.Key.FrameIndex);
                if (!this.files.TryGetValue(key, out FileData file))
                {
                    file = this.files[key] = new FileData();
                }

                for (int i = 0; i < fieldCount; i++)
                {
                    string text = kvp.Value.GetField(i);
                    if (!string.IsNullOrEmpty(text))
                    {
                        file.SetField(fieldIndexes[i], text, this.fields.Count);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the index of the specified field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The zero-based field index if a field with such name exists; otherwise -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <c>fieldName</c> is <b>null</b>.
        /// </exception>
        public int FieldIndex(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            return this.fields.IndexOf(fieldName.ToUpper(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Adds a new field to this Truth object.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The zero-based field index.</returns>
        public int AddField(string fieldName)
        {
            int fieldIndex = this.FieldIndex(fieldName);
            if (fieldIndex >= 0)
            {
                return fieldIndex;
            }

            this.fields.Add(fieldName.ToUpper(CultureInfo.InvariantCulture));

            return this.fields.Count - 1;
        }

        /// <summary>
        /// Deletes a field from this Truth object.
        /// </summary>
        /// <param name="fieldName">The name of the field to delete.</param>
        public void DeleteField(string fieldName)
        {
            int fieldIndex = this.FieldIndex(fieldName);
            if (fieldIndex < 0)
            {
                return;
            }

            this.fields.RemoveAt(fieldIndex);
            foreach (FileData file in this.files.Values)
            {
                file.DeleteField(fieldIndex);
            }
        }

        /// <summary>
        /// Deletes all fields from this Truth object.
        /// </summary>
        public void DeleteAllFields()
        {
            this.fields.Clear();

            foreach (FileData file in this.files.Values)
            {
                file.Clear();
            }
        }

        /// <summary>
        /// Deletes a file, specified by file name, from this Truth.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        public void DeleteFile(string fileName)
        {
            this.DeleteFile(fileName, null);
        }

        /// <summary>
        /// Deletes a file, specified by file name and TIFF index, from this Truth.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <param name="frameIndex">The zero-based index of this image in the TIFF file.</param>
        public void DeleteFile(string fileName, int? frameIndex)
        {
            this.files.Remove(new FileKey(fileName, frameIndex));
        }

        /// <summary>
        /// Deletes all files from this Truth object.
        /// </summary>
        public void DeleteAllFiles()
        {
            if (this.files.Any())
            {
                this.files.Clear();
            }
        }

        /// <summary>
        /// Saves this Truth object into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <b>Truth</b>.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is safe to dispose FileStream twice.")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "For compatibility reason")]
        public void SaveToFile(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    // write fields
                    writer.Write("#FILE");

                    if (this.fields != null)
                    {
                        writer.Write(",");
                        writer.Write(string.Join(",", this.fields.Select(x => qualifyString(x))));
                    }

                    writer.Write(Environment.NewLine);

                    // write files
                    if (this.files != null)
                    {
                        foreach (KeyValuePair<FileKey, FileData> kvp in this.files)
                        {
                            writer.Write(kvp.Key.FilePath);
                            if (kvp.Key.FrameIndex.HasValue)
                            {
                                writer.Write(";");
                                writer.Write(kvp.Key.FrameIndex.Value + 1);
                            }

                            if (kvp.Value.Data != null)
                            {
                                writer.Write(",");
                                writer.WriteLine(string.Join(",", kvp.Value.Data.Select(x => qualifyString(x ?? string.Empty))));
                            }
                        }
                    }
                }
            }

            string qualifyString(string text)
            {
                return (text.IndexOf(',') == -1) ? text : '"' + text.Replace("\"", "\"\"") + '"';
            }
        }

        /// <summary>
        /// Represents the key for the file collection.
        /// </summary>
        private class FileKey
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FileKey"/> class, using the file path.
            /// </summary>
            /// <param name="filePath">A file path.</param>
            /// <param name="frameIndex">A zero-based index of this image in the TIFF file.</param>
            /// <param name="useFullPathAsKey">Flag, directing to use full path as key.</param>
            /// <param name="orderIndex">Order index of this file.</param>
            public FileKey(string filePath, int? frameIndex)
            {
                this.FilePath = filePath.ToUpperInvariant();
                this.FileName = Path.GetFileName(this.FilePath);
                this.FrameIndex = frameIndex;
            }

            /// <summary>
            /// Gets the full file path.
            /// </summary>
            /// <value>
            /// The full file path.
            /// </value>
            public string FilePath { get; }

            /// <summary>
            /// Gets the file name.
            /// </summary>
            /// <value>
            /// The file name.
            /// </value>
            public string FileName { get; }

            /// <summary>
            /// Gets the zero-based index of this image in the multi-page file.
            /// </summary>
            /// <value>
            /// The zero-based index of this image in the multi-page file.
            /// </value>
            public int? FrameIndex { get; private set; }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                FileKey other = obj as FileKey;
                if (other == null)
                {
                    return false;
                }

                return this.FileName == other.FileName &&
                       this.FrameIndex == other.FrameIndex;
            }

            /// <inheritdoc />
            public override int GetHashCode() => this.FileName.GetHashCode() ^ this.FrameIndex.GetValueOrDefault();
        }

        /// <summary>
        /// Contains truth data for a file.
        /// </summary>
        private class FileData
        {
            /// <summary>
            /// Collection of strings that contains truth data for the file.
            /// </summary>
            private string[] data = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileData"/> class, using the file path.
            /// </summary>
            /// <param name="filePath">A file path.</param>
            /// <param name="frameIndex">A zero-based index of this image in the TIFF file.</param>
            /// <param name="useFullPathAsKey">Flag, directing to use full path as key.</param>
            /// <param name="orderIndex">Order index of this file.</param>
            public FileData()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileData"/> class, using the file path and truth data.
            /// </summary>
            /// <param name="filePath">A file path.</param>
            /// <param name="frameIndex">A zero-based index of this image in the TIFF file.</param>
            /// <param name="data">Truth data.</param>
            /// <param name="fieldCount">A number of fields in the truth file.</param>
            /// <param name="useFullPathAsKey">Flag, directing to use full path as key.</param>
            /// <param name="orderIndex">Order index of this file.</param>
            public FileData(IEnumerable<string> data, int fieldCount)
            {
                this.data = data.Take(fieldCount).Select(x => x.Trim().ToUpperInvariant()).ToArray();
            }

            /// <summary>
            /// Gets collection of strings that contains truth data for the file.
            /// </summary>
            /// <value>
            /// The collection of strings that contains truth data for the file.
            /// </value>
            public string[] Data => this.data;

            /// <summary>
            /// Removes all truth data from the file.
            /// </summary>
            public void Clear()
            {
                this.data = null;
            }

            /// <summary>
            /// Removes the truth data for field with the specified index.
            /// </summary>
            /// <param name="index">The index of the field to remove truth data for.</param>
            public void DeleteField(int index)
            {
                if (this.data != null && index < this.data.Length)
                {
                    this.data.RemoveAt(index);
                }
            }

            /// <summary>
            /// Gets the truth data for the specified field.
            /// </summary>
            /// <param name="index">The index of the field to get the truth data for.</param>
            /// <returns>The truth data.</returns>
            public string GetField(int index)
            {
                return this.data?.ElementAtOrDefault(index) ?? string.Empty;
            }

            /// <summary>
            /// Sets the truth data for the specified field.
            /// </summary>
            /// <param name="index">The index of the field to set the truth data for.</param>
            /// <param name="value">The truth data.</param>
            /// <param name="fieldCount">The number of fields in the truth file.</param>
            public void SetField(int index, string value, int fieldCount)
            {
                if (this.data == null || index >= this.data.Length)
                {
                    Array.Resize(ref this.data, Math.Max(index + 1, fieldCount));
                }

                this.data[index] = value?.Trim()?.ToUpper(CultureInfo.InvariantCulture);
            }
        }
    }
}
