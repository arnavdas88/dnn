﻿// -----------------------------------------------------------------------
// <copyright file="ClassificationReportWriter.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Writes the classification test report to a stream.
    /// </summary>
    /// <typeparam name="T">The type of the classification answer.</typeparam>
    public static class ClassificationReportWriter<T>
    {
        /// <summary>
        /// The short results format.
        /// </summary>
        private const string ShortFormat = "{0,8},{1,8},{2,8},";

        /// <summary>
        /// The long results format.
        /// </summary>
        private const string LongFormat = "{0,8},{1,8},{2,8},{3,8},{4,8},{5,8},{6,8}";

        /// <summary>
        /// Writes a test report.
        /// </summary>
        /// <param name="fileName">The path to a file to write the report to.</param>
        /// <param name="report">The report to write.</param>
        /// <param name="mode">Defines the components of the report to write.</param>
        public static void WriteReport(string fileName, ClassificationReport<T> report, ClassificationReportMode mode)
        {
            using (StreamWriter outputFile = File.CreateText(fileName))
            {
                ClassificationReportWriter<T>.WriteReport(outputFile, report, mode);
            }
        }

        /// <summary>
        /// Writes a test report.
        /// </summary>
        /// <param name="writer">The writer used to write the report.</param>
        /// <param name="report">The report to write.</param>
        /// <param name="mode">Defines the components of the report to write.</param>
        public static void WriteReport(TextWriter writer, ClassificationReport<T> report, ClassificationReportMode mode)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            IList<ClassSummary<T>> summaries = report.Classes.OrderBy(x => x.Label).ToList();

            if (summaries.Any())
            {
                // print report header
                if (mode.HasFlag(ClassificationReportMode.Summary))
                {
                    writer.WriteLine("=============================================================================");
                    writer.WriteLine("SUMMARY");
                    int maxClassNameLength = Math.Max(summaries.Max(x => x.Label.ToString().Length), 8);
                    writer.Write(string.Format(CultureInfo.InvariantCulture, "{{0,-{0}}},", maxClassNameLength), "Class");
                    writer.Write(ShortFormat, "Total", "%", "#");
                    writer.Write(LongFormat, "Total", "%", "#", "Error %", "Error #", "Valid %", "Valid #");
                    writer.WriteLine();

                    // print each class
                    foreach (ClassSummary<T> summary in summaries)
                    {
                        ClassificationReportWriter<T>.WriteClassStatistics(writer, summary, maxClassNameLength);
                    }

                    // print summary
                    writer.WriteLine();
                    ClassificationReportWriter<T>.WriteClassStatistics(writer, report.AllClasses, maxClassNameLength);
                }

                // print confusion matrix
                if (mode.HasFlag(ClassificationReportMode.ConfusionMatrix))
                {
                    writer.WriteLine();
                    writer.WriteLine("=============================================================================");
                    writer.WriteLine("CONFUSION MATRIX");
                    writer.WriteLine();
                    ClassificationReportWriter<T>.WriteConfusionMatrix(writer, report.ConfusionMatrix);
                }

                // print reject curves
                if (mode.HasFlag(ClassificationReportMode.RejectCurves))
                {
                    writer.WriteLine();
                    writer.WriteLine("=============================================================================");
                    writer.WriteLine("REJECT CURVES");
                    writer.WriteLine();
                    ClassificationReportWriter<T>.WriteRejectCurves(writer, report.AllClasses, summaries);
                }

                // print errors
                if (mode.HasFlag(ClassificationReportMode.Errors))
                {
                    writer.WriteLine();
                    writer.WriteLine("=============================================================================");
                    writer.WriteLine("ERRORS");
                    writer.WriteLine();
                    foreach (ClassSummary<T> summary in summaries)
                    {
                        ClassificationReportWriter<T>.WriteClassificationErrors(writer, summary);
                    }
                }
            }

            // print file results
            if (mode.HasFlag(ClassificationReportMode.Answers))
            {
                writer.WriteLine();
                writer.WriteLine("=============================================================================");
                writer.WriteLine("FILE RESULTS");
                writer.WriteLine();
                ClassificationResult<T>.Write(writer, report.Results);
            }
        }

        /// <summary>
        /// Writes a test report into a string.
        /// </summary>
        /// <param name="report">The report to write.</param>
        /// <param name="mode">Defines the components of the report to write.</param>
        /// <returns>
        /// The <see cref="string"/> that contains test report.
        /// </returns>
        public static string WriteReport(ClassificationReport<T> report, ClassificationReportMode mode)
        {
            using (StringWriter stream = new StringWriter())
            {
                ClassificationReportWriter<T>.WriteReport(stream, report, mode);
                return stream.ToString();
            }
        }

        /// <summary>
        /// Writes a short report that includes accept rate, accept count, and total count.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="statistics">A classification statistics to get report data from.</param>
        private static void WriteShortStatistics(TextWriter writer, ClassificationStatistics statistics)
        {
            writer.Write(
                ShortFormat,
                statistics.Count,
                Percentage(statistics.Accepted, statistics.Count),
                statistics.Accepted);
        }

        /// <summary>
        /// Writes a long report that includes accept rate, accept count, total count, error rate, error count, valid rate, and valid count.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="statistics">A classification statistics to get report data from.</param>
        private static void WriteLongStatistics(TextWriter writer, ClassificationStatistics statistics)
        {
            writer.Write(
                LongFormat,
                statistics.Count,
                Percentage(statistics.Accepted, statistics.Count),
                statistics.Accepted,
                Percentage(statistics.Errors, statistics.Accepted),
                statistics.Errors,
                Percentage(statistics.Valid, statistics.Count),
                statistics.Valid);
        }

        /// <summary>
        /// Writes class classification statistics.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="summary">Classification results for the class.</param>
        /// <param name="classNameLength">The number of characters in the class name column.</param>
        private static void WriteClassStatistics(TextWriter writer, ClassSummary<T> summary, int classNameLength)
        {
            writer.Write(string.Format(CultureInfo.InvariantCulture, "{{0,-{0}}},", classNameLength), summary.Label);
            WriteShortStatistics(writer, summary.Statistics.All);
            WriteLongStatistics(writer, summary.Statistics.WithTruth);
            writer.WriteLine();
        }

        /// <summary>
        /// Writes class errors.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="summary">Classification results for the class.</param>
        private static void WriteClassificationErrors(TextWriter writer, ClassSummary<T> summary)
        {
            int acceptedErrors = summary.Errors.Count();
            if (acceptedErrors > 0)
            {
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", summary.Label, acceptedErrors));
                writer.WriteLine();
                ClassificationResult<T>.Write(writer, summary.Errors);

                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Writes class reject curve.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="summary">Classification results for the class.</param>
        private static void WriteClassRejectCurve(TextWriter writer, ClassSummary<T> summary)
        {
            float[] errorRates = { 0.001f, 0.005f, 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.09f, 0.10f, 0.11f };

            writer.WriteLine(summary.Label);
            writer.WriteLine();

            writer.WriteLine(
                "{0,-10}{1,-10}{2,-10}{3,-10}",
                "Target",
                "Error",
                "Accept",
                "Threshold");

            RejectCurveTarget[] targets = summary.Statistics.RejectCurveTruth.GetTargets(errorRates);
            foreach (RejectCurveTarget target in targets)
            {
                if (target.Point.HasValue)
                {
                    writer.WriteLine(
                        "{0,-10:P2}{1,-10:P2}{2,-10:P2}{3,-10:N0}",
                        target.Target,
                        target.Point.Value.ErrorRate,
                        target.Point.Value.AcceptRate,
                        target.Point.Value.Threshold);
                }
                else
                {
                    writer.WriteLine(
                        "{0,-10:P2}{1,-10}{1,-10}{1,-10}",
                        target.Target,
                        "N/A");
                }
            }

            writer.WriteLine();

            // write area
            float average = summary.Statistics.RejectCurveTruth.GetArea(0.0f, errorRates.Last(), 100);
            writer.WriteLine(
                "Area: {0:P2}",
                average);

            writer.WriteLine("-----------------------------------------------------------------------------");
            writer.WriteLine();
        }

        /// <summary>
        /// Writes all reject curves.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="all">Classification results for all classes.</param>
        /// <param name="classes">Classification results for each class.</param>
        private static void WriteRejectCurves(TextWriter writer, ClassSummary<T> all, IList<ClassSummary<T>> classes)
        {
            float[] errorRates = { 0.001f, 0.005f, 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.09f, 0.10f, 0.11f };

            List<ClassSummary<T>> combined = new List<ClassSummary<T>>() { all };
            combined.AddRange(classes);

            string separator = new string('-', 10 + (30 * combined.Count));

            // write class names
            writer.Write("{0,-10}", string.Empty);
            foreach (ClassSummary<T> cls in combined)
            {
                writer.Write("{0,-30}", cls.Label);
            }

            writer.WriteLine();
            writer.WriteLine(separator);

            // write header
            writer.Write("{0,-10}", "Target");
            for (int i = 0, ii = combined.Count; i < ii; i++)
            {
                writer.Write("{0,-10}{1,-10}{2,-10}", "Error", "Accept", "Thresh");
            }

            writer.WriteLine();

            // write targets
            foreach (float errorRate in errorRates)
            {
                writer.Write("{0,-10:P2}", errorRate);

                foreach (ClassSummary<T> cls in combined)
                {
                    RejectCurveTarget target = cls.Statistics.RejectCurveTruth.GetTarget(errorRate);
                    if (target.Point.HasValue)
                    {
                        writer.Write(
                            "{0,-10:P2}{1,-10:P2}{2,-10:N0}",
                            target.Point.Value.ErrorRate,
                            target.Point.Value.AcceptRate,
                            target.Point.Value.Threshold);
                    }
                    else
                    {
                        writer.Write(
                            "{0,-10}{0,-10}{0,-10}",
                            "N/A");
                    }
                }

                writer.WriteLine();
            }

            writer.WriteLine(separator);

            // write area
            writer.Write("{0,-10}", "Area:");
            foreach (ClassSummary<T> cls in combined)
            {
                float average = cls.Statistics.RejectCurveTruth.GetArea(0.0f, errorRates.Last(), 100);
                writer.Write("{0,-30:P2}", average);
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Writes confusion matrix.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="matrix">The confusion matrix to write.</param>
        private static void WriteConfusionMatrix(TextWriter writer, ConfusionMatrix<T> matrix)
        {
            writer.Write(matrix.ToString());
        }

        /// <summary>
        /// Calculate a percentage and converts it into report-friendly format.
        /// </summary>
        /// <param name="nominator">The percentage nominator.</param>
        /// <param name="denominator">The percentage denominator.</param>
        /// <returns><c>nominator</c> divided by <c>denominator</c> if <c>denominator</c> is greater than zero; otherwise, <b>N/A</b>.</returns>
        private static string Percentage(int nominator, int denominator)
        {
            if (denominator > 0)
            {
                return ((float)nominator / denominator).ToString("P2", CultureInfo.InvariantCulture);
            }
            else
            {
                return "     N/A";
            }
        }
    }
}
