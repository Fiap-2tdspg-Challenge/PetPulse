using PetPulse.Application.DTOs;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Tests;

public class PaginationQueryTests
{
    [Fact]
    public void TryGetError_ComValoresPadrao_NaoDeveRetornarErro()
    {
        // Arrange
        var query = new PetQuery();

        // Act
        var temErro = query.TryGetError(out var mensagem);

        // Assert
        Assert.False(temErro);
        Assert.Null(mensagem);
        Assert.Equal(1, query.Page);
        Assert.Equal(PaginationQuery.DefaultPageSize, query.PageSize);
    }

    [Theory]
    [InlineData(0, 20, null, null, "page")]
    [InlineData(1, 0, null, null, "pageSize")]
    [InlineData(1, 101, null, null, "pageSize")]
    [InlineData(1, 20, "nome", "cima", "sortDirection")]
    [InlineData(1, 20, "cpf", null, "sortBy")]
    public void TryGetError_ComParametroInvalido_DeveRetornarErroDescrevendoOCampo(
        int page, int pageSize, string? sortBy, string? sortDirection, string campo)
    {
        // Arrange
        var query = new PetQuery { Page = page, PageSize = pageSize, SortBy = sortBy, SortDirection = sortDirection };

        // Act
        var temErro = query.TryGetError(out var mensagem);

        // Assert
        Assert.True(temErro);
        Assert.Contains(campo, mensagem);
    }

    [Fact]
    public void TryGetError_ComSortByEmOutraCaixa_DeveAceitar()
    {
        // Arrange
        var query = new PetQuery { SortBy = "DataNascimento", SortDirection = "DESC" };

        // Act
        var temErro = query.TryGetError(out _);

        // Assert
        Assert.False(temErro);
        Assert.Equal("datanascimento", query.ObterCampoOrdenacao());
        Assert.True(query.EhDescendente());
    }

    [Fact]
    public void HistoricoClinicoQuery_ComDataInicioPosteriorADataFim_DeveRetornarErro()
    {
        // Arrange
        var query = new HistoricoClinicoQuery { DataInicio = new DateOnly(2026, 6, 1), DataFim = new DateOnly(2026, 1, 1) };

        // Act
        var temErro = query.TryGetError(out var mensagem);

        // Assert
        Assert.True(temErro);
        Assert.Contains("dataInicio", mensagem);
    }

    [Fact]
    public void OrdenacaoPadrao_DePet_DeveSerNomeCrescente()
    {
        // Arrange
        var query = new PetQuery();

        // Assert
        Assert.Equal("nome", query.ObterCampoOrdenacao());
        Assert.False(query.EhDescendente());
    }

    [Fact]
    public void OrdenacaoPadrao_DeAlerta_DeveSerMaisRecentePrimeiro()
    {
        // Arrange
        var query = new AlertaInteligenteQuery();

        // Assert
        Assert.Equal("datageracao", query.ObterCampoOrdenacao());
        Assert.True(query.EhDescendente());
    }

    [Fact]
    public void SortBySemDirecao_DeveSerCrescenteMesmoQuandoOPadraoEDecrescente()
    {
        // Arrange
        var query = new AlertaInteligenteQuery { SortBy = "nivelRisco" };

        // Assert
        Assert.Equal("nivelrisco", query.ObterCampoOrdenacao());
        Assert.False(query.EhDescendente());
    }

    [Fact]
    public void ToQueryString_DeveIncluirSoParametrosInformadosEEscaparValores()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new PetQuery
        {
            PageSize = 10,
            Nome = "Rex Junior",
            UsuarioId = usuarioId,
            Castrado = true,
            Sexo = SexoPetEnum.Macho
        };

        // Act
        var queryString = query.ToQueryString(3);

        // Assert
        Assert.Equal(
            $"?page=3&pageSize=10&usuarioId={usuarioId}&nome=Rex%20Junior&sexo=Macho&castrado=true",
            queryString);
    }

    [Fact]
    public void ToQueryString_DeHistorico_DeveFormatarDatasNoPadraoIso()
    {
        // Arrange
        var query = new HistoricoClinicoQuery { DataInicio = new DateOnly(2026, 1, 5), TipoRegistro = TipoRegistroClinicoEnum.Vacina };

        // Act
        var queryString = query.ToQueryString(1);

        // Assert
        Assert.Equal("?page=1&pageSize=20&tipoRegistro=Vacina&dataInicio=2026-01-05", queryString);
    }

    [Theory]
    [InlineData(25, 10, 1, 3, false, true)]
    [InlineData(25, 10, 3, 3, true, false)]
    [InlineData(20, 10, 2, 2, true, false)]
    [InlineData(0, 10, 1, 0, false, false)]
    public void PagedResponse_DeveCalcularTotalDePaginasENavegacao(
        int totalItems, int pageSize, int page, int totalPages, bool hasPrevious, bool hasNext)
    {
        // Act
        var pagina = new PagedResponse<string>([], page, pageSize, totalItems);

        // Assert
        Assert.Equal(totalPages, pagina.TotalPages);
        Assert.Equal(hasPrevious, pagina.HasPrevious);
        Assert.Equal(hasNext, pagina.HasNext);
    }
}
