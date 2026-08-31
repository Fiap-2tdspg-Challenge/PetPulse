using PetPulse.Domain.entites;

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
        var ex = Assert.Throws<Exception>(act);
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
        var ex = Assert.Throws<Exception>(act);
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
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Senha não pode ser vazia.", ex.Message);
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
