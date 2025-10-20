namespace LessonFlow.Shared.Exceptions;

public class UserNotFoundException() : BaseException("No user found with those details", 500, "Users.NotFound");

public class UserHasNoSubjectsException() : BaseException("Users has no subjects", 500, "Teachers.NoSubjects");

public class NoNewSubjectsTaughtException()
    : BaseException("No new subjects taught", 400, "Users.NoNewSubjectsTaught");

public class CreateTimeFromDtoException(string message) : BaseException(message, 400, "Users.CreateTimeFromDto");