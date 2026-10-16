using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PetPulse.Application.DTOs.Login;
using PetPulse.Domain.entites;

namespace PetPulse.Application.Auth;

/// <summary>
/// Emite o token JWT de acesso para um usuário já autenticado.
/// </summary>
/// <remarks>
/// Segue o mesmo padrão de emissão usado no repositório de referência da disciplina
/// (Recommenda): claims básicas (sub/email/name), assinatura HMAC-SHA256 com a chave
/// configurada em "Jwt:Key" e expiração de 2 horas.
/// </remarks>
public static class JwtTokenIssuer
{
    public static LoginResponse Create(Usuario usuario, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");

        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");

        var issuer = jwtSection["Issuer"] ?? "PetPulse";
        var audience = jwtSection["Audience"] ?? "PetPulse.API";

        var expiracao = DateTime.UtcNow.AddHours(2);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Name, usuario.Nome),
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiracao,
            signingCredentials: credenciais);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(accessToken, expiracao, usuario.Email, usuario.Nome);
    }
}
