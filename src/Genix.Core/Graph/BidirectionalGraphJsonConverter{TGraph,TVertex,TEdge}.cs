// -----------------------------------------------------------------------
// <copyright file="BidirectionalGraphJsonConverter{TGraph,TVertex,TEdge}.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a Json.NET converter for <see cref="BidirectionalGraphJsonConverter{TGraph, TVertex, TEdge}"/> class.
    /// </summary>
    /// <typeparam name="TGraph">The type of the graph.</typeparam>
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    /// <typeparam name="TEdge">The type of the edges.</typeparam>
    public class BidirectionalGraphJsonConverter<TGraph, TVertex, TEdge> : JsonConverter
        where TGraph : BidirectionalGraph<TVertex, TEdge>
        where TVertex : ICloneable
        where TEdge : Edge<TVertex>
    {
        private const string EdgeSeparator = "->";

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            BidirectionalGraph<TVertex, TEdge> graph = (BidirectionalGraph<TVertex, TEdge>)value;

            int count = 0;
            Dictionary<TVertex, int> vertices = graph.Vertices.ToDictionary(v => v, v => count++);

            writer.WriteStartObject();

            // write edges
            writer.WritePropertyName("Edges");
            writer.WriteStartArray();
            foreach (TEdge edge in graph.Edges)
            {
                JObject jo = JObject.FromObject(edge, serializer);
                jo.AddFirst(new JProperty(
                    "$type",
                    BidirectionalGraphJsonConverter<TGraph, TVertex, TEdge>.RemoveAssemblyDetails(edge.GetType().AssemblyQualifiedName)));
                jo.AddFirst(new JProperty(
                    "$path",
                    string.Join(BidirectionalGraphJsonConverter<TGraph, TVertex, TEdge>.EdgeSeparator, vertices[edge.Source], vertices[edge.Target])));
                jo.WriteTo(writer);
            }

            writer.WriteEnd();

            // write vertices
            writer.WritePropertyName("Vertices");
            writer.WriteStartArray();
            foreach (TVertex vertex in vertices.Keys)
            {
                JObject jo = JObject.FromObject(vertex, serializer);
                jo.AddFirst(new JProperty(
                    "$type",
                    BidirectionalGraphJsonConverter<TGraph, TVertex, TEdge>.RemoveAssemblyDetails(vertex.GetType().AssemblyQualifiedName)));
                jo.WriteTo(writer);
            }

            writer.WriteEnd();

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject o = JObject.Load(reader);

            TypeNameHandling typeNameHandling = serializer.TypeNameHandling;
            try
            {
                serializer.TypeNameHandling = TypeNameHandling.Auto;

                // read vertices
                TVertex[] vertices = o["Vertices"].ToObject<TVertex[]>(serializer);

                // create graph
                TGraph graph = (TGraph)Activator.CreateInstance(typeof(TGraph), true);

                // read and create edges
                foreach (JToken edgeToken in o["Edges"])
                {
                    string path = edgeToken["$path"].ToString();
                    string[] parts = path.Split(new string[] { BidirectionalGraphJsonConverter<TGraph, TVertex, TEdge>.EdgeSeparator }, StringSplitOptions.None);
                    int sourceIndex = int.Parse(parts[0], CultureInfo.InvariantCulture);
                    int targetIndex = int.Parse(parts[1], CultureInfo.InvariantCulture);

                    TEdge edge = edgeToken.ToObject<TEdge>(serializer);
                    edge.Source = vertices[sourceIndex];
                    edge.Target = vertices[targetIndex];
                    graph.AddEdge(edge);
                }

                return graph;
            }
            finally
            {
                serializer.TypeNameHandling = typeNameHandling;
            }
        }

        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder builder = new StringBuilder();

            // loop through the type name and filter out qualified assembly details from nested type names
            bool writingAssemblyName = false;
            bool skippingAssemblyDetails = false;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;

                    case ']':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;

                    case ',':
                        if (!writingAssemblyName)
                        {
                            writingAssemblyName = true;
                            builder.Append(current);
                        }
                        else
                        {
                            skippingAssemblyDetails = true;
                        }

                        break;

                    default:
                        if (!skippingAssemblyDetails)
                        {
                            builder.Append(current);
                        }

                        break;
                }
            }

            return builder.ToString();
        }
    }
}
