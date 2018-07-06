// -----------------------------------------------------------------------
// <copyright file="Context.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.LanguageModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a base implementation of a context in language model. This is an abstract class.
    /// </summary>
    public abstract class Context : ICloneable
    {
        /// <summary>
        /// The context separator.
        /// </summary>
        public const char WhiteSpace = ' ';

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="minRepeatCount">The minimum number of times the context should be repeated.</param>
        /// <param name="maxRepeatCount">The maximum number of times the context can be repeated.</param>
        protected Context(int minRepeatCount, int maxRepeatCount)
        {
            if (minRepeatCount < 0)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMinimumRepeatCount);
            }

            if (maxRepeatCount < minRepeatCount)
            {
                throw new ArgumentException(Properties.Resources.E_LanguageModel_InvalidMaximumRepeatCount);
            }

            this.MinRepeatCount = minRepeatCount;
            this.MaxRepeatCount = maxRepeatCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class, using the existing <see cref="Context"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Context"/> to copy the data from.</param>
        protected Context(Context other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.MinRepeatCount = other.MinRepeatCount;
            this.MaxRepeatCount = other.MaxRepeatCount;
            this.IsTail = other.IsTail;
            this.Parent = other.Parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        protected Context()
        {
        }

        /// <summary>
        /// Gets a maximum number of times the context can be repeated.
        /// </summary>
        /// <value>
        /// The maximum number of times the context can be repeated.
        /// </value>
        [JsonProperty("maxRepeatCount")]
        public int MaxRepeatCount { get; internal set; } = 1;

        /// <summary>
        /// Gets a minimum number of times the context should be repeated.
        /// </summary>
        /// <value>
        /// The minimum number of times the context should be repeated.
        /// </value>
        [JsonProperty("minRepeatCount")]
        public int MinRepeatCount { get; internal set; } = 1;

        /// <summary>
        /// Gets a parent <see cref="GraphContext"/>.
        /// </summary>
        /// <value>
        /// The <see cref="GraphContext"/> object when this context is a part of the graph; otherwise, <b>null</b>.
        /// </value>
        public GraphContext Parent { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this context is a tail in the parent <see cref="GraphContext"/>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the context is a tail; otherwise, <b>false</b>.
        /// </value>
        public bool IsTail { get; internal set; } = true;

        /// <inheritdoc />
        public abstract State InitialState { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="Context"/> class, using the regular expression.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <param name="cultureInfo">The culture to use while creating the model.</param>
        /// <returns>The <see cref="Context"/> object this method creates.</returns>
        public static Context FromRegex(string regex, CultureInfo cultureInfo)
        {
            return RegexParser.Parse(regex, cultureInfo);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            Context other = obj as Context;
            if (other == null)
            {
                return false;
            }

            return this.MinRepeatCount == other.MinRepeatCount &&
                this.MaxRepeatCount == other.MaxRepeatCount &&
                this.IsTail == other.IsTail &&
                this.Parent == other.Parent;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.MinRepeatCount ^ this.MaxRepeatCount ^ (this.IsTail ? 1 : 0);
        }

        /// <inheritdoc />
        public abstract object Clone();

        /// <summary>
        /// Saves the <see cref="Context"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Context"/>.</param>
        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Saves the <see cref="Context"/> into the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="Context"/>.</returns>
        public byte[] SaveToMemory()
        {
            return UTF8Encoding.UTF8.GetBytes(this.SaveToString());
        }

        /// <summary>
        /// Saves the current <see cref="Context"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="Context"/>.</returns>
        public string SaveToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Converts the context quantifier into a human-readable string.
        /// </summary>
        /// <returns>
        /// The <see cref="String"/> that contains the context quantifier.
        /// </returns>
        protected string QuantifierToString()
        {
            if (this.MinRepeatCount == 1 && this.MaxRepeatCount == 1)
            {
                return string.Empty;
            }
            else if (this.MinRepeatCount == 0 && this.MaxRepeatCount == int.MaxValue)
            {
                return "*";
            }
            else if (this.MinRepeatCount == 1 && this.MaxRepeatCount == int.MaxValue)
            {
                return "+";
            }
            else if (this.MinRepeatCount == 0 && this.MaxRepeatCount == 1)
            {
                return "?";
            }
            else if (this.MinRepeatCount == this.MaxRepeatCount)
            {
                return string.Format(CultureInfo.InvariantCulture, "{{{0}}}", this.MinRepeatCount);
            }
            else if (this.MinRepeatCount == 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "{{,{0}}}", this.MaxRepeatCount);
            }
            else if (this.MaxRepeatCount == int.MaxValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "{{{0},}}", this.MinRepeatCount);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{{{0},{1}}}", this.MinRepeatCount, this.MaxRepeatCount);
            }
        }

        /// <summary>
        /// Describes a character and its location in the <see cref="Context"/> object. This is an abstract class.
        /// </summary>
        internal abstract class ContextState : State
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ContextState"/> class.
            /// </summary>
            /// <param name="character">The character that appears at the current position.</param>
            /// <param name="wordEnd">The value indicating whether the <c>character</c> is at a word ending position.</param>
            /// <param name="contextWordEnd">The value indicating whether the <c>character</c> is at a word ending position within current context.</param>
            /// <param name="charProbability">The probability of the <c>character</c> to appear at the current position.</param>
            /// <param name="wordEndProbability">The probability of the <c>character</c> to end the word at the current position.</param>
            /// <param name="repeatWordEnd">The value indicating whether the context should be repeated after this <c>character</c>.</param>
            /// <param name="repeatCount">The number of times the <see cref="Context"/> was repeated.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected ContextState(
                char character,
                bool wordEnd,
                bool contextWordEnd,
                float charProbability,
                float wordEndProbability,
                bool repeatWordEnd,
                int repeatCount)
                : base(character, wordEnd, charProbability, wordEndProbability)
            {
                this.ContextWordEnd = contextWordEnd;
                this.RepeatWordEnd = repeatWordEnd;
                this.RepeatCount = repeatCount;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextState"/> class.
            /// </summary>
            protected ContextState()
            {
            }

            /// <summary>
            /// Gets or sets a value indicating whether the <see cref="Char"/> is at a word ending position within current context.
            /// </summary>
            /// <value>
            /// <b>true</b> if the character is at word ending position within current context; otherwise, <b>false</b>.
            /// </value>
            public bool ContextWordEnd { get; protected set; }

            /// <summary>
            /// Gets or sets a value indicating whether the context should be repeated after this <see cref="Char"/>.
            /// </summary>
            /// <value>
            /// <b>true</b> if the context should be repeated after this character; otherwise, <b>false</b>.
            /// </value>
            public bool RepeatWordEnd { get; protected set; }

            /// <summary>
            /// Gets or sets the number of times the <see cref="Context"/> was repeated.
            /// </summary>
            /// <value>
            /// The <see cref="Int32"/> that contains repetition count.
            /// </value>
            public int RepeatCount { get; protected set; }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                ContextState other = obj as ContextState;
                if (other == null)
                {
                    return false;
                }

                return this.ContextWordEnd == other.ContextWordEnd &&
                    this.RepeatWordEnd == other.RepeatWordEnd &&
                    this.RepeatCount == other.RepeatCount &&
                    base.Equals(other);
            }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => (int)this.Char;
        }
    }
}
