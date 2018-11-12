// -----------------------------------------------------------------------
// <copyright file="TestImageProvider.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides samples for network learning.
    /// </summary>
    /// <typeparam name="TLabel">The label type.</typeparam>
    public abstract class TestImageProvider<TLabel> : DisposableObject
    {
        /// <summary>
        /// Creates a new <see cref="TestImageProvider{TLabel}"/> from a Json object.
        /// </summary>
        /// <param name="width">The width of the samples this provider generates.</param>
        /// <param name="height">The height of the samples this provider generates.</param>
        /// <param name="classes">The list of classes to generate.</param>
        /// <param name="blankClass">The blank class.</param>
        /// <param name="parameters">The Json parameters.</param>
        /// <returns>The <see cref="TestImageProvider{TLabel}"/> object this method creates.</returns>
        public static TestImageProvider<string> CreateFromJson(int width, int height, ICollection<string> classes, string blankClass, JObject parameters)
        {
            string name = parameters.GetValue("name", StringComparison.OrdinalIgnoreCase)?.Value<string>();
            switch (name?.ToUpperInvariant())
            {
                case "CANVAS":
                    {
                        CanvasDataProvider provider = new CanvasDataProvider(width, height, classes, blankClass);
                        try
                        {
                            if (parameters.GetValue("parameters", StringComparison.OrdinalIgnoreCase) is JObject data)
                            {
                                if (data.GetValue("fonts", StringComparison.OrdinalIgnoreCase) != null)
                                {
                                    provider.Fonts.Clear();
                                }

                                JsonSerializer jsonSerializer = new JsonSerializer();
                                using (JTokenReader jtokenReader = new JTokenReader(data))
                                {
                                    jsonSerializer.Populate(jtokenReader, provider);
                                }
                            }

                            return provider;
                        }
                        catch
                        {
                            provider.Dispose();
                            throw;
                        }
                    }

                case "DIRECTORY":
                    {
                        DirectoryDataProvider provider = new DirectoryDataProvider();
                        try
                        {
                            JToken parametersToken = parameters.GetValue("parameters", StringComparison.OrdinalIgnoreCase);
                            if (parametersToken.Type == JTokenType.String)
                            {
                                string fileName = (string)parametersToken;
                                if (!File.Exists(fileName))
                                {
                                    throw new FileNotFoundException("Cannot find provider parameters file.", fileName);
                                }

                                parametersToken = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(fileName, Encoding.UTF8));
                            }

                            if (parametersToken is JArray arr)
                            {
                                ReadJsonArray(arr);
                            }
                            else
                            {
                                throw new ArgumentException("Cannot recognize directory data provider parameters.");
                            }

                            void ReadJsonArray(JArray jarray)
                            {
                                JsonSerializer jsonSerializer = new JsonSerializer();
                                using (JTokenReader jtokenReader = new JTokenReader(jarray))
                                {
                                    List<Test> tests = new List<Test>();
                                    jsonSerializer.Populate(jtokenReader, tests);

                                    foreach (Test test in tests)
                                    {
                                        if (!string.IsNullOrEmpty(test.Class))
                                        {
                                            provider.Add(test.Path, test.Recursive, test.Class);
                                        }
                                        else
                                        {
                                            provider.Add(test.Path, test.Recursive, null, null);
                                        }
                                    }
                                }
                            }

                            return provider;
                        }
                        catch
                        {
                            provider.Dispose();
                            throw;
                        }
                    }

                default:
                    throw new ArgumentException("Element should have 'name' property. Valid values are: Canvas|Directory.");
            }
        }

        /// <summary>
        /// Generates samples for network learning for specified labels.
        /// </summary>
        /// <param name="labels">The labels to generate samples for.</param>
        /// <returns>The sequence of samples. Each sample consist of image and a ground truth.</returns>
        public abstract IEnumerable<TestImage> Generate(ISet<TLabel> labels);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is initialized by JsonSerializer.")]
        private class Test
        {
            public string Path { get; set; }

            public bool Recursive { get; set; } = true;

            public string Class { get; set; }
        }
    }
}
