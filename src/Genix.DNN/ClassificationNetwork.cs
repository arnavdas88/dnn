// -----------------------------------------------------------------------
// <copyright file="ClassificationNetwork.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Genix.Core;
    using Genix.DNN.LanguageModel;
    using Genix.DNN.Layers;
    using Genix.DNN.Learning;
    using Genix.MachineLearning;
    using Newtonsoft.Json;

    /// <summary>
    /// The base neural network class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Architecture}")]
    public sealed class ClassificationNetwork : Network
    {
        /// <summary>
        /// The classes the network classifies into.
        /// </summary>
        [JsonProperty("Classes", Order = 1)]
        private readonly List<string> classes = new List<string>();

        /// <summary>
        /// The classes the network is allowed to classify.
        /// </summary>
        [JsonProperty("AllowedClasses", Order = 2)]
        private readonly HashSet<string> allowedClasses = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationNetwork"/> class.
        /// </summary>
        /// <param name="graph">The network graph.</param>
        /// <param name="classes">The classes the network should be able to classify.</param>
        /// <param name="allowedClasses">The classes the network is allowed to classify.</param>
        /// <param name="blankClass">The blank class that represents none of the real classes.</param>
        private ClassificationNetwork(NetworkGraph graph, IList<string> classes, IList<string> allowedClasses, string blankClass)
            : base(graph)
        {
            if (classes == null)
            {
                throw new ArgumentNullException(nameof(classes));
            }

            if (classes.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.E_InvalidNetArchitecture_NoClasses, nameof(classes));
            }

            if (!string.IsNullOrEmpty(blankClass) && !classes.Contains(blankClass))
            {
                throw new ArgumentException("The blank class must be included in the list of classes.", nameof(classes));
            }

            this.classes.AddRange(classes);
            if (allowedClasses != null)
            {
                this.allowedClasses.UnionWith(allowedClasses);
                this.allowedClasses.IntersectWith(classes);
            }
            else
            {
                this.allowedClasses.UnionWith(classes);
            }

            this.BlankClass = blankClass;

            // the graph sinks must be loss layers
            if (!graph.Sinks.All(x => x is LossLayer))
            {
                throw new ArgumentException(Properties.Resources.E_InvalidNetArchitecture_MissingLossLayer);
            }

            // the number of classes in loss layers must match the number of network classes
            if (graph.Sinks.OfType<LossLayer>().Any(x => x.NumberOfClasses != classes.Count))
            {
                throw new ArgumentException(Properties.Resources.E_InvalidNetArchitecture_InvalidClassCountInLossLayer);
            }

            this.ConfigureAllowedClasses();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassificationNetwork"/> class, using the existing <see cref="ClassificationNetwork"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ClassificationNetwork"/> to copy the data from.</param>
        /// <param name="cloneLayers">The value indicating whether the network layers should be cloned.</param>
        private ClassificationNetwork(ClassificationNetwork other, bool cloneLayers)
            : base(other, cloneLayers)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.classes.AddRange(other.classes);
            this.allowedClasses.UnionWith(other.allowedClasses);
            this.BlankClass = other.BlankClass;
            this.ConfigureAllowedClasses();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ClassificationNetwork" /> class from being created.
        /// </summary>
        [JsonConstructor]
        private ClassificationNetwork()
        {
        }

        /// <summary>
        /// Gets the collection of classes the network is able to classify.
        /// </summary>
        /// <value>
        /// The collection of class names.
        /// </value>
        public ReadOnlyCollection<string> Classes => new ReadOnlyCollection<string>(this.classes);

        /// <summary>
        /// Gets the classes the network is allowed to classify.
        /// </summary>
        /// <value>
        /// The set of class names.
        /// </value>
        public ISet<string> AllowedClasses => this.allowedClasses;

        /// <summary>
        /// Gets the blank class that represents none of the real classes.
        /// </summary>
        /// <value>
        /// The <see cref="string"/> that represents blank class.
        /// </value>
        [JsonProperty("BlankClass", Order = 3)]
        public string BlankClass { get; private set; }

        /// <summary>
        /// Creates a classification neural network from a string that contains network architecture.
        /// </summary>
        /// <param name="architecture">The network architecture.</param>
        /// <param name="classes">The classes the network should able to classify into.</param>
        /// <returns>The <see cref="ClassificationNetwork"/> object this method creates.</returns>
        public static ClassificationNetwork FromArchitecture(string architecture, IList<string> classes)
        {
            return ClassificationNetwork.FromArchitecture(architecture, classes, classes, null);
        }

        /// <summary>
        /// Creates a classification neural network from a string that contains network architecture.
        /// </summary>
        /// <param name="architecture">The network architecture.</param>
        /// <param name="classes">The classes the network should able to classify into.</param>
        /// <param name="allowedClasses">The classes the network is allowed to classify.</param>
        /// <param name="blankClass">The blank class that represents none of the real classes.</param>
        /// <returns>
        /// The <see cref="ClassificationNetwork"/> object this method creates.
        /// </returns>
        public static ClassificationNetwork FromArchitecture(string architecture, IList<string> classes, IList<string> allowedClasses, string blankClass)
        {
            NetworkGraph graph = NetworkGraphBuilder.CreateNetworkGraph(architecture, true, true);

            return new ClassificationNetwork(graph, classes, allowedClasses, blankClass);
        }

        /// <summary>
        /// Creates a classification neural network from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="ClassificationNetwork"/>.</param>
        /// <returns>The <see cref="ClassificationNetwork"/> this method creates.</returns>
        public static new ClassificationNetwork FromFile(string fileName)
        {
            return ClassificationNetwork.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a classification neural network from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="ClassificationNetwork"/> from.</param>
        /// <returns>The <see cref="ClassificationNetwork"/> this method creates.</returns>
        public static new ClassificationNetwork FromMemory(byte[] buffer)
        {
            return ClassificationNetwork.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a classification neural network from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="ClassificationNetwork"/> from.</param>
        /// <returns>The <see cref="ClassificationNetwork"/> this method creates.</returns>
        public static new ClassificationNetwork FromString(string value)
        {
            return JsonConvert.DeserializeObject<ClassificationNetwork>(value);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <param name="cloneLayers">The value indicating whether the network layers should be cloned.</param>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override Network Clone(bool cloneLayers) => new ClassificationNetwork(this, cloneLayers);

        /// <summary>
        /// Computes output of the network.
        /// </summary>
        /// <param name="x">The input tensor.</param>
        /// <returns>
        /// The object that contains the computed results and tensor.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        public (IList<IList<(string Answer, float Probability)>> Answers, Tensor Y) Execute(Tensor x)
        {
            const float MaxConfidenceDistance = 0.5f;

            Tensor y = this.Forward(null, x);

            float[] yw = y.Weights;
            int mb = y.Rank == 1 ? 1 : y.Axes[0];
            int numAnswers = y.Rank == 1 ? y.Length : y.Strides[0];

            List<IList<(string, float)>> answers = new List<IList<(string, float)>>(mb);

            float[] ywmb = new float[numAnswers];
            int[] ywidx = new int[numAnswers];
            for (int i = 0, offy = 0; i < mb; i++, offy += numAnswers)
            {
                // copy weights into temporary buffer and sort along with their indexes
                Arrays.Copy(numAnswers, yw, offy, ywmb, 0);
                for (int j = 0; j < numAnswers; j++)
                {
                    ywidx[j] = j;
                }

                Arrays.Sort(numAnswers, ywmb, 0, ywidx, 0, false);

                // create answer for a mini-batch item
                List<(string, float)> mbanswers = new List<(string, float)>(numAnswers);

                float probThreshold = Maximum.Max(ywmb[0] - MaxConfidenceDistance, 0.0f);
                for (int j = 0; j < numAnswers; j++)
                {
                    float prob = ywmb[j];
                    if (prob <= probThreshold)
                    {
                        break;
                    }

                    int idx = ywidx[j];
                    string cls = this.classes[idx];
                    mbanswers.Add((cls, prob));
                }

                answers.Add(mbanswers);
            }

            return (answers, y);
        }

        /// <summary>
        /// Computes output of the network on the specified language model.
        /// </summary>
        /// <param name="x">The input tensor.</param>
        /// <param name="model">The language model.</param>
        /// <returns>
        /// The object that contains the computed results and tensor.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        public (IList<(string Answer, float Probability)> Answers, Tensor Y) ExecuteSequence(Tensor x, Context model)
        {
            Tensor y = this.Forward(null, x);

            CTCBeamSearch bs = new CTCBeamSearch(this.classes, model);

            IList<(string[], float)> results = bs.BeamSearch(y);
            List<(string, float)> answers = new List<(string, float)>(results.Count);

            for (int i = 0, ii = results.Count; i < ii; i++)
            {
                (string[] cls, float prob) = results[i];

                answers.Add((string.Concat(cls), prob));
            }

            return (answers, y);
        }

        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly", Justification = "This method has to be called by JsonSerializer.")]
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.ConfigureAllowedClasses();
        }

        /// <summary>
        /// Creates a masking tensor for allowed classes and sets it to all loss layers.
        /// </summary>
        private void ConfigureAllowedClasses()
        {
            if (this.allowedClasses.Count > 0 && this.allowedClasses.Count != this.classes.Count)
            {
                LossLayer[] lossLayers = this.Graph.Sinks.OfType<LossLayer>().ToArray();
                if (lossLayers != null && lossLayers.Length > 0)
                {
                    Tensor mask = new Tensor("mask", new[] { this.classes.Count })
                    {
                        CalculateGradient = false,
                    };

                    float[] maskw = mask.Weights;
                    for (int i = 0, ii = this.classes.Count; i < ii; i++)
                    {
                        maskw[i] = this.allowedClasses.Contains(this.classes[i]) ? 1.0f : 0.0f;
                    }

                    for (int i = 0, ii = lossLayers.Length; i < ii; i++)
                    {
                        lossLayers[i].Mask = mask;
                    }
                }
            }
        }
    }
}
