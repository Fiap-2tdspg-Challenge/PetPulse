using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para criação de alerta inteligente.
/// </summary>
public record AlertaInteligenteRequest(
    [property: Required(ErrorMessage = "O id do pet é obrigatório")]
    Guid PetId,

    [property: Required(ErrorMessage = "O tipo do alerta é obrigatório")]
    TipoAlertaEnum TipoAlerta,

    [property: Required(ErrorMessage = "O nível de risco é obrigatório")]
    NivelRiscoEnum NivelRisco,

    [property: Required(ErrorMessage = "A origem do alerta é obrigatória")]
    OrigemAlertaEnum OrigemAlerta,

    [property: Required(ErrorMessage = "A mensagem é obrigatória")]
    [property: StringLength(500, MinimumLength = 3, ErrorMessage = "A mensagem deve ter entre 3 e 500 caracteres")]
    string Mensagem,

    [property: StringLength(1000, ErrorMessage = "A recomendação deve ter no máximo 1000 caracteres")]
    string? Recomendacao
)
{
    /// <summary>
    /// Constrói a entidade de domínio AlertaInteligente.
    /// </summary>
    public AlertaInteligente ToDomain() =>
        new(
            PetId,
            TipoAlerta,
            NivelRisco,
            OrigemAlerta,
            Mensagem,
            Recomendacao
        );
}