using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.entites.Mongo;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para registrar uma leitura da coleira IoT.
/// </summary>
/// <remarks>
/// Os nomes dos campos são os mesmos do JSON que o firmware do ESP32 (PetPulse-Iot) já expõe em
/// <c>GET /api/dados</c>, então o payload da coleira pode ser enviado como está — campos extras,
/// como <c>pet</c>, são ignorados.
/// </remarks>
public record LeituraIotRequest(
    [Range(1, 400, ErrorMessage = "A frequência cardíaca deve estar entre 1 e 400 bpm")]
    int FrequenciaCardiaca,

    [Range(0, 100, ErrorMessage = "O nível de atividade deve estar entre 0 e 100")]
    int? NivelAtividade,

    [StringLength(30, ErrorMessage = "A descrição da atividade deve ter no máximo 30 caracteres")]
    string? DescAtividade,

    [Range(0d, 100d, ErrorMessage = "A aceleração total deve estar entre 0 e 100 m/s²")]
    double? AceleracaoTotal,

    [Range(1, 400, ErrorMessage = "A pressão sistólica deve estar entre 1 e 400 mmHg")]
    int? PressaoSistolica,

    [Range(1, 300, ErrorMessage = "A pressão diastólica deve estar entre 1 e 300 mmHg")]
    int? PressaoDiastolica,

    [Range(25d, 50d, ErrorMessage = "A temperatura deve estar entre 25 e 50 °C")]
    double? Temperatura,

    [Range(0, 100, ErrorMessage = "O score deve estar entre 0 e 100")]
    int? Score,

    [StringLength(20, ErrorMessage = "O estado deve ter no máximo 20 caracteres")]
    string? Estado,

    [StringLength(500, ErrorMessage = "O alerta deve ter no máximo 500 caracteres")]
    string? Alerta,

    long? Uptime,

    [StringLength(50, ErrorMessage = "O código do dispositivo deve ter no máximo 50 caracteres")]
    string? IdDispositivo,

    DateTime? DataLeitura
)
{
    /// <summary>
    /// Monta o documento do MongoDB, vinculado ao dispositivo (e ao pet) do Oracle.
    /// Sem <c>DataLeitura</c> informada, usa o momento do recebimento.
    /// </summary>
    public LeituraIotDocument ToDocument(DispositivoIot dispositivo)
    {
        ArgumentNullException.ThrowIfNull(dispositivo);

        var agora = DateTime.UtcNow;

        return new LeituraIotDocument
        {
            Id = Guid.NewGuid(),
            DispositivoId = dispositivo.Id,
            PetId = dispositivo.PetId,
            CodigoDispositivo = IdDispositivo,
            FrequenciaCardiaca = FrequenciaCardiaca,
            NivelAtividade = NivelAtividade,
            DescAtividade = DescAtividade,
            AceleracaoTotal = AceleracaoTotal,
            PressaoSistolica = PressaoSistolica,
            PressaoDiastolica = PressaoDiastolica,
            Temperatura = Temperatura,
            Score = Score,
            Estado = Estado,
            Alerta = Alerta,
            Uptime = Uptime,
            DataLeitura = DataLeitura?.ToUniversalTime() ?? agora,
            RegistradoEm = agora
        };
    }
}
