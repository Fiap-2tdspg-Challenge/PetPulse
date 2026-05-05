using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para cadastro de pet.
/// </summary>
public record PetRequest(
    [Required(ErrorMessage = "O id do usuário é obrigatório")]
    Guid UsuarioId,

    [Required(ErrorMessage = "O nome do pet é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    string Nome,

    [Required(ErrorMessage = "A espécie é obrigatória")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "A espécie deve ter entre 2 e 50 caracteres")]
    string Especie,

    [StringLength(100, ErrorMessage = "A raça deve ter no máximo 100 caracteres")]
    string? Raca,

    DateOnly? DataNascimento,

    [Range(0.01, 9999.99, ErrorMessage = "O peso deve ser maior que zero")]
    decimal? Peso,

    [Required(ErrorMessage = "O sexo é obrigatório")]
    SexoPetEnum Sexo,

    bool Castrado,

    [Required(ErrorMessage = "O porte é obrigatório")]
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