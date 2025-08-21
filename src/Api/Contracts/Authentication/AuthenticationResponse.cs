namespace LessonFlow.Api.Contracts.Authentication;

public record AuthenticationResponse(string Token, DateTime TokenExpiration, string RefreshToken, AccountDataResponse AccountData);