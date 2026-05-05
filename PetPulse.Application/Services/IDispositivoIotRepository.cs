using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato de persistência para DispositivoIot.
/// </summary>
public interface IDispositivoIotRepository
{
    IReadOnlyList<DispositivoIot> GetAll();

    DispositivoIot? GetById(Guid id);

    DispositivoIot? GetByPetId(Guid petId);

    DispositivoIot Add(DispositivoIot dispositivo);

    DispositivoIot Update(DispositivoIot dispositivo);

    bool Delete(Guid id);

    bool ExistsById(Guid id);

    bool ExistsByPetId(Guid petId);
}