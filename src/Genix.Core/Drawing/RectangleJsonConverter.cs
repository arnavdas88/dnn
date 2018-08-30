// -----------------------------------------------------------------------
// <copyright file="RectangleJsonConverter.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Drawing
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Json.NET converter for <see cref="Rectangle"/> struct.
    /// </summary>
    public class RectangleJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Rectangle point)
            {
                writer.WriteValue(point.ToString());
            }
            else
            {
                throw new JsonSerializationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected value when converting rectangle. Expected Rectangle, got {0}.",
                    value?.GetType()));
            }
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                throw new JsonSerializationException("Cannot convert null value to Rectangle.");
            }

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected token parsing rectangle. Expected String, got {0}.",
                    reader.TokenType));
            }

            return Rectangle.Parse(reader.Value.ToString());
        }
    }
}
