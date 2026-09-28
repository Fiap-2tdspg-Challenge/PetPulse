using System.ComponentModel.DataAnnotations;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;

namespace PetPulse.Application.Tests;

public class UsuarioDtoTests
{
    [Fact]
    public void ToDomain_ComDadosValidos_DeveMapearParaUsuario()
    {
        // Arrange
        var request = CriarRequestValido();

        // Act
        var usuario = request.ToDomain();

        // Assert
        Assert.Equal("Ana Souza", usuario.Nome);
        Assert.Equal("12345678901", usuario.Cpf);
        Assert.Equal("ana.souza@email.com", usuario.Email);
        Assert.Equal("Senha123456", usuario.Senha);
        Assert.Equal("11999990001", usuario.Telefone);
        Assert.Equal("Rua das Flores, 100", usuario.Endereco);
    }

    [Fact]
    public void FromDomain_ComUsuarioValido_DeveMapearParaResponse()
    {
        // Arrange
        var usuario = new Usuario(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            "Senha123456",
            "11999990001",
            "Rua das Flores, 100");

        // Act
        var response = UsuarioResponse.FromDomain(usuario);

        // Assert
        Assert.Equal(usuario.Id, response.Id);
        Assert.Equal(usuario.Nome, response.Nome);
        Assert.Equal(usuario.Cpf, response.Cpf);
        Assert.Equal(usuario.Email, response.Email);
        Assert.Equal(usuario.Telefone, response.Telefone);
        Assert.Equal(usuario.Endereco, response.Endereco);
        Assert.Equal(usuario.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void UsuarioRequest_ComDadosValidos_DevePassarNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido();

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("", "12345678901", "ana@email.com", "Senha123456", "Nome")]
    [InlineData("Ana Souza", "123", "ana@email.com", "Senha123456", "Cpf")]
    [InlineData("Ana Souza", "12345678901", "email-invalido", "Senha123456", "Email")]
    [InlineData("Ana Souza", "12345678901", "ana@email.com", "1234", "Senha")]
    public void UsuarioRequest_ComDadosInvalidos_DeveFalharNaValidacao(
        string nome, string cpf, string email, string senha, string campoComErro)
    {
        // Arrange
        var request = new UsuarioRequest(nome, cpf, email, senha, null, null);

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(campoComErro));
    }

    private static UsuarioRequest CriarRequestValido() =>
        new(
            "Ana Souza",
            "12345678901",
            "ana.souza@email.com",
            "Senha123456",
            "11999990001",
            "Rua das Flores, 100");

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return (isValid, results);
    }
}
