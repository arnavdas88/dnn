// -----------------------------------------------------------------------
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
        public static void WriteReport(string fileName, ClassificationReport<T> report)
        {
            using (StreamWriter outputFile = File.CreateText(fileName))
            {
                ClassificationReportWriter<T>.WriteReport(outputFile, report);
            }
        }

        /// <summary>
        /// Writes a test report.
        /// </summary>
        /// <param name="writer">The writer used to write the report.</param>
        /// <param name="report">The report to write.</param>
        public static void WriteReport(StreamWriter writer, ClassificationReport<T> report)
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

                // print confusion matrix
                writer.WriteLine();
                writer.WriteLine("=============================================================================");
                writer.WriteLine("CONFUSION MATRIX");
                writer.WriteLine();
                ClassificationReportWriter<T>.WriteConfusionMatrix(writer, report.ConfusionMatrix);

                // print reject curves
                writer.WriteLine();
                writer.WriteLine("=============================================================================");
                writer.WriteLine("REJECT CURVES");
                writer.WriteLine();
                ClassificationReportWriter<T>.WriteRejectCurves(writer, report.AllClasses, summaries);

                // print errors
                writer.WriteLine();
                writer.WriteLine("=============================================================================");
                writer.WriteLine("ERRORS");
                writer.WriteLine();
                foreach (ClassSummary<T> summary in summaries)
                {
                    ClassificationReportWriter<T>.WriteClassificationErrors(writer, summary);
                }
            }

            // print file results
            /*writer.WriteLine();
            writer.WriteLine("=============================================================================");
            writer.WriteLine("FILE RESULTS");
            writer.WriteLine();
            ClassificationResult.Write(writer, report.Results);*/
        }

        /// <summary>
        /// Writes a short report that includes accept rate, accept count, and total count.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="statistics">A classification statistics to get report data from.</param>
        private static void WriteShortStatistics(StreamWriter writer, ClassificationStatistics statistics)
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
        private static void WriteLongStatistics(StreamWriter writer, ClassificationStatistics statistics)
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
        private static void WriteClassStatistics(StreamWriter writer, ClassSummary<T> summary, int classNameLength)
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
        private static void WriteClassificationErrors(StreamWriter writer, ClassSummary<T> summary)
        {
            int acceptedErrors = summary.Errors.Count(/*error => error.IsAccepted*/);
            if (acceptedErrors > 0)
            {
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", summary.Label, acceptedErrors));
                writer.WriteLine();
                foreach (ClassificationResult<T> error in summary.Errors)
                {
                    ////if (error.IsAccepted)
                    {
                        writer.WriteLine("{0},{1},{2}", Truth.MakeFileName(error.SourceId.Id, error.SourceId.FrameIndex), error.Predicted, error.Confidence);
                    }
                }

                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Writes class reject curve.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="summary">Classification results for the class.</param>
        private static void WriteClassRejectCurve(StreamWriter writer, ClassSummary<T> summary)
        {
            double[] errorRates = { 0.001, 0.005, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.10, 0.11 };

            writer.WriteLine(summary.Label);
            writer.WriteLine();

            writer.WriteLine(
                "{0,-10}{1,-10}{2,-10}{3,-10}",
                "Target",
                "Error",
                "Accept",
                "Threshold");

            Genix.Lab.RejectCurveTarget[] targets = summary.Statistics.RejectCurveTruth.GetTargets(errorRates);
            foreach (Genix.Lab.RejectCurveTarget target in targets)
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
            double average = summary.Statistics.RejectCurveTruth.GetArea(0.0, errorRates.Last(), 100);
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
        private static void WriteRejectCurves(StreamWriter writer, ClassSummary<T> all, IList<ClassSummary<T>> classes)
        {
            double[] errorRates = { 0.001, 0.005, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.10, 0.11 };

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
            foreach (double errorRate in errorRates)
            {
                writer.Write("{0,-10:P2}", errorRate);

                foreach (ClassSummary<T> cls in combined)
                {
                    Genix.Lab.RejectCurveTarget target = cls.Statistics.RejectCurveTruth.GetTarget(errorRate);
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
                double average = cls.Statistics.RejectCurveTruth.GetArea(0.0, errorRates.Last(), 100);
                writer.Write("{0,-30:P2}", average);
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Writes confusion matrix.
        /// </summary>
        /// <param name="writer">A stream to write the report to.</param>
        /// <param name="matrix">The confusion matrix to write.</param>
        private static void WriteConfusionMatrix(StreamWriter writer, ConfusionMatrix<T> matrix)
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
                return ((double)nominator / denominator).ToString("P2", CultureInfo.InvariantCulture);
            }
            else
            {
                return "     N/A";
            }
        }
    }
}
