using System.Text.Json.Serialization;
using LessonFlow.Api.Contracts;
using LessonFlow.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Domain.StronglyTypedIds;

[JsonConverter(typeof(StronglyTypedIdJsonConverter<AssessmentId>))]
public record AssessmentId(Guid Value) : IStronglyTypedId
{
    public Guid Value { get; set; } = Value;

    public class StronglyTypedIdEfValueConverter : ValueConverter<AssessmentId, Guid>
    {
        public StronglyTypedIdEfValueConverter(ConverterMappingHints? mappingHints = null)
            : base(id => id.Value, value => new AssessmentId(value), mappingHints)
        {
        }

        public StronglyTypedIdEfValueConverter()
            : base(id => id.Value, value => new AssessmentId(value))
        {
        }
    }
}