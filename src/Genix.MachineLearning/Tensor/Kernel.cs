// -----------------------------------------------------------------------
// <copyright file="Kernel.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes the kernel size, stride, and padding.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Kernel : IEquatable<Kernel>, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel"/> class.
        /// </summary>
        /// <param name="width">The width of the kernel.</param>
        /// <param name="height">The height of the kernel.</param>
        /// <param name="strideX">The horizontal step at which kernel is applied.</param>
        /// <param name="strideY">The vertical step at which kernel is applied.</param>
        public Kernel(int width, int height, int strideX, int strideY)
            : this(width, height, strideX, strideY, 0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel"/> class.
        /// </summary>
        /// <param name="width">The width of the kernel.</param>
        /// <param name="height">The height of the kernel.</param>
        /// <param name="strideX">The horizontal step at which kernel is applied.</param>
        /// <param name="strideY">The vertical step at which kernel is applied.</param>
        /// <param name="paddingX">The size of the horizontal zero padding.</param>
        /// <param name="paddingY">The size of the vertical zero padding.</param>
        public Kernel(int width, int height, int strideX, int strideY, int paddingX, int paddingY)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", width, Properties.Resources.E_InvalidKernelSize);
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", height, Properties.Resources.E_InvalidKernelSize);
            }

            if (strideX <= 0)
            {
                throw new ArgumentOutOfRangeException("strideX", strideX, Properties.Resources.E_InvalidStride);
            }

            if (strideY <= 0)
            {
                throw new ArgumentOutOfRangeException("strideY", strideY, Properties.Resources.E_InvalidStride);
            }

            this.Width = width;
            this.Height = height;
            this.StrideX = strideX;
            this.StrideY = strideY;
            this.PaddingX = paddingX;
            this.PaddingY = paddingY;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel"/> class, using the existing <see cref="Kernel"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Kernel"/> to copy the data from.</param>
        public Kernel(Kernel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Width = other.Width;
            this.Height = other.Height;
            this.StrideX = other.StrideX;
            this.StrideY = other.StrideY;
            this.PaddingX = other.PaddingX;
            this.PaddingY = other.PaddingY;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Kernel"/> class from being created.
        /// </summary>
        [JsonConstructor]
        private Kernel()
        {
        }

        /// <summary>
        /// Gets the width of the kernel.
        /// </summary>
        /// <value>
        /// The width of the kernel.
        /// </value>
        [JsonProperty("Width")]
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the kernel.
        /// </summary>
        /// <value>
        /// The height of the kernel.
        /// </value>
        [JsonProperty("Height")]
        public int Height { get; private set; }

        /// <summary>
        /// Gets the horizontal step at which the kernel is applied.
        /// </summary>
        /// <value>
        /// The horizontal step at which the kernel is applied.
        /// </value>
        [JsonProperty("StrideX")]
        public int StrideX { get; private set; }

        /// <summary>
        /// Gets the vertical step at which the kernel is applied.
        /// </summary>
        /// <value>
        /// The vertical step at which the kernel is applied.
        /// </value>
        [JsonProperty("StrideY")]
        public int StrideY { get; private set; }

        /// <summary>
        /// Gets the size of the horizontal zero padding.
        /// </summary>
        /// <value>
        /// The size of the horizontal zero padding.
        /// </value>
        [JsonProperty("PaddingX")]
        public int PaddingX { get; private set; }

        /// <summary>
        /// Gets the size of the vertical zero padding.
        /// </summary>
        /// <value>
        /// The size of the vertical zero padding.
        /// </value>
        [JsonProperty("PaddingY")]
        public int PaddingY { get; private set; }

        /// <summary>
        /// Gets the size of the kernel.
        /// </summary>
        /// <value>
        /// The <see cref="Width"/> multiplied by <see cref="Height"/>.
        /// </value>
        public int Size => this.Width * this.Height;

        /// <summary>
        /// Determines whether this <see cref="Kernel"/> contains the same data as the specified <see cref="Kernel"/>.
        /// </summary>
        /// <param name="other">The <see cref="Kernel"/> to test.</param>
        /// <returns><b>true</b> if <c>other</c> is a <see cref="Kernel"/> and has the same data as this <see cref="Kernel"/>.</returns>
        public bool Equals(Kernel other)
        {
            if (other == null)
            {
                return false;
            }

            if (other == this)
            {
                return true;
            }

            return this.Width == other.Width &&
                this.Height == other.Height &&
                this.StrideX == other.StrideX &&
                this.StrideY == other.StrideY &&
                this.PaddingX == other.PaddingX &&
                this.PaddingY == other.PaddingY;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as Kernel);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Width ^ this.Height ^ this.StrideX ^ this.StrideY ^ this.PaddingX ^ this.PaddingY;
        }

        /// <inheritdoc />
        public override string ToString() => this.ToString(1, 1);

        /// <summary>
        /// Converts this <see cref="Kernel"/> into a human-readable string.
        /// </summary>
        /// <param name="defaultStrideX">The default horizontal stride value.</param>
        /// <param name="defaultStrideY">The default vertical stride value.</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(int defaultStrideX, int defaultStrideY)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", this.Width);
            if (this.Width != this.Height)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "x{0}", this.Height);
            }

            if (this.StrideX != defaultStrideX || this.StrideY != defaultStrideY)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "+{0}", this.StrideX);
                if (this.StrideX != this.StrideY)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "x{0}", this.StrideY);
                }

                sb.Append("(S)");
            }

            if (this.PaddingX != 0 || this.PaddingY != 0)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "+{0}", this.PaddingX);
                if (this.PaddingX != this.PaddingY)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "x{0}", this.PaddingY);
                }

                sb.Append("(P)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new Kernel(this);
        }

        /// <summary>
        /// Determines whether the kernel dimensions matches the input window.
        /// </summary>
        /// <param name="width">The input width.</param>
        /// <param name="height">The input height.</param>
        /// <returns>
        /// <b>true</b> if the kernel matches the input window; otherwise, <b>false</b>.
        /// </returns>
        public bool SameAsInput(int width, int height)
        {
            return this.Width == width &&
                   this.Height == height &&
                   this.PaddingX == 0 &&
                   this.PaddingY == 0;
        }

        /// <summary>
        /// Computes the output width.
        /// </summary>
        /// <param name="width">The input width.</param>
        /// <returns>
        /// The output width.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateOutputWidth(int width)
        {
            return Kernel.CalculateOutputSize(width, this.Width, this.StrideX, this.PaddingX);
        }

        /// <summary>
        /// Computes the output height.
        /// </summary>
        /// <param name="height">The input height.</param>
        /// <returns>
        /// The output height.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateOutputHeight(int height)
        {
            return Kernel.CalculateOutputSize(height, this.Height, this.StrideY, this.PaddingY);
        }

        /// <summary>
        /// Computes the output shape.
        /// </summary>
        /// <param name="shape">The input shape.</param>
        /// <returns>
        /// The output shape.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Shape CalculateOutputShape(Shape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            return new Shape(
                shape.Format,
                shape.GetAxis(Axis.B),
                this.CalculateOutputWidth(shape.GetAxis(Axis.X)),
                this.CalculateOutputHeight(shape.GetAxis(Axis.Y)),
                shape.GetAxis(Axis.C));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateOutputSize(int size, int kernelSize, int stride, int padding)
        {
            return size != -1 ? ((MinMax.Max(size - kernelSize + (2 * padding), 0) + stride - 1) / stride) + 1 : -1;
        }
    }
}
