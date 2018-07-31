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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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
        private static StreamWriter logFile;

        private static Stopwatch totalTimeCounter = new Stopwatch();
        private static Stopwatch localTimeCounter = new Stopwatch();
        private static long totalImages;

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
        [HandleProcessCorruptedStateExceptions]
        internal static int Main(string[] args)
        {
            int retCode = 0;
            try
            {
                // show application information
                Assembly assembly = Assembly.GetEntryAssembly();

                AssemblyTitleAttribute title = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                Console.WriteLine(title.Title + " Version " + assembly.GetName().Version.ToString());

                AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
                Console.WriteLine(copyright.Copyright);
                Console.WriteLine();

                // configure application
                Options options = Options.Create(args);

                // open log file
                Program.logFile = new StreamWriter(options.LogFileName, true);
                Program.logFile.AutoFlush = true;

                // run application
                DateTime dateStarted = DateTime.Now;
                Program.WriteLine(string.Format(CultureInfo.InvariantCulture, "Started: {0}", dateStarted.ToString("G", CultureInfo.InvariantCulture)));

                Program.totalTimeCounter.Start();
                Program.Test(options);
                Program.totalTimeCounter.Stop();

                // report finish time and processing interval
                Program.WriteLine(string.Empty);
                DateTime dateFinished = DateTime.Now;
                Program.WriteLine(string.Format(CultureInfo.InvariantCulture, "Finished: {0:G}", dateFinished));
                Program.WriteLine(string.Format(CultureInfo.InvariantCulture, "Total time: {0:g}", TimeSpan.FromSeconds((dateFinished - dateStarted).TotalSeconds)));

                // wrap everything up
                if (Program.totalImages > 0)
                {
                    Program.WriteLine(string.Format(CultureInfo.InvariantCulture, "Total images: {0}", Program.totalImages));

                    long millisecsPerImage = Program.totalTimeCounter.ElapsedMilliseconds / Program.totalImages;
                    double imagesPerMinute = 60000.0 / millisecsPerImage;
                    Program.WriteLine(string.Format(CultureInfo.InvariantCulture, "Average time: {0:N0} ms per image, {1:F2} images per minute.", millisecsPerImage, imagesPerMinute));
                }
            }
            catch (Exception e)
            {
                // write error report
                Program.WriteLine(string.Empty);
                Program.WriteException(e);
                retCode = -1;
            }
            finally
            {
                if (Program.logFile != null)
                {
                    Program.logFile.Flush();
                    Program.logFile.Dispose();
                    Program.logFile = null;
                }
            }

            return retCode;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ms", Justification = "Represents milliseconds.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Application-level exception handling.")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 tuples.")]
        private static void Test(Options options)
        {
            ClassificationNetwork network = ClassificationNetwork.FromFile(options.NetworkFileName);

            List<ClassificationResult> results = new List<ClassificationResult>();

            using (TestImageProvider<string> dataProvider = options.CreateTestImageProvider(network))
            {
                Context model = Context.FromRegex(@"\d{1,5}", CultureInfo.InvariantCulture);

                ////int n = 0;
                foreach (TestImage sample in dataProvider.Generate(null))
                {
                    Interlocked.Increment(ref Program.totalImages);

                    ////sample.Image.Save("e:\\temp\\" + sample.Label + "_" + n.ToString(CultureInfo.InvariantCulture) + ".bmp");
                    ////n++;

                    ////if (n < 171) continue;

                    Program.localTimeCounter.Restart();

                    Tensor x = sample.Image.ToTensor(null);
                    ////(IList<IList<ClassificationNetworkResult>> answers, _) = network.Execute(x);
                    (IList<(string Answer, float Probability)> answers, _) = network.ExecuteSequence(x, model);

                    Program.localTimeCounter.Stop();
                    long duration = Program.localTimeCounter.ElapsedMilliseconds;

                    ////string answer = answers.Last().FirstOrDefault()?.Answer;
                    ////int prob = (int)(((answers.Last().FirstOrDefault()?.Probability ?? 0.0f) * 100) + 0.5f);
                    string answer = answers.FirstOrDefault().Answer;
                    int prob = (int)((answers.FirstOrDefault().Probability * 100) + 0.5f);

                    results.Add(new ClassificationResult(
                        sample.Id,
                        sample.FrameIndex,
                        answer,
                        string.Concat(sample.Labels),
                        prob,
                        prob >= 38));

                    ////Program.Write(logFile, ".");
                    Program.Write(string.Format(CultureInfo.InvariantCulture, "({0})\tFile: {1}", Program.totalImages, sample.Id));
                    if (sample.FrameIndex.HasValue)
                    {
                        Program.Write(string.Format(CultureInfo.InvariantCulture, ";{0}", sample.FrameIndex.Value + 1));
                    }

                    Program.WriteLine(string.Format(CultureInfo.InvariantCulture, " ... {0} {1} OK ({2} ms).", answer, prob, duration));
                }
            }

            // write report
            TestReport testReport = new TestReport(results);
            using (StreamWriter outputFile = File.CreateText(options.OutputFileName))
            {
                TestReportWriter.WriteReport(outputFile, testReport);
            }
        }

        /// <summary>
        /// Writes a message to event log and to the program log file.
        /// </summary>
        /// <param name="message">The message to write.</param>
        private static void Write(string message)
        {
            Console.Write(message);

            if (Program.logFile != null)
            {
                Program.logFile.Write(message);
            }
        }

        /// <summary>
        /// Writes a message followed by a new line to event log and to the program log file.
        /// </summary>
        /// <param name="message">The message to write.</param>
        private static void WriteLine(string message)
        {
            Console.WriteLine(message);

            if (Program.logFile != null)
            {
                Program.logFile.WriteLine(message);
            }
        }

        /// <summary>
        /// Writes an exception to event log and to the program log file.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        private static void WriteException(Exception exception)
        {
            foreach (string s in Program.AggregateMessages(exception))
            {
                Program.WriteLine(s);
            }
        }

        /// <summary>
        /// Aggregates messages from the exception and all its inner exceptions.
        /// </summary>
        /// <param name="exception">The exception to aggregate the messages from.</param>
        /// <returns>The collection of strings that contains error messages.</returns>
        private static string[] AggregateMessages(Exception exception)
        {
            List<string> messages = new List<string>();

            // aggregate messages
            for (; exception != null; exception = exception.InnerException)
            {
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    messages.Add(exception.Message);

                    // add aggregate exception messages
                    if (exception is AggregateException aggregateException)
                    {
                        foreach (Exception e in aggregateException.InnerExceptions)
                        {
                            messages.AddRange(Program.AggregateMessages(e));
                        }

                        // all inner exception have been processed
                        break;
                    }
                }
            }

            // filter out duplicate messages
            for (int i = 1; i < messages.Count; i++)
            {
                if (messages[i - 1] == messages[i])
                {
                    messages.RemoveAt(i--);
                }
            }

            // filter out meaningless messages
            if (messages.Count > 1)
            {
                messages.RemoveAll(x => x == "Exception has been thrown by the target of an invocation.");
            }

            return messages.ToArray();
        }

        private class Options
        {
            public string NetworkFileName { get; set; }

            public string ConfigFileName { get; set; }

            public string LogFileName { get; set; }

            public string OutputFileName { get; set; }

            private Program.Configuration Configuration { get; set; }

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NetClassify", Justification = "This is an application name.")]
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
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
                int[] shape = network.InputShape;

                return TestImageProvider<string>.CreateFromJson(
                    shape[(int)Axis.X],
                    shape[(int)Axis.Y],
                    network.Classes,
                    network.BlankClass,
                    this.Configuration.DataProvider);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is initialized by JsonSerializer.")]
        private class Configuration
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
            [JsonProperty("dataProvider", Required = Required.Always)]
            public JObject DataProvider { get; private set; }

            /*[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is initialized by JsonSerializer.")]
            public class NamedParameters
            {
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("parameters")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public JObject Parameters { get; private set; }
            }*/
        }
    }
}