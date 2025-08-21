using System.Text.Json.Serialization;
using LessonFlow.Api.Contracts;
using LessonFlow.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Domain.StronglyTypedIds;

[JsonConverter(typeof(StronglyTypedIdJsonConverter<SubjectId>))]
public record SubjectId(Guid Value) : IStronglyTypedId
{
    public Guid Value { get; set; } = Value;
    public override string ToString() => Value.ToString();

    public class StronglyTypedIdEfValueConverter(ConverterMappingHints? mappingHints = null)
        : ValueConverter<SubjectId, Guid>(id => id.Value, value => new SubjectId(value), mappingHints);
}