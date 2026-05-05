using PetPulse.Domain.Commom;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.entites;

public class HistoricoClinico : BaseEntity
{
    public Guid PetId { get; private set; }

    public Pet Pet { get; private set; } = null!;

    public TipoRegistroClinicoEnum TipoRegistro { get; private set; }

    public string Descricao { get; private set; }

    public DateOnly DataRegistro { get; private set; }

    public DateOnly? DataRetorno { get; private set; }

    public string? ProfissionalClinica { get; private set; }

    public string? Observacoes { get; private set; }

    private HistoricoClinico()
    {
    }

    public HistoricoClinico(
        Guid petId,
        TipoRegistroClinicoEnum tipoRegistro,
        string descricao,
        DateOnly dataRegistro,
        DateOnly? dataRetorno,
        string? profissionalClinica,
        string? observacoes)
    {
        PetId = petId;
        TipoRegistro = tipoRegistro;
        AtualizarDescricao(descricao);
        DataRegistro = dataRegistro;
        DataRetorno = dataRetorno;
        ProfissionalClinica = profissionalClinica;
        Observacoes = observacoes;
    }

    public void AtualizarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new Exception("Descrição do histórico clínico não pode ser vazia.");

        Descricao = descricao;
    }

    public void AtualizarDados(
        TipoRegistroClinicoEnum tipoRegistro,
        string descricao,
        DateOnly dataRegistro,
        DateOnly? dataRetorno,
        string? profissionalClinica,
        string? observacoes)
    {
        TipoRegistro = tipoRegistro;
        AtualizarDescricao(descricao);
        DataRegistro = dataRegistro;
        DataRetorno = dataRetorno;
        ProfissionalClinica = profissionalClinica;
        Observacoes = observacoes;
    }
}