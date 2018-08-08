// -----------------------------------------------------------------------
// <copyright file="IFeatureDetector.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.DocumentAnalysis.FeatureDetectors
{
    using System.Threading;
    using Genix.Imaging;

    /// <summary>
    /// Defines a contract for feature (point of interest) detection.
    /// </summary>
    public interface IFeatureDetector
    {
        /// <summary>
        /// Finds points of interest on the specified <see cref="Image"/> and returns feature vectors.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to process.</param>
        /// <param name="cancellationToken">The cancellationToken token used to notify the <see cref="IFeatureDetector"/> that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Features"/> object that contains the feature vectors.
        /// </returns>
        Features Detect(Image image, CancellationToken cancellationToken);
    }
}
