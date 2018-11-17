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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Threading;
    using Genix.DNN;
    using Genix.Imaging.Lab;
    using Genix.Lab;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Imaging;
    using Genix.MachineLearning.LanguageModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
            private Options options = null;

            private Stopwatch totalTimeCounter = new Stopwatch();
            private Stopwatch localTimeCounter = new Stopwatch();
            private long totalImages;

            protected override bool OnConfigure(string[] args)
            {
                // configure application
                this.options = Options.Create(args);

                // open log file
                this.OpenLogFile(this.options.LogFileName);

                return true;
            }

            protected override int OnRun()
            {
                this.totalTimeCounter.Start();
                this.Test();
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

            private void Test()
            {
                ClassificationNetwork network = ClassificationNetwork.FromFile(this.options.NetworkFileName);

                List<ClassificationResult<string>> results = new List<ClassificationResult<string>>();

                using (TestImageProvider<string> dataProvider = this.options.CreateTestImageProvider(network))
                {
                    ////Context model = Context.FromRegex(@"\d{1,5}", CultureInfo.InvariantCulture);

                    ////int n = 0;
                    foreach (TestImage sample in dataProvider.Generate(network.AllowedClasses))
                    {
                        Interlocked.Increment(ref this.totalImages);

                        ////sample.Image.Save("e:\\temp\\" + sample.Label + "_" + n.ToString(CultureInfo.InvariantCulture) + ".bmp");
                        ////n++;

                        ////if (n < 171) continue;

                        this.localTimeCounter.Restart();

                        Tensor x = ImageExtensions.FromImage(
                            sample.Image,
                            network.InputShape.GetAxis(Axis.X),
                            network.InputShape.GetAxis(Axis.Y),
                            null,
                            Shape.BWHC);
                        IList<IList<(string Answer, float Probability)>> answers = network.Execute(x).Answers;
                        ////(IList<(string Answer, float Probability)> answers, _) = network.ExecuteSequence(x, model);

                        this.localTimeCounter.Stop();
                        long duration = this.localTimeCounter.ElapsedMilliseconds;

                        foreach (IList<(string answer, float probability)> answer in answers)
                        {
                            string text = answer.FirstOrDefault().answer;
                            float prob = answer.FirstOrDefault().probability;

                            results.Add(new ClassificationResult<string>(
                                sample.SourceId,
                                text,
                                string.Concat(sample.Labels),
                                prob,
                                prob >= 0.38f));

                            this.WriteLine(
                                null,
                                "({0})\tFile: {1} ... OK ({2} ms) {3} {4:F4}",
                                this.totalImages,
                                sample.SourceId.ToFileName(false),
                                duration,
                                text,
                                prob);
                        }

                        /*string answer = answers.Last().FirstOrDefault()?.Answer;
                        int prob = (int)(((answers.Last().FirstOrDefault()?.Probability ?? 0.0f) * 100) + 0.5f);

                        results.Add(new ClassificationResult<string>(
                            sample.SourceId,
                            answer,
                            string.Concat(sample.Labels),
                            prob,
                            prob >= 0.38f));

                        ////this.Write(".");
                        this.Write(
                            null,
                            "({0})\tFile: {1} ... OK ({4} ms) {2} {3:F4}",
                            this.totalImages,
                            sample.SourceId.ToFileName(false),
                            duration,
                            answer,
                            prob);*/
                    }
                }

                // write report
                ClassificationReport<string> testReport = new ClassificationReport<string>(results);
                using (StreamWriter outputFile = File.CreateText(this.options.OutputFileName))
                {
                    ClassificationReportWriter<string>.WriteReport(outputFile, testReport, ClassificationReportMode.All);
                }
            }

            private class Options
            {
                public string NetworkFileName { get; set; }

                public string ConfigFileName { get; set; }

                public string LogFileName { get; set; }

                public string OutputFileName { get; set; }

                private InnerProgram.Configuration Configuration { get; set; }

                public static Options Create(string[] args)
                {
                    // configure application
                    if (args.Length < 2)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Usage: NetClassify.exe <network> <config>");
                        Console.WriteLine();
                        Console.WriteLine("  <network>     Path to a file that contains the network.");
                        Console.WriteLine("  <config>      Path to a file that contains the test parameters.");

                        Environment.Exit(0);
                    }

                    Options options = new Options();

                    options.NetworkFileName = args[0];
                    options.ConfigFileName = args[1];
                    options.LogFileName = Path.ChangeExtension(options.NetworkFileName, ".log");
                    options.OutputFileName = Path.ChangeExtension(options.NetworkFileName, ".res");

                    if (!File.Exists(options.NetworkFileName))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Network file '{0}' does not exist.", options.NetworkFileName));
                    }

                    if (!File.Exists(options.ConfigFileName))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Configuration file '{0}' does not exist.", options.ConfigFileName));
                    }

                    options.Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(options.ConfigFileName, Encoding.UTF8));

                    return options;
                }

                public TestImageProvider<string> CreateTestImageProvider(ClassificationNetwork network)
                {
                    Shape shape = network.InputShape;

                    return TestImageProvider<string>.CreateFromJson(
                        0,
                        2 * shape.GetAxis(Axis.Y),
                        network.Classes,
                        network.BlankClass,
                        this.Configuration.DataProvider);
                }
            }

            private class Configuration
            {
                [JsonProperty("dataProvider", Required = Required.Always)]
                public JObject DataProvider { get; private set; }

                /*public class NamedParameters
                {
                    [JsonProperty("name")]
                    public string Name { get; set; }

                    [JsonProperty("parameters")]
                     public JObject Parameters { get; private set; }
                }*/
            }
        }
    }
}