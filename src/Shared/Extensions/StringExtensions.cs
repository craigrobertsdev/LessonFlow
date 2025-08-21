namespace LessonFlow.Extensionss;

public static class StringExtensions
{
    public static string CapitaliseFirstLetter(this string str)
    {
        var firstLetter = str[0].ToString().ToUpper();

        return firstLetter + str[1..];
    }

    public static string GetCssClassString(this string subjectName)
    {
        return subjectName.Split(" ")
            .Select((word, index) => index == 0 ? word.ToLower() : word.CapitaliseFirstLetter())
            .Aggregate("", (str1, str2) => str1 + str2);
    }
}