using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// Paginação, ordenação e filtros de <c>GET /api/Pet</c>.
/// </summary>
public sealed class PetQuery : PaginationQuery
{
    public static readonly IReadOnlyCollection<string> CamposPermitidos =
        ["nome", "especie", "peso", "dataNascimento", "dataCadastro"];

    /// <summary>Filtra pelos pets de um usuário/tutor.</summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>Filtra pelo nome (contém o texto, sem diferenciar maiúsculas).</summary>
    public string? Nome { get; set; }

    /// <summary>Filtra pela espécie (texto exato, sem diferenciar maiúsculas). Ex.: Cachorro.</summary>
    public string? Especie { get; set; }

    /// <summary>Filtra pelo porte (Pequeno, Medio, Grande, NaoInformado ou 1 a 4).</summary>
    public PortePetEnum? Porte { get; set; }

    /// <summary>Filtra pelo sexo (Macho, Fêmea, NaoInformado ou 1 a 3).</summary>
    public SexoPetEnum? Sexo { get; set; }

    /// <summary>Filtra por castrado (true/false).</summary>
    public bool? Castrado { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => CamposPermitidos;

    protected override string OrdenacaoPadrao => "nome";

    protected override IEnumerable<KeyValuePair<string, string?>> Filtros()
    {
        yield return new("usuarioId", Formatar(UsuarioId));
        yield return new("nome", Nome);
        yield return new("especie", Especie);
        yield return new("porte", Formatar(Porte));
        yield return new("sexo", Formatar(Sexo));
        yield return new("castrado", Formatar(Castrado));
    }
}
