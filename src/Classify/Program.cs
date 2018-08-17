// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.NetClassify
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using Genix.DocumentAnalysis;
    using Genix.DocumentAnalysis.Classification;
    using Genix.Imaging.Lab;
    using Genix.Lab;

    internal sealed class Program
    {
        [HandleProcessCorruptedStateExceptions]
        public static int Main(string[] args)
        {
            InnerProgram program = new InnerProgram();
            return program.Run(args);
        }

        private class InnerProgram : Lab.Program
        {
            private readonly Stopwatch totalTimeCounter = new Stopwatch();
            private readonly Stopwatch localTimeCounter = new Stopwatch();
            private long totalImages = 0;

            protected override bool OnConfigure(string[] args)
            {
                return true;
            }

            protected override int OnRun()
            {
                this.totalTimeCounter.Start();
                PointsOfInterestClassifier classifier = this.Learn();
                this.Test(classifier);
                this.totalTimeCounter.Stop();
                return 0;
            }

            protected override void OnFinish()
            {
                if (this.totalImages > 0)
                {
                    this.WriteLine(null, "Total images: {0}", this.totalImages);

                    long millisecsPerImage = this.totalTimeCounter.ElapsedMilliseconds / this.totalImages;
                    double imagesPerMinute = 60000.0 / millisecsPerImage;
                    this.WriteLine(null, "Average time: {0:N0} ms per image, {1:F2} images per minute.", millisecsPerImage, imagesPerMinute);
                }
            }

            private PointsOfInterestClassifier Learn()
            {
                this.WriteLine(null, "Learning...");

                PointsOfInterestClassifier classifier = new PointsOfInterestClassifier();

                using (DirectoryDataProvider dataProvider = new DirectoryDataProvider(0, 0))
                {
                    dataProvider.AddDirectory(
                        @"Z:\Test\Classification2\Data\t95_r285_q954_train.txt",
                        ////@"Z:\Test\Classification2\Data\Gerber_train.txt",
                        false,
                        @"Z:\Test\Classification2\Data\t95_r285_q954_truth.txt",
                        ////@"Z:\Test\Classification2\Data\Gerber_truth.txt",
                        "#Class");

                    ClassifierProgress<TestImage> progress = new ClassifierProgress<TestImage>(
                        (source, index) =>
                        {
                            this.StopwatchRestart();
                        },
                        (source, index, answer, exception) =>
                        {
                            long duration = this.StopwatchStop();
                            Interlocked.Increment(ref this.totalImages);

                            this.Write(
                                null,
                                "({0})\tFile: {1} ... ",
                                index,
                                source.SourceId.ToFileName());

                            if (exception != null)
                            {
                                this.WriteLine(null, "ERROR.");
                                this.WriteException(null, exception);
                            }
                            else
                            {
                                this.WriteLine(null, "OK ({0} ms)", duration);
                            }
                        });

                    classifier.Train<TestImage>(
                        dataProvider.Generate(null),
                        (x, cancellationToken) => new ImageSource(x.SourceId, x.Image),
                        x => string.Concat(x.Labels),
                        progress,
                        CancellationToken.None);
                }

                return classifier;
            }

            private void Test(PointsOfInterestClassifier classifier)
            {
                this.WriteLine(null, "Testing...");

                List<ClassificationResult<string>> results = new List<ClassificationResult<string>>();

                using (DirectoryDataProvider dataProvider = new DirectoryDataProvider(0, 0))
                {
                    dataProvider.AddDirectory(
                        @"Z:\Test\Classification2\Data\t95_r285_q954_test.txt",
                        ////@"Z:\Test\Classification2\Data\Gerber_test.txt",
                        false,
                        @"Z:\Test\Classification2\Data\t95_r285_q954_truth.txt",
                        ////@"Z:\Test\Classification2\Data\Gerber_truth.txt",
                        "#Class");

                    ClassifierProgress<TestImage> progress = new ClassifierProgress<TestImage>(
                        (source, index) =>
                        {
                            this.StopwatchRestart();
                        },
                        (source, index, answer, exception) =>
                        {
                            long duration = this.StopwatchStop();
                            Interlocked.Increment(ref this.totalImages);

                            this.Write(
                                null,
                                "({0})\tFile: {1} ... ",
                                index,
                                source.SourceId.ToFileName());

                            if (exception != null)
                            {
                                this.WriteLine(null, "ERROR.");
                                this.WriteException(null, exception);
                            }
                            else
                            {
                                this.WriteLine(
                                    null,
                                    "OK ({0} ms) {1} {2:F4}",
                                    duration,
                                    answer?.ClassName ?? string.Empty,
                                    answer?.Confidence ?? 0.0);
                            }
                        });

                    foreach ((TestImage image, Answer answer) in classifier.Classify<TestImage>(
                        dataProvider.Generate(null),
                        (x, cancellationToken) => new ImageSource(x.SourceId, x.Image),
                        progress,
                        CancellationToken.None))
                    {
                        results.Add(new ClassificationResult<string>(
                            answer.Id,
                            answer.ClassName,
                            string.Concat(image.Labels),
                            answer.Confidence,
                            answer.Confidence >= 0.2f));
                    }
                }

                // write report
                ClassificationReport<string> testReport = new ClassificationReport<string>(results);
                using (StreamWriter outputFile = new StreamWriter(Console.OpenStandardOutput()))
                ////using (StreamWriter outputFile = File.CreateText(this.options.OutputFileName))
                {
                    ClassificationReportWriter<string>.WriteReport(outputFile, testReport);
                }
            }
        }
    }
}