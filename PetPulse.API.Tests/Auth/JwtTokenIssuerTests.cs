using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using PetPulse.Application.Auth;
using PetPulse.Domain.entites;

namespace PetPulse.API.Tests.Auth;

public class JwtTokenIssuerTests
{
    private static IConfiguration CriarConfiguracao() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "chave-de-teste-bem-grande-para-assinar-o-token-0123456789",
                ["Jwt:Issuer"] = "PetPulse.Tests",
                ["Jwt:Audience"] = "PetPulse.API.Tests"
            })
            .Build();

    private static Usuario CriarUsuarioValido() =>
        new(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            "Senha123456",
            "11999990001",
            "Rua das Flores, 100");

    [Fact]
    public void Create_DeveGerarTokenComAsClaimsDoUsuario()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var configuracao = CriarConfiguracao();

        // Act
        var response = JwtTokenIssuer.Create(usuario, configuracao);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.AccessToken);

        Assert.Equal(usuario.Id.ToString(), token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(usuario.Email, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(usuario.Nome, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal("PetPulse.Tests", token.Issuer);
    }

    [Fact]
    public void Create_DeveGerarTokenComExpiracaoDeDuasHoras()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var configuracao = CriarConfiguracao();
        var antes = DateTime.UtcNow;

        // Act
        var response = JwtTokenIssuer.Create(usuario, configuracao);

        // Assert
        var diferenca = response.Expiracao - antes;
        Assert.True(diferenca.TotalMinutes is > 119 and < 121);
    }

    [Fact]
    public void Create_SemJwtKeyConfigurada_DeveLancarInvalidOperationException()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        var configuracaoSemChave = new ConfigurationBuilder().Build();

        // Act
        var act = () => JwtTokenIssuer.Create(usuario, configuracaoSemChave);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }
}
