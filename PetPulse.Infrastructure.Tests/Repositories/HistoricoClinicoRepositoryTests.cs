using Microsoft.EntityFrameworkCore;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Infrastructure.Tests.Repositories;

public class HistoricoClinicoRepositoryTests
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

    private static HistoricoClinico CriarHistoricoValido(Guid petId, DateOnly dataRegistro) =>
        new(
            petId: petId,
            tipoRegistro: TipoRegistroClinicoEnum.Consulta,
            descricao: "Consulta de rotina",
            dataRegistro: dataRegistro,
            dataRetorno: null,
            profissionalClinica: "Dr. João",
            observacoes: "Sem observações");

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
        var repository = new HistoricoClinicoRepository(context);
        var petId = CriarPetPersistido(context);
        var historico = CriarHistoricoValido(petId, new DateOnly(2026, 1, 1));

        // Act
        var historicoAdicionado = repository.Add(historico);
        var historicoEncontrado = repository.GetById(historicoAdicionado.Id);

        // Assert
        Assert.NotNull(historicoEncontrado);
        Assert.Equal("Consulta de rotina", historicoEncontrado!.Descricao);
    }

    [Fact]
    public void GetByPetId_DeveRetornarApenasHistoricosDoPetInformadoOrdenadosPorDataDecrescente()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new HistoricoClinicoRepository(context);
        var petId1 = CriarPetPersistido(context);
        var petId2 = CriarPetPersistido(context);

        var historicoAntigo = CriarHistoricoValido(petId1, new DateOnly(2025, 1, 1));
        var historicoRecente = CriarHistoricoValido(petId1, new DateOnly(2026, 1, 1));
        var historicoDeOutroPet = CriarHistoricoValido(petId2, new DateOnly(2026, 6, 1));

        context.HistoricosClinicos.AddRange(historicoAntigo, historicoRecente, historicoDeOutroPet);
        context.SaveChanges();

        // Act
        var resultado = repository.GetByPetId(petId1);

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Equal(historicoRecente.Id, resultado[0].Id);
        Assert.Equal(historicoAntigo.Id, resultado[1].Id);
    }

    [Fact]
    public void GetById_QuandoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new HistoricoClinicoRepository(context);

        // Act
        var resultado = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void Delete_QuandoExiste_DeveRemoverERetornarTrue()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new HistoricoClinicoRepository(context);
        var petId = CriarPetPersistido(context);
        var historico = CriarHistoricoValido(petId, new DateOnly(2026, 1, 1));
        context.HistoricosClinicos.Add(historico);
        context.SaveChanges();

        // Act
        var resultado = repository.Delete(historico.Id);

        // Assert
        Assert.True(resultado);
        Assert.Null(repository.GetById(historico.Id));
    }

    [Fact]
    public void Delete_QuandoNaoExiste_DeveRetornarFalse()
    {
        // Arrange
        using var context = CriarContexto();
        var repository = new HistoricoClinicoRepository(context);

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
        var repository = new HistoricoClinicoRepository(context);
        var petId = CriarPetPersistido(context);
        var historico = CriarHistoricoValido(petId, new DateOnly(2026, 1, 1));
        context.HistoricosClinicos.Add(historico);
        context.SaveChanges();

        // Act
        var resultado = repository.ExistsById(historico.Id);

        // Assert
        Assert.True(resultado);
    }
}
