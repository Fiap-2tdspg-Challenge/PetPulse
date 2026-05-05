using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;


/// <summary>
/// Contrato de persistência para HistoricoClinico.
/// </summary>
public interface IHistoricoClinicoRepository
{
    IReadOnlyList<HistoricoClinico> GetAll();

    HistoricoClinico? GetById(Guid id);

    IReadOnlyList<HistoricoClinico> GetByPetId(Guid petId);

    HistoricoClinico Add(HistoricoClinico historico);

    HistoricoClinico Update(HistoricoClinico historico);

    bool Delete(Guid id);

    bool ExistsById(Guid id);
}