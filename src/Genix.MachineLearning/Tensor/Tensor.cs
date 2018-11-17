// -----------------------------------------------------------------------
// <copyright file="Tensor.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

////#define USE_ARRAYPOOL

namespace Genix.MachineLearning
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Genix.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a multi-dimensional tensor used to store weights, gradients, etc.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tensor : ICloneable
    {
#if USE_ARRAYPOOL
        private static ArrayPool<float> arrayPool = ArrayPool<float>.Create();
        private bool ownWeights = false;
        private bool ownGradient = false;
#endif

        // The back-propagation gradient associated with this tensor.
        private float[] gradient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, Shape shape)
        {
            this.Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            this.Name = name;

#if USE_ARRAYPOOL
            this.Weights = Tensor.arrayPool.Rent(this.Length);
            this.ownWeights = true;
#else
            this.Weights = new float[shape.Length];
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        /// <param name="weights">The values to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, Shape shape, float[] weights)
        {
            this.Name = name;
            this.Shape = shape ?? throw new ArgumentNullException(nameof(shape));

            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            if (weights.Length != shape.Length)
            {
                throw new ArgumentException("The length of the weights vector does not match the tensor length.", nameof(weights));
            }

            this.Weights = weights;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions and all the weights having the specified value.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, Shape shape, float value)
            : this(name, shape)
        {
            this.Set(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="axes">The tensor dimensions along its axes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, int[] axes)
        {
            this.Shape = new Shape(axes);
            this.Name = name;

#if USE_ARRAYPOOL
            this.Weights = Tensor.arrayPool.Rent(this.Length);
            this.ownWeights = true;
#else
            this.Weights = new float[this.Shape.Length];
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class.
        /// </summary>
        /// <param name="other">The <see cref="Tensor"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(Tensor other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Name = other.Name;
            this.Shape = other.Shape;
            this.Weights = other.Weights.ToArray();
            this.gradient = other.gradient?.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [JsonConstructor]
        protected Tensor()
        {
        }

#if USE_ARRAYPOOL
        /// <summary>
        /// Finalizes an instance of the <see cref="Tensor"/> class.
        /// </summary>
        ~Tensor()
        {
            if (this.Weights != null && this.ownWeights)
            {
                Tensor.arrayPool.Return(this.Weights);
                this.Weights = null;
            }

            if (this.gradient != null && this.ownGradient)
            {
                Tensor.arrayPool.Return(this.gradient);
                this.gradient = null;
            }
        }
#endif

        /// <summary>
        /// Gets the tensor name.
        /// </summary>
        /// <value>
        /// The <see cref="string"/> that contains tensor name.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the tensor shape.
        /// </summary>
        /// <value>
        /// The <see cref="MachineLearning.Shape"/> object.
        /// </value>
        [JsonProperty("shape")]
        public Shape Shape { get; private set; }

        /// <summary>
        /// Gets the total number of weights in all the dimensions of the <see cref="Tensor"/>.
        /// </summary>
        /// <value>
        /// The total number of weights in all the dimensions of the <see cref="Tensor"/>.
        /// </value>
        [JsonIgnore]
        public int Length => this.Shape.Length;

        /// <summary>
        /// Gets the rank (number of dimensions) of the <see cref="Tensor"/>.
        /// </summary>
        /// <value>
        /// The rank (number of dimensions) of the <see cref="Tensor"/>.
        /// </value>
        [JsonIgnore]
        public int Rank => this.Shape.Rank;

        /// <summary>
        /// Gets the axes dimensions.
        /// </summary>
        /// <value>
        /// The axes dimensions.
        /// </value>
        [JsonIgnore]
        public int[] Axes => this.Shape.Axes;

        /// <summary>
        /// Gets the axes strides.
        /// </summary>
        /// <value>
        /// The axes strides.
        /// </value>
        [JsonIgnore]
        public int[] Strides => this.Shape.Strides;

        /// <summary>
        /// Gets the weights stored in this <see cref="Tensor"/>.
        /// </summary>
        /// <value>
        /// The array of <see cref="float"/>.
        /// </value>
        [JsonProperty("weights")]
        public float[] Weights { get; private set; }

        /// <summary>
        /// Gets the back-propagation gradient associated with this <see cref="Tensor"/>.
        /// </summary>
        /// <value>
        /// The array of <see cref="float"/>.
        /// </value>
        [JsonIgnore]
        public float[] Gradient
        {
            get
            {
                if (this.gradient == null)
                {
                    lock (this)
                    {
                        if (this.gradient == null)
                        {
#if USE_ARRAYPOOL
                            this.gradient = Tensor.arrayPool.Rent(this.Length);
                            this.IsGradientInitialized = false;
                            this.ownGradient = true;
#else
                            this.gradient = new float[this.Length];
                            this.IsGradientInitialized = true;
#endif
                        }
                    }
                }

                return this.gradient;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the memory allocated for <see cref="Gradient"/> property has been initialized.
        /// </summary>
        /// <value>
        /// <b>true</b> if the memory was initialized; otherwise, <b>false</b>.
        /// </value>
        [JsonIgnore]
        public bool IsGradientInitialized { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether a gradient for this tensor should be calculated.
        /// </summary>
        /// <value>
        /// <b>true</b> to calculate gradient for this tensor; otherwise, <b>false</b>.
        /// </value>
        [JsonIgnore]
        public bool CalculateGradient { get; set; } = true;

        /// <summary>
        /// Gets or sets the element at the specified position.
        /// </summary>
        /// <param name="position">A weight position.</param>
        /// <returns>The number of elements in the specified axis.</returns>
        public float this[int position]
        {
            get => this.Weights[position];
            set => this.Weights[position] = value;
        }

        /// <summary>
        /// Gets or sets the tensor weight at the specified position.
        /// </summary>
        /// <param name="axes">The weight coordinates.</param>
        /// <returns>
        /// The weight at the specified position.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Pass array of indexes.")]
        public float this[params int[] axes]
        {
            get => this.Weights[this.Shape.Position(axes)];
            set => this.Weights[this.Shape.Position(axes)] = value;
        }

        /// <summary>
        /// Creates a one-hot tensor.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        /// <param name="on">The value that one-hot location takes.</param>
        /// <param name="off">The value that all other locations take.</param>
        /// <param name="position">The one-hot element coordinates in the tensor.</param>
        /// <returns>The tensor this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor OneHot(string name, Shape shape, float on, float off, params int[] position)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            Tensor tensor = new Tensor(name, shape);

            if (off != 0.0f)
            {
                tensor.Set(off);
            }

            int pos = tensor.Shape.Position(position);
            tensor.Weights[pos] = on;

            return tensor;
        }

        /// <summary>
        /// Creates a one-hot tensor.
        /// The one-hot location takes the value 1, while all other locations take the value 0.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        /// <param name="position">The one-hot element coordinates in the tensor.</param>
        /// <returns>The tensor this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor OneHot(string name, Shape shape, params int[] position) =>
            Tensor.OneHot(name, shape, 1.0f, 0.0f, position);

        /// <summary>
        /// Creates a tensor with all elements set to 1.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor shape.</param>
        /// <returns>The tensor this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Ones(string name, Shape shape) => new Tensor(name, shape, 1.0f);

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file from which to create the <see cref="Tensor"/>.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromFile(string fileName) => Tensor.FromString(File.ReadAllText(fileName, Encoding.UTF8));

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read the <see cref="Tensor"/> from.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromMemory(byte[] buffer) => Tensor.FromString(UTF8Encoding.UTF8.GetString(buffer));

        /// <summary>
        /// Creates a <see cref="Tensor"/> from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to read the <see cref="Tensor"/> from.</param>
        /// <returns>The <see cref="Tensor"/> this method creates.</returns>
        public static Tensor FromString(string value) => JsonConvert.DeserializeObject<Tensor>(value);

        /// <inheritdoc />
        public override string ToString() => ////string.Join("f, ", this.Weights.Take(24));
            string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1}: {2}",
                    this.Name ?? "noname",
                    string.Join("x", this.Shape.Axes),
                    this.Weights != null ? string.Join(" ", this.Weights.Take(24)) : null);

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone() => new Tensor(this);

        /// <summary>
        /// Saves the current <see cref="Tensor"/> into the specified file.
        /// </summary>
        /// <param name="fileName">A string that contains the name of the file to which to save this <see cref="Tensor"/>.</param>
        public void SaveToFile(string fileName) => File.WriteAllText(fileName, this.SaveToString(), Encoding.UTF8);

        /// <summary>
        /// Saves the current <see cref="Tensor"/> to the memory buffer.
        /// </summary>
        /// <returns>The buffer that contains saved <see cref="Tensor"/>.</returns>
        public byte[] SaveToMemory() => UTF8Encoding.UTF8.GetBytes(this.SaveToString());

        /// <summary>
        /// Saves the current <see cref="Tensor"/> to the text string.
        /// </summary>
        /// <returns>The string that contains saved <see cref="Tensor"/>.</returns>
        public string SaveToString() => JsonConvert.SerializeObject(this);

        /// <summary>
        /// Copies values from this tensor to another tensor.
        /// </summary>
        /// <returns>
        /// The <see cref="Tensor"/> that contains copied data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor Copy()
        {
            Tensor y = new Tensor("copy", this.Shape);
            Vectors.Copy(this.Length, this.Weights, 0, y.Weights, 0);
            return y;
        }

        /// <summary>
        /// Changes the <see cref="Tensor"/> dimensions.
        /// </summary>
        /// <param name="shape">The new tensor shape.</param>
        /// <returns>
        /// <b>true</b> if the tensor was changed; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Reshape(Shape shape)
        {
            // validate new shape
            if (shape.Length != this.Length)
            {
                throw new ArgumentException("The size of new shape must be the same as tensor length.", nameof(shape));
            }

            if (this.Shape.Format == shape.Format && Shape.AreSame(this.Shape.Axes, shape.Axes))
            {
                return false;
            }

            this.Shape.Attach(shape);
            return true;
        }

        /// <summary>
        /// Sets all the weights in the tensor to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Vectors.Set(this.Length, 0.0f, this.Weights, 0);

        /// <summary>
        /// Sets all values in the tensor to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float value) => Vectors.Set(this.Length, value, this.Weights, 0);

        /// <summary>
        /// Sets all values in the tensor to the specified values.
        /// </summary>
        /// <param name="weights">The weights to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] weights)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            if (weights.Length < this.Length)
            {
                throw new ArgumentException("The number of weights is less than the tensor length.", nameof(weights));
            }

            Vectors.Copy(this.Length, weights, 0, this.Weights, 0);
        }

        /// <summary>
        /// Replaces all specified values in the tensor with another specified value.
        /// </summary>
        /// <param name="oldValue">The value to be replaced.</param>
        /// <param name="newValue">The value to replace all occurrences of <c>oldValue</c>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(float oldValue, float newValue)
        {
            Arrays.Replace(this.Length, this.Weights, 0, oldValue, newValue, this.Weights, 0);
        }

        /// <summary>
        /// Computes absolute value of elements of the tensor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abs() => Vectors.Abs(this.Length, this.Weights, 0);

        /// <summary>
        /// Adds a constant value to each element of a tensor.
        /// </summary>
        /// <param name="alpha">The constant value.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) + alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddC(float alpha) => Vectors.AddC(this.Length, alpha, this.Weights, 0);

        /// <summary>
        /// Adds all values from a tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) + x(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Tensor x) => Vectors.Add(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Subtracts a constant value from each element of a tensor.
        /// </summary>
        /// <param name="alpha">The constant value.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) - alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubC(float alpha) => Vectors.SubC(this.Length, alpha, this.Weights, 0);

        /// <summary>
        /// Subtracts each element of a tensor from a constant value.
        /// </summary>
        /// <param name="alpha">The constant value.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := alpha - this(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubCRev(float alpha) => Vectors.SubCRev(this.Length, alpha, this.Weights, 0);

        /// <summary>
        /// Subtracts all values of a tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to subtract.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) - x(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(Tensor x) => Vectors.Sub(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Multiplies elements of this tensor by a scalar.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) * alpha</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MulC(float alpha) => Vectors.MulC(this.Length, alpha, this.Weights, 0);

        /// <summary>
        /// Multiplies elements of this tensor to elements of another tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to multiply.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) * x(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Mul(Tensor x) => Vectors.Mul(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Divides elements of this tensor to elements of another tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to divide.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) / x(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Div(Tensor x) => Vectors.Div(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Adds all values multiplied by a specified factor from a tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this := alpha * x + this</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProductC(Tensor x, float alpha) => Vectors.AddProductC(this.Length, x.Weights, 0, alpha, this.Weights, 0);

        /// <summary>
        /// Adds all values multiplied by a specified factor from a tensor.
        /// </summary>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this := alpha * x + beta * this</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MultiplyAndAdd(float alpha, Tensor x, float beta) => Mathematics.MultiplyAndAdd(this.Length, alpha, x.Weights, 0, beta, this.Weights, 0);

        /// <summary>
        /// Performs element by element multiplication of two tensors and adds results of multiplication to this tensor.
        /// </summary>
        /// <param name="a">The input tensor <paramref name="a"/>.</param>
        /// <param name="b">The input tensor <paramref name="b"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) += a(i) * b(i)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProduct(Tensor a, Tensor b) => Vectors.AddProduct(this.Length, a.Weights, 0, b.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Randomizes all values in the tensor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Randomize()
        {
            // weight normalization is done to equalize the output
            // variance of every neuron, otherwise neurons with a lot
            // of incoming connections have outputs of larger variance
            double standardDeviation = Math.Sqrt(1.0 / this.Shape.Strides[0]);

            this.Randomize(new GaussianGenerator(0.0, standardDeviation));
        }

        /// <summary>
        /// Randomizes all values in the tensor.
        /// </summary>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Randomize(RandomNumberGenerator<float> random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            random.Generate(this.Length, this.Weights);
        }

        /// <summary>
        /// Randomizes all gradient values in the tensor.
        /// </summary>
        /// <param name="random">The random numbers generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RandomizeGradient(RandomNumberGenerator<float> random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            random.Generate(this.Length, this.Gradient);
        }

        /// <summary>
        /// Returns the position of minimum value in the tensor.
        /// </summary>
        /// <returns>
        /// The position of minimum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgMin() => Vectors.ArgMin(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the position of maximum value in the tensor.
        /// </summary>
        /// <returns>
        /// The position of maximum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgMax() => Vectors.ArgMax(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the position of minimum and maximum values in the tensor.
        /// </summary>
        /// <param name="min">The position of minimum value in the array.</param>
        /// <param name="max">The position of maximum value in the array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ArgMinMax(out int min, out int max) => Vectors.ArgMinMax(this.Length, this.Weights, 0, out min, out max);

        /// <summary>
        /// Returns the minimum value in the tensor.
        /// </summary>
        /// <returns>
        /// The minimum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min() => Vectors.Min(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the maximum value in the tensor.
        /// </summary>
        /// <returns>
        /// The maximum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max() => Vectors.Max(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the minimum and maximum values in the tensor.
        /// </summary>
        /// <param name="min">The minimum value in the array.</param>
        /// <param name="max">The maximum value in the array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MinMax(out float min, out float max) => Vectors.MinMax(this.Length, this.Weights, 0, out min, out max);

        /// <summary>
        /// Computes the L1-Norm (sum of magnitudes) of the tensor elements.
        /// </summary>
        /// <returns>
        /// The L1-Norm of tensor elements in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float L1Norm() => Vectors.L1Norm(this.Length, this.Weights, 0);

        /// <summary>
        /// Computes the L2-Norm (Euclidian norm) of the tensor elements.
        /// </summary>
        /// <returns>
        /// The L2-Norm of tensor elements in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float L2Norm() => Vectors.L2Norm(this.Length, this.Weights, 0);

        /// <summary>
        /// Clips tensor values to a specified minimum and maximum values.
        /// </summary>
        /// <param name="minValue">The minimum value to clip by.</param>
        /// <param name="maxValue">The maximum value to clip by.</param>
        /// <remarks>
        /// The method performs operation defined as <c>tensor(i) := min(max(tensor(i), minValue), maxValue)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clip(float minValue, float maxValue) => Vectors.Clip(this.Length, minValue, maxValue, this.Weights, 0);

        /// <summary>
        /// Sets all the weights in the tensor's gradient to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearGradient()
        {
            if (this.gradient != null)
            {
                Vectors.Set(this.Length, 0, this.gradient, 0);
                this.IsGradientInitialized = true;
            }
        }

        /// <summary>
        /// Sets all values in the tensor gradient to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetGradient(float value)
        {
            Vectors.Set(this.Length, value, this.Gradient, 0);
            this.IsGradientInitialized = true;
        }

        /// <summary>
        /// Sets all values in the tensor gradient to the specified values.
        /// </summary>
        /// <param name="weights">The weights to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetGradient(float[] weights)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            if (weights.Length < this.Length)
            {
                throw new ArgumentException("The number of weights is less than the tensor length.", nameof(weights));
            }

            Vectors.Copy(this.Length, weights, 0, this.Gradient, 0);
            this.IsGradientInitialized = true;
        }

        /// <summary>
        /// Adds the specified values to the tensor's gradient.
        /// </summary>
        /// <param name="weights">The weights to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddGradient(float[] weights)
        {
            if (this.IsGradientInitialized)
            {
                Vectors.Add(this.Length, weights, 0, this.Gradient, 0);
            }
            else
            {
                Vectors.Copy(this.Length, weights, 0, this.Gradient, 0);
                this.IsGradientInitialized = true;
            }
        }

        /// <summary>
        /// Subtracts the specified values from the tensor's gradient.
        /// </summary>
        /// <param name="weights">The weights to subtract.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubGradient(float[] weights)
        {
            if (this.IsGradientInitialized)
            {
                Vectors.Sub(this.Length, weights, 0, this.Gradient, 0);
            }
            else
            {
                Vectors.Neg(this.Length, weights, 0, this.Gradient, 0);
                this.IsGradientInitialized = true;
            }
        }

        /// <summary>
        /// Clips tensor values to a specified minimum and maximum values.
        /// </summary>
        /// <param name="minValue">The minimum value to clip by.</param>
        /// <param name="maxValue">The maximum value to clip by.</param>
        /// <remarks>
        /// The method performs operation defined as <c>gradient(i) := min(max(gradient(i), minValue), maxValue)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClipGradient(float minValue, float maxValue)
        {
            if (this.IsGradientInitialized)
            {
                Vectors.Clip(this.Length, minValue, maxValue, this.Gradient, 0);
            }
            else
            {
                Vectors.Set(this.Length, minValue, this.Gradient, 0);
                this.IsGradientInitialized = true;
            }
        }

        /// <summary>
        /// Transposes a rank-2 tensor.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrices A, B, and C are row-major or column-major.</param>
        public void Transpose(MatrixLayout matrixLayout)
        {
            if (this.Shape.Rank != 2)
            {
                throw new InvalidOperationException("Only Rank-2 tensors can be transposed.");
            }

            int axis0 = this.Shape.Axes[0];
            int axis1 = this.Shape.Axes[1];

            int m = matrixLayout == MatrixLayout.RowMajor ? axis0 : axis1;
            int n = matrixLayout == MatrixLayout.RowMajor ? axis1 : axis0;

            Matrix.Transpose(matrixLayout, m, n, this.Weights, 0);

            this.Shape = new Shape(new[] { axis1, axis0 });
        }

        /// <summary>
        /// Validates the tensor.
        /// </summary>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Validate()
        {
            Debug.Assert(!this.Weights.Take(this.Length).Any(w => float.IsNaN(w) || float.IsInfinity(w)), "Tensor contains invalid weight.");

            if (this.gradient != null)
            {
                Debug.Assert(!this.gradient.Take(this.Length).Any(w => float.IsNaN(w) || float.IsInfinity(w)), "Tensor contains invalid gradient.");
            }
        }

        /// <summary>
        /// Detaches the weights from this <see cref="Tensor"/>.
        /// </summary>
        /// <returns>
        /// The weights array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] DetachWeights()
        {
            float[] w = this.Weights;
            this.Weights = null;
            return w;
        }

        /// <summary>
        /// Attaches the gradient weights to this <see cref="Tensor"/>.
        /// </summary>
        /// <param name="gradient">The gradient to attach.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachGradient(float[] gradient)
        {
            this.gradient = gradient;
            this.IsGradientInitialized = false;
        }

        /// <summary>
        /// Detaches the gradient weights from this <see cref="Tensor"/>.
        /// </summary>
        /// <returns>
        /// The gradient weights array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] DetachGradient()
        {
            float[] dw = this.gradient;
            this.gradient = null;
            return dw;
        }
    }
}