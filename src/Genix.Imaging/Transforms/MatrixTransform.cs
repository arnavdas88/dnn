// -----------------------------------------------------------------------
// <copyright file="MatrixTransform.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Imaging
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a affine transformation in 2-D space of an <see cref="Image"/>.
    /// </summary>
    public class MatrixTransform : TransformBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixTransform"/> class, using shift transformation.
        /// </summary>
        /// <param name="shiftX">The horizontal shift, in pixels.</param>
        /// <param name="shiftY">The vertical shift, in pixels.</param>
        public MatrixTransform(int shiftX, int shiftY)
        {
            this.Matrix = new Matrix(1, 0, 0, 1, shiftX, shiftY);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixTransform"/> class, using scale transformation.
        /// </summary>
        /// <param name="scaleX">The horizontal scale.</param>
        /// <param name="scaleY">The vertical scale.</param>
        public MatrixTransform(double scaleX, double scaleY)
        {
            this.Matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixTransform"/> class, using rotate transformation.
        /// </summary>
        /// <param name="origin">The point the rotation is about.</param>
        /// <param name="angle">The rotation angle, in radians.</param>
        public MatrixTransform(Point origin, double angle)
        {
            this.Matrix = Matrix.Identity;
            this.Matrix.RotateAt(angle, origin.X, origin.Y);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixTransform"/> class, using affine transformation.
        /// </summary>
        /// <param name="matrix">The matrix the describes transformation.</param>
        public MatrixTransform(Matrix matrix)
        {
            this.Matrix = matrix;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="MatrixTransform" /> class from being created.
        /// </summary>
        [JsonConstructor]
        private MatrixTransform()
        {
        }

        /// <summary>
        /// Gets a transformation matrix.
        /// </summary>
        /// <value>
        /// The transformation matrix.
        /// </value>
        [JsonProperty("matrix")]
        public Matrix Matrix { get; private set; }

        /// <inheritdoc />
        public override Point Convert(Point value)
        {
            return this.Matrix.Transform(value);
        }

        /// <inheritdoc />
        public override Rect Convert(Rect value)
        {
            return Rect.Transform(value, this.Matrix);
        }

        /// <inheritdoc />
        public override TransformBase Append(TransformBase transform)
        {
            if (transform is IdentityTransform)
            {
                return this;
            }
            else if (transform is MatrixTransform matrixTransform)
            {
                Matrix matrix = this.Matrix;
                matrix.Append(matrixTransform.Matrix);
                return new MatrixTransform(matrix);
            }
            else if (transform is CompositeTransform compositeTransform)
            {
                return new CompositeTransform(
                    Enumerable.Repeat(this, 1).Concat(compositeTransform.Transforms));
            }
            else
            {
                return new CompositeTransform(new TransformBase[] { this, transform });
            }
        }
    }
}
