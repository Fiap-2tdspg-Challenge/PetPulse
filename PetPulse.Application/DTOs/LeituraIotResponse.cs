using PetPulse.Domain.entites.Mongo;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de resposta para uma leitura da coleira IoT (histórico no MongoDB).
/// </summary>
public record LeituraIotResponse(
    Guid Id,
    Guid DispositivoId,
    Guid PetId,
    string? CodigoDispositivo,
    int FrequenciaCardiaca,
    int? NivelAtividade,
    string? DescAtividade,
    double? AceleracaoTotal,
    int? PressaoSistolica,
    int? PressaoDiastolica,
    double? Temperatura,
    int? Score,
    string? Estado,
    string? Alerta,
    long? Uptime,
    DateTime DataLeitura,
    DateTime RegistradoEm
)
{
    /// <summary>
    /// Mapeia o documento do MongoDB para DTO.
    /// </summary>
    public static LeituraIotResponse FromDocument(LeituraIotDocument leitura) =>
        new(
            leitura.Id,
            leitura.DispositivoId,
            leitura.PetId,
            leitura.CodigoDispositivo,
            leitura.FrequenciaCardiaca,
            leitura.NivelAtividade,
            leitura.DescAtividade,
            leitura.AceleracaoTotal,
            leitura.PressaoSistolica,
            leitura.PressaoDiastolica,
            leitura.Temperatura,
            leitura.Score,
            leitura.Estado,
            leitura.Alerta,
            leitura.Uptime,
            leitura.DataLeitura,
            leitura.RegistradoEm
        );
}
