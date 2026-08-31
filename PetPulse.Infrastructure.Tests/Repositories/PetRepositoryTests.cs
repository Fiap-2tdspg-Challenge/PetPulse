using Microsoft.EntityFrameworkCore;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Infrastructure.Tests.Repositories;

public class PetRepositoryTests
{
    private static PetPulseContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PetPulseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PetPulseContext(options);
    }

    private static Usuario CriarUsuarioValido() =>
        new(
            nome: "Maria Silva",
            cpf: "12345678900",
            email: "maria@teste.com",
            senha: "senha123",
            telefone: "11999999999",
            endereco: "Rua Teste, 123");

    private static Pet CriarPetValido(Guid usuarioId) =>
        new(
            usuarioId: usuarioId,
            nome: "Rex",
            especie: "Cachorro",
            raca: "Labrador",
            dataNascimento: new DateOnly(2020, 1, 1),
            peso: 20.5m,
            sexo: SexoPetEnum.Macho,
            castrado: true,
            porte: PortePetEnum.Grande);

    [Fact]
    public void Add_DevePersistirEPermitirBuscarPorId()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new PetRepository(context);
        var usuario = CriarUsuarioValido();
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        var pet = CriarPetValido(usuario.Id);

        // Act
        var petAdicionado = repository.Add(pet);
        var petEncontrado = repository.GetById(petAdicionado.Id);

        // Assert
        Assert.NotNull(petEncontrado);
        Assert.Equal("Rex", petEncontrado!.Nome);
    }

    [Fact]
    public void GetById_QuandoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new PetRepository(context);

        // Act
        var resultado = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void GetByUsuarioId_DeveRetornarApenasPetsDoUsuarioInformado()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new PetRepository(context);

        var usuario1 = CriarUsuarioValido();
        var usuario2 = CriarUsuarioValido();
        context.Usuarios.AddRange(usuario1, usuario2);
        context.SaveChanges();

        var petDoUsuario1 = CriarPetValido(usuario1.Id);
        var petDoUsuario2 = CriarPetValido(usuario2.Id);
        context.Pets.AddRange(petDoUsuario1, petDoUsuario2);
        context.SaveChanges();

        // Act
        var resultado = repository.GetByUsuarioId(usuario1.Id);

        // Assert
        Assert.Single(resultado);
        Assert.Equal(petDoUsuario1.Id, resultado[0].Id);
    }

    [Fact]
    public void Delete_QuandoExiste_DeveRemoverERetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new PetRepository(context);
        var usuario = CriarUsuarioValido();
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        var pet = CriarPetValido(usuario.Id);
        context.Pets.Add(pet);
        context.SaveChanges();

        // Act
        var resultado = repository.Delete(pet.Id);

        // Assert
        Assert.True(resultado);
        Assert.Null(repository.GetById(pet.Id));
    }

    [Fact]
    public void Delete_QuandoNaoExiste_DeveRetornarFalse()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new PetRepository(context);

        // Act
        var resultado = repository.Delete(Guid.NewGuid());

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void ExistsById_QuandoExiste_DeveRetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new PetRepository(context);
        var usuario = CriarUsuarioValido();
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        var pet = CriarPetValido(usuario.Id);
        context.Pets.Add(pet);
        context.SaveChanges();

        // Act
        var resultado = repository.ExistsById(pet.Id);

        // Assert
        Assert.True(resultado);
    }
}
