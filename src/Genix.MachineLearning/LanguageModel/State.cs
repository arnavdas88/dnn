// -----------------------------------------------------------------------
// <copyright file="State.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.LanguageModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Describes a character and its location in the <see cref="Context"/> object. This is an abstract class.
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="character">The character that appears at the current position.</param>
        /// <param name="wordEnd">The value indicating whether the <c>character</c> is at a word ending position.</param>
        /// <param name="charProbability">The probability of the <c>character</c> to appear at the current position.</param>
        /// <param name="wordEndProbability">The probability of the <c>character</c> to end the word at the current position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected State(char character, bool wordEnd, float charProbability, float wordEndProbability)
        {
            this.Char = character;
            this.WordEnd = wordEnd;
            this.CharProbability = charProbability;
            this.WordEndProbability = wordEndProbability;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        protected State()
        {
        }

        /// <summary>
        /// Gets or sets the character that appears at the current position.
        /// </summary>
        /// <value>
        /// The character that appears at the current position.
        /// </value>
        public char Char { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Char"/> is at a word ending position.
        /// </summary>
        /// <value>
        /// <b>true</b> if the character is at word ending position; otherwise, <b>false</b>.
        /// </value>
        public bool WordEnd { get; protected set; }

        /// <summary>
        /// Gets or sets the probability of the <see cref="Char"/> to appear at the current position.
        /// </summary>
        /// <value>
        /// The value between 0.0f and 1.0f.
        /// </value>
        public float CharProbability { get; protected set; }

        /// <summary>
        /// Gets or sets the probability of the <see cref="Char"/> to end the word at the current position.
        /// </summary>
        /// <value>
        /// The value between 0.0f and 1.0f.
        /// </value>
        public float WordEndProbability { get; protected set; }

        /// <summary>
        /// Create a <see cref="State"/> out of two other states.
        /// </summary>
        /// <param name="state1">The first <see cref="State"/>.</param>
        /// <param name="state2">The second <see cref="State"/>.</param>
        /// <returns>
        /// The <see cref="CompositeState"/> object this method creates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Create(State state1, State state2)
        {
            return CompositeState.Create(state1, state2);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            State other = obj as State;
            if (other == null)
            {
                return false;
            }

            return this.Char == other.Char &&
                this.WordEnd == other.WordEnd &&
                this.CharProbability == other.CharProbability &&
                this.WordEndProbability == other.WordEndProbability;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.Char;

        /// <inheritdoc />
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "{0}: {1}: {2}",
                this.Char,
                this.CharProbability,
                this.WordEnd);

        /// <summary>
        /// Creates new states for all characters that extend from the current states.
        /// </summary>
        /// <returns>The <see cref="State"/> enumeration.</returns>
        public abstract IDictionary<char, State> NextStates();

        /// <summary>
        /// Enumerate all words that come out of this state.
        /// </summary>
        /// <returns>
        /// The sequence of tuples containing the word and its frequency.
        /// </returns>
        public IEnumerable<(string Text, float Probability)> Enumerate()
        {
            char[] chars = new char[256]; // TODO: calculate max word length

            return this.Enumerate(chars, 0, 1.0f);
        }

        private IEnumerable<(string Text, float Probability)> Enumerate(char[] value, int startIndex, float prob)
        {
            IDictionary<char, State> nextstates = this.NextStates();
            if (nextstates != null)
            {
                foreach (State nextState in nextstates.Values)
                {
                    value[startIndex] = nextState.Char;

                    float charProb = prob * nextState.CharProbability;

                    if (nextState.WordEnd)
                    {
                        yield return (new string(value, 0, startIndex + 1), charProb * nextState.WordEndProbability);
                    }

                    foreach (var s in nextState.Enumerate(value, startIndex + 1, charProb))
                    {
                        yield return s;
                    }
                }
            }
        }
    }
}
