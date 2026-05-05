using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de resposta de pet.
/// </summary>
public record PetResponse(
    Guid Id,
    Guid UsuarioId,
    string Nome,
    string Especie,
    string? Raca,
    DateOnly? DataNascimento,
    decimal? Peso,
    SexoPetEnum Sexo,
    bool Castrado,
    PortePetEnum Porte,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Mapeia a entidade Pet para DTO.
    /// </summary>
    public static PetResponse FromDomain(Pet pet) =>
        new(
            pet.Id,
            pet.UsuarioId,
            pet.Nome,
            pet.Especie,
            pet.Raca,
            pet.DataNascimento,
            pet.Peso,
            pet.Sexo,
            pet.Castrado,
            pet.Porte,
            pet.CreatedAt
        );
}