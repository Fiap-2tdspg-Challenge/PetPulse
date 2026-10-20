using System.Globalization;

namespace PetPulse.Application.DTOs;

/// <summary>
/// Parâmetros de paginação, ordenação e filtros das listagens (query string).
/// </summary>
/// <remarks>
/// A paginação por offset (<c>page</c> / <c>pageSize</c>) segue o modelo do repositório de
/// referência da disciplina (Recommenda: <c>PaginationQuery</c>). O PetPulse acrescenta a
/// ordenação (<c>sortBy</c> / <c>sortDirection</c>) e os filtros, definidos em cada classe
/// filha (ex.: <see cref="PetQuery"/>).
/// </remarks>
public abstract class PaginationQuery
{
    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 100;

    /// <summary>Número da página (começa em 1).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Quantidade de itens por página (de 1 a 100, padrão 20).</summary>
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>Campo de ordenação (os valores aceitos dependem do recurso).</summary>
    public string? SortBy { get; set; }

    /// <summary>Direção da ordenação: <c>asc</c> ou <c>desc</c>.</summary>
    public string? SortDirection { get; set; }

    /// <summary>Campos aceitos em <see cref="SortBy"/>, em camelCase.</summary>
    protected abstract IReadOnlyCollection<string> CamposOrdenacao { get; }

    /// <summary>Campo usado quando <see cref="SortBy"/> não é informado.</summary>
    protected abstract string OrdenacaoPadrao { get; }

    /// <summary>Se a ordenação padrão (sem <see cref="SortBy"/>) é decrescente.</summary>
    protected virtual bool DescendentePadrao => false;

    /// <summary>Filtros da classe filha, usados para montar a query string dos links HATEOAS.</summary>
    protected abstract IEnumerable<KeyValuePair<string, string?>> Filtros();

    /// <summary>Validações extras dos filtros da classe filha (ex.: data inicial após a final).</summary>
    protected virtual bool TryGetFiltroError(out string? message)
    {
        message = null;
        return false;
    }

    /// <summary>
    /// Campo de ordenação efetivo, em minúsculas (o informado ou o padrão do recurso).
    /// </summary>
    public string ObterCampoOrdenacao() =>
        string.IsNullOrWhiteSpace(SortBy)
            ? OrdenacaoPadrao.ToLowerInvariant()
            : SortBy.Trim().ToLowerInvariant();

    /// <summary>
    /// Se a ordenação efetiva é decrescente. Sem <see cref="SortDirection"/>, usa a direção padrão
    /// do recurso quando <see cref="SortBy"/> também não foi informado, ou crescente caso contrário.
    /// </summary>
    public bool EhDescendente() =>
        string.IsNullOrWhiteSpace(SortDirection)
            ? string.IsNullOrWhiteSpace(SortBy) && DescendentePadrao
            : SortDirection.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Valida page, pageSize, sortBy, sortDirection e os filtros da classe filha.
    /// </summary>
    public bool TryGetError(out string? message)
    {
        if (Page < 1)
        {
            message = "page deve ser >= 1.";
            return true;
        }

        if (PageSize is < 1 or > MaxPageSize)
        {
            message = $"pageSize deve estar entre 1 e {MaxPageSize}.";
            return true;
        }

        if (!string.IsNullOrWhiteSpace(SortDirection)
            && !SortDirection.Trim().Equals("asc", StringComparison.OrdinalIgnoreCase)
            && !SortDirection.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            message = "sortDirection deve ser 'asc' ou 'desc'.";
            return true;
        }

        if (!string.IsNullOrWhiteSpace(SortBy)
            && !CamposOrdenacao.Any(campo => campo.Equals(SortBy.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            message = $"sortBy inválido. Valores aceitos: {string.Join(", ", CamposOrdenacao)}.";
            return true;
        }

        return TryGetFiltroError(out message);
    }

    /// <summary>
    /// Monta a query string desta mesma consulta (mesmos filtros e ordenação) apontando para
    /// outra página. Usada nos links de navegação HATEOAS (self, first, prev, next, last).
    /// </summary>
    public string ToQueryString(int page)
    {
        var parametros = new List<KeyValuePair<string, string?>>
        {
            new("page", page.ToString(CultureInfo.InvariantCulture)),
            new("pageSize", PageSize.ToString(CultureInfo.InvariantCulture)),
            new("sortBy", SortBy),
            new("sortDirection", SortDirection)
        };

        parametros.AddRange(Filtros());

        var pares = parametros
            .Where(parametro => !string.IsNullOrWhiteSpace(parametro.Value))
            .Select(parametro => $"{Uri.EscapeDataString(parametro.Key)}={Uri.EscapeDataString(parametro.Value!.Trim())}");

        return "?" + string.Join("&", pares);
    }

    /// <summary>Formata valores de filtro para a query string (enum pelo nome, bool em minúsculas).</summary>
    protected static string? Formatar<T>(T? valor) where T : struct
    {
        object? caixa = valor;

        return caixa switch
        {
            null => null,
            bool booleano => booleano ? "true" : "false",
            DateOnly data => data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => Convert.ToString(caixa, CultureInfo.InvariantCulture)
        };
    }
}
