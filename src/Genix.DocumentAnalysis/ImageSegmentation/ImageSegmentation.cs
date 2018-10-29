// -----------------------------------------------------------------------
// <copyright file="ImageSegmentation.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#define SESSION_DIAG

namespace Genix.DocumentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Genix.Drawing;
    using Genix.Imaging;

    /// <summary>
    /// Segments the <see cref="Image"/> into shapes of different types (i.e. lines, pictures, tables).
    /// </summary>
    public class ImageSegmentation
    {
        /// <summary>
        /// List all available locators. The order in array important - it specifies the order of locators run.
        /// </summary>
        private static readonly Dictionary<LocatorTypes, Type> Types = new Dictionary<LocatorTypes, Type>()
        {
            { LocatorTypes.PictureLocator, typeof(PictureLocator) },
            { LocatorTypes.LineLocator, typeof(LineLocator) },
            { LocatorTypes.CheckboxLocator, typeof(CheckboxLocator) },
            { LocatorTypes.TextLocator, typeof(TextLocator) },
        };

        /// <summary>
        /// The locators that this <see cref="ImageSegmentation"/> uses.
        /// </summary>
        private readonly Dictionary<LocatorTypes, LocatorBase> locators = new Dictionary<LocatorTypes, LocatorBase>();

#if SESSION_DIAG
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Dictionary<string, long> performance = new Dictionary<string, long>();
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSegmentation"/> class.
        /// </summary>
        public ImageSegmentation()
        {
        }

        /// <summary>
        /// Gets or sets the types of locators this <see cref="ImageSegmentation"/> uses to segment the <see cref="Image"/>.
        /// </summary>
        /// <value>
        /// The <see cref="LocatorTypes"/> enumeration value.
        /// </value>
        public LocatorTypes Locators { get; set; } = LocatorTypes.All;

        /// <summary>
        /// Segments the <see cref="Image"/> into the specified <see cref="Shape"/> objects.
        /// </summary>
        /// <param name="image">An image to parse.</param>
        /// <param name="areas">The areas on the <paramref name="image"/> to locate shapes in.</param>
        /// <param name="cancellationToken">The cancellation token used to notify the <see cref="ImageSegmentation"/> that operation should be canceled.</param>
        /// <returns>The <see cref="PageShape"/> object this method creates.</returns>
        public PageShape Segment(Image image, IList<Rectangle> areas, CancellationToken cancellationToken)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            // check if full image
            if (areas != null && areas.Any() && Rectangle.Union(areas) == image.Bounds)
            {
                areas = null;
            }

            PageShape page = new PageShape(image.Bounds, image.HorizontalResolution, image.VerticalResolution);
            Image originalImage = image.Clone(true);

            foreach (LocatorTypes locatorType in ImageSegmentation.Types.Keys)
            {
                if (this.Locators.HasFlag(locatorType))
                {
#if SESSION_DIAG
                    this.StartStopwatch();
#endif
                    LocatorBase locator = InitializeLocator(locatorType);
                    if (locatorType == LocatorTypes.LineLocator)
                    {
                        // always run line locator on full image
                        locator.Locate(page, image, originalImage, null, cancellationToken);

                        // delete found lines from the image
                        ////LineLocator.DeleteLines(parameters.LineLocatorParameters, image, page.GetAllLines(), cancellationToken);
                    }
                    else
                    {
                        locator.Locate(page, image, originalImage, areas, cancellationToken);
                    }
#if SESSION_DIAG
                    this.StopStopwatch(locatorType.ToString());
#endif
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            return page;

            LocatorBase InitializeLocator(LocatorTypes locatorType)
            {
                if (!this.locators.TryGetValue(locatorType, out LocatorBase locator))
                {
                    try
                    {
                        this.locators[locatorType] = locator = (LocatorBase)Activator.CreateInstance(
                            ImageSegmentation.Types[locatorType],
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            new object[] { },
                            null);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("Cannot create locator.", e);
                    }
                }

                return locator;
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that contains a performance report for this <see cref="ImageSegmentation"/>.
        /// </summary>
        /// <param name="count">The number of segmentations performed.</param>
        /// <returns>The <see cref="string"/> that contains a performance report.</returns>
        public string PrintPerformanceReport(int count)
        {
#if SESSION_DIAG
            return string.Join(
                Environment.NewLine,
                this.performance.Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", kvp.Key, kvp.Value / count)));
#else
            return null;
#endif
        }

#if SESSION_DIAG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartStopwatch()
        {
            this.stopwatch.Restart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopStopwatch(string actionName)
        {
            this.stopwatch.Stop();
            long timeSpent = this.stopwatch.ElapsedTicks;

            if (this.performance.TryGetValue(actionName, out long value))
            {
                this.performance[actionName] = value + timeSpent;
            }
            else
            {
                this.performance[actionName] = timeSpent;
            }
        }
#endif
    }
}
