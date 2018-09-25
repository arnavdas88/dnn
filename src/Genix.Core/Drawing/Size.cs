// -----------------------------------------------------------------------
// <copyright file="Size.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a dimension in 2D coordinate space.
    /// </summary>
    [TypeConverter(typeof(SizeConverter))]
    [JsonConverter(typeof(SizeJsonConverter))]
    public struct Size
        : IEquatable<Size>
    {
        /// <summary>
        /// Represents a <see cref="Size"/> that has <see cref="Width"/> and <see cref="Height"/> values set to zero.
        /// </summary>
        public static readonly Size Empty;

        /// <summary>
        /// The horizontal dimension of this <see cref="Size"/>.
        /// </summary>
        private int width;

        /// <summary>
        /// The vertical dimension of this <see cref="Size"/>.
        /// </summary>
        private int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="Size"/> struct with the specified dimensions.
        /// </summary>
        /// <param name="width">The horizontal dimension.</param>
        /// <param name="height">The vertical dimension.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Size(int width, int height)
        {
            this.width = width >= 0 ? width : throw new ArgumentOutOfRangeException(nameof(width), Core.Properties.Resources.E_InvalidSizeWidth);
            this.height = height >= 0 ? height : throw new ArgumentOutOfRangeException(nameof(height), Core.Properties.Resources.E_InvalidSizeHeight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Size"/> struct from the <see cref="System.Drawing.Size"/>.
        /// </summary>
        /// <param name="size">The <see cref="System.Drawing.Size"/> that contains the dimensions.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><see cref="System.Drawing.Size.Width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><see cref="System.Drawing.Size.Height"/> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Size(System.Drawing.Size size)
        {
            this.width = size.Width >= 0 ? size.Width : throw new ArgumentOutOfRangeException(nameof(size), Core.Properties.Resources.E_InvalidSizeWidth);
            this.height = size.Height >= 0 ? size.Height : throw new ArgumentOutOfRangeException(nameof(size), Core.Properties.Resources.E_InvalidSizeHeight);
        }

        /// <summary>
        /// Gets or sets the horizontal dimension of this <see cref="Size"/>.
        /// </summary>
        /// <value>
        /// The horizontal dimension of this <see cref="Size"/>.
        /// </value>
        public int Width
        {
            get => this.width;
            set => this.width = value;
        }

        /// <summary>
        /// Gets or sets the vertical dimension of this <see cref="Size"/>.
        /// </summary>
        /// <value>
        /// The vertical dimension of this <see cref="Size"/>.
        /// </value>
        public int Height
        {
            get => this.height;
            set => this.height = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Size"/> is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if both <see cref="Width"/> and <see cref="Height"/> are 0; otherwise, <b>false</b>.
        /// </value>
        public bool IsEmpty => this.width == 0 && this.height == 0;

        /// <summary>
        /// Compares two <see cref="Size"/> objects.
        /// The result specifies whether the values of the <see cref="Width"/> and <see cref="Height"/> properties of the two <see cref="Size"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="Size"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Size"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="Width"/> and <see cref="Height"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Size left, Size right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Size"/> objects.
        /// The result specifies whether the values of the <see cref="Width"/> and <see cref="Height"/> properties of the two <see cref="Size"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="Size"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Size"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="Width"/> and <see cref="Height"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Size left, Size right) => !left.Equals(right);

        /// <summary>
        /// Initializes a new instance of the <see cref="Size"/> structure using the value represented by the specified string.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains a <see cref="Size"/> in the following format:Width Height.</param>
        /// <returns>The <see cref="Size"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> does not consist of two values represented by a sequence of digits (0 through 9).
        /// </exception>
        public static Size Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] split = value.Split(' ');
            if (split?.Length == 2 &&
                int.TryParse(split[0], out int width) &&
                int.TryParse(split[1], out int height))
            {
                return new Size(width, height);
            }
            else
            {
                throw new ArgumentException(Core.Properties.Resources.E_InvalidSizeFormat, nameof(value));
            }
        }

        /// <summary>
        /// Performs vector addition of two <see cref="Size"/> structs.
        /// </summary>
        /// <param name="size1">The first <see cref="Size"/> to add.</param>
        /// <param name="size2">The second <see cref="Size"/> to add.</param>
        /// <returns>The <see cref="Size"/> that contains the result of addition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size Add(Size size1, Size size2) => new Size(size1.width + size2.width, size1.height + size2.height);

        /// <summary>
        /// Performs vector addition of two <see cref="Size"/> structs.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to expand.</param>
        /// <param name="dx">The horizontal dimension.</param>
        /// <param name="dy">The vertical dimension.</param>
        /// <returns>The expanded <see cref="Size"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The result has a negative width.</para>
        /// <para>-or-</para>
        /// <para>The result has a negative height.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size Add(Size size, int dx, int dy) => new Size(size.width + dx, size.height + dy);

        /// <summary>
        /// Contracts a <see cref="Size"/> by another <see cref="Size"/>.
        /// </summary>
        /// <param name="size1">The <see cref="Size"/> to subtract from.</param>
        /// <param name="size2">The <see cref="Size"/> to subtract.</param>
        /// <returns>The contracted <see cref="Size"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The result has a negative width.</para>
        /// <para>-or-</para>
        /// <para>The result has a negative height.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size Subtract(Size size1, Size size2) => new Size(size1.width - size2.width, size1.height - size2.height);

        /// <summary>
        /// Scales the specified <see cref="Size"/> location.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to scale.</param>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        /// <returns>The scaled <see cref="Size"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size Scale(Size size, int dx, int dy) => new Size(size.width * dx, size.height * dy);

        /// <summary>
        /// Scales the specified <see cref="Size"/> location.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to scale.</param>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        /// <returns>The scaled <see cref="Size"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size Scale(Size size, float dx, float dy)
        {
            return new Size(
                (int)Math.Round(dx * size.width, MidpointRounding.AwayFromZero),
                (int)Math.Round(dy * size.height, MidpointRounding.AwayFromZero));
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Size other) => other.width == this.width && other.height == this.height;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Size))
            {
                return false;
            }

            return this.Equals((Size)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => unchecked(this.width ^ this.height);

        /// <inheritdoc />
        public override string ToString() =>
            this.width.ToString(CultureInfo.CurrentCulture) + " " + this.height.ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Sets <see cref="Width"/> and <see cref="Height"/> values set to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.width = this.height = 0;

        /// <summary>
        /// Sets this <see cref="Size"/> position.
        /// </summary>
        /// <param name="width">The horizontal dimension.</param>
        /// <param name="height">The vertical dimension.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="width"/> is a negative value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="height"/> is a negative value.</para>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int width, int height)
        {
            this.width = width >= 0 ? width : throw new ArgumentOutOfRangeException(nameof(width), Core.Properties.Resources.E_InvalidSizeWidth);
            this.height = height >= 0 ? height : throw new ArgumentOutOfRangeException(nameof(height), Core.Properties.Resources.E_InvalidSizeHeight);
        }

        /// <summary>
        /// Scales this <see cref="Size"/> location.
        /// </summary>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(int dx, int dy)
        {
            this.width *= dx;
            this.height *= dy;
        }

        /// <summary>
        /// Scales this <see cref="Size"/> location.
        /// </summary>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(float dx, float dy)
        {
            this.width = (int)Math.Round(dx * this.width, MidpointRounding.AwayFromZero);
            this.height = (int)Math.Round(dy * this.height, MidpointRounding.AwayFromZero);
        }
    }
}
