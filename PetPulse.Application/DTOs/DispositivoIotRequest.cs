using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para cadastro/atualização de dispositivo IoT.
/// </summary>
public record DispositivoIotRequest(
    [ Required(ErrorMessage = "O id do pet é obrigatório")]
    Guid PetId,

    [ Required(ErrorMessage = "A data de vinculação é obrigatória")]
    DateOnly DataVinculacao,

    [ Range(1, int.MaxValue, ErrorMessage = "O intervalo de coleta deve ser maior que zero")]
    int? IntervaloColetaMinutos,

    [ Range(1, int.MaxValue, ErrorMessage = "A frequência cardíaca deve ser maior que zero")]
    int? FrequenciaCardiaca,

    [ Range(0, 9999.99, ErrorMessage = "O nível de atividade não pode ser negativo")]
    decimal? NivelAtividade,

    [ Range(0.01, 9999.99, ErrorMessage = "A pressão deve ser maior que zero")]
    decimal? Pressao,

    DateTime? DataUltimaLeitura,

    [ Required(ErrorMessage = "O status do dispositivo é obrigatório")]
    StatusDispositivoEnum Status
)
{
    /// <summary>
    /// Constrói a entidade de domínio DispositivoIot.
    /// </summary>
    public DispositivoIot ToDomain() =>
        new(
            PetId,
            DataVinculacao,
            IntervaloColetaMinutos,
            FrequenciaCardiaca,
            NivelAtividade,
            Pressao,
            DataUltimaLeitura,
            Status
        );
}