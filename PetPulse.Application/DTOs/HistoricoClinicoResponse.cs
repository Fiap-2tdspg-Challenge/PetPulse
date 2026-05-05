using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de resposta de histórico clínico.
/// </summary>
public record HistoricoClinicoResponse(
    Guid Id,
    Guid PetId,
    TipoRegistroClinicoEnum TipoRegistro,
    string Descricao,
    DateOnly DataRegistro,
    DateOnly? DataRetorno,
    string? ProfissionalClinica,
    string? Observacoes,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Mapeia a entidade HistoricoClinico para DTO.
    /// </summary>
    public static HistoricoClinicoResponse FromDomain(HistoricoClinico historico) =>
        new(
            historico.Id,
            historico.PetId,
            historico.TipoRegistro,
            historico.Descricao,
            historico.DataRegistro,
            historico.DataRetorno,
            historico.ProfissionalClinica,
            historico.Observacoes,
            historico.CreatedAt
        );
}