using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de resposta de dispositivo IoT.
/// </summary>
public record DispositivoIotResponse(
    Guid Id,
    Guid PetId,
    DateOnly DataVinculacao,
    int? IntervaloColetaMinutos,
    int? FrequenciaCardiaca,
    decimal? NivelAtividade,
    decimal? Pressao,
    DateTime? DataUltimaLeitura,
    StatusDispositivoEnum Status,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Mapeia a entidade DispositivoIot para DTO.
    /// </summary>
    public static DispositivoIotResponse FromDomain(DispositivoIot dispositivo) =>
        new(
            dispositivo.Id,
            dispositivo.PetId,
            dispositivo.DataVinculacao,
            dispositivo.IntervaloColetaMinutos,
            dispositivo.FrequenciaCardiaca,
            dispositivo.NivelAtividade,
            dispositivo.Pressao,
            dispositivo.DataUltimaLeitura,
            dispositivo.Status,
            dispositivo.CreatedAt
        );
}