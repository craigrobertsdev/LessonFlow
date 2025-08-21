namespace LessonFlow.Exceptions;

public class InputException(string message) : BaseException(message, 400, "Input.Error");