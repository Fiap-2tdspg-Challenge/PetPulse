using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;


/// <summary>
/// Contrato de persistência para Pet.
/// </summary>
public interface IPetRepository
{
    IReadOnlyList<Pet> GetAll();

    Pet? GetById(Guid id);

    IReadOnlyList<Pet> GetByUsuarioId(Guid usuarioId);

    Pet Add(Pet pet);

    Pet Update(Pet pet);

    bool Delete(Guid id);

    bool ExistsById(Guid id);
}