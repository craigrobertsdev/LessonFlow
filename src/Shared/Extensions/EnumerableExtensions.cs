namespace LessonFlow.Extensionss;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ElseCreateNew<T>(this IEnumerable<T> sequence, int count) where T : notnull, new()
    {
        return sequence.Fill(count);
    }

    /// <summary>
    ///     Creates a sequence with the specified number of elements, filling in the gaps with new instances of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list to be filled</param>
    /// <param name="count">The required number of elements in the resulting sequence</param>
    /// <returns>The list for further calls to be chained</returns>
    public static IEnumerable<T> Fill<T>(this IEnumerable<T> list, int count) where T : notnull, new()
    {
        return list.Concat(Enumerable.Repeat(new T(), count - list.Count()));
    }


    /// <summary>
    ///     Compares two sequences to see if the source contains all the elements of the target.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> target)
    {
        return !target.Except(source).Any();
    }
}