﻿// -----------------------------------------------------------------------
// <copyright file="PointPolar.tt" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//     Generated on: 11/27/2018 8:23:11 AM
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated. Re-run the T4 template to update this file.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Genix.Geometry
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;


        /// <summary>
    /// Represents a point defines by polar coordinates.
    /// </summary>
    [TypeConverter(typeof(PointPolarFConverter))]
    [JsonConverter(typeof(PointPolarFJsonConverter))]
    public struct PointPolarF
        : IEquatable<PointPolarF>
    {
        /// <summary>
        /// Represents a <see cref="PointPolarF"/> that has <see cref="Rho"/> and <see cref="Theta"/> values set to zero.
        /// </summary>
        public static readonly PointPolarF Empty;

        /// <summary>
        /// The radial coordinate of this <see cref="PointPolarF"/>.
        /// </summary>
        private float rho;

        /// <summary>
        /// The angular coordinate of this <see cref="PointPolarF"/>.
        /// </summary>
        private float theta;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarF"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="rho">The radial coordinate of the point.</param>
        /// <param name="theta">The angular coordinate of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointPolarF(float rho, float theta)
        {
            this.rho = rho;
            this.theta = theta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarF"/> struct from the <see cref="PointPolarF"/>.
        /// </summary>
        /// <param name="point">The <see cref="PointPolarF"/> that contains the position of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointPolarF(PointPolarF point)
        {
            this.rho = point.rho;
            this.theta = point.theta;
        }

        /// <summary>
        /// Gets or sets the radial coordinate of this <see cref="PointPolarF"/>.
        /// </summary>
        /// <value>
        /// The radial coordinate of this <see cref="PointPolarF"/>.
        /// </value>
        public float Rho
        {
            get => this.rho;
            set => this.rho = value;
        }

        /// <summary>
        /// Gets or sets the angular coordinate of this <see cref="PointPolarF"/>.
        /// </summary>
        /// <value>
        /// The angular coordinate of this <see cref="PointPolarF"/>.
        /// </value>
        public float Theta
        {
            get => this.theta;
            set => this.theta = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PointPolarF"/> is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if both <see cref="Rho"/> and <see cref="Theta"/> are 0; otherwise, <b>false</b>.
        /// </value>
        public bool IsEmpty => this.rho == 0 && this.theta == 0;

        /// <summary>
        /// Compares two <see cref="PointPolarF"/> objects.
        /// The result specifies whether the values of the <see cref="Rho"/> and <see cref="Theta"/> properties of the two <see cref="PointPolarF"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="PointPolarF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PointPolarF"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="Rho"/> and <see cref="Theta"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PointPolarF left, PointPolarF right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="PointPolarF"/> objects.
        /// The result specifies whether the values of the <see cref="Rho"/> and <see cref="Theta"/> properties of the two <see cref="PointPolarF"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="PointPolarF"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PointPolarF"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="Rho"/> and <see cref="Theta"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PointPolarF left, PointPolarF right) => !left.Equals(right);

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarF"/> structure using the value represented by the specified string.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains a <see cref="PointPolarF"/> in the following format:Rho Theta.</param>
        /// <returns>The <see cref="PointPolarF"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> does not consist of two values represented by an optional sign followed by a sequence of digits (0 through 9).
        /// </exception>
        public static PointPolarF Parse(string value)
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
                return new PointPolarF(x, y);
            }
            else
            {
                throw new ArgumentException(Genix.Core.Properties.Resources.E_InvalidPointFormat, nameof(value));
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PointPolarF other) => other.rho == this.rho && other.theta == this.theta;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
            {
                return false;
            }

            return this.Equals((PointPolarF)obj);
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
        /// Sets this <see cref="PointPolarF"/> position.
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
    /// Represents a Json.NET converter for <see cref="PointPolarF"/> struct.
    /// </summary>
    public class PointPolarFJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is PointPolarF point)
            {
                writer.WriteValue(point.ToString());
            }
            else
            {
                throw new JsonSerializationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected value when converting point. Expected PointPolarF, got {0}.",
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

            return PointPolarF.Parse(reader.Value.ToString());
        }
    }

    /// <summary>
    /// Provides a unified way of converting <see cref="PointPolarF"/> to <see cref="string"/>.
    /// </summary>
    internal class PointPolarFConverter : TypeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarFConverter"/> class.
        /// </summary>
        public PointPolarFConverter()
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
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointPolarFConverter", destType.ToString()));
            }

            return ((PointPolarF)value).ToString();
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) => srcType == typeof(string);

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return PointPolarF.Empty;
            }

            if (value.GetType() != typeof(string))
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointPolarFConverter", value.GetType().ToString()));
            }

            return PointPolarF.Parse((string)value);
        }
    }

        /// <summary>
    /// Represents a point defines by polar coordinates.
    /// </summary>
    [TypeConverter(typeof(PointPolarDConverter))]
    [JsonConverter(typeof(PointPolarDJsonConverter))]
    public struct PointPolarD
        : IEquatable<PointPolarD>
    {
        /// <summary>
        /// Represents a <see cref="PointPolarD"/> that has <see cref="Rho"/> and <see cref="Theta"/> values set to zero.
        /// </summary>
        public static readonly PointPolarD Empty;

        /// <summary>
        /// The radial coordinate of this <see cref="PointPolarD"/>.
        /// </summary>
        private double rho;

        /// <summary>
        /// The angular coordinate of this <see cref="PointPolarD"/>.
        /// </summary>
        private double theta;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarD"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="rho">The radial coordinate of the point.</param>
        /// <param name="theta">The angular coordinate of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointPolarD(double rho, double theta)
        {
            this.rho = rho;
            this.theta = theta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarD"/> struct from the <see cref="PointPolarD"/>.
        /// </summary>
        /// <param name="point">The <see cref="PointPolarD"/> that contains the position of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointPolarD(PointPolarD point)
        {
            this.rho = point.rho;
            this.theta = point.theta;
        }

        /// <summary>
        /// Gets or sets the radial coordinate of this <see cref="PointPolarD"/>.
        /// </summary>
        /// <value>
        /// The radial coordinate of this <see cref="PointPolarD"/>.
        /// </value>
        public double Rho
        {
            get => this.rho;
            set => this.rho = value;
        }

        /// <summary>
        /// Gets or sets the angular coordinate of this <see cref="PointPolarD"/>.
        /// </summary>
        /// <value>
        /// The angular coordinate of this <see cref="PointPolarD"/>.
        /// </value>
        public double Theta
        {
            get => this.theta;
            set => this.theta = value;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PointPolarD"/> is empty.
        /// </summary>
        /// <value>
        /// <b>true</b> if both <see cref="Rho"/> and <see cref="Theta"/> are 0; otherwise, <b>false</b>.
        /// </value>
        public bool IsEmpty => this.rho == 0 && this.theta == 0;

        /// <summary>
        /// Compares two <see cref="PointPolarD"/> objects.
        /// The result specifies whether the values of the <see cref="Rho"/> and <see cref="Theta"/> properties of the two <see cref="PointPolarD"/> objects are equal.
        /// </summary>
        /// <param name="left">The <see cref="PointPolarD"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PointPolarD"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the <see cref="Rho"/> and <see cref="Theta"/> values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PointPolarD left, PointPolarD right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="PointPolarD"/> objects.
        /// The result specifies whether the values of the <see cref="Rho"/> and <see cref="Theta"/> properties of the two <see cref="PointPolarD"/> objects are unequal.
        /// </summary>
        /// <param name="left">The <see cref="PointPolarD"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PointPolarD"/> structure that is to the right of the equality operator.</param>
        /// <returns><b>true</b> if the values of either <see cref="Rho"/> and <see cref="Theta"/> properties of <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <b>false</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PointPolarD left, PointPolarD right) => !left.Equals(right);

        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarD"/> structure using the value represented by the specified string.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that contains a <see cref="PointPolarD"/> in the following format:Rho Theta.</param>
        /// <returns>The <see cref="PointPolarD"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> does not consist of two values represented by an optional sign followed by a sequence of digits (0 through 9).
        /// </exception>
        public static PointPolarD Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] split = value.Split(' ');
            if (split?.Length == 2 &&
                double.TryParse(split[0], out double x) &&
                double.TryParse(split[1], out double y))
            {
                return new PointPolarD(x, y);
            }
            else
            {
                throw new ArgumentException(Genix.Core.Properties.Resources.E_InvalidPointFormat, nameof(value));
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PointPolarD other) => other.rho == this.rho && other.theta == this.theta;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is PointD))
            {
                return false;
            }

            return this.Equals((PointPolarD)obj);
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
        /// Sets this <see cref="PointPolarD"/> position.
        /// </summary>
        /// <param name="rho">The radial coordinate of the point.</param>
        /// <param name="theta">The angular coordinate of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double rho, double theta)
        {
            this.rho = rho;
            this.theta = theta;
        }
    }

    /// <summary>
    /// Represents a Json.NET converter for <see cref="PointPolarD"/> struct.
    /// </summary>
    public class PointPolarDJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is PointPolarD point)
            {
                writer.WriteValue(point.ToString());
            }
            else
            {
                throw new JsonSerializationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected value when converting point. Expected PointPolarD, got {0}.",
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

            return PointPolarD.Parse(reader.Value.ToString());
        }
    }

    /// <summary>
    /// Provides a unified way of converting <see cref="PointPolarD"/> to <see cref="string"/>.
    /// </summary>
    internal class PointPolarDConverter : TypeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointPolarDConverter"/> class.
        /// </summary>
        public PointPolarDConverter()
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
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointPolarDConverter", destType.ToString()));
            }

            return ((PointPolarD)value).ToString();
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) => srcType == typeof(string);

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return PointPolarD.Empty;
            }

            if (value.GetType() != typeof(string))
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointPolarDConverter", value.GetType().ToString()));
            }

            return PointPolarD.Parse((string)value);
        }
    }
}