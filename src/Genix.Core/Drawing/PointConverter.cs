// -----------------------------------------------------------------------
// <copyright file="PointConverter.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>
    /// Provides a unified way of converting <see cref="Point"/> to <see cref="string"/>.
    /// </summary>
    internal class PointConverter : TypeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointConverter"/> class.
        /// </summary>
        public PointConverter()
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
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointConverter", destType.ToString()));
            }

            return ((Point)value).ToString();
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) => srcType == typeof(string);

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return Point.Empty;
            }

            if (value.GetType() != typeof(string))
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Genix.Core.Properties.Resources.E_TypeConversionNotSupported, "PointConverter", value.GetType().ToString()));
            }

            return Point.Parse((string)value);
        }
    }
}
