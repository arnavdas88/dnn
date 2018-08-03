// -----------------------------------------------------------------------
// <copyright file="Labels.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning.Classifcation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Helpers method to manage classification labels.
    /// </summary>
    public static class Labels
    {
        /// <summary>
        /// Calculates the amount of positive and negative samples in the set and returns their proportion.
        /// </summary>
        /// <param name="labels">The labels.</param>
        /// <param name="positives">The number of positive samples in <paramref name="labels"/>.</param>
        /// <param name="negatives">The number of negatives samples in <paramref name="labels"/>.</param>
        /// <returns>
        /// The percentage of positive samples in <paramref name="labels"/>.
        /// </returns>
        public static float GetRatio(IEnumerable<bool> labels, out int positives, out int negatives)
        {
            int count = 0;
            positives = 0;
            foreach (bool label in labels)
            {
                count++;
                if (label)
                {
                    positives++;
                }
            }

            negatives = count - positives;
            return positives / (float)(positives + negatives);
        }
    }
}
