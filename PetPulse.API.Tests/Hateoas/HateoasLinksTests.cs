using PetPulse.Application.DTOs;
using PetPulse.Domain.Enum;
using PetPulse.Hateoas;

namespace PetPulse.API.Tests.Hateoas;

public class HateoasLinksTests
{
    [Fact]
    public void Paginacao_NaPrimeiraPagina_DeveTerNextESemPrev()
    {
        // Arrange
        var query = new PetQuery { Page = 1, PageSize = 10 };
        var pagina = new PagedResponse<string>(["a"], Page: 1, PageSize: 10, TotalItems: 25);

        // Act
        var links = HateoasLinks.Paginacao(Rotas.Pet, query, pagina);

        // Assert
        Assert.Contains(links, l => l.Rel == "self" && l.Href == "/api/Pet?page=1&pageSize=10");
        Assert.Contains(links, l => l.Rel == "next" && l.Href == "/api/Pet?page=2&pageSize=10");
        Assert.Contains(links, l => l.Rel == "last" && l.Href == "/api/Pet?page=3&pageSize=10");
        Assert.DoesNotContain(links, l => l.Rel == "prev");
        Assert.Contains(links, l => l.Rel == "create" && l.Method == "POST");
    }

    [Fact]
    public void Paginacao_NaUltimaPagina_DeveTerPrevESemNext()
    {
        // Arrange
        var query = new PetQuery { Page = 3, PageSize = 10 };
        var pagina = new PagedResponse<string>(["a"], Page: 3, PageSize: 10, TotalItems: 25);

        // Act
        var links = HateoasLinks.Paginacao(Rotas.Pet, query, pagina);

        // Assert
        Assert.Contains(links, l => l.Rel == "prev" && l.Href == "/api/Pet?page=2&pageSize=10");
        Assert.DoesNotContain(links, l => l.Rel == "next");
    }

    [Fact]
    public void Paginacao_DeveManterFiltrosEOrdenacaoNosLinks()
    {
        // Arrange
        var query = new PetQuery { Page = 1, PageSize = 5, SortBy = "peso", SortDirection = "desc", Especie = "Cachorro", Porte = PortePetEnum.Grande };
        var pagina = new PagedResponse<string>(["a"], Page: 1, PageSize: 5, TotalItems: 12);

        // Act
        var links = HateoasLinks.Paginacao(Rotas.Pet, query, pagina);

        // Assert
        var next = Assert.Single(links, l => l.Rel == "next");
        Assert.Equal("/api/Pet?page=2&pageSize=5&sortBy=peso&sortDirection=desc&especie=Cachorro&porte=Grande", next.Href);
    }

    [Fact]
    public void Paginacao_SemResultados_DeveApontarLastParaPrimeiraPagina()
    {
        // Arrange
        var query = new PetQuery();
        var pagina = new PagedResponse<string>([], Page: 1, PageSize: 20, TotalItems: 0);

        // Act
        var links = HateoasLinks.Paginacao(Rotas.Pet, query, pagina);

        // Assert
        Assert.Contains(links, l => l.Rel == "last" && l.Href == "/api/Pet?page=1&pageSize=20");
        Assert.DoesNotContain(links, l => l.Rel == "next");
    }

    [Fact]
    public void Pet_DeveTerCrudERecursosRelacionados()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var links = HateoasLinks.Pet(petId, usuarioId);

        // Assert
        Assert.Contains(links, l => l.Rel == "self" && l.Method == "GET" && l.Href == $"/api/Pet/{petId}");
        Assert.Contains(links, l => l.Rel == "update" && l.Method == "PUT");
        Assert.Contains(links, l => l.Rel == "delete" && l.Method == "DELETE");
        Assert.Contains(links, l => l.Rel == "usuario" && l.Href == $"/api/Usuario/{usuarioId}");
        Assert.Contains(links, l => l.Rel == "historicos-clinicos" && l.Href == $"/api/HistoricoClinico/pet/{petId}");
        Assert.Contains(links, l => l.Rel == "dispositivo-iot" && l.Href == $"/api/DispositivoIot/pet/{petId}");
    }

    [Fact]
    public void DispositivoIot_DeveTerLinksDasLeiturasNoMongo()
    {
        // Arrange
        var dispositivoId = Guid.NewGuid();

        // Act
        var links = HateoasLinks.DispositivoIot(dispositivoId, Guid.NewGuid());

        // Assert
        Assert.Contains(links, l => l.Rel == "leituras" && l.Method == "GET" && l.Href == $"/api/DispositivoIot/{dispositivoId}/leituras");
        Assert.Contains(links, l => l.Rel == "registrar-leitura" && l.Method == "POST");
    }

    [Fact]
    public void AlertaInteligente_Aberto_DeveOferecerVisualizarEResolver()
    {
        // Act
        var links = HateoasLinks.AlertaInteligente(Guid.NewGuid(), Guid.NewGuid(), StatusAlertaEnum.Aberto);

        // Assert
        Assert.Contains(links, l => l.Rel == "visualizar" && l.Method == "PUT");
        Assert.Contains(links, l => l.Rel == "resolver" && l.Method == "PUT");
    }

    [Fact]
    public void AlertaInteligente_Visualizado_DeveOferecerSoResolver()
    {
        // Act
        var links = HateoasLinks.AlertaInteligente(Guid.NewGuid(), Guid.NewGuid(), StatusAlertaEnum.Visualizado);

        // Assert
        Assert.DoesNotContain(links, l => l.Rel == "visualizar");
        Assert.Contains(links, l => l.Rel == "resolver");
    }

    [Fact]
    public void AlertaInteligente_Resolvido_NaoDeveOferecerAcoesDeEstado()
    {
        // Act
        var links = HateoasLinks.AlertaInteligente(Guid.NewGuid(), Guid.NewGuid(), StatusAlertaEnum.Resolvido);

        // Assert
        Assert.DoesNotContain(links, l => l.Rel == "visualizar");
        Assert.DoesNotContain(links, l => l.Rel == "resolver");
        Assert.Contains(links, l => l.Rel == "self");
    }
}
