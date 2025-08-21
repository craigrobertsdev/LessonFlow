using System.Text.Json.Serialization;
using LessonFlow.Api.Contracts;
using LessonFlow.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Domain.StronglyTypedIds;

[JsonConverter(typeof(StronglyTypedIdJsonConverter<SchoolEventId>))]
public record SchoolEventId(Guid Value) : IStronglyTypedId
{
    public Guid Value { get; set; } = Value;

    public class StronglyTypedIdEfValueConverter(ConverterMappingHints? mappingHints = null)
        : ValueConverter<SchoolEventId, Guid>(id => id.Value, value => new SchoolEventId(value), mappingHints);
}