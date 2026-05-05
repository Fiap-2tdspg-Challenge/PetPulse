using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para cadastro de pet.
/// </summary>
public record PetRequest(
    [property: Required(ErrorMessage = "O id do usuário é obrigatório")]
    Guid UsuarioId,

    [property: Required(ErrorMessage = "O nome do pet é obrigatório")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    string Nome,

    [property: Required(ErrorMessage = "A espécie é obrigatória")]
    [property: StringLength(50, MinimumLength = 2, ErrorMessage = "A espécie deve ter entre 2 e 50 caracteres")]
    string Especie,

    [property: StringLength(100, ErrorMessage = "A raça deve ter no máximo 100 caracteres")]
    string? Raca,

    DateOnly? DataNascimento,

    [property: Range(0.01, 9999.99, ErrorMessage = "O peso deve ser maior que zero")]
    decimal? Peso,

    [property: Required(ErrorMessage = "O sexo é obrigatório")]
    SexoPetEnum Sexo,

    bool Castrado,

    [property: Required(ErrorMessage = "O porte é obrigatório")]
    PortePetEnum Porte
)
{
    /// <summary>
    /// Constrói a entidade de domínio Pet.
    /// </summary>
    public Pet ToDomain() =>
        new(
            UsuarioId,
            Nome,
            Especie,
            Raca,
            DataNascimento,
            Peso,
            Sexo,
            Castrado,
            Porte
        );
}