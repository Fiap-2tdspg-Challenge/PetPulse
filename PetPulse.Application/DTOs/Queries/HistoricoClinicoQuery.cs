using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// Paginação, ordenação e filtros de <c>GET /api/HistoricoClinico</c>.
/// Sem ordenação informada, lista do registro mais recente para o mais antigo.
/// </summary>
public sealed class HistoricoClinicoQuery : PaginationQuery
{
    public static readonly IReadOnlyCollection<string> CamposPermitidos =
        ["dataRegistro", "tipoRegistro", "dataCadastro"];

    /// <summary>Filtra pelos registros de um pet.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Filtra pelo tipo (Vacina, Consulta, Doenca, Medicamento, Observacao, Exame ou 1 a 6).</summary>
    public TipoRegistroClinicoEnum? TipoRegistro { get; set; }

    /// <summary>Registros com data a partir de (yyyy-MM-dd).</summary>
    public DateOnly? DataInicio { get; set; }

    /// <summary>Registros com data até (yyyy-MM-dd).</summary>
    public DateOnly? DataFim { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => CamposPermitidos;

    protected override string OrdenacaoPadrao => "dataRegistro";

    protected override bool DescendentePadrao => true;

    protected override bool TryGetFiltroError(out string? message)
    {
        if (DataInicio.HasValue && DataFim.HasValue && DataInicio.Value > DataFim.Value)
        {
            message = "dataInicio não pode ser posterior a dataFim.";
            return true;
        }

        message = null;
        return false;
    }

    protected override IEnumerable<KeyValuePair<string, string?>> Filtros()
    {
        yield return new("petId", Formatar(PetId));
        yield return new("tipoRegistro", Formatar(TipoRegistro));
        yield return new("dataInicio", Formatar(DataInicio));
        yield return new("dataFim", Formatar(DataFim));
    }
}
