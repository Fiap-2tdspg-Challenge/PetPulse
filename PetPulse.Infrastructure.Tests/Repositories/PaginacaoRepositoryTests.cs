using Microsoft.EntityFrameworkCore;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;
using PetPulse.Infrastructure.Persistence;
using PetPulse.Infrastructure.Repositories;

namespace PetPulse.Infrastructure.Tests.Repositories;

/// <summary>
/// Paginação, ordenação e filtros dos repositórios (GetPaged), com EF Core InMemory.
/// </summary>
public class PaginacaoRepositoryTests
{
    private static PetPulseContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PetPulseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PetPulseContext(options);
    }

    private static Usuario CriarUsuario(string nome, string email, string cpf) =>
        new(nome, cpf, email, "senha12345", null, null);

    private static Pet CriarPet(Guid usuarioId, string nome, string especie, decimal peso, PortePetEnum porte) =>
        new(usuarioId, nome, especie, null, null, peso, SexoPetEnum.Macho, false, porte);

    private static (PetPulseContext Context, Usuario Usuario) CriarBaseComPets()
    {
        var context = CriarContexto();
        var usuario = CriarUsuario("Ana Souza", "ana@teste.com", "11111111111");
        context.Usuarios.Add(usuario);
        context.SaveChanges();

        context.Pets.AddRange(
            CriarPet(usuario.Id, "Thor", "Cachorro", 30m, PortePetEnum.Grande),
            CriarPet(usuario.Id, "Mel", "Gato", 4m, PortePetEnum.Pequeno),
            CriarPet(usuario.Id, "Bob", "Cachorro", 12m, PortePetEnum.Medio),
            CriarPet(usuario.Id, "Luna", "Cachorro", 8m, PortePetEnum.Pequeno),
            CriarPet(usuario.Id, "Nina", "Gato", 5m, PortePetEnum.Pequeno));
        context.SaveChanges();

        return (context, usuario);
    }

    [Fact]
    public void Pet_GetPaged_SemParametros_DeveOrdenarPorNomeERetornarTotal()
    {
        // Arrange
        var (context, _) = CriarBaseComPets();
        using var _ctx = context;
        var repository = new PetRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new PetQuery());

        // Assert
        Assert.Equal(5, total);
        Assert.Equal(new[] { "Bob", "Luna", "Mel", "Nina", "Thor" }, itens.Select(pet => pet.Nome));
    }

    [Fact]
    public void Pet_GetPaged_DevePaginar()
    {
        // Arrange
        var (context, _) = CriarBaseComPets();
        using var _ctx = context;
        var repository = new PetRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new PetQuery { Page = 2, PageSize = 2 });

        // Assert
        Assert.Equal(5, total);
        Assert.Equal(new[] { "Mel", "Nina" }, itens.Select(pet => pet.Nome));
    }

    [Fact]
    public void Pet_GetPaged_ComFiltroDeEspecieEOrdenacaoPorPesoDesc_DeveFiltrarEOrdenar()
    {
        // Arrange
        var (context, _) = CriarBaseComPets();
        using var _ctx = context;
        var repository = new PetRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new PetQuery
        {
            Especie = "cachorro",
            SortBy = "peso",
            SortDirection = "desc"
        });

        // Assert
        Assert.Equal(3, total);
        Assert.Equal(new[] { "Thor", "Bob", "Luna" }, itens.Select(pet => pet.Nome));
    }

    [Fact]
    public void Pet_GetPaged_ComFiltroDePorteEUsuario_DeveCombinarOsFiltros()
    {
        // Arrange
        var (context, usuario) = CriarBaseComPets();
        using var _ctx = context;
        var repository = new PetRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new PetQuery
        {
            UsuarioId = usuario.Id,
            Porte = PortePetEnum.Pequeno,
            Especie = "Gato"
        });

        // Assert
        Assert.Equal(2, total);
        Assert.All(itens, pet => Assert.Equal(PortePetEnum.Pequeno, pet.Porte));
    }

    [Fact]
    public void Usuario_GetPaged_ComFiltroDeNome_DeveIgnorarMaiusculas()
    {
        // Arrange
        using var context = CriarContexto();
        context.Usuarios.AddRange(
            CriarUsuario("Ana Souza", "ana@teste.com", "11111111111"),
            CriarUsuario("Bruno Lima", "bruno@teste.com", "22222222222"),
            CriarUsuario("Mariana Alves", "mariana@teste.com", "33333333333"));
        context.SaveChanges();
        var repository = new UsuarioRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new UsuarioQuery { Nome = "ANA" });

        // Assert
        Assert.Equal(2, total);
        Assert.Equal(new[] { "Ana Souza", "Mariana Alves" }, itens.Select(usuario => usuario.Nome));
    }

    [Fact]
    public void Alerta_GetPaged_ComFiltroDeStatusEOrdenacaoPorRisco_DeveFiltrarEOrdenar()
    {
        // Arrange
        using var context = CriarContexto();
        var petId = Guid.NewGuid();

        var baixo = new AlertaInteligente(petId, TipoAlertaEnum.Vacina, NivelRiscoEnum.Baixo, OrigemAlertaEnum.Sistema, "Vacina próxima", null);
        var alto = new AlertaInteligente(petId, TipoAlertaEnum.FrequenciaCardiaca, NivelRiscoEnum.Alto, OrigemAlertaEnum.DispositivoIot, "BPM alto", null);
        var medioResolvido = new AlertaInteligente(petId, TipoAlertaEnum.Atividade, NivelRiscoEnum.Medio, OrigemAlertaEnum.DispositivoIot, "Pouca atividade", null);
        medioResolvido.Resolver();

        context.AlertasInteligentes.AddRange(baixo, alto, medioResolvido);
        context.SaveChanges();
        var repository = new AlertaInteligenteRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new AlertaInteligenteQuery
        {
            Status = StatusAlertaEnum.Aberto,
            SortBy = "nivelRisco",
            SortDirection = "desc"
        });

        // Assert
        Assert.Equal(2, total);
        Assert.Equal(new[] { NivelRiscoEnum.Alto, NivelRiscoEnum.Baixo }, itens.Select(alerta => alerta.NivelRisco));
    }

    [Fact]
    public void Historico_GetPaged_ComIntervaloDeDatas_DeveFiltrarPeloPeriodo()
    {
        // Arrange
        using var context = CriarContexto();
        var petId = Guid.NewGuid();

        context.HistoricosClinicos.AddRange(
            new HistoricoClinico(petId, TipoRegistroClinicoEnum.Vacina, "V8", new DateOnly(2026, 1, 10), null, null, null),
            new HistoricoClinico(petId, TipoRegistroClinicoEnum.Consulta, "Check-up", new DateOnly(2026, 3, 5), null, null, null),
            new HistoricoClinico(petId, TipoRegistroClinicoEnum.Exame, "Hemograma", new DateOnly(2026, 6, 20), null, null, null));
        context.SaveChanges();
        var repository = new HistoricoClinicoRepository(context);

        // Act
        var (itens, total) = repository.GetPaged(new HistoricoClinicoQuery
        {
            DataInicio = new DateOnly(2026, 2, 1),
            DataFim = new DateOnly(2026, 12, 31)
        });

        // Assert — padrão: mais recente primeiro
        Assert.Equal(2, total);
        Assert.Equal(new[] { "Hemograma", "Check-up" }, itens.Select(historico => historico.Descricao));
    }
}
