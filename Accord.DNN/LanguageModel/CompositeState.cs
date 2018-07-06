// -----------------------------------------------------------------------
// <copyright file="CompositeState.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Accord.DNN.LanguageModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class CompositeState : State
    {
        /// <summary>
        /// The children <see cref="CompositeState"/> this state is referencing.
        /// </summary>
        private readonly List<State> states = new List<State>();

        /// <summary>
        /// <see cref="NextStates"/> method cached result.
        /// </summary>
        private IDictionary<char, State> nextstates = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeState"/> class.
        /// </summary>
        /// <param name="states">The children <see cref="CompositeState"/> this state is referencing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CompositeState(IList<State> states)
        {
            this.AddStates(states);
            this.InitializeState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeState"/> class.
        /// </summary>
        /// <param name="states">The children <see cref="CompositeState"/> this state is referencing.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CompositeState(params State[] states)
        {
            this.AddStates(states);
            this.InitializeState();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="CompositeState"/> class from being created.
        /// </summary>
        private CompositeState()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Create(State state1, State state2)
        {
            if (state1 != null)
            {
                return state2 != null && !object.Equals(state2, state1) ? new CompositeState(state1, state2) : state1;
            }
            else
            {
                return state2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Create(State state1, State state2, State state3)
        {
            if (state1 != null)
            {
                if (state2 != null && state2 != state1)
                {
                    return state3 != null && state3 != state2 && state3 != state1 ?
                        new CompositeState(state1, state2, state3) :
                        new CompositeState(state1, state2);
                }
                else
                {
                    return CompositeState.Create(state1, state3);
                }
            }
            else
            {
                return CompositeState.Create(state2, state3);
            }
        }

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<State> Merge(IEnumerable<State> states1, IEnumerable<State> states2)
        {
            if (states1 == null)
            {
                return states2 ?? Enumerable.Empty<State>();
            }

            if (states2 == null)
            {
                return states1;
            }

            Dictionary<char, State> set = states1.ToDictionary(x => x.Char);

            foreach (State state2 in states2)
            {
                State state1;
                if (set.TryGetValue(state2.Char, out state1))
                {
                    set[state2.Char] = new CompositeState(state1, state2);
                }
                else
                {
                    set[state2.Char] = state2;
                }
            }

            return set.Values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<State> Merge(IEnumerable<State> states1, IEnumerable<State> states2, IEnumerable<State> states3)
        {
            if (states1 == null)
            {
                return CompositeState.Merge(states2, states3);
            }

            if (states2 == null)
            {
                return CompositeState.Merge(states1, states3);
            }

            if (states3 == null)
            {
                return CompositeState.Merge(states1, states2);
            }

            Dictionary<char, State> set = states1.ToDictionary(x => x.Char);

            foreach (State state2 in states2)
            {
                State state1;
                if (set.TryGetValue(state2.Char, out state1))
                {
                    set[state2.Char] = new CompositeState(state1, state2);
                }
                else
                {
                    set[state2.Char] = state2;
                }
            }

            foreach (State state3 in states3)
            {
                State state1;
                if (set.TryGetValue(state3.Char, out state1))
                {
                    CompositeState compositeState1 = state1 as CompositeState;
                    if (compositeState1 != null)
                    {
                        compositeState1.Add(state3);
                    }
                    else
                    {
                        set[state3.Char] = new CompositeState(state1, state3);
                    }
                }
                else
                {
                    set[state3.Char] = state3;
                }
            }

            return set.Values;
        }*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDictionary<char, State> Merge(params IDictionary<char, State>[] states)
        {
            IDictionary<char, State> set = null;

            for (int i = 0, ii = states.Length; i < ii; i++)
            {
                IDictionary<char, State> state = states[i];
                if (state != null && state.Count > 0)
                {
                    if (set == null)
                    {
                        set = state;
                    }
                    else
                    {
                        foreach (State s2 in state.Values)
                        {
                            State s1;
                            if (set.TryGetValue(s2.Char, out s1))
                            {
                                set[s2.Char] = CompositeState.Create(s1, s2);
                            }
                            else
                            {
                                set[s2.Char] = s2;
                            }
                        }
                    }
                }
            }

            return set;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            CompositeState other = obj as CompositeState;
            if (other == null)
            {
                return false;
            }

            return Enumerable.SequenceEqual(this.states, other.states);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.states[0].GetHashCode();

        public void Add(State state)
        {
            this.AddState(state);
            this.InitializeState();
        }

        /// <inheritdoc />
        public override IDictionary<char, State> NextStates()
        {
            if (this.nextstates == null)
            {
                IDictionary<char, State> set = null;

                IList<State> s = this.states;
                for (int i = 0, ii = s.Count; i < ii; i++)
                {
                    IDictionary<char, State> nextStates = s[i].NextStates();

                    if (set == null)
                    {
                        set = nextStates;
                    }
                    else
                    {
                        foreach (State s2 in nextStates.Values)
                        {
                            State s1;
                            if (set.TryGetValue(s2.Char, out s1))
                            {
                                set[s2.Char] = CompositeState.Create(s1, s2);
                            }
                            else
                            {
                                set[s2.Char] = s2;
                            }
                        }
                    }
                }

                this.nextstates = set;
            }

            return this.nextstates;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddStates(IList<State> newstates)
        {
            for (int i = 0, ii = newstates.Count; i < ii; i++)
            {
                ////Debug.Assert(!(newstates[i] is CompositeState));
                this.AddState(newstates[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddState(State state)
        {
            if (state != null)
            {
                if (state is CompositeState compositeState)
                {
                    this.AddStates(compositeState.states);
                }
                else
                {
                    this.states.Add(state);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeState()
        {
            IList<State> s = this.states;
            for (int i = 0, ii = s.Count; i < ii; i++)
            {
                State state = s[i];
                if (i == 0)
                {
                    this.Char = state.Char;
                    this.WordEnd = state.WordEnd;
                    this.CharProbability = state.CharProbability;
                    this.WordEndProbability = state.WordEndProbability;
                }
                else
                {
                    this.WordEnd |= state.WordEnd;
                    this.CharProbability += state.CharProbability;
                    this.WordEndProbability += state.WordEndProbability;
                }
            }
        }
    }
}
