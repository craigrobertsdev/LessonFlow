using LessonFlow.Domain.StronglyTypedIds;

namespace LessonFlow.Shared.Exceptions;

public class ResourceNotFoundException(ResourceId id) : BaseException($"No resource found with that ID: {id}", 404,
    "Resource.NotFound");
