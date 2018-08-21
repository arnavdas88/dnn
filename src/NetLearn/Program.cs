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
        [HandleProcessCorruptedStateExceptions]
        public static int Main(string[] args)
        {
            InnerProgram program = new InnerProgram();
            return program.Run(args);
        }

        private class InnerProgram : Lab.Program
        {
            private Options options = null;

            protected override bool OnConfigure(string[] args)
            {
                this.options = Options.Create(args);
                return true;
            }

            protected override int OnRun()
            {
                Assembly assembly = Assembly.GetEntryAssembly();

                IList<LearningTask> tasks = LearningTask.Create(this.options.Configuration);
                Parallel.ForEach(
                     tasks,
                     (task, state, i) =>
                     {
                         // learning
                         this.Learn((int)i, task, CancellationToken.None);

                         // testing
                         if (!string.IsNullOrEmpty(this.options.TestConfigFileName))
                         {
                             ProcessStartInfo processStartInfo = new ProcessStartInfo()
                             {
                                 FileName = Path.Combine(Path.GetDirectoryName(assembly.Location), "NetClassify.exe"),
                                 Arguments = string.Format(
                                     CultureInfo.InvariantCulture,
                                     "\"{0}\" \"{1}\"",
                                     task.OutputFileName,
                                     this.options.TestConfigFileName),
                                 UseShellExecute = false,
                                 WorkingDirectory = System.Environment.CurrentDirectory,
                             };

                             Process.Start(processStartInfo);
                         }
                     });

                return 0;
            }

            protected override void OnFinish()
            {
            }

            private void Learn(int taskIndex, LearningTask task, CancellationToken cancellationToken)
            {
                using (StreamWriter logFile = File.CreateText(task.LogFileName))
                {
                    logFile.AutoFlush = true;

                    try
                    {
                        // report starting time
                        DateTime dateStarted = DateTime.Now;
                        this.WriteLine(logFile, string.Format(CultureInfo.InvariantCulture, "Started: {0}", dateStarted.ToString("G", CultureInfo.InvariantCulture)));

                        ClassificationNetwork net = File.Exists(task.Architecture) ?
                            ClassificationNetwork.FromFile(task.Architecture) :
                            ClassificationNetwork.FromArchitecture(task.Architecture, task.Classes, task.Classes, task.BlankClass);

                        // learning
                        this.Learn(taskIndex, task, net, logFile, cancellationToken);
                        net.SaveToFile(task.OutputFileName);

                        // report finish time and processing interval
                        DateTime dateFinished = DateTime.Now;
                        this.WriteLine(logFile, string.Empty);
                        this.WriteLine(logFile, string.Format(CultureInfo.InvariantCulture, "Finished: {0:G}", dateFinished));
                        this.WriteLine(logFile, string.Format(CultureInfo.InvariantCulture, "Total time: {0:g}", TimeSpan.FromSeconds((dateFinished - dateStarted).TotalSeconds)));
                    }
                    finally
                    {
                        logFile.Flush();
                    }
                }
            }

            private void Learn(int taskIndex, LearningTask task, ClassificationNetwork net, StreamWriter logFile, CancellationToken cancellationToken)
            {
                this.WriteLine(logFile, "Learning...");

                ImageDistortion filter = new ImageDistortion();
                Stopwatch timer = new Stopwatch();

                this.WriteLine(logFile, "  Epochs: {0}", task.Epochs);

                this.WriteTrainerParameters(logFile, task.Trainer, task.Algorithm, task.Loss);

                this.WriteLine(logFile, "Image distortion:");
                this.WriteLine(logFile, "  Shift: {0}", task.Shift);
                this.WriteLine(logFile, "  Rotate: {0}", task.Rotate);
                this.WriteLine(logFile, "  Scale: {0}", task.Scale);
                this.WriteLine(logFile, "  Crop: {0}", task.Crop);

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

                        this.Write(logFile, s);
                        this.WriteDebugInformation(logFile);
                        this.WriteLine(logFile, string.Empty);
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

                                return filter
                                    .Distort(
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
            /// Writes a trainer parameters to event log.
            /// </summary>
            /// <param name="logFile">The log file to write to.</param>
            /// <param name="trainer">The trainer which parameters to write.</param>
            /// <param name="algorithm">The training algorithm.</param>
            /// <param name="loss">The loss function.</param>
            private void WriteTrainerParameters(
               StreamWriter logFile,
               ClassificationNetworkTrainer trainer,
               ITrainingAlgorithm algorithm,
               ILoss<int[]> loss)
            {
                this.WriteLine(logFile, "Trainer parameters:");
                this.WriteLine(logFile, "  Batch Size: {0}", trainer.BatchSize);
                this.WriteLine(logFile, "  L1 Rate: {0}", trainer.RateL1);
                this.WriteLine(logFile, "  L2 Rate: {0}", trainer.RateL2);
                this.WriteLine(logFile, "  Clip Value: {0}", trainer.ClipValue);

                this.WriteLine(logFile, "Algorithm parameters:");
                this.WriteLine(logFile, "  Algorithm: {0}", algorithm.GetType().Name);
                if (algorithm is Adadelta adadelta)
                {
                    this.WriteLine(logFile, "  Learning Rate: {0}", adadelta.LearningRate);
                    this.WriteLine(logFile, "  Decay: {0}", adadelta.Decay);
                    this.WriteLine(logFile, "  Rho: {0}", adadelta.Rho);
                    this.WriteLine(logFile, "  Eps: {0}", adadelta.Eps);
                }

                if (algorithm is Adagrad adagrad)
                {
                    this.WriteLine(logFile, "  Learning Rate: {0}", adagrad.LearningRate);
                    this.WriteLine(logFile, "  Eps: {0}", adagrad.Eps);
                }

                if (algorithm is Adam adam)
                {
                    this.WriteLine(logFile, "  Learning Rate: {0}", adam.LearningRate);
                    this.WriteLine(logFile, "  Beta1: {0}", adam.Beta1);
                    this.WriteLine(logFile, "  Beta2: {0}", adam.Beta2);
                    this.WriteLine(logFile, "  Eps: {0}", adam.Eps);
                }

                if (algorithm is RMSProp rmsProp)
                {
                    this.WriteLine(logFile, "  Learning Rate: {0}", rmsProp.LearningRate);
                    this.WriteLine(logFile, "  Rho: {0}", rmsProp.Rho);
                    this.WriteLine(logFile, "  Eps: {0}", rmsProp.Eps);
                }

                if (algorithm is SGD sgd)
                {
                    this.WriteLine(logFile, "  Learning Rate: {0}", sgd.LearningRate);
                    this.WriteLine(logFile, "  Decay: {0}", sgd.Decay);
                    this.WriteLine(logFile, "  Momentum: {0}", sgd.Momentum);
                    this.WriteLine(logFile, "  Nesterov: {0}", sgd.Nesterov);
                }

                this.WriteLine(logFile, "Loss parameters:");
                this.WriteLine(logFile, "  Loss: {0}", loss.GetType().Name);
                if (loss is LogLikelihoodLoss logLikelihoodLoss)
                {
                    this.WriteLine(logFile, "  LSR: {0}", logLikelihoodLoss.LSR);
                }
            }

            private class Options
            {
                public string ConfigFileName { get; set; }

                public InnerProgram.Configuration Configuration { get; set; }

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
                    [JsonProperty("classes", Required = Required.Always)]
                    public IList<string> Classes { get; } = new List<string>();

                    [JsonProperty("blankClass")]
                    public string BlankClass { get; set; }

                    [JsonProperty("architecture", Required = Required.Always)]
                    public IList<string> Architecture { get; } = new List<string>();
                }

                public class TaskParameters
                {
                    [JsonProperty("epochs")]
                    public int Epochs { get; private set; } = 1;

                    [JsonProperty("Shift")]
                    public bool Shift { get; private set; } = true;

                    [JsonProperty("Rotate")]
                    public bool Rotate { get; private set; } = true;

                    [JsonProperty("Scale")]
                    public bool Scale { get; private set; } = true;

                    [JsonProperty("Crop")]
                    public bool Crop { get; private set; } = false;

                    [JsonProperty("dataProvider", Required = Required.Always)]
                    public JObject DataProvider { get; private set; }

                    [JsonProperty("trainerParameters")]
                    public JObject TrainerParameters { get; private set; }

                    [JsonProperty("algorithm", Required = Required.Always)]
                    public NamedParameters Algorithm { get; } = new NamedParameters();

                    [JsonProperty("loss", Required = Required.Always)]
                    public NamedParameters Loss { get; } = new NamedParameters();
                }

                public class NamedParameters
                {
                    [JsonProperty("name")]
                    public string Name { get; set; }

                    [JsonProperty("parameters")]
                    public JObject Parameters { get; private set; }
                }
            }

            private class LearningTask
            {
                public string Architecture { get; private set; }

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

                                Architecture = architechture,
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
}
