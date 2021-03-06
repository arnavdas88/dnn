﻿// -----------------------------------------------------------------------
// <copyright file="Session.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define NOLEARNING
#define SESSION_DIAG

namespace Genix.DNN
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Genix.Core;
    using Genix.MachineLearning;

    /// <summary>
    /// Contains a sequence of operations performed on a <see cref="Tensor"/>.
    /// </summary>
    public sealed partial class Session
    {
#if !NOLEARNING
        /// <summary>
        /// The sequence of operations performed on a <see cref="Tensor"/>.
        /// </summary>
#if SESSION_DIAG
        private readonly Stack<(string Name, Action Action)> actions = new Stack<(string, Action)>();
#else
        private readonly Stack<Action> actions = new Stack<Action>();
#endif
#endif

        private readonly Dictionary<int, Stack<float[]>> cache = new Dictionary<int, Stack<float[]>>();
        private readonly HashSet<Tensor> tensors = new HashSet<Tensor>();

#if SESSION_DIAG
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Dictionary<string, long> performance = new Dictionary<string, long>();
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        public Session()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="calculateGradients">Determines whether the gradients should be calculated.</param>
        public Session(bool calculateGradients) => this.CalculateGradients = calculateGradients;

        /// <summary>
        /// Gets a value indicating whether the gradients should be calculated.
        /// </summary>
        /// <value>
        /// <b>true</b> to calculate gradients; otherwise, <b>false</b>.
        /// </value>
        public bool CalculateGradients { get; private set; } = true;

        /// <summary>
        /// Runs an operation on the graph.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="action">The operation to run.</param>
        /// <returns>
        /// The result of the operation.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "actionName", Justification = "Reserved for debugging.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RunOperation<T>(string actionName, Func<T> action)
        {
#if SESSION_DIAG
            this.StartStopwatch();
#endif

            T result = action();

#if SESSION_DIAG
            this.StopStopwatch(actionName);
#endif

            return result;
        }

#if !NOLEARNING
        /// <summary>
        /// Adds an operation to the graph.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="action">The operation to add to the graph.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "actionName", Justification = "Reserved for debugging.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(string actionName, Action action)
        {
#if SESSION_DIAG
            this.actions.Push((actionName, action));
#else
            this.actions.Push(action);
#endif
        }
#endif

        /// <summary>
        /// Executes all the action in the graph backwards.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unroll()
        {
#if !NOLEARNING
            while (this.actions.Count > 0)
            {
#if SESSION_DIAG
                (string actionName, Action action) = this.actions.Pop();

                this.StartStopwatch();
                action.Invoke();
                this.StopStopwatch("unroll: " + actionName);
#else
                this.actions.Pop().Invoke();
#endif
            }
#endif
        }

        /// <summary>
        /// Clears all the actions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndSession()
        {
#if !NOLEARNING
            this.actions.Clear();
#endif

            foreach (Tensor x in this.tensors)
            {
                int key = x.Length;
                if (!this.cache.TryGetValue(key, out Stack<float[]> stack))
                {
                    stack = this.cache[key] = new Stack<float[]>();
                }

                stack.Push(x.DetachWeights());

                float[] dw = x.DetachGradient();
                if (dw != null)
                {
                    stack.Push(dw);
                }
            }

            this.tensors.Clear();
        }

        /// <summary>
        /// Detaches the tensor from the session.
        /// </summary>
        /// <param name="x">The tensor to detach.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DetachTensor(Tensor x)
        {
            this.tensors.Remove(x);
        }

        /// <summary>
        /// Allocates the tensor.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The new tensor shape.</param>
        /// <param name="calculateGradient">Determines whether the gradient should be calculated for the allocated tensor.</param>
        /// <returns>
        /// The tensor this method allocates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Tensor AllocateTensor(string name, Shape shape, bool calculateGradient)
        {
            int length = shape.Length;
            Tensor x;
            if (this.cache.TryGetValue(length, out Stack<float[]> stack) && stack.Count > 0)
            {
                x = new Tensor(name, shape, stack.Pop());

#if !NOLEARNING
                if (calculateGradient && stack.Count > 0)
                {
                    x.AttachGradient(stack.Pop(), true);
                }
#endif
            }
            else
            {
                x = new Tensor(name, shape);
            }

            x.CalculateGradient = calculateGradient;
            this.tensors.Add(x);
            return x;
        }

        /// <summary>
        /// Allocates a number of tensors of the same shape.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="count">The number of tensors to allocate.</param>
        /// <param name="shape">The new tensor shape.</param>
        /// <param name="calculateGradient">Determines whether the gradients should be calculated for the allocated tensors.</param>
        /// <returns>
        /// The tensors this method allocates.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Tensor[] AllocateTensors(string name, int count, Shape shape, bool calculateGradient)
        {
            Tensor[] ys = new Tensor[count];
            for (int i = 0; i < count; i++)
            {
                ys[i] = this.AllocateTensor(name, shape, calculateGradient);
            }

            return ys;
        }

        internal string PrintPerformanceReport(int count)
        {
#if SESSION_DIAG
            return string.Join(
                Environment.NewLine,
                this.performance.Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", kvp.Key, kvp.Value / count)));
#else
            return null;
#endif
        }

#if SESSION_DIAG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartStopwatch()
        {
            this.stopwatch.Restart();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopStopwatch(string actionName)
        {
            this.stopwatch.Stop();
            long timeSpent = this.stopwatch.ElapsedTicks;

            if (this.performance.TryGetValue(actionName, out long value))
            {
                this.performance[actionName] = value + timeSpent;
            }
            else
            {
                this.performance[actionName] = timeSpent;
            }
        }
#endif
    }
}
