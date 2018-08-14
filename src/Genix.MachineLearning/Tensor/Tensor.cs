// -----------------------------------------------------------------------
// <copyright file="Tensor.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.MachineLearning
{
    using System;
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
    public class Tensor : Shape, ICloneable
    {
        // The back-propagation gradient associated with this tensor.
        private float[] gradient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, int[] shape)
            : base(shape)
        {
            this.Name = name;
            this.Weights = new float[this.Length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions and all the weights having the specified value.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, int[] shape, float value)
            : this(name, shape)
        {
            this.Set(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions and the weights having the specified values.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        /// <param name="values">The values to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, int[] shape, float[] values)
            : base(shape)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length != this.Length)
            {
                throw new ArgumentException("The number of weights does not match the tensor length.", nameof(values));
            }

            this.Name = name;
            this.Weights = values;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class
        /// with the specified dimensions and the weights and gradient having the specified values.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        /// <param name="values">The values to set.</param>
        /// <param name="gradient">The gradient values to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(string name, int[] shape, float[] values, float[] gradient)
            : this(name, shape, values)
        {
            if (gradient == null)
            {
                throw new ArgumentNullException(nameof(gradient));
            }

            if (gradient.Length != this.Length)
            {
                throw new ArgumentException("The number of weights in gradient does not match the tensor length.", nameof(gradient));
            }

            this.gradient = gradient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tensor"/> class.
        /// </summary>
        /// <param name="other">The <see cref="Tensor"/> to copy the data from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tensor(Tensor other)
            : base(other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Name = other.Name;
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

        /// <summary>
        /// Gets the tensor name.
        /// </summary>
        /// <value>
        /// The <see cref="string"/> that contains tensor name.
        /// </value>
        [JsonProperty("Name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the weights stored in this <see cref="Tensor"/>.
        /// </summary>
        /// <value>
        /// The array of <see cref="float"/>.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Provides direct access to weights to improve performance.")]
        [JsonProperty("Weights")]
        public float[] Weights { get; private set; }

        /// <summary>
        /// Gets the back-propagation gradient associated with this <see cref="Tensor"/>.
        /// </summary>
        /// <value>
        /// The array of <see cref="float"/>.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Provides direct access to weights to improve performance.")]
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
                            this.gradient = new float[this.Length];
                        }
                    }
                }

                return this.gradient;
            }
        }

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
            get => this.Weights[this.Position(axes)];
            set => this.Weights[this.Position(axes)] = value;
        }

        /// <summary>
        /// Creates a one-hot tensor.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        /// <param name="on">The value that one-hot location takes.</param>
        /// <param name="off">The value that all other locations take.</param>
        /// <param name="axes">The one-hot element coordinates in the tensor.</param>
        /// <returns>The tensor this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor OneHot(string name, int[] shape, float on, float off, params int[] axes)
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

            int position = tensor.Position(axes);
            tensor.Weights[position] = on;

            return tensor;
        }

        /// <summary>
        /// Creates a one-hot tensor.
        /// The one-hot location takes the value 1, while all other locations take the value 0.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        /// <param name="axes">The one-hot element coordinates in the tensor.</param>
        /// <returns>The tensor this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor OneHot(string name, int[] shape, params int[] axes) => Tensor.OneHot(name, shape, 1.0f, 0.0f, axes);

        /// <summary>
        /// Creates a tensor with all elements set to 1.
        /// </summary>
        /// <param name="name">The tensor name.</param>
        /// <param name="shape">The tensor dimensions.</param>
        /// <returns>The tensor this method creates.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor Ones(string name, params int[] shape) => new Tensor(name, shape, 1.0f);

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
                    string.Join("x", this.Axes),
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
            Tensor y = new Tensor("copy", this.Axes);
            Arrays.Copy(this.Length, this.Weights, 0, y.Weights, 0);
            return y;
        }

        /// <summary>
        /// Sets all the weights in the tensor to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Array32f.Set(this.Length, 0.0f, this.Weights, 0);

        /// <summary>
        /// Sets all the weights in the tensor's gradient to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearGradient()
        {
            if (this.gradient != null)
            {
                Array32f.Set(this.Length, 0, this.gradient, 0);
            }
        }

        /// <summary>
        /// Sets all values in the tensor to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float value) => Array32f.Set(this.Length, value, this.Weights, 0);

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

            if (weights.Length != this.Length)
            {
                throw new ArgumentException("The number of weights does not match the tensor length.", nameof(weights));
            }

            Arrays.Copy(this.Length, weights, 0, this.Weights, 0);
        }

        /// <summary>
        /// Sets all values in the tensor gradient to the specified value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetGradient(float value) => Array32f.Set(this.Length, value, this.Gradient, 0);

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

            if (weights.Length != this.Length)
            {
                throw new ArgumentException("The number of weights does not match the tensor length.", nameof(weights));
            }

            Arrays.Copy(this.Length, weights, 0, this.Gradient, 0);
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
        public void Abs() => Math32f.Abs(this.Length, this.Weights, 0);

        /// <summary>
        /// Adds all values from a tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) + x(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Tensor x) => Math32f.Add(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Subtracts all values of a tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to subtract.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) - x(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sub(Tensor x) => Math32f.Sub(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Multiplies elements of this tensor by a scalar.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) * alpha</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(float alpha) => Math32f.MulC(this.Length, alpha, this.Weights, 0);

        /// <summary>
        /// Multiplies elements of this tensor to elements of another tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to multiply.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) * x(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(Tensor x) => Math32f.Mul(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Divides elements of this tensor to elements of another tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to divide.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this(i) := this(i) / x(i)</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Divide(Tensor x) => Math32f.Div(this.Length, x.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Adds all values multiplied by a specified factor from a tensor.
        /// </summary>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this := alpha * x + this</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProductC(Tensor x, float alpha) => Math32f.AddProductC(this.Length, x.Weights, 0, alpha, this.Weights, 0);

        /// <summary>
        /// Adds all values multiplied by a specified factor from a tensor.
        /// </summary>
        /// <param name="alpha">The scalar <paramref name="alpha"/>.</param>
        /// <param name="x">The tensor that contains the data to add.</param>
        /// <param name="beta">The scalar <c>beta</c>.</param>
        /// <remarks>
        /// The method performs operation defined as <c>this := alpha * x + beta * this</c>.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Do not validate parameters to improve performance.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddProduct(Tensor a, Tensor b) => Math32f.AddProduct(this.Length, a.Weights, 0, b.Weights, 0, this.Weights, 0);

        /// <summary>
        /// Randomizes all values in the tensor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Randomize()
        {
            // weight normalization is done to equalize the output
            // variance of every neuron, otherwise neurons with a lot
            // of incoming connections have outputs of larger variance
            double standardDeviation = Math.Sqrt(1.0 / this.Strides[0]);

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
        public int ArgMin() => Maximum.ArgMin(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the position of maximum value in the tensor.
        /// </summary>
        /// <returns>
        /// The position of maximum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgMax() => Maximum.ArgMax(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the position of minimum and maximum values in the tensor.
        /// </summary>
        /// <param name="min">The position of minimum value in the array.</param>
        /// <param name="max">The position of maximum value in the array.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Need to return two parameters.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ArgMinMax(out int min, out int max) => Maximum.ArgMinMax(this.Length, this.Weights, 0, out min, out max);

        /// <summary>
        /// Returns the minimum value in the tensor.
        /// </summary>
        /// <returns>
        /// The minimum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min() => Maximum.Min(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the maximum value in the tensor.
        /// </summary>
        /// <returns>
        /// The maximum value in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max() => Maximum.Max(this.Length, this.Weights, 0);

        /// <summary>
        /// Returns the minimum and maximum values in the tensor.
        /// </summary>
        /// <param name="min">The minimum value in the array.</param>
        /// <param name="max">The maximum value in the array.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Need to return two parameters.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MinMax(out float min, out float max) => Maximum.MinMax(this.Length, this.Weights, 0, out min, out max);

        /// <summary>
        /// Computes the L1-Norm (sum of magnitudes) of the tensor elements.
        /// </summary>
        /// <returns>
        /// The L1-Norm of tensor elements in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float L1Norm() => Math32f.L1Norm(this.Length, this.Weights, 0);

        /// <summary>
        /// Computes the L2-Norm (Euclidian norm) of the tensor elements.
        /// </summary>
        /// <returns>
        /// The L2-Norm of tensor elements in the tensor.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float L2Norm() => Math32f.L2Norm(this.Length, this.Weights, 0);

        /// <summary>
        /// Clips tensor values to a specified minimum and maximum values.
        /// </summary>
        /// <param name="minValue">The minimum value to clip by.</param>
        /// <param name="maxValue">The maximum value to clip by.</param>
        /// <remarks>
        /// The method performs operation defined as <c>tensor(i) := min(max(tensor(i), minValue), maxValue)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clip(float minValue, float maxValue)
        {
            Arrays.Clip(this.Length, minValue, maxValue, this.Weights, 0);
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
            Arrays.Clip(this.Length, minValue, maxValue, this.Gradient, 0);
        }

        /// <summary>
        /// Transposes a rank-2 tensor.
        /// </summary>
        /// <param name="matrixLayout">Specifies whether the matrices A, B, and C are row-major or column-major.</param>
        public void Transpose(MatrixLayout matrixLayout)
        {
            if (this.Rank != 2)
            {
                throw new InvalidOperationException("Only Rank-2 tensors can be transposed.");
            }

            int axis0 = this.Axes[0];
            int axis1 = this.Axes[1];

            int m = matrixLayout == MatrixLayout.RowMajor ? axis0 : axis1;
            int n = matrixLayout == MatrixLayout.RowMajor ? axis1 : axis0;

            Matrix.Transpose(matrixLayout, m, n, this.Weights, 0);

            this.InitializeShape(new[] { axis1, axis0 });
        }

        /// <summary>
        /// Validates the tensor.
        /// </summary>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Validate()
        {
            Debug.Assert(!this.Weights.Any(w => float.IsNaN(w) || float.IsInfinity(w)), "Tensor contains invalid weight.");

            if (this.gradient != null)
            {
                Debug.Assert(!this.gradient.Any(w => float.IsNaN(w) || float.IsInfinity(w)), "Tensor contains invalid gradient.");
            }
        }

        /// <summary>
        /// Changes the <see cref="Tensor"/> dimensions.
        /// </summary>
        /// <param name="shape">The new <see cref="Tensor"/> dimensions.</param>
        /// <returns>
        /// <b>true</b> if the tensor was changed; otherwise, <b>false</b>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Reshape(params int[] shape)
        {
            // validate new shape
            if (Shape.ShapeLength(shape) != this.Length)
            {
                throw new ArgumentException("The size of new shape must be the same as tensor length.", nameof(shape));
            }

            if (Shape.AreSame(this.Axes, shape))
            {
                return false;
            }

            this.InitializeShape(shape);
            return true;
        }

        /// <summary>
        /// Detaches the weights from this <see cref="Tensor"/>.
        /// </summary>
        /// <returns>
        /// The weights array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal float[] DetachWeights()
        {
            float[] w = this.Weights;
            this.Weights = null;
            return w;
        }

        /// <summary>
        /// Detaches the gradient weights from this <see cref="Tensor"/>.
        /// </summary>
        /// <returns>
        /// The gradient weights array.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal float[] DetachGradient()
        {
            float[] dw = this.gradient;
            this.gradient = null;
            return dw;
        }
    }
}