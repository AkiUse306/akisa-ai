using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AkisaAi.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AkisaAi.Api.Services;

public sealed class JwtTokenService
{
    private readonly byte[] _secret;

    public JwtTokenService(IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"] ?? "akisa-ai-development-super-secret-key";
        _secret = Encoding.UTF8.GetBytes(secret);
    }

    public string CreateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName)
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenValidationParameters GetValidationParameters()
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
