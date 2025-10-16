using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LessonFlow.Database.Converters;

public class LessonPlanIdListConverter() : ValueConverter<List<Guid>, string>(
    l => JsonSerializer.Serialize(l, (JsonSerializerOptions)null!),
    l => JsonSerializer.Deserialize<List<Guid>>(l, (JsonSerializerOptions)null!)!);