using Microsoft.Extensions.Configuration;
using Moq;
using PetPulse.Application.Auth;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;

namespace PetPulse.API.Tests.Auth;

public class LoginServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly IConfiguration _configuration;
    private readonly LoginService _loginService;

    public LoginServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "chave-de-teste-bem-grande-para-assinar-o-token-0123456789",
                ["Jwt:Issuer"] = "PetPulse.Tests",
                ["Jwt:Audience"] = "PetPulse.API.Tests"
            })
            .Build();

        _loginService = new LoginService(_usuarioRepository.Object, _configuration);
    }

    private static Usuario CriarUsuarioComSenha(string senha) =>
        new(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            senha,
            "11999990001",
            "Rua das Flores, 100");

    [Fact]
    public void Login_ComEmailESenhaCorretos_DeveRetornarToken()
    {
        // Arrange
        var usuario = CriarUsuarioComSenha("Senha123456");
        _usuarioRepository.Setup(r => r.GetByEmail(usuario.Email)).Returns(usuario);

        // Act
        var resultado = _loginService.Login(usuario.Email, "Senha123456");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuario.Email, resultado!.Email);
    }

    [Fact]
    public void Login_ComSenhaIncorreta_DeveRetornarNull()
    {
        // Arrange
        var usuario = CriarUsuarioComSenha("Senha123456");
        _usuarioRepository.Setup(r => r.GetByEmail(usuario.Email)).Returns(usuario);

        // Act
        var resultado = _loginService.Login(usuario.Email, "SenhaErrada999");

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void Login_ComEmailNaoCadastrado_DeveRetornarNull()
    {
        // Arrange
        _usuarioRepository.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((Usuario?)null);

        // Act
        var resultado = _loginService.Login("naoexiste@email.com", "qualquerSenha");

        // Assert
        Assert.Null(resultado);
    }
}
