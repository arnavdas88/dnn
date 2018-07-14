// -----------------------------------------------------------------------
// <copyright file="NetworkTrainer.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DNN.Learning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Accord.DNN;
    using Genix.Core;
    using Layers;

    /// <summary>
    /// Represents a basic trainer for neural nets.
    /// </summary>
    /// <typeparam name="TExpected">The type for the expected values.</typeparam>
    public class NetworkTrainer<TExpected>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTrainer{TExpected}"/> class.
        /// </summary>
        public NetworkTrainer()
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
        /// Performs one epoch of SGD algorithm.
        /// </summary>
        /// <param name="net">The network to train.</param>
        /// <param name="input">The sequence of learning samples.</param>
        /// <param name="epoch">The zero-based index of learning epoch.</param>
        /// <param name="algorithm">The training algorithm.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <see cref="TrainingResult"/> object that contains training results for the epoch.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need a sequence of generic arguments here.")]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 tuples.")]
        public TrainingResult RunEpoch(
            Network net,
            IEnumerable<(Tensor, TExpected)> input,
            int epoch,
            ITrainingAlgorithm algorithm,
            ILoss<TExpected> lossFunction,
            CancellationToken cancellationToken)
        {
            if (net == null)
            {
                throw new ArgumentNullException(nameof(net));
            }

            float costLoss = 0.0f;
            float lossL1 = 0.0f;
            float lossL2 = 0.0f;

            int totalSamples = 0;
            int batchCount = 0;
            foreach (IList<(Tensor, TExpected)> batch in Partitioner.Partition(input, this.BatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                costLoss += NetworkTrainer<TExpected>.LearnBatch(net, batch, lossFunction, cancellationToken);

                batchCount++;
                totalSamples += batch.Count;

                // perform an update for all sets of weights
                (float lossL1, float lossL2) losses = this.UpdateLayers(net, epoch, batch.Count, totalSamples, algorithm, cancellationToken);
                lossL1 += losses.lossL1;
                lossL2 += losses.lossL2;
            }

            // collect memory
            /*GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();*/

            return new TrainingResult()
            {
                CostLoss = costLoss / totalSamples,
                L1Loss = lossL1 / batchCount,
                L2Loss = lossL2 / batchCount
            };
        }

        /// <summary>
        /// Performs one iteration of SGD algorithm.
        /// </summary>
        /// <param name="network">The network to train.</param>
        /// <param name="samples">The sample to learn on.</param>
        /// <param name="lossFunction">The loss function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The calculated loss.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "StyleCop incorrectly interprets C# 7.0 tuples.")]
        private static float LearnBatch(
            Network network,
            IList<(Tensor X, TExpected Expected)> samples,
            ILoss<TExpected> lossFunction,
            CancellationToken cancellationToken)
        {
            float costLoss = 0.0f;
            object syncObject = new object();

            CommonParallel.For(
                0,
                samples.Count,
                ////samples.Count,
                (a, b) =>
                {
                    Session session = new Session(true);

                    ////Tensor[] xs = samples.Select(x => x.X).Skip(a).Take(b - a).ToArray();
                    ////TExpected[] yes = samples.Select(x => x.Expected).Skip(a).Take(b - a).ToArray();

                    ////float loss = network.LearnMany(session, xs, yes, lossFunction).Loss;

                    float loss = 0.0f;
                    for (int i = a; i < b; i++)
                    {
                        loss += network.LearnOne(session, samples[i].X, samples[i].Expected, lossFunction).Loss;
                    }

                    lock (syncObject)
                    {
                        costLoss += loss;
                    }
                },
                cancellationToken);

            return costLoss;
        }

        private (float, float) UpdateLayers(
            Network net,
            int epoch,
            int batchSize,
            int totalSamples,
            ITrainingAlgorithm algorithm,
            CancellationToken cancellationToken)
        {
            object syncObject = new object();
            float lossL1 = 0.0f;
            float lossL2 = 0.0f;

            ////foreach (StochasticLayer layer in net.Graph.Vertices.OfType<StochasticLayer>().Where(x => x.IsTrainable))
            Parallel.ForEach(
                net.Graph.Vertices.OfType<TrainableLayer>().Where(x => x.IsTrainable),
                new ParallelOptions()
                {
                    CancellationToken = cancellationToken
                },
                layer =>
                {
                    foreach (var wg in layer.EnumGradients())
                    {
                        (float lossL1, float lossL2) losses = this.LearnLayerWeights(epoch, wg, batchSize, totalSamples, algorithm);
                        lock (syncObject)
                        {
                            lossL1 += losses.lossL1;
                            lossL2 += losses.lossL2;
                        }
                    }
                });

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
        private (float, float) LearnLayerWeights(
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
                Mathematics.Multiply(layer.w.Length, 1.0f / batchSize, dw, 0, dw, 0);
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

                Mathematics.MultiplyAndAdd(layer.w.Length, rateL2, w, 0, dw, 0);
            }

            if (!float.IsNaN(this.ClipValue))
            {
                layer.w.ClipGradient(-this.ClipValue, this.ClipValue);
            }

            algorithm.ComputeDeltas(epoch, dw, totalSamples);
            layer.w.Validate();

            // update weights
            Mathematics.Add(layer.w.Length, dw, 0, w, 0);

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
            public static IEnumerable<IList<T>> Partition<T>(IEnumerable<T> source, int partitionSize)
            {
                using (IEnumerator<T> enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IList<T> partition = Partitioner.YieldPartition(enumerator, partitionSize);
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
            private static IList<T> YieldPartition<T>(IEnumerator<T> source, int partitionSize)
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
