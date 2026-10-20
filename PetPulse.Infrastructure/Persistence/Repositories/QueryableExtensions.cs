using System.Linq.Expressions;
using PetPulse.Domain.Commom;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Extensões de ordenação e paginação usadas pelos repositórios EF Core.
/// </summary>
internal static class QueryableExtensions
{
    /// <summary>
    /// Ordena pelo campo informado na direção pedida e desempata pelo Id, para que a paginação
    /// seja estável (sem itens repetidos ou pulados entre páginas quando o campo se repete).
    /// </summary>
    public static IOrderedQueryable<T> OrdenarPor<T, TChave>(
        this IQueryable<T> query,
        Expression<Func<T, TChave>> chave,
        bool descendente) where T : BaseEntity
    {
        var ordenada = descendente ? query.OrderByDescending(chave) : query.OrderBy(chave);

        return ordenada.ThenBy(entidade => entidade.Id);
    }

    /// <summary>
    /// Conta o total de itens e retorna apenas os itens da página pedida (paginação por offset,
    /// mesmo modelo do repositório de referência da disciplina).
    /// </summary>
    public static (IReadOnlyList<T> Items, int Total) Paginar<T>(
        this IOrderedQueryable<T> query,
        int page,
        int pageSize)
    {
        var total = query.Count();

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }
}
