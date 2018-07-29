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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides samples for network learning.
    /// </summary>
    /// <typeparam name="TLabel">The label type.</typeparam>
    public abstract class TestImageProvider<TLabel> : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="TestImageProvider{TLabel}"/> class.
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This member overrides <see cref="object.Finalize"/>, and more complete documentation might be available in that topic.
        /// </remarks>
        ~TestImageProvider()
        {
            // Use C# destructor syntax for finalization code.
            // This destructor will run only if the Dispose method does not get called.
            // It gives your base class the opportunity to finalize.
            // Do not provide destructor in types derived from this class.

            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of readability and maintainability.
            if (!this.disposed)
            {
                this.Dispose(false);

                this.disposed = true;
            }
        }

        /// <summary>
        /// Gets or sets the width of the samples this provider generates.
        /// </summary>
        /// <value>
        /// The width of the samples this provider generates.
        /// </value>
        public int Width { get; protected set; }

        /// <summary>
        /// Gets or sets the height of the samples this provider generates.
        /// </summary>
        /// <value>
        /// The height of the samples this provider generates.
        /// </value>
        public int Height { get; protected set; }

        /// <summary>
        /// Creates a new <see cref="TestImageProvider{TLabel}"/> from a Json object.
        /// </summary>
        /// <param name="width">The width of the samples this provider generates.</param>
        /// <param name="height">The height of the samples this provider generates.</param>
        /// <param name="classes">The list of classes to generate.</param>
        /// <param name="blankClass">The blank class.</param>
        /// <param name="parameters">The Json parameters.</param>
        /// <returns>The <see cref="TestImageProvider{TLabel}"/> object this method creates.</returns>
        public static TestImageProvider<string> CreateFromJson(int width, int height, IList<string> classes, string blankClass, JObject parameters)
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
                        DirectoryDataProvider provider = new DirectoryDataProvider(width, height);
                        try
                        {
                            if (parameters.GetValue("parameters", StringComparison.OrdinalIgnoreCase) is JArray data)
                            {
                                JsonSerializer jsonSerializer = new JsonSerializer();
                                using (JTokenReader jtokenReader = new JTokenReader(data))
                                {
                                    List<Test> tests = new List<Test>();
                                    jsonSerializer.Populate(jtokenReader, tests);

                                    foreach (Test test in tests)
                                    {
                                        provider.AddDirectory(test.Path, test.Recursive, test.Class);
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
        /// Releases all resources used by this object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method frees all unmanaged resources used by the object.
        /// The method invokes the protected <see cref="Dispose(bool)"/> method with the <c>disposing</c> parameter set to <b>true</b>.
        /// </para>
        /// <para>
        /// Call <b>Dispose</b> when you are finished using the object.
        /// The <b>Dispose</b> method leaves the object in an unusable state.
        /// After calling <b>Dispose</b>, you must release all references to the object so the garbage collector can reclaim the memory that the object was occupying.
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);

                this.disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Generates samples for network learning for specified labels.
        /// </summary>
        /// <param name="labels">The labels to generate samples for.</param>
        /// <returns>The sequence of samples. Each sample consist of image and a ground truth.</returns>
        public abstract IEnumerable<TestImage> Generate(ISet<TLabel> labels);

        /// <summary>
        /// Infrastructure. Releases the unmanaged resources used by the object and, optionally, releases managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        /// <remarks>
        /// <para>
        /// This method is called by the public <see cref="Dispose()"/> method and the <see cref="Finalize"/> method.
        /// <see cref="Dispose()"/> invokes the protected <b>Dispose(Boolean)</b> method with the <paramref name="disposing"/> parameter set to <b>true</b>.
        /// <see cref="Finalize"/> invokes <b>Dispose(Boolean)</b> with <paramref name="disposing"/> set to <b>false</b>.
        /// </para>
        /// <para>
        /// When the <paramref name="disposing"/> parameter is <b>true</b>,
        /// this method releases all resources held by any managed objects that this object references.
        /// This method invokes the <see cref="Dispose()"/> method of each referenced object.
        /// </para>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // Dispose(bool disposing) executes in two distinct scenarios.
            // If disposing equals true, the method has been called directly or indirectly by a user's code.
            // Managed and unmanaged resources can be disposed.
            // If disposing equals false, the method has been called by the runtime from inside the finalizer and you should not reference other objects.
            // Only unmanaged resources can be disposed.
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
