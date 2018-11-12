// -----------------------------------------------------------------------
// <copyright file="PointPolar.cs" company="Noname, Inc.">
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
    /// Represents a point defines by polar coordinates.
    /// </summary>
    [TypeConverter(typeof(PointPolarConverter))]
    [JsonConverter(typeof(PointPolarJsonConverter))]
    public struct PointPolar
        : IEquatable<PointPolar>
    {
        /// <summary>
        /// Represents a <see cref="PointPolar"/> that has <see cref="Rho"/> and <see cref="Theta"/> values set to zero.
        /// </summary>
        public static readonly PointPolar Empty;

        /// <summary>
        /// The radial coordinate of this <see cref="PointPolar"/>.
        /// </summary>
        private float rho;

        /// <summary>
        /// The angular coordinate of this <see cref="PointPolar"/>.
        /// </summary>
        private float theta;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolar"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="rho">The radial coordinate of the point.</param>
        /// <param name="theta">The angular coordinate of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointPolar(float rho, float theta)
        {
            this.rho = rho;
            this.theta = theta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolar"/> struct from the <see cref="PointPolar"/>.
        /// </summary>
        /// <param name="point">The <see cref="PointPolar"/> that contains the position of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointPolar(PointPolar point)
        {
            this.rho = point.rho;
            this.theta = point.theta;
        }

        /// <summary>
        /// Gets or sets the radial coordinate of this <see cref="PointPolar"/>.
        /// </summary>
        /// <value>
        /// The radial coordinate of this <see cref="PointPolar"/>.
        /// </value>
        public float Rho
        {
            get => this.rho;
            set => this.rho = value;
        }

        /// <summary>
        /// Gets or sets the angular coordinate of this <see cref="PointPolar"/>.
        /// </summary>
        /// <value>
        /// The angular coordinate of this <see cref="PointPolar"/>.
        /// </value>
        public float Theta
        {
            get => this.theta;
            set => this.theta = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PointPolar"/> is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if both <see cref="Rho"/> and <see cref="Theta"/> are 0; otherwise, <b>false</b>.
        /// </value>
        public bool IsEmpty => this.rho == 0 && this.theta == 0;

        /// <summary>
        /// Compares two <see cref="PointPolar"/> objects.
        /// The result specifies whether the values of the <see cref="Rho"/> and <see cref="Theta"/> properties of the two <see cref="PointPolar"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="PointPolar"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PointPolar"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="Rho"/> and <see cref="Theta"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PointPolar left, PointPolar right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="PointPolar"/> objects.
        /// The result specifies whether the values of the <see cref="Rho"/> and <see cref="Theta"/> properties of the two <see cref="PointPolar"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="PointPolar"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PointPolar"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="Rho"/> and <see cref="Theta"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PointPolar left, PointPolar right) => !left.Equals(right);

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolar"/> structure using the value represented by the specified string.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains a <see cref="PointPolar"/> in the following format:Rho Theta.</param>
        /// <returns>The <see cref="PointPolar"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> does not consist of two values represented by an optional sign followed by a sequence of digits (0 through 9).
        /// </exception>
        public static PointPolar Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] split = value.Split(' ');
            if (split?.Length == 2 &&
                float.TryParse(split[0], out float x) &&
                float.TryParse(split[1], out float y))
            {
                return new PointPolar(x, y);
            }
            else
            {
                throw new ArgumentException(Genix.Core.Properties.Resources.E_InvalidPointFormat, nameof(value));
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PointPolar other) => other.rho == this.rho && other.theta == this.theta;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            return this.Equals((PointPolar)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => unchecked((int)this.rho ^ (int)this.theta);

        /// <inheritdoc />
        public override string ToString() =>
            this.rho.ToString(CultureInfo.CurrentCulture) + " " + this.theta.ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Sets <see cref="Rho"/> and <see cref="Theta"/> values set to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => this.rho = this.theta = 0;

        /// <summary>
        /// Sets this <see cref="PointPolar"/> position.
        /// </summary>
        /// <param name="rho">The radial coordinate of the point.</param>
        /// <param name="theta">The angular coordinate of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float rho, float theta)
        {
            this.rho = rho;
            this.theta = theta;
        }
    }

    /// <summary>
    /// Represents a Json.NET converter for <see cref="PointPolar"/> struct.
    /// </summary>
    public class PointPolarJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is PointPolar point)
            {
                writer.WriteValue(point.ToString());
            }
            else
            {
                throw new JsonSerializationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected value when converting point. Expected PointPolar, got {0}.",
                    value?.GetType()));
            }
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                throw new JsonSerializationException("Cannot convert null value to PointPolar.");
            }

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected token parsing point. Expected String, got {0}.",
                    reader.TokenType));
            }

            return PointPolar.Parse(reader.Value.ToString());
        }
    }

    /// <summary>
    /// Provides a unified way of converting <see cref="PointPolar"/> to <see cref="string"/>.
    /// </summary>
    internal class PointPolarConverter : TypeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarConverter"/> class.
        /// </summary>
        public PointPolarConverter()
        {
        }

        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType) => destType == typeof(string);

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == null)
            {
                throw new ArgumentNullException(nameof(destType));
            }

            if (destType != typeof(string))
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointPolarConverter", destType.ToString()));
            }

            return ((PointPolar)value).ToString();
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) => srcType == typeof(string);

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return PointPolar.Empty;
            }

            if (value.GetType() != typeof(string))
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointPolarConverter", value.GetType().ToString()));
            }

            return PointPolar.Parse((string)value);
        }
    }
}