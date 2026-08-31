using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.Tests;

public class PetTests
{
    [Fact]
    public void CriarPet_ComDadosValidos_DeveCriarPet()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var nascimento = new DateOnly(2021, 4, 10);

        // Act
        var pet = new Pet(
            usuarioId,
            "Thor",
            "Cachorro",
            "Golden Retriever",
            nascimento,
            28.5m,
            SexoPetEnum.Macho,
            true,
            PortePetEnum.Grande);

        // Assert
        Assert.Equal("Thor", pet.Nome);
        Assert.Equal(usuarioId, pet.UsuarioId);
        Assert.True(pet.Castrado);
    }

    [Fact]
    public void Construtor_ComNomeVazio_DeveLancarException()
    {
        // Arrange & Act
        var act = () => new Pet(
            Guid.NewGuid(),
            "",
            "Cachorro",
            null,
            null,
            null,
            SexoPetEnum.Macho,
            false,
            PortePetEnum.Medio);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Nome do pet não pode ser vazio.", ex.Message);
    }

    [Fact]
    public void AtualizarDataNascimento_ComDataFutura_DeveLancarException()
    {
        // Arrange
        var pet = CriarPetValido();
        var dataFutura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var act = () => pet.AtualizarDataNascimento(dataFutura);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Data de nascimento não pode ser futura.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AtualizarPeso_ComPesoZeroOuNegativo_DeveLancarException(decimal pesoInvalido)
    {
        // Arrange
        var pet = CriarPetValido();

        // Act
        var act = () => pet.AtualizarPeso(pesoInvalido);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Peso deve ser maior que zero.", ex.Message);
    }

    [Fact]
    public void AtualizarDados_ComDadosValidos_DeveAtualizarTodosOsCampos()
    {
        // Arrange
        var pet = CriarPetValido();
        var novaDataNascimento = new DateOnly(2020, 1, 1);

        // Act
        pet.AtualizarDados(
            "Thor Atualizado",
            "Cachorro",
            "Labrador",
            novaDataNascimento,
            30.0m,
            SexoPetEnum.Macho,
            false,
            PortePetEnum.Medio);

        // Assert
        Assert.Equal("Thor Atualizado", pet.Nome);
        Assert.Equal("Labrador", pet.Raca);
        Assert.Equal(novaDataNascimento, pet.DataNascimento);
        Assert.Equal(30.0m, pet.Peso);
        Assert.False(pet.Castrado);
        Assert.Equal(PortePetEnum.Medio, pet.Porte);
    }

    [Fact]
    public void AtualizarDados_ComNomeVazio_DeveLancarExceptionENaoAlterarNomeOriginal()
    {
        // Arrange
        var pet = CriarPetValido();

        // Act
        var act = () => pet.AtualizarDados(
            "",
            "Cachorro",
            "Golden Retriever",
            new DateOnly(2021, 4, 10),
            28.5m,
            SexoPetEnum.Macho,
            true,
            PortePetEnum.Grande);

        // Assert
        Assert.Throws<Exception>(act);
        Assert.Equal("Thor", pet.Nome);
    }

    private static Pet CriarPetValido() =>
        new(
            Guid.NewGuid(),
            "Thor",
            "Cachorro",
            "Golden Retriever",
            new DateOnly(2021, 4, 10),
            28.5m,
            SexoPetEnum.Macho,
            true,
            PortePetEnum.Grande);
}
