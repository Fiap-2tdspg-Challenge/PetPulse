using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;


/// <summary>
/// Contrato de persistência para Pet.
/// </summary>
public interface IPetRepository
{
    IReadOnlyList<Pet> GetAll();

    /// <summary>
    /// Lista paginada, filtrada e ordenada conforme <see cref="PetQuery"/>.
    /// </summary>
    /// <returns>Itens da página solicitada e o total de itens que atendem aos filtros.</returns>
    (IReadOnlyList<Pet> Items, int Total) GetPaged(PetQuery query);

    Pet? GetById(Guid id);

    IReadOnlyList<Pet> GetByUsuarioId(Guid usuarioId);

    Pet Add(Pet pet);

    Pet Update(Pet pet);

    bool Delete(Guid id);

    bool ExistsById(Guid id);
}