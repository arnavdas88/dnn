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

            private BaseCommand command;
            private long totalImages = 0;

            protected override bool OnConfigure(string[] args)
            {
                if (args == null)
                {
                    throw new ArgumentNullException(nameof(args));
                }

                CommandLineParser parser = new CommandLineParser("classify");
                parser.AddCommand(new LearnCommand());
                parser.AddCommand(new TestCommand());

                this.command = parser.Parse(args) as BaseCommand;
                if (this.command != null)
                {
                    if (!string.IsNullOrEmpty(this.command.LogFileName))
                    {
                        this.OpenLogFile(this.command.LogFileName);
                    }
                }

                return this.command != null;
            }

            protected override int OnRun()
            {
                this.totalTimeCounter.Start();
                this.command.Run(this);
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

            private PointsOfInterestClassifier Learn(string sourcePath, string truthPath)
            {
                this.WriteLine(null, "Learning...");

                PointsOfInterestClassifier classifier = new PointsOfInterestClassifier();

                using (DirectoryDataProvider dataProvider = new DirectoryDataProvider(0, 0))
                {
                    dataProvider.Add(sourcePath, false, truthPath, "#Class");

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
                                source.SourceId.ToFileName(false));

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

            private void Test(PointsOfInterestClassifier classifier, string sourcePath, string truthPath)
            {
                this.WriteLine(null, "Testing...");

                List<ClassificationResult<string>> results = new List<ClassificationResult<string>>();

                using (DirectoryDataProvider dataProvider = new DirectoryDataProvider(0, 0))
                {
                    dataProvider.Add(sourcePath, false, truthPath, "#Class");

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
                                source.SourceId.ToFileName(false));

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

            private class LearnCommand : BaseCommand
            {
                private readonly CommandLineArgument source;
                private readonly CommandLineArgument truth;

                public LearnCommand()
                    : base("learn", "learn classifier", false)
                {
                    this.source = this.AddArgument("source", "source path or list file", CommandLineArgumentTypes.FileMustExist);
                    this.truth = this.AddArgument("truth", "path to a truth file", CommandLineArgumentTypes.FileMustExist);
                }

                private string Source => this.source.Value;

                private string Truth => this.truth.Value;

                public override void Run(InnerProgram program)
                {
                    ////string train = @"Z:\Test\Classification2\Data\t95_r285_q954_train.txt";
                    string test = @"Z:\Test\Classification2\Data\t95_r285_q954_test.txt";
                    ////string truth = @"Z:\Test\Classification2\Data\t95_r285_q954_truth.txt";
                    ////string source = @"Z:\Test\Classification2\Data\Gerber_test.txt";
                    ////string source = @"Z:\Test\Classification2\Data\Gerber_test.txt";
                    ////string truth = @"Z:\Test\Classification2\Data\Gerber_truth.txt";

                    PointsOfInterestClassifier classifier = program.Learn(this.Source, this.Truth);
                    program.Test(classifier, test, this.Truth);
                }
            }

            private class TestCommand : BaseCommand
            {
                private readonly CommandLineArgument source;
                private readonly CommandLineArgument truth;

                public TestCommand()
                    : base("test", "test classifier", false)
                {
                    this.source = this.AddArgument("source", "source path or list file", CommandLineArgumentTypes.FileMustExist);
                    this.truth = this.AddArgument("truth", "path to a truth file", CommandLineArgumentTypes.FileMustExist);
                }

                private string Source => this.source.Value;

                private string Truth => this.truth.Value;

                public override void Run(InnerProgram program)
                {
                    ////string test = @"Z:\Test\Classification2\Data\t95_r285_q954_test.txt";
                    ////string source = @"Z:\Test\Classification2\Data\Gerber_test.txt";
                    ////string truth = @"Z:\Test\Classification2\Data\t95_r285_q954_truth.txt";
                    ////string truth = @"Z:\Test\Classification2\Data\Gerber_truth.txt";

                    PointsOfInterestClassifier classifier = null;
                    program.Test(classifier, this.Source, this.Truth);
                }
            }

            private abstract class BaseCommand : CommandLineCommand
            {
                private readonly CommandLineOption logFileName;

                public BaseCommand(string name, string description, bool useLog)
                    : base(name, description)
                {
                    if (useLog)
                    {
                        this.logFileName = this.AddOption("log", "log file", "path to log file", CommandLineOptionTypes.PathMustExist);
                    }
                }

                public string LogFileName => this.logFileName?.Value;

                public abstract void Run(InnerProgram program);
            }
        }
    }
}