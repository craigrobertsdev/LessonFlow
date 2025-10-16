using System.ComponentModel;
using System.Text.Json.Serialization;
using LessonFlow.Api.Contracts;
using LessonFlow.Database.Converters;
using LessonFlow.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Domain.StronglyTypedIds;

[TypeConverter(typeof(UserIdToStringTypeConverter))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<UserId>))]
public record UserId(Guid Value) : IStronglyTypedId
{
    public Guid Value { get; set; } = Value;

    public override string ToString() => Value.ToString();
    public class StronglyTypedIdEfValueConverter(ConverterMappingHints? mappingHints = null)
        : ValueConverter<UserId, Guid>(id => id.Value, value => new UserId(value), mappingHints);

    public class IdToStringConverter(ConverterMappingHints? mappingHints = null)
        : ValueConverter<UserId, string>(id => id.Value.ToString(), value => new UserId(Guid.Parse(value)),
            mappingHints);
}