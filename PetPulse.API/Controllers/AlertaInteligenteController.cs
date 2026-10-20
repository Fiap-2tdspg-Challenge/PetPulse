using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Hateoas;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de alertas inteligentes dos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/alertainteligente
/// Exemplo: http://localhost:5292/api/alertainteligente
///
/// Todos os endpoints exigem um token JWT válido (ver <c>POST /api/auth/login</c>).
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class AlertaInteligenteController : ControllerBase
{
    private readonly IAlertaInteligenteRepository _alertaInteligenteRepository;
    private readonly IPetRepository _petRepository;
    
    
    /// <summary>
    /// Inicializa o controller com os repositórios necessários.
    /// </summary>
    /// <param name="alertaInteligenteRepository">Repositório de alertas inteligentes.</param>
    /// <param name="petRepository">Repositório de pets.</param>
    public AlertaInteligenteController(
        IAlertaInteligenteRepository alertaInteligenteRepository,
        IPetRepository petRepository)
    {
        _alertaInteligenteRepository = alertaInteligenteRepository;
        _petRepository = petRepository;
    }

    
    /// <summary>
    /// Lista os alertas inteligentes com paginação, ordenação e filtros.
    /// </summary>
    /// <remarks>
    /// Ordenação: sortBy = dataGeracao (padrão, decrescente), nivelRisco, status ou tipoAlerta; sortDirection = asc ou desc.
    /// Filtros: petId, status, nivelRisco e tipoAlerta.
    /// Exemplo: GET /api/AlertaInteligente?status=Aberto&amp;nivelRisco=Alto
    /// </remarks>
    /// <param name="query">Paginação (page, pageSize), ordenação (sortBy, sortDirection) e filtros.</param>
    /// <returns>Página de resultados com links HATEOAS de navegação.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="400">Parâmetros de paginação, ordenação ou filtro inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AlertaInteligenteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] AlertaInteligenteQuery query)
    {
        if (query.TryGetError(out var erro))
            return BadRequest(erro);

        var (alertas, total) = _alertaInteligenteRepository.GetPaged(query);

        var pagina = new PagedResponse<AlertaInteligenteResponse>(
            alertas.Select(ComLinks).ToList(),
            query.Page,
            query.PageSize,
            total);

        return Ok(pagina with { Links = HateoasLinks.Paginacao(Rotas.AlertaInteligente, query, pagina) });
    }

    /// <summary>
    /// Busca um alerta inteligente pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do alerta inteligente.</param>
    /// <returns>Alerta inteligente encontrado.</returns>
    /// <response code="200">Alerta inteligente encontrado com sucesso.</response>
    /// <response code="404">Alerta inteligente não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlertaInteligenteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var alerta = _alertaInteligenteRepository.GetById(id);

        if (alerta is null)
            return NotFound("Alerta inteligente não encontrado.");

        return Ok(ComLinks(alerta));
    }

    /// <summary>
    /// Lista os alertas inteligentes vinculados a um pet específico.
    /// </summary>
    /// <param name="petId">Identificador único do pet.</param>
    /// <returns>Lista de alertas inteligentes do pet.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertaInteligenteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByPetId(Guid petId)
    {
        if (!_petRepository.ExistsById(petId))
            return NotFound("Pet não encontrado.");

        var alertas = _alertaInteligenteRepository
            .GetByPetId(petId)
            .Select(ComLinks)
            .ToList();

        return Ok(alertas);
    }
    
    /// <summary>
    /// Lista os alertas inteligentes por status.
    /// </summary>
    /// <param name="status">Status do alerta inteligente.</param>
    /// <returns>Lista de alertas com o status informado.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertaInteligenteResponse>), StatusCodes.Status200OK)]
    public IActionResult GetByStatus(StatusAlertaEnum status)
    {
        var alertas = _alertaInteligenteRepository
            .GetByStatus(status)
            .Select(ComLinks)
            .ToList();

        return Ok(alertas);
    }

    /// <summary>
    /// Cria um novo alerta inteligente.
    /// </summary>
    /// <param name="request">Dados necessários para criação do alerta inteligente.</param>
    /// <returns>Alerta inteligente criado.</returns>
    /// <response code="201">Alerta inteligente criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet informado não encontrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AlertaInteligenteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] AlertaInteligenteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_petRepository.ExistsById(request.PetId))
            return NotFound("Pet informado não existe.");

        var alerta = request.ToDomain();

        _alertaInteligenteRepository.Add(alerta);

        var response = ComLinks(alerta);

        return CreatedAtAction(
            nameof(GetById),
            new { id = alerta.Id },
            response
        );
    }

    /// <summary>
    /// Atualiza os dados principais de um alerta inteligente.
    /// </summary>
    /// <param name="id">Identificador único do alerta inteligente.</param>
    /// <param name="request">Novos dados do alerta inteligente.</param>
    /// <returns>Alerta inteligente atualizado.</returns>
    /// <response code="200">Alerta inteligente atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet ou alerta inteligente não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AlertaInteligenteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] AlertaInteligenteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_petRepository.ExistsById(request.PetId))
            return NotFound("Pet informado não existe.");

        var alerta = _alertaInteligenteRepository.GetById(id);

        if (alerta is null)
            return NotFound("Alerta inteligente não encontrado.");

        alerta.AtualizarDados(
            request.TipoAlerta,
            request.NivelRisco,
            request.OrigemAlerta,
            request.Mensagem,
            request.Recomendacao
        );

        _alertaInteligenteRepository.Update(alerta);

        return Ok(ComLinks(alerta));
    }

    /// <summary>
    /// Marca um alerta inteligente como visualizado.
    /// </summary>
    /// <param name="id">Identificador único do alerta inteligente.</param>
    /// <returns>Alerta inteligente atualizado.</returns>
    /// <response code="200">Alerta marcado como visualizado.</response>
    /// <response code="404">Alerta inteligente não encontrado.</response>
    [HttpPut("{id:guid}/visualizar")]
    [ProducesResponseType(typeof(AlertaInteligenteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Visualizar(Guid id)
    {
        var alerta = _alertaInteligenteRepository.GetById(id);

        if (alerta is null)
            return NotFound("Alerta inteligente não encontrado.");

        alerta.Visualizar();

        _alertaInteligenteRepository.Update(alerta);

        return Ok(ComLinks(alerta));
    }

    /// <summary>
    /// Marca um alerta inteligente como resolvido.
    /// </summary>
    /// <param name="id">Identificador único do alerta inteligente.</param>
    /// <returns>Alerta inteligente atualizado.</returns>
    /// <response code="200">Alerta marcado como resolvido.</response>
    /// <response code="404">Alerta inteligente não encontrado.</response>
    [HttpPut("{id:guid}/resolver")]
    [ProducesResponseType(typeof(AlertaInteligenteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Resolver(Guid id)
    {
        var alerta = _alertaInteligenteRepository.GetById(id);

        if (alerta is null)
            return NotFound("Alerta inteligente não encontrado.");

        alerta.Resolver();

        _alertaInteligenteRepository.Update(alerta);

        return Ok(ComLinks(alerta));
    }

    /// <summary>
    /// Remove um alerta inteligente pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do alerta inteligente.</param>
    /// <response code="204">Alerta inteligente removido com sucesso.</response>
    /// <response code="404">Alerta inteligente não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var removed = _alertaInteligenteRepository.Delete(id);

        if (!removed)
            return NotFound("Alerta inteligente não encontrado.");

        return NoContent();
    }

    /// <summary>
    /// Converte a entidade para DTO e anexa os links HATEOAS do recurso.
    /// </summary>
    private static AlertaInteligenteResponse ComLinks(AlertaInteligente alerta) =>
        AlertaInteligenteResponse.FromDomain(alerta) with { Links = HateoasLinks.AlertaInteligente(alerta.Id, alerta.PetId, alerta.Status) };
}
