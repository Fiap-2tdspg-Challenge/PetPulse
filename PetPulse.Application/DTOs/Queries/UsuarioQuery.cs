namespace PetPulse.Application.DTOs;

/// <summary>
/// Paginação, ordenação e filtros de <c>GET /api/Usuario</c>.
/// </summary>
public sealed class UsuarioQuery : PaginationQuery
{
    public static readonly IReadOnlyCollection<string> CamposPermitidos = ["nome", "email", "dataCadastro"];

    /// <summary>Filtra pelo nome (contém o texto, sem diferenciar maiúsculas).</summary>
    public string? Nome { get; set; }

    /// <summary>Filtra pelo e-mail (contém o texto, sem diferenciar maiúsculas).</summary>
    public string? Email { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => CamposPermitidos;

    protected override string OrdenacaoPadrao => "nome";

    protected override IEnumerable<KeyValuePair<string, string?>> Filtros()
    {
        yield return new("nome", Nome);
        yield return new("email", Email);
    }
}
