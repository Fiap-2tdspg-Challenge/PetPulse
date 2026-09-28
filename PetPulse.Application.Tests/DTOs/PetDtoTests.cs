using System.ComponentModel.DataAnnotations;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Tests;

public class PetDtoTests
{
    [Fact]
    public void ToDomain_ComDadosValidos_DeveMapearParaPet()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = CriarRequestValido(usuarioId);

        // Act
        var pet = request.ToDomain();

        // Assert
        Assert.Equal(usuarioId, pet.UsuarioId);
        Assert.Equal("Rex", pet.Nome);
        Assert.Equal("Cão", pet.Especie);
        Assert.Equal("Labrador", pet.Raca);
        Assert.Equal(10.5m, pet.Peso);
        Assert.Equal(SexoPetEnum.Macho, pet.Sexo);
        Assert.False(pet.Castrado);
        Assert.Equal(PortePetEnum.Medio, pet.Porte);
    }

    [Fact]
    public void FromDomain_ComPetValido_DeveMapearParaResponse()
    {
        // Arrange
        var pet = new Pet(
            Guid.NewGuid(), "Rex", "Cão", "Labrador",
            new DateOnly(2020, 1, 1), 10.5m, SexoPetEnum.Macho, false, PortePetEnum.Medio);

        // Act
        var response = PetResponse.FromDomain(pet);

        // Assert
        Assert.Equal(pet.Id, response.Id);
        Assert.Equal(pet.UsuarioId, response.UsuarioId);
        Assert.Equal(pet.Nome, response.Nome);
        Assert.Equal(pet.Especie, response.Especie);
        Assert.Equal(pet.Raca, response.Raca);
        Assert.Equal(pet.DataNascimento, response.DataNascimento);
        Assert.Equal(pet.Peso, response.Peso);
        Assert.Equal(pet.Sexo, response.Sexo);
        Assert.Equal(pet.Castrado, response.Castrado);
        Assert.Equal(pet.Porte, response.Porte);
        Assert.Equal(pet.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void PetRequest_ComDadosValidos_DevePassarNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid());

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void PetRequest_ComNomeVazio_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { Nome = "" };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Nome"));
    }

    [Fact]
    public void PetRequest_ComPesoNegativo_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { Peso = -1m };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Peso"));
    }

    private static PetRequest CriarRequestValido(Guid usuarioId) =>
        new(
            usuarioId,
            "Rex",
            "Cão",
            "Labrador",
            new DateOnly(2020, 1, 1),
            10.5m,
            SexoPetEnum.Macho,
            false,
            PortePetEnum.Medio);

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return (isValid, results);
    }
}
