namespace LessonFlow.Services.FileStorage;

public class ResourceDirectory
{
    public List<ResourceDirectory> Children = [];
    public ResourceDirectory(string name, ResourceDirectory? parent)
    {

    }
}