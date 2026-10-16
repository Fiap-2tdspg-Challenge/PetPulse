using PetPulse.Domain.entites;
using PetPulse.Domain.Exceptions;

namespace PetPulse.Domain.Tests;

public class UsuarioTests
{
    [Fact]
    public void CriarUsuario_ComDadosValidos_DeveCriarUsuario()
    {
        // Arrange & Act
        var usuario = new Usuario(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            "Senha123456",
            "11999990001",
            "Rua das Flores, 100");

        // Assert
        Assert.Equal("Ana Souza", usuario.Nome);
        Assert.Equal("ana.souza@email.com", usuario.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AtualizarNome_ComNomeInvalido_DeveLancarException(string nomeInvalido)
    {
        // Arrange
        var usuario = CriarUsuarioValido();

        // Act
        var act = () => usuario.AtualizarNome(nomeInvalido);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal("Nome do usuário não pode ser vazio.", ex.Message);
    }

    [Theory]
    [InlineData("emailinvalido.com")]
    [InlineData("")]
    public void AtualizarEmail_ComEmailInvalido_DeveLancarException(string emailInvalido)
    {
        // Arrange
        var usuario = CriarUsuarioValido();

        // Act
        var act = () => usuario.AtualizarEmail(emailInvalido);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal("E-mail inválido.", ex.Message);
    }

    [Fact]
    public void AtualizarSenha_ComSenhaVazia_DeveLancarException()
    {
        // Arrange
        var usuario = CriarUsuarioValido();

        // Act
        var act = () => usuario.AtualizarSenha("");

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal("Senha não pode ser vazia.", ex.Message);
    }

    [Fact]
    public void AtualizarSenha_DeveArmazenarHashENaoASenhaCrua()
    {
        // Arrange
        var usuario = CriarUsuarioValido();

        // Act
        usuario.AtualizarSenha("Senha123456");

        // Assert
        Assert.NotEqual("Senha123456", usuario.Senha);
    }

    [Fact]
    public void VerificarSenha_ComASenhaCorreta_DeveRetornarTrue()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        usuario.AtualizarSenha("Senha123456");

        // Act
        var resultado = usuario.VerificarSenha("Senha123456");

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void VerificarSenha_ComASenhaIncorreta_DeveRetornarFalse()
    {
        // Arrange
        var usuario = CriarUsuarioValido();
        usuario.AtualizarSenha("Senha123456");

        // Act
        var resultado = usuario.VerificarSenha("SenhaErrada999");

        // Assert
        Assert.False(resultado);
    }

    private static Usuario CriarUsuarioValido() =>
        new(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            "Senha123456",
            "11999990001",
            "Rua das Flores, 100");
}
