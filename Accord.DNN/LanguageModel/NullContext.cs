// -----------------------------------------------------------------------
// <copyright file="NullContext.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.LanguageModel
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
    public sealed class NullContext : Context
    {
        private State initialState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullContext"/> class.
        /// </summary>
        public NullContext() : base(1, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullContext"/> class, using the existing <see cref="NullContext"/> object.
        /// </summary>
        /// <param name="other">The <see cref="NullContext"/> to copy the data from.</param>
        public NullContext(NullContext other) : base(other)
        {
        }

        /// <inheritdoc />
        public override State InitialState
        {
            get
            {
                if (this.initialState == null)
                {
                    this.initialState = new NullState(this);
                }

                return this.initialState;
            }
        }

        /// <summary>
        /// Creates a <see cref="NullContext"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="NullContext"/>.</param>
        /// <returns>The <see cref="NullContext"/> this method creates.</returns>
        public static NullContext FromFile(string fileName)
        {
            return NullContext.FromString(File.ReadAllText(fileName, Encoding.UTF8));
        }

        /// <summary>
        /// Creates a <see cref="NullContext"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="NullContext"/> from.</param>
        /// <returns>The <see cref="NullContext"/> this method creates.</returns>
        public static NullContext FromMemory(byte[] buffer)
        {
            return NullContext.FromString(UTF8Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Creates a graph from the specified <see cref="System.String"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.String"/> to read the <see cref="NullContext"/> from.</param>
        /// <returns>The <see cref="NullContext"/> this method creates.</returns>
        public static NullContext FromString(string value)
        {
            return JsonConvert.DeserializeObject<NullContext>(value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            NullContext other = obj as NullContext;
            if (other == null)
            {
                return false;
            }

            return base.Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => string.Empty;

        /// <inheritdoc />
        public override object Clone() => new NullContext(this);

        private sealed class NullState : ContextState
        {
            /// <summary>
            /// <see cref="NextStates"/> method cached result.
            /// </summary>
            private IDictionary<char, State> nextstates;

            /// <summary>
            /// Initializes a new instance of the <see cref="NullState"/> class.
            /// </summary>
            /// <param name="context">The <see cref="NullContext"/> this state belongs to.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NullState(NullContext context)
                : base((char)0, false, false, 0.0f, 0.0f, false, 0)
            {
                this.Context = context;
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="NullState"/> class from being created.
            /// </summary>
            private NullState()
            {
            }

            public NullContext Context { get; private set; }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }

                NullState other = obj as NullState;
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
                    NullContext context = this.Context;

                    this.nextstates = context.Parent?.GetInitialState(context)?.NextStates();
                }

                return this.nextstates;
            }
        }
    }
}
