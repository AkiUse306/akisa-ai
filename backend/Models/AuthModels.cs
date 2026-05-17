using System.ComponentModel.DataAnnotations;

namespace AkisaAi.Api.Models;

public sealed record RegisterRequest(
    [property: Required] string Username,
    [property: Required] string Password,
    string? DisplayName = null
);

public sealed record LoginRequest(
    [property: Required] string Username,
    [property: Required] string Password
);

public sealed record TokenResponse(string AccessToken, string RefreshToken, string UserId, string SessionId);
