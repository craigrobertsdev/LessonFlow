using System.Text.RegularExpressions;

namespace LessonFlow.Services.FileStorage;

public partial class FileSystemUtils
{
    [GeneratedRegex("^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*$", RegexOptions.Compiled)]
    public static partial Regex ValidNameRegex { get; }
}