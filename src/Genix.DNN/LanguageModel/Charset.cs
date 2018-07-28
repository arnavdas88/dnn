// -----------------------------------------------------------------------
// <copyright file="Charset.cs" company="Noname, Inc.">
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
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a language model built on character classes.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Charset : Context
    {
        /// <summary>
        /// The characters to include in the generated sequences.
        /// </summary>
        [JsonProperty("characters")]
        private readonly Dictionary<char, float> characters = new Dictionary<char, float>();

        private State initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="Charset"/> class.
        /// </summary>
        /// <param name="characters">The characters to include in the generated sequences.</param>
        /// <param name="minRepeatCount">The minimum number of times the context should be repeated.</param>
        /// <param name="maxRepeatCount">The maximum number of times the context can be repeated.</param>
        public Charset(IEnumerable<char> characters, int minRepeatCount, int maxRepeatCount)
            : base(minRepeatCount, maxRepeatCount)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            Dictionary<char, int> chars = new Dictionary<char, int>();
            int totalCount = 0;
            foreach (char ch in characters)
            {
                chars[ch] = chars.TryGetValue(ch, out int count) ? count + 1 : 1;
                totalCount++;
            }

            if (chars.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_EmptyCharset);
            }

            foreach (KeyValuePair<char, int> kvp in chars.OrderBy(x => x.Key))
            {
                this.characters.Add(kvp.Key, (float)kvp.Value / totalCount);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Charset"/> class.
        /// </summary>
        /// <param name="characters">The characters to include in the generated sequences and their counts.</param>
        /// <param name="minRepeatCount">The minimum number of times the context should be repeated.</param>
        /// <param name="maxRepeatCount">The maximum number of times the context can be repeated.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        public Charset(IEnumerable<(char, int)> characters, int minRepeatCount, int maxRepeatCount)
            : base(minRepeatCount, maxRepeatCount)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            Dictionary<char, int> chars = new Dictionary<char, int>();
            int totalCount = 0;
            foreach ((char ch, int count) in characters)
            {
                if (count <= 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_LanguageModel_InvalidCharacterCount, count, ch));
                }

                chars[ch] = chars.TryGetValue(ch, out int oldcount) ? oldcount + count : count;
                totalCount += count;
            }

            if (chars.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_EmptyCharset);
            }

            foreach (KeyValuePair<char, int> kvp in chars.OrderBy(x => x.Key))
            {
                this.characters.Add(kvp.Key, (float)kvp.Value / totalCount);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Charset"/> class.
        /// </summary>
        /// <param name="characters">The characters to include in the generated sequences and their frequencies.</param>
        /// <param name="minRepeatCount">The minimum number of times the context should be repeated.</param>
        /// <param name="maxRepeatCount">The maximum number of times the context can be repeated.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Use lightweight tuples to simplify design.")]
        public Charset(IEnumerable<(char, float)> characters, int minRepeatCount, int maxRepeatCount)
            : base(minRepeatCount, maxRepeatCount)
        {
            if (characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            Dictionary<char, float> chars = new Dictionary<char, float>();
            float totalFrequency = 0.0f;
            foreach ((char ch, float frequency) in characters)
            {
                if (frequency <= 0.0f)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.E_LanguageModel_InvalidCharacterFrequency, frequency, ch));
                }

                chars[ch] = chars.TryGetValue(ch, out float oldfrequency) ? oldfrequency + frequency : frequency;
                totalFrequency += frequency;
            }

            if (chars.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_EmptyCharset);
            }

            foreach (KeyValuePair<char, float> kvp in chars.OrderBy(x => x.Key))
            {
                this.characters.Add(kvp.Key, kvp.Value / totalFrequency);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Charset"/> class, using the existing <see cref="Charset"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Charset"/> to copy the data from.</param>
        public Charset(Charset other)
            : base(other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.characters = other.characters;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Charset"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private Charset()
        {
        }

        /// <summary>
        /// Gets the characters and their frequencies.
        /// </summary>
        /// <value>
        /// The characters and their frequencies.
        /// </value>
        public IDictionary<char, float> Characters => this.characters;

        /// <inheritdoc />
        public override State InitialState
        {
            get
            {
                if (this.initialState == null)
                {
                    this.initialState = new CharsetState(
                        (char)0,
                        false,
                        this.MinRepeatCount == 0,
                        0.0f,
                        0.0f,
                        false,
                        0,
                        this);
                }

                return this.initialState;
            }
        }

        /// <summary>
        /// Creates a <see cref="Charset"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Charset"/>.</param>
        /// <returns>The <see cref="Charset"/> this method creates.</returns>
        public static Charset FromFile(string fileName)
        {
            return Charset.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a <see cref="Charset"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Charset"/> from.</param>
        /// <returns>The <see cref="Charset"/> this method creates.</returns>
        public static Charset FromMemory(byte[] buffer)
        {
            return Charset.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a graph from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="Charset"/> from.</param>
        /// <returns>The <see cref="Charset"/> this method creates.</returns>
        public static Charset FromString(string value)
        {
            return JsonConvert.DeserializeObject<Charset>(value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            Charset other = obj as Charset;
            if (other == null)
            {
                return false;
            }

            return base.Equals(other) &&
                this.characters.Count == other.characters.Count && !this.characters.Except(other.characters).Any();
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode() ^ this.characters.Count;

        /// <inheritdoc />
        public override string ToString()
        {
            if (this.Characters.Count == 1)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}",
                    this.Characters.Keys.First(),
                    this.QuantifierToString());
            }
            else
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "({0}){1}",
                    string.Join("|", this.Characters.Keys),
                    this.QuantifierToString());
            }
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new Charset(this);
        }

        private sealed class CharsetState : ContextState
        {
            /// <summary>
            /// <see cref="NextStates()"/> method cached result.
            /// </summary>
            private IDictionary<char, State> nextstates = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="CharsetState"/> class.
            /// </summary>
            /// <param name="character">The character that appears at the current position.</param>
            /// <param name="wordEnd">the value indicating whether the <c>character</c> is at a word ending position.</param>
            /// <param name="contextWordEnd">The value indicating whether the <c>character</c> is at a word ending position within current context.</param>
            /// <param name="charProbability">The probability of the <see cref="char"/> to appear at the current position.</param>
            /// <param name="wordProbability">The probability of the word in the language model.</param>
            /// <param name="repeatWordEnd">The value indicating whether the context should be repeated after this <c>character</c>.</param>
            /// <param name="repeatCount">The number of times the <see cref="Context"/> was repeated.</param>
            /// <param name="context">The <see cref="Charset"/> this state belongs to.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CharsetState(
                char character,
                bool wordEnd,
                bool contextWordEnd,
                float charProbability,
                float wordProbability,
                bool repeatWordEnd,
                int repeatCount,
                Charset context)
                : base(character, wordEnd, contextWordEnd, charProbability, wordProbability, repeatWordEnd, repeatCount)
            {
                this.Context = context;
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="CharsetState"/> class from being created.
            /// </summary>
            private CharsetState()
            {
            }

            public Charset Context { get; }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                CharsetState other = obj as CharsetState;
                if (other == null)
                {
                    return false;
                }

                return this.Context == other.Context && base.Equals(other);
            }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => base.GetHashCode();

            /// <inheritdoc />
            public override IDictionary<char, State> NextStates()
            {
                if (this.nextstates == null)
                {
                    Charset context = this.Context;

                    // we are at the end of the word
                    // look into parent context for continuation
                    IDictionary<char, State> parentStates = this.ContextWordEnd ? context.Parent?.GetInitialState(context)?.NextStates() : null;

                    // if the context can be repeated
                    // look into next character
                    IDictionary<char, State> nextStates = null;

                    int repeatCount = this.RepeatCount + 1;
                    if (repeatCount <= context.MaxRepeatCount)
                    {
                        nextStates = CharsetState.NextStates(this.Char, context, repeatCount);
                    }

                    // combine next and parent states
                    this.nextstates = CompositeState.Merge(nextStates, parentStates);
                }

                return this.nextstates;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static IDictionary<char, State> NextStates(char current, Charset context, int repeatCount)
            {
                bool contextWordEnd = repeatCount >= context.MinRepeatCount;
                bool wordEnd = contextWordEnd && context.IsTail;

                Dictionary<char, State> states = new Dictionary<char, State>();
                foreach (KeyValuePair<char, float> kvp in context.characters)
                {
                    if (kvp.Key == LanguageModel.Context.WhiteSpace)
                    {
                        if (current == (char)0 || current == LanguageModel.Context.WhiteSpace)
                        {
                            // never start with spaces
                            // never repeat spaces
                            continue;
                        }

                        // wordEnd=false; never end on space
                        states.Add(
                            kvp.Key,
                            new CharsetState(kvp.Key, false, false, kvp.Value, 1.0f, false, repeatCount, context));
                    }
                    else
                    {
                        states.Add(
                            kvp.Key,
                            new CharsetState(kvp.Key, wordEnd, contextWordEnd, kvp.Value, 1.0f, contextWordEnd, repeatCount, context));
                    }
                }

                return states;
            }
        }
    }
}
