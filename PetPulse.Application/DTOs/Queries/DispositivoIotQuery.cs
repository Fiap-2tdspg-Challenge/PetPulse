using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// Paginação, ordenação e filtros de <c>GET /api/DispositivoIot</c>.
/// Sem ordenação informada, lista da leitura mais recente para a mais antiga.
/// </summary>
public sealed class DispositivoIotQuery : PaginationQuery
{
    public static readonly IReadOnlyCollection<string> CamposPermitidos =
        ["dataUltimaLeitura", "dataVinculacao", "frequenciaCardiaca", "dataCadastro"];

    /// <summary>Filtra pelo dispositivo de um pet.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Filtra pelo status (Ativo, Inativo, Manutencao ou 1 a 3).</summary>
    public StatusDispositivoEnum? Status { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => CamposPermitidos;

    protected override string OrdenacaoPadrao => "dataUltimaLeitura";

    protected override bool DescendentePadrao => true;

    protected override IEnumerable<KeyValuePair<string, string?>> Filtros()
    {
        yield return new("petId", Formatar(PetId));
        yield return new("status", Formatar(Status));
    }
}
