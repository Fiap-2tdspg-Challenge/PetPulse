using PetPulse.Application.DTOs;
using PetPulse.Domain.Enum;

namespace PetPulse.Hateoas;

/// <summary>
/// Rotas base dos recursos da API, usadas na montagem dos links HATEOAS.
/// </summary>
public static class Rotas
{
    public const string Usuario = "/api/Usuario";
    public const string Pet = "/api/Pet";
    public const string HistoricoClinico = "/api/HistoricoClinico";
    public const string DispositivoIot = "/api/DispositivoIot";
    public const string AlertaInteligente = "/api/AlertaInteligente";
}

/// <summary>
/// Monta os links HATEOAS dos recursos e das listagens paginadas.
/// </summary>
/// <remarks>
/// Os links são caminhos relativos à API (ex.: <c>/api/Pet/{id}</c>). Cada recurso informa o que
/// o cliente pode fazer a partir dele — consultar, atualizar, excluir e navegar para os recursos
/// relacionados — e as listagens informam como navegar entre as páginas. No alerta, os links de
/// ação dependem do estado: <c>visualizar</c> só aparece enquanto ele está aberto e
/// <c>resolver</c> só enquanto ainda não foi resolvido.
/// </remarks>
public static class HateoasLinks
{
    private const string Get = "GET";
    private const string Post = "POST";
    private const string Put = "PUT";
    private const string Delete = "DELETE";

    /// <summary>
    /// Links de navegação de uma listagem paginada: self, first, prev, next, last e create.
    /// </summary>
    public static IReadOnlyList<LinkDto> Paginacao<T>(string rota, PaginationQuery query, PagedResponse<T> pagina)
    {
        var links = new List<LinkDto>
        {
            new("self", rota + query.ToQueryString(pagina.Page), Get),
            new("first", rota + query.ToQueryString(1), Get)
        };

        if (pagina.HasPrevious)
            links.Add(new("prev", rota + query.ToQueryString(pagina.Page - 1), Get));

        if (pagina.HasNext)
            links.Add(new("next", rota + query.ToQueryString(pagina.Page + 1), Get));

        links.Add(new("last", rota + query.ToQueryString(Math.Max(pagina.TotalPages, 1)), Get));
        links.Add(new("create", rota, Post));

        return links;
    }

    public static IReadOnlyList<LinkDto> Usuario(Guid id) =>
    [
        .. Crud(Rotas.Usuario, id),
        new("pets", $"{Rotas.Pet}/usuario/{id}", Get)
    ];

    public static IReadOnlyList<LinkDto> Pet(Guid id, Guid usuarioId) =>
    [
        .. Crud(Rotas.Pet, id),
        new("usuario", $"{Rotas.Usuario}/{usuarioId}", Get),
        new("historicos-clinicos", $"{Rotas.HistoricoClinico}/pet/{id}", Get),
        new("alertas", $"{Rotas.AlertaInteligente}/pet/{id}", Get),
        new("dispositivo-iot", $"{Rotas.DispositivoIot}/pet/{id}", Get)
    ];

    public static IReadOnlyList<LinkDto> HistoricoClinico(Guid id, Guid petId) =>
    [
        .. Crud(Rotas.HistoricoClinico, id),
        new("pet", $"{Rotas.Pet}/{petId}", Get)
    ];

    public static IReadOnlyList<LinkDto> DispositivoIot(Guid id, Guid petId) =>
    [
        .. Crud(Rotas.DispositivoIot, id),
        new("pet", $"{Rotas.Pet}/{petId}", Get),
        new("leituras", $"{Rotas.DispositivoIot}/{id}/leituras", Get),
        new("registrar-leitura", $"{Rotas.DispositivoIot}/{id}/leituras", Post)
    ];

    public static IReadOnlyList<LinkDto> AlertaInteligente(Guid id, Guid petId, StatusAlertaEnum status)
    {
        var links = new List<LinkDto>(Crud(Rotas.AlertaInteligente, id))
        {
            new("pet", $"{Rotas.Pet}/{petId}", Get)
        };

        if (status == StatusAlertaEnum.Aberto)
            links.Add(new("visualizar", $"{Rotas.AlertaInteligente}/{id}/visualizar", Put));

        if (status != StatusAlertaEnum.Resolvido)
            links.Add(new("resolver", $"{Rotas.AlertaInteligente}/{id}/resolver", Put));

        return links;
    }

    private static IEnumerable<LinkDto> Crud(string rota, Guid id) =>
    [
        new("self", $"{rota}/{id}", Get),
        new("update", $"{rota}/{id}", Put),
        new("delete", $"{rota}/{id}", Delete)
    ];
}
