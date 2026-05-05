using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para criação de alerta inteligente.
/// </summary>
public record AlertaInteligenteRequest(
    [ Required(ErrorMessage = "O id do pet é obrigatório")]
    Guid PetId,

    [ Required(ErrorMessage = "O tipo do alerta é obrigatório")]
    TipoAlertaEnum TipoAlerta,

    [ Required(ErrorMessage = "O nível de risco é obrigatório")]
    NivelRiscoEnum NivelRisco,

    [ Required(ErrorMessage = "A origem do alerta é obrigatória")]
    OrigemAlertaEnum OrigemAlerta,

    [ Required(ErrorMessage = "A mensagem é obrigatória")]
    [ StringLength(500, MinimumLength = 3, ErrorMessage = "A mensagem deve ter entre 3 e 500 caracteres")]
    string Mensagem,

    [ StringLength(1000, ErrorMessage = "A recomendação deve ter no máximo 1000 caracteres")]
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