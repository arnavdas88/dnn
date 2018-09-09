// -----------------------------------------------------------------------
// <copyright file="Trainer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Learning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Genix.Core;
    using Genix.MachineLearning;

    /// <summary>
    /// Represents a basic trainer for trainable machines.
    /// </summary>
    /// <typeparam name="TExpected">The type for the expected values.</typeparam>
    public class Trainer<TExpected>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Trainer{TExpected}"/> class.
        /// </summary>
        public Trainer()
        {
        }

        /// <summary>
        /// Gets or sets the number of samples in a mini-batch.
        /// </summary>
        /// <value>
        /// The number of samples in a mini-batch. The default value is 1.
        /// </value>
        public int BatchSize { get; set; } = 1;

        /// <summary>
        /// Gets or sets L1 regularization rate for the SGD algorithm.
        /// </summary>
        /// <value>
        /// L1 regularization rate for the SGD algorithm. Default value is 0.0f.
        /// </value>
        public float RateL1 { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets L2 regularization rate for the SGD algorithm.
        /// </summary>
        /// <value>
        /// L2 regularization rate for the SGD algorithm. Default value is 0.0f.
        /// </value>
        public float RateL2 { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the gradient clipping value.
        /// </summary>
        /// <value>
        /// The gradient clipping value for the SGD algorithm.
        /// All gradients will be clipped to a maximum value of <see cref="ClipValue"/> and# a minimum value of -<see cref="ClipValue"/>.
        /// Default value is <see cref="float.NaN"/>.
        /// </value>
        public float ClipValue { get; set; } = float.NaN;

        /// <summary>
        /// Gets or sets the degree of parallelism for the learning queries.
        /// </summary>
        /// <value>
        /// The degree of parallelism for the query.
        /// The default value is Math.Min(System.Environment.ProcessorCount, 512).
        /// </value>
        public int MaxDegreeOfParallelism { get; set; } = MinMax.Min(System.Environment.ProcessorCount, 512);

        /// <summary>
        /// Performs one epoch of SGD algorithm.
        /// </summary>
        /// <param name="machine">The machine to train.</param>
        /// <param name="input">The sequence of learning samples.</param>
        /// <param name="epoch">The zero-based index of learning epoch.</param>
        /// <param name="algorithm">The training algorithm.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <see cref="TrainingResult"/> object that contains training results for the epoch.
        /// </returns>
        public TrainingResult RunEpoch(
            ITrainableMachine machine,
            IEnumerable<(Tensor, TExpected)> input,
            int epoch,
            ITrainingAlgorithm algorithm,
            ILoss<TExpected> lossFunction,
            CancellationToken cancellationToken)
        {
            if (machine == null)
            {
                throw new ArgumentNullException(nameof(machine));
            }

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = this.MaxDegreeOfParallelism,
            };

            float costLoss = 0.0f;
            float lossL1 = 0.0f;
            float lossL2 = 0.0f;

            int totalSamples = 0;
            int batchCount = 0;
            foreach (List<(Tensor, TExpected)> batch in Partitioner.Partition(input, this.BatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                costLoss += Trainer<TExpected>.LearnBatch(machine, batch, lossFunction, parallelOptions);

                batchCount++;
                totalSamples += batch.Count;

                // perform an update for all sets of weights
                (float lossL1, float lossL2) losses = this.UpdateLayers(machine, epoch, batch.Count, totalSamples, algorithm, parallelOptions);
                lossL1 += losses.lossL1;
                lossL2 += losses.lossL2;
            }

            return new TrainingResult()
            {
                CostLoss = costLoss / totalSamples,
                L1Loss = lossL1 / batchCount,
                L2Loss = lossL2 / batchCount,
            };
        }

        /// <summary>
        /// Performs one iteration of SGD algorithm.
        /// </summary>
        /// <param name="machine">The machine to train.</param>
        /// <param name="samples">The sample to learn on.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <param name="parallelOptions">The object that configures the behavior of this operation..</param>
        /// <returns>
        /// The calculated loss.
        /// </returns>
        private static float LearnBatch(
            ITrainableMachine machine,
            List<(Tensor X, TExpected Expected)> samples,
            ILoss<TExpected> lossFunction,
            ParallelOptions parallelOptions)
        {
            float costLoss = 0.0f;
            object syncObject = new object();

            Action<int, int> body = (a, b) =>
            {
                float loss = machine.Learn(samples.GetRange(a, b - a), lossFunction);
                lock (syncObject)
                {
                    costLoss += loss;
                }
            };

            CommonParallel.For(0, samples.Count, body, parallelOptions);

            return costLoss;
        }

        private (float, float) UpdateLayers(
            ITrainableMachine machine,
            int epoch,
            int batchSize,
            int totalSamples,
            ITrainingAlgorithm algorithm,
            ParallelOptions parallelOptions)
        {
            object syncObject = new object();
            float lossL1 = 0.0f;
            float lossL2 = 0.0f;

            foreach (var w in machine.EnumWeights()
                                     .AsParallel()
                                     .WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism)
                                     .WithCancellation(parallelOptions.CancellationToken))
            {
                (float lossL1, float lossL2) losses = this.UpdateWeights(epoch, w, batchSize, totalSamples, algorithm);
                lock (syncObject)
                {
                    lossL1 += losses.lossL1;
                    lossL2 += losses.lossL2;
                }
            }

            return (lossL1, lossL2);
        }

        /// <summary>
        /// Learns a single layer selected weights.
        /// </summary>
        /// <param name="epoch">The zero-based index of learning epoch.</param>
        /// <param name="layer">The layer to train.</param>
        /// <param name="batchSize">The number of samples in the mini-batch.</param>
        /// <param name="totalSamples">The total number of samples processed to the moment.</param>
        /// <param name="algorithm">The training algorithm.</param>
        /// <returns>The tuple that contains L1 and L2 losses.</returns>
        private (float, float) UpdateWeights(
            int epoch,
            (Tensor w, float RateL1Multiplier, float RateL2Multiplier) layer,
            int batchSize,
            int totalSamples,
            ITrainingAlgorithm algorithm)
        {
            float l1 = 0.0f;
            float l2 = 0.0f;

            float[] w = layer.w.Weights;
            float[] dw = layer.w.Gradient;

            if (batchSize > 1)
            {
                Vectors.MulC(layer.w.Length, 1.0f / batchSize, dw, 0);
            }

            float rateL1 = this.RateL1 * layer.RateL1Multiplier;
            if (rateL1 != 0.0f)
            {
                l1 = layer.w.L1Norm() * rateL1;

                for (int i = 0, ii = w.Length; i < ii; i++)
                {
                    dw[i] += rateL1 * (w[i] > 0 ? 1 : -1);
                }
            }

            float rateL2 = this.RateL2 * layer.RateL2Multiplier;
            if (rateL2 != 0.0f)
            {
                l2 = layer.w.L2Norm() * rateL2;

                Math32f.AddProductC(layer.w.Length, w, 0, rateL2, dw, 0);
            }

            if (!float.IsNaN(this.ClipValue))
            {
                layer.w.ClipGradient(-this.ClipValue, this.ClipValue);
            }

            algorithm.ComputeDeltas(epoch, dw, totalSamples);
            layer.w.Validate();

            // update weights
            Vectors.Add(layer.w.Length, dw, 0, w, 0);

            // zero out gradient so that we can begin accumulating anew
            layer.w.ClearGradient();

            return (l1, l2);
        }

        private static class Partitioner
        {
            /// <summary>
            /// Partitions the sequence of elements into groups of fixed size.
            /// </summary>
            /// <typeparam name="T">The type of elements in the sequence.</typeparam>
            /// <param name="source">The sequence to partition.</param>
            /// <param name="partitionSize">The number of elements in group.</param>
            /// <returns>The sequence of partitions.</returns>
            public static IEnumerable<List<T>> Partition<T>(IEnumerable<T> source, int partitionSize)
            {
                using (IEnumerator<T> enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        List<T> partition = Partitioner.YieldPartition(enumerator, partitionSize);
                        if (partition.Count > 0)
                        {
                            yield return partition;
                        }
                    }
                }
            }

            /// <summary>
            /// Retrieves a collection of elements of specified size from the current position in the sequence.
            /// </summary>
            /// <typeparam name="T">The type of elements in the sequence.</typeparam>
            /// <param name="source">The sequence to partition.</param>
            /// <param name="partitionSize">The number of elements in group.</param>
            /// <returns>One partition.</returns>
            private static List<T> YieldPartition<T>(IEnumerator<T> source, int partitionSize)
            {
                List<T> partition = new List<T>();

                do
                {
                    partition.Add(source.Current);
                }
                while (--partitionSize > 0 && source.MoveNext());

                return partition;
            }
        }
    }
}
