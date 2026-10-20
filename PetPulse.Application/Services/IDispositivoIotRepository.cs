using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato de persistência para DispositivoIot.
/// </summary>
public interface IDispositivoIotRepository
{
    /// <summary>
    /// Lista paginada, filtrada e ordenada conforme <see cref="DispositivoIotQuery"/>.
    /// </summary>
    /// <returns>Itens da página solicitada e o total de itens que atendem aos filtros.</returns>
    (IReadOnlyList<DispositivoIot> Items, int Total) GetPaged(DispositivoIotQuery query);

    DispositivoIot? GetById(Guid id);

    DispositivoIot? GetByPetId(Guid petId);

    DispositivoIot Add(DispositivoIot dispositivo);

    DispositivoIot Update(DispositivoIot dispositivo);

    bool Delete(Guid id);

    bool ExistsById(Guid id);

    bool ExistsByPetId(Guid petId);
}