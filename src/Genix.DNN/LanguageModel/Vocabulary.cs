// -----------------------------------------------------------------------
// <copyright file="Vocabulary.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.LanguageModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a language model built on words.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Vocabulary : Context
    {
        private const int NodeSize = 16;
        private const int SeekToBegin = 0;
        private const int SeekToEnd = -1;
        private const char Separator = LanguageModel.Context.WhiteSpace;

        [JsonProperty("charCount")]
        private readonly int charCount;

        [JsonProperty("wordCount")]
        private readonly int wordCount;

        [JsonProperty("uniqueWordCount")]
        private readonly int uniqueWordCount;

        [JsonProperty("maxWordLength")]
        private readonly int maxWordLength;

        [JsonProperty("nodes")]
        [JsonConverter(typeof(NodesConverter))]
        private readonly Node[] nodes;

        private State initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vocabulary"/> class.
        /// </summary>
        /// <param name="words">The words to include in the vocabulary.</param>
        /// <param name="minRepeatCount">The minimum number of times the context should be repeated.</param>
        /// <param name="maxRepeatCount">The maximum number of times the context can be repeated.</param>
        public Vocabulary(IEnumerable<string> words, int minRepeatCount, int maxRepeatCount)
            : base(minRepeatCount, maxRepeatCount)
        {
            if (words == null)
            {
                throw new ArgumentNullException(nameof(words));
            }

            List<WorkNode> workNodes = new List<WorkNode>();
            foreach (string word in words)
            {
                int length = word.Length;
                this.maxWordLength = Maximum.Max(this.maxWordLength, length);
                Vocabulary.AddWord(workNodes, word, 0, length, 1);
            }

            if (workNodes.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_EmptyVocabulary);
            }

            this.nodes = Vocabulary.PackWorkNodes(
                workNodes,
                out this.charCount,
                out this.wordCount,
                out this.uniqueWordCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vocabulary"/> class.
        /// </summary>
        /// <param name="words">The words to include in the vocabulary and their counts.</param>
        /// <param name="minRepeatCount">The minimum number of times the context should be repeated.</param>
        /// <param name="maxRepeatCount">The maximum number of times the context can be repeated.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        public Vocabulary(IEnumerable<(string, int)> words, int minRepeatCount, int maxRepeatCount)
            : base(minRepeatCount, maxRepeatCount)
        {
            if (words == null)
            {
                throw new ArgumentNullException(nameof(words));
            }

            List<WorkNode> workNodes = new List<WorkNode>();
            foreach ((string word, int count) in words)
            {
                if (count <= 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_LanguageModel_InvalidWordCount, count, word));
                }

                int length = word.Length;
                this.maxWordLength = Maximum.Max(this.maxWordLength, length);
                Vocabulary.AddWord(workNodes, word, 0, length, count);
            }

            if (workNodes.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_EmptyVocabulary);
            }

            this.nodes = Vocabulary.PackWorkNodes(
                workNodes,
                out this.charCount,
                out this.wordCount,
                out this.uniqueWordCount);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vocabulary"/> class, using the existing <see cref="Vocabulary"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Vocabulary"/> to copy the data from.</param>
        public Vocabulary(Vocabulary other)
            : base(other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.nodes = other.nodes;
            this.charCount = other.charCount;
            this.wordCount = other.wordCount;
            this.uniqueWordCount = other.uniqueWordCount;
            this.maxWordLength = other.maxWordLength;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Vocabulary"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private Vocabulary()
        {
        }

        /// <summary>
        /// Gets the number of characters in all words included in the vocabulary.
        /// </summary>
        /// <value>
        /// The number of characters in all words included in the vocabulary.
        /// </value>
        public int CharCount => this.charCount;

        /// <summary>
        /// Gets the number of words included in the vocabulary.
        /// </summary>
        /// <value>
        /// The number of words included in the vocabulary.
        /// </value>
        public int WordCount => this.wordCount;

        /// <summary>
        /// Gets the number of unique words included in the vocabulary.
        /// </summary>
        /// <value>
        /// The number of unique words included in the vocabulary.
        /// </value>
        public int UniqueWordCount => this.uniqueWordCount;

        /// <summary>
        /// Gets the maximum word length in the vocabulary.
        /// </summary>
        /// <value>
        /// The maximum word length in the vocabulary.
        /// </value>
        public int MaxWordLength => this.maxWordLength;

        /// <inheritdoc />
        public override State InitialState
        {
            get
            {
                if (this.initialState == null)
                {
                    this.initialState = new VocabularyState(
                        (char)0,
                        false,
                        this.MinRepeatCount == 0,
                        0.0f,
                        0.0f,
                        false,
                        1,
                        this,
                        Vocabulary.SeekToBegin);
                }

                return this.initialState;
            }
        }

        /// <summary>
        /// Creates a <see cref="Vocabulary"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Vocabulary"/>.</param>
        /// <returns>The <see cref="Vocabulary"/> this method creates.</returns>
        public static Vocabulary FromFile(string fileName)
        {
            return Vocabulary.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a <see cref="Vocabulary"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Vocabulary"/> from.</param>
        /// <returns>The <see cref="Vocabulary"/> this method creates.</returns>
        public static Vocabulary FromMemory(byte[] buffer)
        {
            return Vocabulary.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a graph from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="Vocabulary"/> from.</param>
        /// <returns>The <see cref="Vocabulary"/> this method creates.</returns>
        public static Vocabulary FromString(string value)
        {
            return JsonConvert.DeserializeObject<Vocabulary>(value);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            Vocabulary other = obj as Vocabulary;
            if (other == null)
            {
                return false;
            }

            return this.nodes.Length == other.nodes.Length &&
                NativeMethods.MemCmp(this.nodes, other.nodes, this.nodes.Length * Vocabulary.NodeSize) == 0 &&
                base.Equals(other);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => base.GetHashCode() ^ (this.nodes?.Length ?? 0);

        /// <inheritdoc />
        public override string ToString()
        {
            List<string> words = this.InitialState.Enumerate().Select(x => x.Text).ToList();
            if (words.Count == 1)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}",
                    words[0],
                    this.QuantifierToString());
            }
            else
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "({0}){1}",
                    string.Join("|", words),
                    this.QuantifierToString());
            }
        }

        /// <inheritdoc />
        public override object Clone() => new Vocabulary(this);

        private static void AddWord(IList<WorkNode> workNodes, string s, int startIndex, int length, int count)
        {
            if (startIndex < length)
            {
                char ch = s[startIndex];
                bool wordEnd = startIndex + 1 == length;

                WorkNode node = Vocabulary.AddOrUpdateWorkNode(workNodes, ch, wordEnd, count);

                if (!wordEnd)
                {
                    if (node.Children == null)
                    {
                        node.Children = new List<WorkNode>();
                    }

                    Vocabulary.AddWord(node.Children, s, startIndex + 1, length, count);
                }
            }
        }

        private static WorkNode AddOrUpdateWorkNode(IList<WorkNode> workNodes, char ch, bool wordEnd, int count)
        {
            WorkNode node;

            int index = Vocabulary.BinarySearch(workNodes, ch);
            if (index >= 0)
            {
                node = workNodes[index];
            }
            else
            {
                node = new WorkNode() { Char = ch };
                workNodes.Insert(~index, node);
            }

            node.CharCount += count;

            if (wordEnd)
            {
                node.WordEndsCount += count;
            }

            return node;
        }

        private static int BinarySearch(IList<WorkNode> workNodes, char ch)
        {
            int lo = 0;
            int hi = workNodes.Count - 1;
            while (lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                WorkNode node = workNodes[i];

                if (node.Char == ch)
                {
                    return i;
                }

                if (node.Char < ch)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static Node[] PackWorkNodes(IList<WorkNode> workNodes, out int charCount, out int wordCount, out int uniqueWordCount)
        {
            charCount = 0;
            wordCount = 0;
            uniqueWordCount = 0;
            int nodeCount = Vocabulary.CountWorkNodes(workNodes, ref charCount, out int tailsCount, ref wordCount, ref uniqueWordCount);

            Node[] nodes = new Node[nodeCount];
            Vocabulary.AppendWorkNodes(nodes, 0, workNodes, tailsCount);

            return nodes;
        }

        private static int CountWorkNodes(IList<WorkNode> workNodes, ref int charCount, out int tailsCount, ref int wordCount, ref int uniqueWordCount)
        {
            tailsCount = 0;

            int count = workNodes.Count;
            int totalCount = count;

            for (int i = 0; i < count; i++)
            {
                WorkNode node = workNodes[i];

                charCount += node.CharCount;
                tailsCount += node.CharCount;

                if (node.WordEndsCount > 0)
                {
                    wordCount += node.WordEndsCount;
                    uniqueWordCount++;

                    node.TailsCount += node.WordEndsCount;
                }

                if (node.Children != null)
                {
                    totalCount += Vocabulary.CountWorkNodes(node.Children, ref charCount, out int childrenTailsCount, ref wordCount, ref uniqueWordCount);

                    node.TailsCount += childrenTailsCount;
                }
            }

            return totalCount;
        }

        private static int AppendWorkNodes(Node[] nodes, int startIndex, IList<WorkNode> workNodes, int tailsCount)
        {
            int count = workNodes.Count;
            int totalCount = count;

            // calculate total number of characters in the branch
            /*int charCount = 0;
            for (int i = 0; i < count; i++)
            {
                WorkNode node = workNodes[i];
                charCount += node.CharCount;
            }

            // count word end as a character and add it to the total number of outcomes of a current node
            charCount += wordEndCount;*/

            // add branch
            for (int i = 0; i < count; i++)
            {
                WorkNode node = workNodes[i];

                nodes[startIndex + i].Char = node.Char;
                nodes[startIndex + i].WordEnd = (byte)(node.WordEndsCount > 0 ? 1 : 0);
                nodes[startIndex + i].IsLast = (byte)(i + 1 == count ? 1 : 0);
                nodes[startIndex + i].CharFrequency = (float)node.CharCount / tailsCount;

                if (node.WordEndsCount > 0)
                {
                    // calculate total number of this node tails including word ends
                    /*int totalWordEndCount = node.WordEndCount;
                    if (node.Children != null)
                    {
                        for (int j = 0, jj = node.Children.Count; j < jj; j++)
                        {
                            totalWordEndCount += node.Children[j].CharCount;
                        }
                    }*/

                    nodes[startIndex + i].WordEndFrequency = (float)node.WordEndsCount / node.TailsCount;
                }
            }

            // add children
            int addIndex = startIndex + count;
            for (int i = 0; i < count; i++)
            {
                WorkNode node = workNodes[i];
                if (node.Children != null && node.Children.Count > 0)
                {
                    nodes[startIndex + i].Children = addIndex;

                    int addCount = Vocabulary.AppendWorkNodes(nodes, addIndex, node.Children, node.TailsCount);

                    totalCount += addCount;
                    addIndex += addCount;
                }
                else
                {
                    nodes[startIndex + i].Children = Vocabulary.SeekToEnd;
                }
            }

            return totalCount;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Node
        {
            [FieldOffset(0)]
            public char Char;

            [FieldOffset(2)]
            public byte WordEnd;

            [FieldOffset(3)]
            public byte IsLast;

            [FieldOffset(4)]
            public float CharFrequency;

            [FieldOffset(8)]
            public float WordEndFrequency;

            [FieldOffset(12)]
            public int Children;

            /// <inheritdoc />
            public override string ToString()
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}, End: {1}, Last: {2}, CharFreq: {3}, WordEndFreq: {4}",
                    this.Char,
                    this.WordEnd,
                    this.IsLast,
                    this.CharFrequency,
                    this.WordEndFrequency);
            }

            public void ToByteArray(byte[] ba, int startIndex)
            {
                unsafe
                {
                    fixed (byte* ptr = &ba[startIndex])
                    {
                        *(char*)(ptr + 0) = this.Char;
                        *(byte*)(ptr + 2) = this.WordEnd;
                        *(byte*)(ptr + 3) = this.IsLast;
                        *(float*)(ptr + 4) = this.CharFrequency;
                        *(float*)(ptr + 8) = this.WordEndFrequency;
                        *(int*)(ptr + 12) = this.Children;
                    }
                }
            }

            public void FromByteArray(byte[] ba, int startIndex)
            {
                unsafe
                {
                    fixed (byte* ptr = &ba[startIndex])
                    {
                        this.Char = *(char*)(ptr + 0);
                        this.WordEnd = *(byte*)(ptr + 2);
                        this.IsLast = *(byte*)(ptr + 3);
                        this.CharFrequency = *(float*)(ptr + 4);
                        this.WordEndFrequency = *(float*)(ptr + 8);
                        this.Children = *(int*)(ptr + 12);
                    }
                }
            }
        }

        internal sealed class VocabularyState : ContextState
        {
            /// <summary>
            /// <see cref="NextStates()"/> method cached result.
            /// </summary>
            private IDictionary<char, State> nextstates = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="VocabularyState"/> class.
            /// </summary>
            /// <param name="character">The character that appears at the current position.</param>
            /// <param name="wordEnd">the value indicating whether the <c>character</c> is at a word ending position.</param>
            /// <param name="contextWordEnd">The value indicating whether the <c>character</c> is at a word ending position within current context.</param>
            /// <param name="charProbability">The probability of the <see cref="char"/> to appear at the current position.</param>
            /// <param name="wordEndProbability">The probability of the <see cref="char"/> to end the word at the current position.</param>
            /// <param name="repeatWordEnd">The value indicating whether the context should be repeated after this <c>character</c>.</param>
            /// <param name="repeatCount">The number of times the <see cref="Context"/> was repeated.</param>
            /// <param name="context">The <see cref="Vocabulary"/> this state belongs to.</param>
            /// <param name="seek">The position of characters that extend from the state.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public VocabularyState(
                char character,
                bool wordEnd,
                bool contextWordEnd,
                float charProbability,
                float wordEndProbability,
                bool repeatWordEnd,
                int repeatCount,
                Vocabulary context,
                int seek)
                : base(character, wordEnd, contextWordEnd, charProbability, wordEndProbability, repeatWordEnd, repeatCount)
            {
                this.Context = context;
                this.Seek = seek;
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="VocabularyState"/> class from being created.
            /// </summary>
            private VocabularyState()
            {
            }

            /// <summary>
            /// Gets the context this state belongs to.
            /// </summary>
            /// <value>
            /// The <see cref="Vocabulary"/> object.
            /// </value>
            public Vocabulary Context { get; private set; }

            /// <summary>
            /// Gets the position of characters that extend from the state.
            /// </summary>
            /// <value>
            /// The zero-based index in <see cref="Vocabulary.nodes"/> array.
            /// </value>
            public int Seek { get; private set; }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                VocabularyState other = obj as VocabularyState;
                if (other == null)
                {
                    return false;
                }

                return this.Context == other.Context &&
                    this.Seek == this.Seek &&
                    base.Equals(other);
            }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => base.GetHashCode();

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override IDictionary<char, State> NextStates()
            {
                if (this.nextstates == null)
                {
                    Vocabulary context = this.Context;
                    int repeatCount = this.RepeatCount;

                    IDictionary<char, State> parentStates = null;
                    if (this.ContextWordEnd)
                    {
                        parentStates = context.Parent?.GetInitialState(context)?.NextStates();
                    }

                    IDictionary<char, State> repeatStates = null;
                    if (this.RepeatWordEnd && repeatCount < context.MaxRepeatCount)
                    {
                        /*if (Vocabulary.Separator != (char)0)
                        {*/
                        VocabularyState state = new VocabularyState(
                                Vocabulary.Separator,
                                false,
                                false,
                                1.0f,
                                0.0f,
                                false,
                                repeatCount + 1,
                                context,
                                Vocabulary.SeekToBegin);

                        repeatStates = new Dictionary<char, State>();
                        repeatStates.Add(Vocabulary.Separator, state);

                        /*}
                        else
                        {
                            repeatStates = new VocabularyState(
                                (char)0,
                                false,
                                false,
                                0.0f,
                                0.0f,
                                false,
                                repeatCount + 1,
                                context,
                                Vocabulary.SeekToBegin).NextStates();
                        }*/
                    }

                    // if the state does not point to the end
                    // look into next character
                    IDictionary<char, State> nextStates = null;

                    int seek = this.Seek;
                    if (seek != Vocabulary.SeekToEnd)
                    {
                        nextStates = VocabularyState.NextStates(seek, context, repeatCount);
                    }

                    // combine next and parent states
                    this.nextstates = CompositeState.Merge(nextStates, repeatStates, parentStates);
                }

                return this.nextstates;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static IDictionary<char, State> NextStates(int seek, Vocabulary context, int repeatCount)
            {
                Node[] nodes = context.nodes;

                bool contextWordEnd = repeatCount >= context.MinRepeatCount;
                bool wordEnd = contextWordEnd && context.IsTail;

                Dictionary<char, State> states = new Dictionary<char, State>();
                do
                {
                    bool end = nodes[seek].WordEnd != 0;

                    VocabularyState state = new VocabularyState(
                        nodes[seek].Char,
                        end && wordEnd,
                        end && contextWordEnd,
                        nodes[seek].CharFrequency,
                        nodes[seek].WordEndFrequency,
                        end,
                        repeatCount,
                        context,
                        nodes[seek].Children);

                    states.Add(nodes[seek].Char, state);
                }
                while (nodes[seek++].IsLast == 0);

                return states;
            }
        }

        /// <summary>
        /// C# Interop interface to msvcrt.dll.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
            [SuppressUnmanagedCodeSecurity]
            public static extern int MemCmp(Node[] b1, Node[] b2, int count);
        }

        private class WorkNode
        {
            public char Char { get; set; }

            /// <summary>
            /// Gets or sets the number of words that pass through this character.
            /// </summary>
            public int CharCount { get; set; }

            /// <summary>
            /// Gets or sets the number of word ends at this character.
            /// </summary>
            public int WordEndsCount { get; set; }

            /// <summary>
            /// Gets or sets the number of tails for this character. This is a sum of this <see cref="WordEndsCount"/> and all children's <see cref="CharCount"/>.
            /// </summary>
            public int TailsCount { get; set; }

            public List<WorkNode> Children { get; set; }

            /// <inheritdoc />
            public override string ToString() =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}, Chars: {1}, Ends: {2}, Tails: {3}",
                    this.Char,
                    this.CharCount,
                    this.WordEndsCount,
                    this.TailsCount);
        }

        private class NodesConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => true;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (writer == null)
                {
                    throw new ArgumentNullException(nameof(writer));
                }

                Node[] nodes = (Node[])value;

                int size = Vocabulary.NodeSize * nodes.Length;
                byte[] memory = new byte[size];
                for (int i = 0, ii = nodes.Length, startIndex = 0; i < ii; i++, startIndex += Vocabulary.NodeSize)
                {
                    nodes[i].ToByteArray(memory, startIndex);
                }

                writer.WriteValue(memory);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader == null)
                {
                    throw new ArgumentNullException(nameof(reader));
                }

                if (serializer == null)
                {
                    throw new ArgumentNullException(nameof(serializer));
                }

                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                string s = reader.Value as string;

                byte[] memory = Convert.FromBase64String(s);

                Node[] nodes = new Node[memory.Length / Vocabulary.NodeSize];
                for (int i = 0, ii = nodes.Length, startIndex = 0; i < ii; i++, startIndex += Vocabulary.NodeSize)
                {
                    nodes[i].FromByteArray(memory, startIndex);
                }

                return nodes;
            }
        }
    }
}
