using PetPulse.Domain.Commom;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.entites;

public class AlertaInteligente : BaseEntity
{
    public Guid PetId { get; private set; }

    public Pet Pet { get; private set; } = null!;

    public TipoAlertaEnum TipoAlerta { get; private set; }

    public NivelRiscoEnum NivelRisco { get; private set; }

    public OrigemAlertaEnum OrigemAlerta { get; private set; }

    public string Mensagem { get; private set; }

    public string? Recomendacao { get; private set; }

    public DateTime DataGeracao { get; private set; }

    public StatusAlertaEnum Status { get; private set; }

    private AlertaInteligente()
    {
    }

    public AlertaInteligente(
        Guid petId,
        TipoAlertaEnum tipoAlerta,
        NivelRiscoEnum nivelRisco,
        OrigemAlertaEnum origemAlerta,
        string mensagem,
        string? recomendacao)
    {
        PetId = petId;
        TipoAlerta = tipoAlerta;
        NivelRisco = nivelRisco;
        OrigemAlerta = origemAlerta;
        AtualizarMensagem(mensagem);
        Recomendacao = recomendacao;
        DataGeracao = DateTime.UtcNow;
        Status = StatusAlertaEnum.Aberto;
    }
    
    
    public void Visualizar()
    {
        Status = StatusAlertaEnum.Visualizado;
    }

    public void Resolver()
    {
        Status = StatusAlertaEnum.Resolvido;
    }
    
    public void AtualizarRecomendacao(string? recomendacao)
    {
        Recomendacao = recomendacao;
    }
    
    public void AtualizarDados(
        TipoAlertaEnum tipoAlerta,
        NivelRiscoEnum nivelRisco,
        OrigemAlertaEnum origemAlerta,
        string mensagem,
        string? recomendacao)
    {
        TipoAlerta = tipoAlerta;
        NivelRisco = nivelRisco;
        OrigemAlerta = origemAlerta;
        AtualizarMensagem(mensagem);
        AtualizarRecomendacao(recomendacao);
    }
    
    public void AtualizarMensagem(string mensagem)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
            throw new Exception("Mensagem do alerta não pode ser vazia.");

        Mensagem = mensagem;
    }
}