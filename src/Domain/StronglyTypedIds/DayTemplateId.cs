using System.Text.Json.Serialization;
using LessonFlow.Api.Contracts;
using LessonFlow.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Domain.StronglyTypedIds;

[JsonConverter(typeof(StronglyTypedIdJsonConverter<DayTemplateId>))]
public record DayTemplateId(Guid Value) : IStronglyTypedId
{
    public Guid Value { get; set; } = Value;

    public class StronglyTypedIdEfValueConverter(ConverterMappingHints? mappingHints = null)
        : ValueConverter<DayTemplateId, Guid>(id => id.Value, value => new DayTemplateId(value), mappingHints);
}