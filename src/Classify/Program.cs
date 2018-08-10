// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.NetClassify
{
    using System.Diagnostics;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using Genix.DocumentAnalysis;
    using Genix.DocumentAnalysis.Classification;
    using Genix.Imaging.Lab;

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
            private Stopwatch totalTimeCounter = new Stopwatch();
            private Stopwatch localTimeCounter = new Stopwatch();
            private long totalImages = 0;

            protected override bool OnConfigure(string[] args)
            {
                return true;
            }

            protected override int OnRun()
            {
                this.totalTimeCounter.Start();
                this.Learn();
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

            private void Learn()
            {
                ClassifierProgress<TestImage> progress = new ClassifierProgress<TestImage>(
                    (source, index) =>
                    {
                        this.StopwatchRestart();
                    },
                    (source, index, exception) =>
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

                PointsOfInterestClassifier classifier = new PointsOfInterestClassifier();

                using (DirectoryDataProvider dataProvider = new DirectoryDataProvider(0, 0))
                {
                    dataProvider.AddDirectory(
                        @"Z:\Test\Classification2\Data\Gerber_train.txt",
                        false,
                        @"Z:\Test\Classification2\Data\Gerber_truth.txt",
                        "#Class");

                    classifier.Train<TestImage>(
                        dataProvider.Generate(null),
                        (x, cancellationToken) => new ImageSource(x.SourceId, x.Image),
                        x => string.Concat(x.Labels),
                        progress,
                        CancellationToken.None);
                }
            }
        }
    }
}