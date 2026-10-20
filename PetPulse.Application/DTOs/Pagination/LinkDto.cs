namespace PetPulse.Application.DTOs;

/// <summary>
/// Link HATEOAS: indica ao cliente o que ele pode fazer a partir de um recurso.
/// </summary>
/// <param name="Rel">Relação do link com o recurso (ex.: self, update, delete, next).</param>
/// <param name="Href">Caminho do endpoint.</param>
/// <param name="Method">Método HTTP a usar no endpoint.</param>
public record LinkDto(string Rel, string Href, string Method);
