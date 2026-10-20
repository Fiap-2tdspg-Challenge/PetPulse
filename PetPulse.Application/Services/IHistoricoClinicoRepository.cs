using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;


/// <summary>
/// Contrato de persistência para HistoricoClinico.
/// </summary>
public interface IHistoricoClinicoRepository
{
    /// <summary>
    /// Lista paginada, filtrada e ordenada conforme <see cref="HistoricoClinicoQuery"/>.
    /// </summary>
    /// <returns>Itens da página solicitada e o total de itens que atendem aos filtros.</returns>
    (IReadOnlyList<HistoricoClinico> Items, int Total) GetPaged(HistoricoClinicoQuery query);

    HistoricoClinico? GetById(Guid id);

    IReadOnlyList<HistoricoClinico> GetByPetId(Guid petId);

    HistoricoClinico Add(HistoricoClinico historico);

    HistoricoClinico Update(HistoricoClinico historico);

    bool Delete(Guid id);

    bool ExistsById(Guid id);
}