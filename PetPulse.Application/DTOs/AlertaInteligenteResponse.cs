using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;


/// <summary>
/// DTO de resposta de alerta inteligente.
/// </summary>
public record AlertaInteligenteResponse(
    Guid Id,
    Guid PetId,
    TipoAlertaEnum TipoAlerta,
    NivelRiscoEnum NivelRisco,
    OrigemAlertaEnum OrigemAlerta,
    string Mensagem,
    string? Recomendacao,
    DateTime DataGeracao,
    StatusAlertaEnum Status,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Mapeia a entidade AlertaInteligente para DTO.
    /// </summary>
    public static AlertaInteligenteResponse FromDomain(AlertaInteligente alerta) =>
        new(
            alerta.Id,
            alerta.PetId,
            alerta.TipoAlerta,
            alerta.NivelRisco,
            alerta.OrigemAlerta,
            alerta.Mensagem,
            alerta.Recomendacao,
            alerta.DataGeracao,
            alerta.Status,
            alerta.CreatedAt
        );
}