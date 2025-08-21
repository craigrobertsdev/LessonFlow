namespace LessonFlow.Exceptions;

public class InvalidCredentialsException()
    : BaseException("Invalid credentials", 401, "Authentication.InvalidCredentials");

public class DuplicateEmailException()
    : BaseException("That email is already in use", 409, "Authentication.DuplicateEmail");

public class UserRegistrationFailedException(string? message = null) : BaseException(
    message ?? "Users registration failed. Please check details and try again.", 500,
    "Authentication.UserRegistrationFailed");

public class PasswordsDoNotMatchException()
    : BaseException("Passwords do not match", 400, "Authentication.PasswordsDoNotMatch");