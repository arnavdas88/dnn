// -----------------------------------------------------------------------
// <copyright file="SizeConverter.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>
    /// Provides a unified way of converting <see cref="Size"/> to <see cref="string"/>.
    /// </summary>
    internal class SizeConverter : TypeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeConverter"/> class.
        /// </summary>
        public SizeConverter()
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
                    string.Format(CultureInfo.InvariantCulture, Core.Properties.Resources.E_TypeConversionNotSupported, "SizeConverter", destType.ToString()));
            }

            return ((Size)value).ToString();
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) => srcType == typeof(string);

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return Size.Empty;
            }

            if (value.GetType() != typeof(string))
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, Core.Properties.Resources.E_TypeConversionNotSupported, "SizeConverter", value.GetType().ToString()));
            }

            return Size.Parse((string)value);
        }
    }
}
