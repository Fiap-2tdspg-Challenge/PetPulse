using PetPulse.Domain.Commom;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.entites;

public class DispositivoIot : BaseEntity
{
    public Guid PetId { get; private set; }

    public Pet Pet { get; private set; } = null!;

    public DateOnly DataVinculacao { get; private set; }

    public int? IntervaloColetaMinutos { get; private set; }

    public int? FrequenciaCardiaca { get; private set; }

    public decimal? NivelAtividade { get; private set; }

    public decimal? Pressao { get; private set; }

    public DateTime? DataUltimaLeitura { get; private set; }

    public StatusDispositivoEnum Status { get; private set; }

    private DispositivoIot()
    {
    }

    public DispositivoIot(
        Guid petId,
        DateOnly dataVinculacao,
        int? intervaloColetaMinutos,
        int? frequenciaCardiaca,
        decimal? nivelAtividade,
        decimal? pressao,
        DateTime? dataUltimaLeitura,
        StatusDispositivoEnum status)
    {
        PetId = petId;
        DataVinculacao = dataVinculacao;
        Status = status;

        AtualizarLeitura(
            intervaloColetaMinutos,
            frequenciaCardiaca,
            nivelAtividade,
            pressao,
            dataUltimaLeitura
        );
    }

    public void AtualizarLeitura(
        int? intervaloColetaMinutos,
        int? frequenciaCardiaca,
        decimal? nivelAtividade,
        decimal? pressao,
        DateTime? dataUltimaLeitura)
    {
        if (intervaloColetaMinutos.HasValue && intervaloColetaMinutos.Value <= 0)
            throw new Exception("Intervalo de coleta deve ser maior que zero.");

        if (frequenciaCardiaca.HasValue && frequenciaCardiaca.Value <= 0)
            throw new Exception("Frequência cardíaca deve ser maior que zero.");

        if (nivelAtividade.HasValue && nivelAtividade.Value < 0)
            throw new Exception("Nível de atividade não pode ser negativo.");

        if (pressao.HasValue && pressao.Value <= 0)
            throw new Exception("Pressão deve ser maior que zero.");

        IntervaloColetaMinutos = intervaloColetaMinutos;
        FrequenciaCardiaca = frequenciaCardiaca;
        NivelAtividade = nivelAtividade;
        Pressao = pressao;
        DataUltimaLeitura = dataUltimaLeitura;
    }

    public void AlterarStatus(StatusDispositivoEnum status)
    {
        Status = status;
    }
}