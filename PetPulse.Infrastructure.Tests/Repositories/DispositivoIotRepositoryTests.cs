using Microsoft.EntityFrameworkCore;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Infrastructure.Tests.Repositories;

public class DispositivoIotRepositoryTests
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

    private static DispositivoIot CriarDispositivoValido(Guid petId) =>
        new(
            petId: petId,
            dataVinculacao: new DateOnly(2026, 1, 1),
            intervaloColetaMinutos: 15,
            frequenciaCardiaca: 80,
            nivelAtividade: 5m,
            pressao: 12m,
            dataUltimaLeitura: DateTime.UtcNow,
            status: StatusDispositivoEnum.Ativo);

    private static Guid CriarPetPersistido(PetPulseContext context)
    {
        var usuario = CriarUsuarioValido();
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        var pet = CriarPetValido(usuario.Id);
        context.Pets.Add(pet);
        context.SaveChanges();

        return pet.Id;
    }

    [Fact]
    public void Add_DevePersistirEPermitirBuscarPorId()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);
        var petId = CriarPetPersistido(context);
        var dispositivo = CriarDispositivoValido(petId);

        // Act
        var dispositivoAdicionado = repository.Add(dispositivo);
        var dispositivoEncontrado = repository.GetById(dispositivoAdicionado.Id);

        // Assert
        Assert.NotNull(dispositivoEncontrado);
        Assert.Equal(StatusDispositivoEnum.Ativo, dispositivoEncontrado!.Status);
    }

    [Fact]
    public void GetByPetId_QuandoExiste_DeveRetornarODispositivoDoPet()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);
        var petId = CriarPetPersistido(context);
        var dispositivo = CriarDispositivoValido(petId);
        context.DispositivosIot.Add(dispositivo);
        context.SaveChanges();

        // Act
        var resultado = repository.GetByPetId(petId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(dispositivo.Id, resultado!.Id);
    }

    [Fact]
    public void GetByPetId_QuandoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);

        // Act
        var resultado = repository.GetByPetId(Guid.NewGuid());

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ExistsByPetId_QuandoPetJaTemDispositivo_DeveRetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);
        var petId = CriarPetPersistido(context);
        var dispositivo = CriarDispositivoValido(petId);
        context.DispositivosIot.Add(dispositivo);
        context.SaveChanges();

        // Act
        var resultado = repository.ExistsByPetId(petId);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void ExistsByPetId_QuandoPetNaoTemDispositivo_DeveRetornarFalse()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);
        var petId = CriarPetPersistido(context);

        // Act
        var resultado = repository.ExistsByPetId(petId);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void Delete_QuandoExiste_DeveRemoverERetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);
        var petId = CriarPetPersistido(context);
        var dispositivo = CriarDispositivoValido(petId);
        context.DispositivosIot.Add(dispositivo);
        context.SaveChanges();

        // Act
        var resultado = repository.Delete(dispositivo.Id);

        // Assert
        Assert.True(resultado);
        Assert.Null(repository.GetById(dispositivo.Id));
    }

    [Fact]
    public void Delete_QuandoNaoExiste_DeveRetornarFalse()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new DispositivoIotRepository(context);

        // Act
        var resultado = repository.Delete(Guid.NewGuid());

        // Assert
        Assert.False(resultado);
    }
}
