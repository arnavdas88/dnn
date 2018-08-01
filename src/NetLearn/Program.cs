// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.NetLearn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Genix.DNN;
    using Genix.DNN.Learning;
    using Genix.Imaging.Lab;
    using Genix.MachineLearning;
    using Genix.MachineLearning.Imaging;
    using Genix.MachineLearning.Learning;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class Program
    {
        // Locker for log operations.
        private static object locker = new object();

        private static PerformanceCounter performanceCounterNetTotalHeap = null;
        private static PerformanceCounter performanceCounterNetGCHandles = null;

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

                // create performance counters
                Program.performanceCounterNetTotalHeap = Program.CreatePerformanceCounter(".NET CLR Memory", "# bytes in all heaps");
                Program.performanceCounterNetGCHandles = Program.CreatePerformanceCounter(".NET CLR Memory", "# GC Handles");

                // run application
                DateTime dateStarted = DateTime.Now;
                Program.WriteLine(null, string.Format(CultureInfo.InvariantCulture, "Started: {0}", dateStarted.ToString("G", CultureInfo.InvariantCulture)));

                IList<LearningTask> tasks = LearningTask.Create(options.Configuration);
                Parallel.ForEach(
                    tasks,
                    (task, state, i) =>
                    {
                        // learning
                        Program.Learn((int)i, task, CancellationToken.None);

                        // testing
                        if (!string.IsNullOrEmpty(options.TestConfigFileName))
                        {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo()
                            {
                                FileName = Path.Combine(Path.GetDirectoryName(assembly.Location), "NetClassify.exe"),
                                Arguments = string.Format(
                                    CultureInfo.InvariantCulture,
                                    "\"{0}\" \"{1}\"",
                                    task.OutputFileName,
                                    options.TestConfigFileName),
                                UseShellExecute = false,
                                WorkingDirectory = System.Environment.CurrentDirectory,
                            };

                            Process.Start(processStartInfo);
                        }
                    });

                // report finish time and processing interval
                Program.WriteLine(null, string.Empty);
                DateTime dateFinished = DateTime.Now;
                Program.WriteLine(null, string.Format(CultureInfo.InvariantCulture, "Finished: {0:G}", dateFinished));
                Program.WriteLine(null, string.Format(CultureInfo.InvariantCulture, "Total time: {0:g}", TimeSpan.FromSeconds((dateFinished - dateStarted).TotalSeconds)));

                // wrap everything up
            }
            catch (Exception e)
            {
                // write error report
                Program.WriteException(null, e);
                retCode = -1;
            }
            finally
            {
                if (Program.performanceCounterNetGCHandles != null)
                {
                    Program.performanceCounterNetGCHandles.Dispose();
                    Program.performanceCounterNetGCHandles = null;
                }

                if (Program.performanceCounterNetTotalHeap != null)
                {
                    Program.performanceCounterNetTotalHeap.Dispose();
                    Program.performanceCounterNetTotalHeap = null;
                }
            }

            return retCode;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
        private static void Learn(int taskIndex, LearningTask task, CancellationToken cancellationToken)
        {
            using (StreamWriter logFile = File.CreateText(task.LogFileName))
            {
                logFile.AutoFlush = true;

                try
                {
                    // report starting time
                    DateTime dateStarted = DateTime.Now;
                    Program.WriteLine(logFile, string.Format(CultureInfo.InvariantCulture, "Started: {0}", dateStarted.ToString("G", CultureInfo.InvariantCulture)));

                    ClassificationNetwork net = File.Exists(task.Architechture) ?
                        ClassificationNetwork.FromFile(task.Architechture) :
                        ClassificationNetwork.FromArchitecture(task.Architechture, task.Classes, task.Classes, task.BlankClass);

                    // learning
                    Program.Learn(taskIndex, task, net, logFile, cancellationToken);
                    net.SaveToFile(task.OutputFileName);

                    // report finish time and processing interval
                    DateTime dateFinished = DateTime.Now;
                    Program.WriteLine(logFile, string.Empty);
                    Program.WriteLine(logFile, string.Format(CultureInfo.InvariantCulture, "Finished: {0:G}", dateFinished));
                    Program.WriteLine(logFile, string.Format(CultureInfo.InvariantCulture, "Total time: {0:g}", TimeSpan.FromSeconds((dateFinished - dateStarted).TotalSeconds)));
                }
                finally
                {
                    logFile.Flush();
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ms", Justification = "This is an abbreviation for milliseconds.")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
        private static void Learn(int taskIndex, LearningTask task, ClassificationNetwork net, StreamWriter logFile, CancellationToken cancellationToken)
        {
            Program.WriteLine(logFile, "Learning...");

            ImageDistortion filter = new ImageDistortion();
            Stopwatch timer = new Stopwatch();

            Program.WriteLine(logFile, "  Epochs: {0}", task.Epochs);

            Program.WriteTrainerParameters(logFile, task.Trainer, task.Algorithm, task.Loss);

            Program.WriteLine(logFile, "Image distortion:");
            Program.WriteLine(logFile, "  Shift: {0}", task.Shift);
            Program.WriteLine(logFile, "  Rotate: {0}", task.Rotate);
            Program.WriteLine(logFile, "  Scale: {0}", task.Scale);
            Program.WriteLine(logFile, "  Crop: {0}", task.Crop);

            int[] shape = net.InputShape;
            using (TestImageProvider<string> dataProvider = task.CreateTestImageProvider(net))
            {
                ////int n = 0;
                for (int epoch = 0; epoch < task.Epochs; epoch++)
                {
                    timer.Restart();

                    /*foreach (var sample in generateSamples(epoch))
                    {
                        n = (int)sample.Item1.L1Norm();
                    }*/

                    TrainingResult result = task.Trainer.RunEpoch(
                        epoch,
                        net,
                        GenerateSamples(epoch),
                        task.Algorithm,
                        task.Loss,
                        cancellationToken);

                    timer.Stop();

                    string s = string.Format(
                        CultureInfo.InvariantCulture,
                        "Net: {0}, Epoch: {1}, Time: {2} ms, {3}",
                        taskIndex,
                        epoch,
                        timer.ElapsedMilliseconds,
                        result);

                    Program.Write(logFile, s);
                    Program.WriteDebugInformation(logFile);
                    Program.WriteLine(logFile, string.Empty);
                }

                IEnumerable<(Tensor, string[])> GenerateSamples(int epoch)
                {
                    return dataProvider
                        .Generate(null).ToList()
                        .SelectMany(x =>
                        {
                            string[] labels = x.Labels;
                            if (!(task.Loss is CTCLoss))
                            {
                                int b = net.OutputShapes.First()[0];
                                if (labels.Length == 1 && b > 1)
                                {
                                    labels = Enumerable.Repeat(labels[0], b).ToArray();
                                }
                            }

                            if (epoch == 0)
                            {
                                ////x.Image.Save("e:\\temp\\" + x.Id + "_" + n.ToString(CultureInfo.InvariantCulture) + "_.bmp");
                            }

                            ////return (Tensor.FromBitmap(null, x.Image), labels);

                            return filter.Distort(
                                    x.Image,
                                    shape[(int)Axis.X],
                                    shape[(int)Axis.Y],
                                    task.Shift,
                                    task.Rotate && x.FontStyle != FontStyle.Italic,
                                    task.Scale,
                                    task.Crop)
                                .Select(bitmap =>
                                {
                                    /*if (epoch == 0)
                                    {
                                        bitmap.Save(@"d:\dnn\temp\" + (++n).ToString(CultureInfo.InvariantCulture) + "_" + x.Id + ".bmp");
                                    }*/

                                    return (bitmap.ToTensor(null), labels);
                                });
                        });
                }
            }
        }

        /// <summary>
        /// Writes a message to event log.
        /// </summary>
        /// <param name="logFile">The log file to write to.</param>
        /// <param name="message">The message to write.</param>
        private static void Write(StreamWriter logFile, string message)
        {
            lock (Program.locker)
            {
                Console.Write(message);

                if (logFile != null)
                {
                    logFile.Write(message);
                }
            }
        }

        /// <summary>
        /// Writes a message followed by a new line to event log.
        /// </summary>
        /// <param name="logFile">The log file to write to.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="arg">The arguments to write.</param>
        private static void WriteLine(StreamWriter logFile, string message, params object[] arg)
        {
            lock (Program.locker)
            {
                Console.WriteLine(message, arg);

                if (logFile != null)
                {
                    logFile.WriteLine(message, arg);
                }
            }
        }

        /// <summary>
        /// Writes an exception to event log.
        /// </summary>
        /// <param name="logFile">The log file to write to.</param>
        /// <param name="exception">The exception to write.</param>
        private static void WriteException(StreamWriter logFile, Exception exception)
        {
            foreach (string s in Program.AggregateMessages(exception))
            {
                Program.WriteLine(logFile, s);
            }

            Program.WriteLine(logFile, string.Empty);

            foreach (string s in Program.AggregateStackTrace(exception))
            {
                Program.WriteLine(logFile, s);
            }
        }

        /// <summary>
        /// Writes a trainer parameters to event log.
        /// </summary>
        /// <param name="logFile">The log file to write to.</param>
        /// <param name="trainer">The trainer which parameters to write.</param>
        /// <param name="algorithm">The training algorithm.</param>
        /// <param name="loss">The loss function.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Eps", Justification = "This is an abbreviation for epsilon.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Nesterov", Justification = "This is a name of the algorithm.")]
        private static void WriteTrainerParameters(
            StreamWriter logFile,
            ClassificationNetworkTrainer trainer,
            ITrainingAlgorithm algorithm,
            ILoss<int[]> loss)
        {
            Program.WriteLine(logFile, "Trainer parameters:");
            Program.WriteLine(logFile, "  Batch Size: {0}", trainer.BatchSize);
            Program.WriteLine(logFile, "  L1 Rate: {0}", trainer.RateL1);
            Program.WriteLine(logFile, "  L2 Rate: {0}", trainer.RateL2);
            Program.WriteLine(logFile, "  Clip Value: {0}", trainer.ClipValue);

            Program.WriteLine(logFile, "Algorithm parameters:");
            Program.WriteLine(logFile, "  Algorithm: {0}", algorithm.GetType().Name);
            if (algorithm is Adadelta adadelta)
            {
                Program.WriteLine(logFile, "  Learning Rate: {0}", adadelta.LearningRate);
                Program.WriteLine(logFile, "  Decay: {0}", adadelta.Decay);
                Program.WriteLine(logFile, "  Rho: {0}", adadelta.Rho);
                Program.WriteLine(logFile, "  Eps: {0}", adadelta.Eps);
            }

            if (algorithm is Adagrad adagrad)
            {
                Program.WriteLine(logFile, "  Learning Rate: {0}", adagrad.LearningRate);
                Program.WriteLine(logFile, "  Eps: {0}", adagrad.Eps);
            }

            if (algorithm is Adam adam)
            {
                Program.WriteLine(logFile, "  Learning Rate: {0}", adam.LearningRate);
                Program.WriteLine(logFile, "  Beta1: {0}", adam.Beta1);
                Program.WriteLine(logFile, "  Beta2: {0}", adam.Beta2);
                Program.WriteLine(logFile, "  Eps: {0}", adam.Eps);
            }

            if (algorithm is RMSProp rmsProp)
            {
                Program.WriteLine(logFile, "  Learning Rate: {0}", rmsProp.LearningRate);
                Program.WriteLine(logFile, "  Rho: {0}", rmsProp.Rho);
                Program.WriteLine(logFile, "  Eps: {0}", rmsProp.Eps);
            }

            if (algorithm is SGD sgd)
            {
                Program.WriteLine(logFile, "  Learning Rate: {0}", sgd.LearningRate);
                Program.WriteLine(logFile, "  Decay: {0}", sgd.Decay);
                Program.WriteLine(logFile, "  Momentum: {0}", sgd.Momentum);
                Program.WriteLine(logFile, "  Nesterov: {0}", sgd.Nesterov);
            }

            Program.WriteLine(logFile, "Loss parameters:");
            Program.WriteLine(logFile, "  Loss: {0}", loss.GetType().Name);
            if (loss is LogLikelihoodLoss logLikelihoodLoss)
            {
                Program.WriteLine(logFile, "  LSR: {0}", logLikelihoodLoss.LSR);
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

        /// <summary>
        /// Aggregates stack trace from the exception and all its inner exceptions.
        /// </summary>
        /// <param name="exception">The exception to aggregate the stack trace from.</param>
        /// <returns>The collection of strings that contains stack trace.</returns>
        private static string[] AggregateStackTrace(Exception exception)
        {
            List<string> messages = new List<string>();

            // aggregate messages
            for (; exception != null; exception = exception.InnerException)
            {
                // add aggregate exception messages
                if (exception is AggregateException aggregateException)
                {
                    foreach (Exception e in aggregateException.InnerExceptions)
                    {
                        messages.AddRange(Program.AggregateStackTrace(e));
                    }

                    // all inner exception have been processed
                    break;
                }
                else if (exception.InnerException == null)
                {
                    if (!string.IsNullOrEmpty(exception.StackTrace))
                    {
                        messages.Add(exception.StackTrace);
                    }
                }
            }

            return messages.ToArray();
        }

        /// <summary>
        /// Writes program debug information to event log.
        /// </summary>
        /// <param name="stream">The stream to write debug information to.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No need, this is an internal module, used for testing purposes.")]
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Calling GC methods is required for accurate calculation of memory usage.")]
        private static void WriteDebugInformation(StreamWriter stream)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            long workingSet = Process.GetCurrentProcess().WorkingSet64 / 1000;
            long virtualSize = Process.GetCurrentProcess().VirtualMemorySize64 / 1000;
            long peakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64 / 1000;
            long netTotalHeap = (long)Program.performanceCounterNetTotalHeap.NextValue() / 1000;
            long netGCHandles = (long)Program.performanceCounterNetGCHandles.NextValue();
            long unmanaged = workingSet - netTotalHeap;

            string s = string.Format(
                CultureInfo.InvariantCulture,
                " {0:N0} KB, ({1:N0} KB managed, {2} GC handles, {3:N0} KB unmanaged, {4:N0} KB Peak Working Set, {5:N0} KB Virtual Memory)",
                workingSet,
                netTotalHeap,
                netGCHandles,
                unmanaged > 0 ? unmanaged : 0,
                peakWorkingSet,
                virtualSize);

            Program.Write(stream, s);
        }

        /// <summary>
        /// Create a new instance of the <see cref="PerformanceCounter"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <returns>The <see cref="PerformanceCounter"/> this method creates.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Keep method non static for consistency with other methods.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Application-level exception handling.")]
        private static PerformanceCounter CreatePerformanceCounter(string categoryName, string counterName)
        {
            PerformanceCounter performanceCounter = null;
            for (int i = 0; i < 10 && performanceCounter == null; i++)
            {
                try
                {
                    performanceCounter = new PerformanceCounter(categoryName, counterName, Process.GetCurrentProcess().ProcessName, true);
                }
                catch
                {
                    Thread.Sleep(5000);
                }
            }

            if (performanceCounter == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Failed to create performance counter <{0} : {1}>.", categoryName, counterName));
            }

            return performanceCounter;
        }

        private class Options
        {
            public string ConfigFileName { get; set; }

            public Program.Configuration Configuration { get; set; }

            public string TestConfigFileName { get; set; }

            public static Options Create(string[] args)
            {
                Options options = new Options();

                if (args.Length > 0)
                {
                    options.ConfigFileName = args[0];
                }

                if (!File.Exists(options.ConfigFileName))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "File '{0}' does not exist.", options.ConfigFileName));
                }

                options.Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(options.ConfigFileName, Encoding.UTF8));

                if (args.Length > 1)
                {
                    options.TestConfigFileName = args[1];
                }

                return options;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is initialized by JsonSerializer.")]
        private class Configuration
        {
            [JsonProperty("network", Required = Required.Always)]
            public NetworkParameters Network { get; } = new NetworkParameters();

            [JsonProperty("tasks", Required = Required.Always)]
            public IList<TaskParameters> Tasks { get; } = new List<TaskParameters>();

            private static bool HasDuplicates<T>(IList<T> values, out T value)
            {
                value = default(T);

                for (int i = 0, ii = values.Count; i + 1 < ii; i++)
                {
                    T value1 = values[i];

                    for (int j = i + 1; j < ii; j++)
                    {
                        T value2 = values[j];
                        if (object.Equals(value1, value2))
                        {
                            value = value1;
                            return true;
                        }
                    }
                }

                return false;
            }

            [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly", Justification = "This method has to be called by JsonSerializer.")]
            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                // validate classes
                if (this.Network.Classes.Count == 0)
                {
                    throw new ArgumentException("Element 'network.classes' must define at least one class");
                }

                if (Configuration.HasDuplicates(this.Network.Classes, out string duplicateClass))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Class '{0}' appears more than once in 'network.classes' element.", duplicateClass));
                }

                // validate architecture
                if (this.Network.Architecture.Count == 0)
                {
                    throw new ArgumentException("Element 'network.architecture' must define at least one network architecture");
                }

                if (Configuration.HasDuplicates(this.Network.Architecture, out string duplicateArchitecture))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Architecture '{0}' appears more than once in 'network.architecture' element.", duplicateArchitecture));
                }
            }

            public class NetworkParameters
            {
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                [JsonProperty("classes", Required = Required.Always)]
                public IList<string> Classes { get; } = new List<string>();

                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                [JsonProperty("blankClass")]
                public string BlankClass { get; set; }

                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                [JsonProperty("architecture", Required = Required.Always)]
                public IList<string> Architecture { get; } = new List<string>();
            }

            [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is initialized by JsonSerializer.")]
            public class TaskParameters
            {
                [JsonProperty("epochs")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public int Epochs { get; private set; } = 1;

                [JsonProperty("Shift")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public bool Shift { get; private set; } = true;

                [JsonProperty("Rotate")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public bool Rotate { get; private set; } = true;

                [JsonProperty("Scale")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public bool Scale { get; private set; } = true;

                [JsonProperty("Crop")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public bool Crop { get; private set; } = false;

                [JsonProperty("dataProvider", Required = Required.Always)]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public JObject DataProvider { get; private set; }

                [JsonProperty("trainerParameters")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public JObject TrainerParameters { get; private set; }

                [JsonProperty("algorithm", Required = Required.Always)]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public NamedParameters Algorithm { get; } = new NamedParameters();

                [JsonProperty("loss", Required = Required.Always)]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public NamedParameters Loss { get; } = new NamedParameters();
            }

            [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is initialized by JsonSerializer.")]
            public class NamedParameters
            {
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("parameters")]
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property is initialized by JsonSerializer.")]
                public JObject Parameters { get; private set; }
            }
        }

        private class LearningTask
        {
            public string Architechture { get; private set; }

            public IList<string> Classes { get; private set; }

            public string BlankClass { get; private set; }

            public int Epochs { get; private set; }

            public bool Shift { get; private set; }

            public bool Rotate { get; private set; }

            public bool Scale { get; private set; }

            public bool Crop { get; private set; }

            public string OutputFileName { get; private set; }

            public string LogFileName { get; private set; }

            public ClassificationNetworkTrainer Trainer { get; private set; }

            public ITrainingAlgorithm Algorithm { get; private set; }

            public ILoss<int[]> Loss { get; private set; }

            private Configuration.TaskParameters TaskParameters { get; set; }

            public static IList<LearningTask> Create(Configuration config)
            {
                List<LearningTask> tasks = new List<LearningTask>();

                string timestamp = DateTime.Now.ToString("G", CultureInfo.InvariantCulture)
                    .Replace("/", string.Empty)
                    .Replace(' ', '_')
                    .Replace(":", string.Empty);

                foreach (string architechture in config.Network.Architecture)
                {
                    foreach (Configuration.TaskParameters task in config.Tasks)
                    {
                        LearningTask learningTask = new LearningTask()
                        {
                            TaskParameters = task,

                            Architechture = architechture,
                            Classes = config.Network.Classes,
                            BlankClass = config.Network.BlankClass,

                            Epochs = task.Epochs,
                            Shift = task.Shift,
                            Rotate = task.Rotate,
                            Scale = task.Scale,
                            Crop = task.Crop,

                            OutputFileName = timestamp + '_' + architechture + ".net",
                            LogFileName = timestamp + '_' + architechture + ".log",
                        };

                        learningTask.Trainer = learningTask.CreateTrainer();
                        learningTask.Algorithm = learningTask.CreateAlgorithm();
                        learningTask.Loss = learningTask.CreateLoss();

                        tasks.Add(learningTask);
                    }
                }

                return tasks;
            }

            public TestImageProvider<string> CreateTestImageProvider(ClassificationNetwork network)
            {
                int[] shape = network.InputShape;

                return TestImageProvider<string>.CreateFromJson(
                    0,
                    2 * shape[(int)Axis.Y],
                    network.Classes,
                    network.BlankClass,
                    this.TaskParameters.DataProvider);
            }

            private ClassificationNetworkTrainer CreateTrainer()
            {
                ClassificationNetworkTrainer trainer = new ClassificationNetworkTrainer();

                JsonSerializer jsonSerializer = new JsonSerializer();
                using (JTokenReader jtokenReader = new JTokenReader(this.TaskParameters.TrainerParameters))
                {
                    jsonSerializer.Populate(jtokenReader, trainer);
                }

                return trainer;
            }

            private ITrainingAlgorithm CreateAlgorithm()
            {
                ITrainingAlgorithm algorithm = null;
                switch (this.TaskParameters.Algorithm.Name)
                {
                    case "Adadelta":
                    default:
                        algorithm = new Adadelta();
                        break;

                    case "Adagrad":
                        algorithm = new Adagrad();
                        break;

                    case "Adam":
                        algorithm = new Adam();
                        break;

                    case "RMSProp":
                        algorithm = new RMSProp();
                        break;

                    case "SGD":
                        algorithm = new SGD();
                        break;
                }

                JsonSerializer jsonSerializer = new JsonSerializer();
                using (JTokenReader jtokenReader = new JTokenReader(this.TaskParameters.Algorithm.Parameters))
                {
                    jsonSerializer.Populate(jtokenReader, algorithm);
                }

                return algorithm;
            }

            private ILoss<int[]> CreateLoss()
            {
                ILoss<int[]> loss = null;
                switch (this.TaskParameters.Loss.Name)
                {
                    case "LogLikelihood":
                    default:
                        loss = new LogLikelihoodLoss();
                        break;

                    case "CTC":
                        loss = new CTCLoss();
                        break;
                }

                JsonSerializer jsonSerializer = new JsonSerializer();
                using (JTokenReader jtokenReader = new JTokenReader(this.TaskParameters.Loss.Parameters))
                {
                    jsonSerializer.Populate(jtokenReader, loss);
                }

                return loss;
            }
        }
    }
}
