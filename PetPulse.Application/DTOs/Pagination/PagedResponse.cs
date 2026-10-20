namespace PetPulse.Application.DTOs;

/// <summary>
/// Envelope padrão das listagens paginadas — mesmo modelo do repositório de referência da
/// disciplina (Recommenda: <c>PagedResponse</c>), com os links HATEOAS de navegação.
/// </summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    /// <summary>Total de páginas com o pageSize atual.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>Indica se existe página anterior.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>Indica se existe próxima página.</summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>Links HATEOAS de navegação (self, first, prev, next, last) e de criação.</summary>
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}
