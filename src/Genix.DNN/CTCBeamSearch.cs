// -----------------------------------------------------------------------
// <copyright file="CTCBeamSearch.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;
    using Genix.MachineLearning.LanguageModel;

    /// <summary>
    /// Performs a beam search decoding the logits.
    /// </summary>
    public class CTCBeamSearch
    {
        /// <summary>
        /// The index of a non-actual class.
        /// </summary>
        private const int NoClass = int.MaxValue;

        /// <summary>
        /// The language model.
        /// </summary>
        private readonly Context languageModel;

        /// <summary>
        /// The classes the network classifies into.
        /// </summary>
        private readonly List<string> classes = new List<string>();

        /// <summary>
        /// The map of <see cref="classes"/> into their indexes.
        /// </summary>
        private readonly Dictionary<char, int> classes2Idx;

        /// <summary>
        /// The classes present in language model that are absent in <see cref="classes"/>.
        /// </summary>
        private readonly List<string> missingClasses = new List<string>();

        /// <summary>
        /// The map of <see cref="missingClasses"/> into their indexes.
        /// </summary>
        private readonly Dictionary<char, int> missingClasses2Idx = new Dictionary<char, int>();

        /// <summary>
        /// The logit used for classes in <see cref="missingClasses"/>.
        /// </summary>
        private readonly float missingProb = (float)Math.Log(0.01f);

        /// <summary>
        /// The index of the class in the <see cref="classes"/> that represents a white space.
        /// </summary>
        private readonly int spaceLabelIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="CTCBeamSearch"/> class.
        /// </summary>
        /// <param name="classes">The classes the network is able to classify.</param>
        public CTCBeamSearch(IList<string> classes)
        {
            if (classes == null)
            {
                throw new ArgumentNullException(nameof(classes));
            }

            this.classes.AddRange(classes);

            this.classes2Idx = new Dictionary<char, int>(classes.Count);
            for (int i = 0, ii = classes.Count; i < ii; i++)
            {
                this.classes2Idx.Add(this.classes[i][0], i);
            }

            this.classes2Idx.TryGetValue(' ', out this.spaceLabelIndex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CTCBeamSearch"/> class.
        /// </summary>
        /// <param name="classes">The classes the network is able to classify.</param>
        /// <param name="languageModel">The language model.</param>
        public CTCBeamSearch(IList<string> classes, Context languageModel)
            : this(classes)
        {
            this.languageModel = languageModel ?? throw new ArgumentNullException(nameof(languageModel));
        }

        /// <summary>
        /// Gets or sets the index of blank label in the alphabet.
        /// </summary>
        /// <value>
        /// The zero-based index of blank label in the alphabet. Default is 0.
        /// </value>
        public int BlankLabelIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum number of hypotheses at each input step.
        /// </summary>
        /// <value>
        /// The maximum number of hypotheses at each input step.
        /// </value>
        public int BufferCount { get; set; } = 20;

        /// <summary>
        /// Gets or sets the maximum number of final hypotheses.
        /// </summary>
        /// <value>
        /// The maximum number of final hypotheses.
        /// </value>
        public int ResultCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether the character frequencies from the language model should be used.
        /// </summary>
        /// <value>
        /// <b>true</b> to use character frequencies; otherwise, <b>false</b>.
        /// </value>
        public bool UseStatistics { get; set; } = false;

        /// <summary>
        /// Performs a beam search decoding the logits.
        /// </summary>
        /// <param name="y">The logits that have been predicted.</param>
        /// <returns>The collection of decoded class sequences along with their weights.</returns>
        public IList<(string[] Classes, float Probability)> BeamSearch(Tensor y)
        {
            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            int T = y.Axes[0];          // Number of mini-batches (time)
            int A = y.Strides[0];       // Number of classes (alphabet size)
#pragma warning restore SA1312 // Variable names must begin with lower-case letter

            // allocate buffers
            BufferManager manager = new BufferManager(T, 2 * this.BufferCount);

            Buffers flip = new Buffers(manager, this.BufferCount);
            Buffers flop = new Buffers(manager, this.BufferCount);

            // convert predicted probabilities into log space
            float[] ylog = new float[y.Length];
            Vectors.Log(y.Length, y.Weights, 0, ylog, 0);

            // run algoritm
            if (this.languageModel != null)
            {
                flop = this.BeamSearch(flip, flop, T, A, ylog, this.languageModel);
            }
            else
            {
                flop = this.BeamSearch(flip, flop, T, A, ylog);
            }

            return this.CreateFinalAnswer(flop);
        }

#pragma warning disable SA1313 // Variable names must begin with lower-case letter
        private Buffers BeamSearch(Buffers flip, Buffers flop, int T, int A, float[] ylog)
#pragma warning restore SA1313 // Variable names must begin with lower-case letter
        {
            // create array that contains classes indexes
            // and then sort both probabilities and indexes
            int[] cls = new int[ylog.Length];
            for (int t = 0, off = 0; t < T; t++, off += A)
            {
                for (int i = 0; i < A; i++)
                {
                    cls[off + i] = i;
                }

                Arrays.Sort(A, ylog, off, cls, off, false);
            }

            // initialize first buffer
            int[] initial = new int[1];
            for (int i = 0; i < A && i < 3; i++)
            {
                int idx = cls[i];
                float prob = ylog[i];

                if (idx == this.BlankLabelIndex)
                {
                    flop.Add(initial, 0, Buffer.InitialHash, prob, float.NegativeInfinity, null);
                }
                else if (idx != this.spaceLabelIndex)
                {
                    // do not start with spaces
                    initial[0] = idx;
                    flop.Add(initial, 1, Buffer.HashCode(Buffer.InitialHash, idx), float.NegativeInfinity, prob, null);
                }
            }

            // flip between buffers
            for (int t = 1, off = A; t < T; t++, off += A)
            {
                Swapping.Swap(ref flip, ref flop);

                for (Buffer buffer = flip.Head; buffer != null; buffer = buffer.Next)
                {
                    int[] values = buffer.Classes;
                    int length = buffer.Length;

                    int lastIdx = length > 0 ? values[length - 1] : CTCBeamSearch.NoClass;

                    float probBlank = float.NegativeInfinity;
                    float probNoBlank = float.NegativeInfinity;

                    for (int i = 0; i < A && i < 3; i++)
                    {
                        int idx = cls[off + i];
                        float prob = ylog[off + i];

                        if (idx == this.BlankLabelIndex)
                        {
                            // get probability of blank character
                            probBlank = buffer.Prob + prob;
                        }
                        else
                        {
                            if (idx == lastIdx)
                            {
                                // get probability of duplicated last character
                                if (!float.IsNegativeInfinity(buffer.ProbNoBlank))
                                {
                                    probNoBlank = buffer.ProbNoBlank + prob;
                                }

                                // when there are two consecutive same characters -
                                // append only there is a blank between them
                                if (float.IsNegativeInfinity(buffer.ProbBlank))
                                {
                                    continue;
                                }

                                // do not repeat blanks
                                if (idx == this.spaceLabelIndex)
                                {
                                    continue;
                                }

                                // same character - append to blank path only
                                prob += buffer.ProbBlank;
                            }
                            else
                            {
                                // different character - append to both blank and noblank paths
                                prob += buffer.Prob;
                            }

                            values[length] = idx;
                            flop.Add(values, length + 1, Buffer.HashCode(buffer.Hash, idx), float.NegativeInfinity, prob, null);
                        }
                    }

                    // repeat buffer with blank and duplicated characters
                    if (!float.IsNegativeInfinity(probBlank) || !float.IsNegativeInfinity(probNoBlank))
                    {
                        flop.Add(values, length, buffer.Hash, probBlank, probNoBlank, null);
                    }
                }

                flip.Free();
            }

            return flop;
        }

#pragma warning disable SA1313 // Variable names must begin with lower-case letter
        private Buffers BeamSearch(Buffers flip, Buffers flop, int T, int A, float[] ylog, Context context)
#pragma warning restore SA1313 // Variable names must begin with lower-case letter
        {
            State initialState = context.InitialState;

            // add blank
            int[] initial = new int[1];
            flop.Add(initial, 0, Buffer.InitialHash, ylog[this.BlankLabelIndex], float.NegativeInfinity, initialState);

            // add characters from context
            IDictionary<char, State> nextstates = initialState.NextStates();
            if (nextstates != null)
            {
                foreach (State nextstate in nextstates.Values)
                {
                    int idx = this.TryGetClass(nextstate.Char);
                    float prob = idx >= 0 ? ylog[idx] : this.missingProb;

                    if (this.UseStatistics)
                    {
                        prob += nextstate.CharProbability;
                    }

                    initial[0] = idx;
                    flop.Add(initial, 1, Buffer.HashCode(Buffer.InitialHash, idx), float.NegativeInfinity, prob, nextstate);
                }
            }

            // flip between buffers
            for (int t = 1, off = A; t < T; t++, off += A)
            {
                Swapping.Swap(ref flip, ref flop);

                for (Buffer buffer = flip.Head; buffer != null; buffer = buffer.Next)
                {
                    int[] values = buffer.Classes;
                    int length = buffer.Length;
                    State state = buffer.State;

                    int lastIdx = length > 0 ? values[length - 1] : CTCBeamSearch.NoClass;

                    // repeat buffer with added blank and duplicated last character
                    float probBlank = buffer.Prob + ylog[off + this.BlankLabelIndex];

                    // no length check here -
                    // if prob is not neg. infinity it means there is at least one character
                    float probNoBlank = float.NegativeInfinity;
                    if (!float.IsNegativeInfinity(buffer.ProbNoBlank))
                    {
                        probNoBlank = buffer.ProbNoBlank + (lastIdx >= 0 ? ylog[off + lastIdx] : this.missingProb);
                    }

                    flop.Add(values, length, buffer.Hash, probBlank, probNoBlank, state);

                    // add context characters
                    nextstates = state.NextStates();
                    if (nextstates != null)
                    {
                        foreach (State nextstate in nextstates.Values)
                        {
                            int idx = this.TryGetClass(nextstate.Char);
                            float prob = idx >= 0 ? ylog[off + idx] : this.missingProb;

                            if (idx == lastIdx)
                            {
                                // when there are two consecutive same characters -
                                // append only there is a blank between them
                                if (float.IsNegativeInfinity(buffer.ProbBlank))
                                {
                                    continue;
                                }

                                // same character - append to blank path only
                                prob += buffer.ProbBlank;
                            }
                            else
                            {
                                // different character - append to both blank and noblank paths
                                prob += buffer.Prob;
                            }

                            if (this.UseStatistics)
                            {
                                prob += nextstate.CharProbability;
                            }

                            values[length] = idx;
                            flop.Add(values, length + 1, Buffer.HashCode(buffer.Hash, idx), float.NegativeInfinity, prob, nextstate);
                        }
                    }
                }

                flip.Free();
            }

            return flop;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TryGetClass(char ch)
        {
            if (this.classes2Idx.TryGetValue(ch, out int idx))
            {
                return idx;
            }

            if (!this.missingClasses2Idx.TryGetValue(ch, out idx))
            {
                idx = ~this.missingClasses.Count;
                this.missingClasses.Add(new string(ch, 1));

                this.missingClasses2Idx[ch] = idx;
            }

            return idx;
        }

        private IList<(string[], float)> CreateFinalAnswer(Buffers flop)
        {
            List<(string[] Classes, float Prob)> final = new List<(string[], float)>(MinMax.Max(this.ResultCount, flop.Count));
            float amax = 0.0f;
            float esum = float.NegativeInfinity;

            for (Buffer buffer = flop.Head; buffer != null && final.Count < this.ResultCount; buffer = buffer.Next)
            {
                if (buffer.State == null || buffer.State.WordEnd)
                {
                    float prob = buffer.Prob;
                    if (buffer.State != null && this.UseStatistics)
                    {
                        prob += buffer.State.WordEndProbability;
                    }

                    if (final.Count == 0)
                    {
                        amax = prob;
                    }
                    else if (amax - prob > (float)Math.Log(100.0f))
                    {
                        continue;
                    }

                    esum = Mathematics.LogSumExp(esum, prob);

                    int length = buffer.Length;
                    int[] cls = buffer.Classes;

                    string[] hypotheses = new string[length];
                    for (int i = 0; i < length; i++)
                    {
                        int idx = cls[i];
                        hypotheses[i] = idx < 0 ? this.missingClasses[~idx] : this.classes[idx];
                    }

                    final.Add((hypotheses, prob));
                }
            }

            // normalize probabilities
            if (!float.IsNegativeInfinity(esum))
            {
                for (int i = 0; i < final.Count; i++)
                {
                    final[i] = (final[i].Classes, (float)Math.Exp(final[i].Prob - esum));
                }
            }

            return final;
        }

        /// <summary>
        /// Represents a sequence of hypotheses at input step.
        /// </summary>
        private class Buffer : LinkedCollectionItem<Buffer>
        {
            /// <summary>
            /// The initial value of <see cref="Hash"/>.
            /// </summary>
            public const int InitialHash = 314159265;

            /// <summary>
            /// Initializes a new instance of the <see cref="Buffer"/> class.
            /// </summary>
            /// <param name="maxLength">The maximum number of hypotheses / length of input sequence.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Buffer(int maxLength)
            {
                this.Classes = new int[maxLength];
            }

            /// <summary>
            /// Gets the sequence of hypotheses.
            /// </summary>
            /// <value>
            /// The collection of hypotheses indexes.
            /// </value>
            public int[] Classes { get; }

            /// <summary>
            /// Gets the number of hypotheses in <see cref="Classes"/> sequence.
            /// </summary>
            /// <value>
            /// The number of hypotheses in <see cref="Classes"/> sequence.
            /// </value>
            public int Length { get; private set; }

            /// <summary>
            /// Gets the hash code that represents <see cref="Classes"/>.
            /// </summary>
            /// <value>
            /// The hash code that represents <see cref="Classes"/>.
            /// </value>
            public int Hash { get; private set; } = Buffer.InitialHash;

            /// <summary>
            /// Gets the overall probability of <see cref="Classes"/>.
            /// </summary>
            /// <value>
            /// The log-sum of <see cref="ProbBlank"/> and <see cref="ProbNoBlank"/>.
            /// </value>
            public float Prob { get; private set; } = float.NegativeInfinity;

            public float ProbBlank { get; private set; } = float.NegativeInfinity;

            public float ProbNoBlank { get; private set; } = float.NegativeInfinity;

            /// <summary>
            /// Gets the state of the language model that represents <see cref="Classes"/>.
            /// </summary>
            /// <value>
            /// The <see cref="State"/> object.
            /// </value>
            public State State { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int HashCode(int hash, int idx)
            {
                ////return (hash << 7) ^ value;
                hash = ((hash >> 3) ^ hash ^ (idx >> 4)) & 0x0FFFFFFF | (hash << 28);
                int cst = hash >> 24;
                hash = ((cst >> 3) ^ cst ^ idx) & 0x0F | (hash << 4);
                return hash == 0 ? Buffer.InitialHash : hash;
            }

            public override string ToString() => string.Format(
                CultureInfo.InvariantCulture,
                "{0}: {1}: {2}: {3}",
                string.Join(" ", this.Classes.Take(this.Length)),
                this.Prob,
                this.ProbBlank,
                this.ProbNoBlank);
            ////new string(this.Classes, 0, this.Length),

            /// <summary>
            /// Removes all the data from this <see cref="Buffer"/>.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                this.Length = 0;
                this.Hash = 0;
                this.Prob = float.NegativeInfinity;
                this.ProbBlank = float.NegativeInfinity;
                this.ProbNoBlank = float.NegativeInfinity;
                this.State = null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(int[] classes, int length, int hash, float probBlank, float probNoBlank, State state)
            {
                Arrays.Copy(length, classes, 0, this.Classes, 0);
                this.Length = length;
                this.Hash = hash;

                this.ProbBlank = probBlank;
                this.ProbNoBlank = probNoBlank;
                this.Prob = Mathematics.LogSumExp(probBlank, probNoBlank);

                this.State = state;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float probBlank, float probNoBlank, State state)
            {
                this.ProbBlank = Mathematics.LogSumExp(this.ProbBlank, probBlank);
                this.ProbNoBlank = Mathematics.LogSumExp(this.ProbNoBlank, probNoBlank);
                this.Prob = Mathematics.LogSumExp(this.ProbBlank, this.ProbNoBlank);

                this.State = State.Create(this.State, state);
            }
        }

        /// <summary>
        /// Manages the collection of <see cref="Buffer"/> objects at each input step.
        /// </summary>
        private class Buffers : LinkedCollection<Buffer>
        {
            /// <summary>
            /// The <see cref="BufferManager"/> used to allocation of new  <see cref="Buffer"/> objects.
            /// </summary>
            private readonly BufferManager manager;

            /// <summary>
            /// The maximum number of buffers at each input step.
            /// </summary>
            private readonly int capacity;

            /// <summary>
            /// Initializes a new instance of the <see cref="Buffers"/> class.
            /// </summary>
            /// <param name="manager">The <see cref="BufferManager"/> used to allocation of new  <see cref="Buffer"/> objects.</param>
            /// <param name="capacity">The maximum number of buffers at each input step.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Buffers(BufferManager manager, int capacity)
            {
                this.manager = manager;
                this.capacity = capacity;
            }

            /// <summary>
            /// Add a new set of hypotheses at the input step.
            /// </summary>
            /// <param name="classes">The new set of hypotheses.</param>
            /// <param name="length">The number of elements in <c>classes</c>.</param>
            /// <param name="hash">The hash code that describes <c>classes</c>.</param>
            /// <param name="probBlank">The probability of <c>classes</c> sequence that ends with blank class.</param>
            /// <param name="probNoBlank">The probability of <c>classes</c> sequence that does not end with blank class.</param>
            /// <param name="state">The state of the language model that represents <c>classes</c>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(int[] classes, int length, int hash, float probBlank, float probNoBlank, State state)
            {
                float prob = Mathematics.LogSumExp(probBlank, probNoBlank);

                // search the matching buffer
                Buffer after = null;
                for (Buffer buffer = this.Head; buffer != null; buffer = buffer.Next)
                {
                    if (buffer.Length == length &&
                        buffer.Hash == hash &&
                        buffer.Classes[0] == classes[0] &&
                        Arrays.Equals(length, buffer.Classes, 0, classes, 0))
                    {
                        // update buffer
                        buffer.Update(probBlank, probNoBlank, state);

                        // move buffer up
                        for (after = buffer.Prev; after != null; after = after.Prev)
                        {
                            if (after.Prob >= buffer.Prob)
                            {
                                break;
                            }
                        }

                        if (after != buffer.Prev)
                        {
                            this.Remove(buffer);

                            if (after == null)
                            {
                                this.AddHead(buffer);
                            }
                            else
                            {
                                this.AddAfter(after, buffer);
                            }
                        }

                        // we are done
                        return;
                    }

                    if (buffer.Prob >= prob)
                    {
                        after = buffer;
                    }
                }

                // add into buffer
                Buffer newbuffer = null;
                if (this.Count >= this.capacity)
                {
                    // list is full
                    if (after != this.Tail)
                    {
                        newbuffer = this.RemoveTail();
                    }
                }
                else
                {
                    newbuffer = this.manager.Allocate();
                }

                if (newbuffer != null)
                {
                    newbuffer.Set(classes, length, hash, probBlank, probNoBlank, state);

                    if (after == null)
                    {
                        this.AddHead(newbuffer);
                    }
                    else
                    {
                        this.AddAfter(after, newbuffer);
                    }
                }
            }

            /// <summary>
            /// Releases all buffers used at the input step and puts them into a set of reusable buffers.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Free()
            {
                while (this.Head != null)
                {
                    this.manager.Free(this.RemoveHead());
                }
            }
        }

        /// <summary>
        /// Manages allocation of <see cref="Buffer"/> objects.
        /// </summary>
        private class BufferManager : Stack<Buffer>
        {
            private readonly int maxLength;

            /// <summary>
            /// Initializes a new instance of the <see cref="BufferManager"/> class.
            /// </summary>
            /// <param name="maxLength">The maximum number of classes in the buffers / length of input sequence.</param>
            /// <param name="capacity">The initial number of <see cref="Buffer"/> objects that the <see cref="BufferManager"/> can contain.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BufferManager(int maxLength, int capacity)
                : base(capacity)
            {
                this.maxLength = maxLength;
            }

            /// <summary>
            /// Allocates a new <see cref="Buffer"/>.
            /// </summary>
            /// <returns>The <see cref="Buffer"/> this method creates.</returns>
            /// <remarks>
            /// This method reuses previously allocated <see cref="Buffer"/> objects or creates a new one if none are available.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Buffer Allocate()
            {
                return this.Count > 0 ? this.Pop() : new Buffer(this.maxLength);
            }

            /// <summary>
            /// Puts the specified <see cref="Buffer"/> into a collection of reusable objects.
            /// </summary>
            /// <param name="buffer">The <see cref="Buffer"/> that is no longer needed.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Free(Buffer buffer)
            {
                buffer.Clear();
                this.Push(buffer);
            }
        }
    }
}
