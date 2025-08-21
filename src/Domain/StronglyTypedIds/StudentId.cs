using System.Text.Json.Serialization;
using LessonFlow.Api.Contracts;
using LessonFlow.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Domain.StronglyTypedIds;

[JsonConverter(typeof(StronglyTypedIdJsonConverter<StudentId>))]
public record StudentId(Guid Value) : IStronglyTypedId
{
    public Guid Value { get; set; } = Value;

    public class StronglyTypedIdEfValueConverter(ConverterMappingHints? mappingHints = null)
        : ValueConverter<StudentId, Guid>(id => id.Value, value => new StudentId(value), mappingHints);
}