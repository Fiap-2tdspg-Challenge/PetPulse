using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// Paginação, ordenação e filtros de <c>GET /api/AlertaInteligente</c>.
/// Sem ordenação informada, lista do alerta mais recente para o mais antigo.
/// </summary>
public sealed class AlertaInteligenteQuery : PaginationQuery
{
    public static readonly IReadOnlyCollection<string> CamposPermitidos =
        ["dataGeracao", "nivelRisco", "status", "tipoAlerta"];

    /// <summary>Filtra pelos alertas de um pet.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Filtra pelo status (Aberto, Visualizado, Resolvido ou 1 a 3).</summary>
    public StatusAlertaEnum? Status { get; set; }

    /// <summary>Filtra pelo nível de risco (Baixo, Medio, Alto ou 1 a 3).</summary>
    public NivelRiscoEnum? NivelRisco { get; set; }

    /// <summary>Filtra pelo tipo (Atividade, Vacina, Medicamento, CheckUp, FrequenciaCardiaca, Pressao ou 1 a 6).</summary>
    public TipoAlertaEnum? TipoAlerta { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => CamposPermitidos;

    protected override string OrdenacaoPadrao => "dataGeracao";

    protected override bool DescendentePadrao => true;

    protected override IEnumerable<KeyValuePair<string, string?>> Filtros()
    {
        yield return new("petId", Formatar(PetId));
        yield return new("status", Formatar(Status));
        yield return new("nivelRisco", Formatar(NivelRisco));
        yield return new("tipoAlerta", Formatar(TipoAlerta));
    }
}
