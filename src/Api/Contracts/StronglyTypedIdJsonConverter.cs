using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LessonFlow.Domain.Common.Interfaces;

namespace LessonFlow.Api.Contracts;

public class StronglyTypedIdJsonConverter<T> : JsonConverterFactory where T : IStronglyTypedId
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(StronglyTypedIdConverterInner),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [options],
            null)!;

        return converter;
    }

    private class StronglyTypedIdConverterInner(JsonSerializerOptions options) : JsonConverter<T>
    {
        private readonly JsonConverter<Guid> _valueConverter = (JsonConverter<Guid>)options.GetConverter(typeof(Guid));

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var id = (T)Activator.CreateInstance(
                typeToConvert,
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [Guid.Empty],
                null)!;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return id;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                reader.Read();
                var value = _valueConverter.Read(ref reader, typeof(Guid), options);

                id.GetType().GetProperty(propertyName!)!.SetValue(id, value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var property = value.GetType().GetProperty("Value");
            var propertyValue = (Guid)property!.GetValue(value)!;
            writer.WritePropertyName("Value");
            _valueConverter.Write(writer, propertyValue, options);

            writer.WriteEndObject();
        }
    }
}