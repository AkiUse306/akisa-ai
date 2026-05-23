using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AkisaAi.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AkisaAi.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class JwtTokenService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private readonly byte[] _secret;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public JwtTokenService(IConfiguration configuration)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var secret = configuration["Jwt:Secret"] ?? "akisa-ai-development-super-secret-key";
        _secret = Encoding.UTF8.GetBytes(secret);
        _accessTokenExpiryMinutes = int.TryParse(configuration["Jwt:AccessTokenExpiryMinutes"], out var atEval) ? atEval : 60;
        _refreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var rtEval) ? rtEval : 7;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string CreateJwtToken(User user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName),
            new Claim("tokenType", "access")
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string CreateRefreshToken(User user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("tokenType", "refresh")
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public TokenValidationParameters GetValidationParameters()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_secret)
        };
    }
}
