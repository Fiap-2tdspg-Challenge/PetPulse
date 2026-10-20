using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Hateoas;
using PetPulse.Domain.entites;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de histórico clínico dos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/historicoclinico
/// Exemplo: http://localhost:5292/api/historicoclinico
///
/// Todos os endpoints exigem um token JWT válido (ver <c>POST /api/auth/login</c>).
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class HistoricoClinicoController : ControllerBase
{
    private readonly IHistoricoClinicoRepository _historicoClinicoRepository;
    private readonly IPetRepository _petRepository;

    public HistoricoClinicoController(
        IHistoricoClinicoRepository historicoClinicoRepository,
        IPetRepository petRepository)
    {
        _historicoClinicoRepository = historicoClinicoRepository;
        _petRepository = petRepository;
    }

    /// <summary>
    /// Lista os registros de histórico clínico com paginação, ordenação e filtros.
    /// </summary>
    /// <remarks>
    /// Ordenação: sortBy = dataRegistro (padrão, decrescente), tipoRegistro ou dataCadastro; sortDirection = asc ou desc.
    /// Filtros: petId, tipoRegistro, dataInicio e dataFim (yyyy-MM-dd).
    /// Exemplo: GET /api/HistoricoClinico?petId={id}&amp;tipoRegistro=Vacina&amp;dataInicio=2026-01-01
    /// </remarks>
    /// <param name="query">Paginação (page, pageSize), ordenação (sortBy, sortDirection) e filtros.</param>
    /// <returns>Página de resultados com links HATEOAS de navegação.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="400">Parâmetros de paginação, ordenação ou filtro inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<HistoricoClinicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] HistoricoClinicoQuery query)
    {
        if (query.TryGetError(out var erro))
            return BadRequest(erro);

        var (historicos, total) = _historicoClinicoRepository.GetPaged(query);

        var pagina = new PagedResponse<HistoricoClinicoResponse>(
            historicos.Select(ComLinks).ToList(),
            query.Page,
            query.PageSize,
            total);

        return Ok(pagina with { Links = HateoasLinks.Paginacao(Rotas.HistoricoClinico, query, pagina) });
    }

    /// <summary>
    /// Busca um registro de histórico clínico pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do histórico clínico.</param>
    /// <returns>Histórico clínico encontrado.</returns>
    /// <response code="200">Histórico clínico encontrado com sucesso.</response>
    /// <response code="404">Histórico clínico não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoClinicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var historico = _historicoClinicoRepository.GetById(id);

        if (historico is null)
            return NotFound("Histórico clínico não encontrado.");

        return Ok(ComLinks(historico));
    }

    /// <summary>
    /// Lista os registros de histórico clínico de um pet específico.
    /// </summary>
    /// <param name="petId">Identificador único do pet.</param>
    /// <returns>Lista de históricos clínicos do pet.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoClinicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByPetId(Guid petId)
    {
        if (!_petRepository.ExistsById(petId))
            return NotFound("Pet não encontrado.");

        var historicos = _historicoClinicoRepository
            .GetByPetId(petId)
            .Select(ComLinks)
            .ToList();

        return Ok(historicos);
    }

   
    /// <summary>
    /// Cria um novo registro de histórico clínico para um pet.
    /// </summary>
    /// <param name="request">Dados necessários para criação do histórico clínico.</param>
    /// <returns>Histórico clínico criado.</returns>
    /// <response code="201">Histórico clínico criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet informado não encontrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(HistoricoClinicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] HistoricoClinicoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_petRepository.ExistsById(request.PetId))
            return NotFound("Pet informado não existe.");

        var historico = request.ToDomain();

        _historicoClinicoRepository.Add(historico);

        var response = ComLinks(historico);

        return CreatedAtAction(
            nameof(GetById),
            new { id = historico.Id },
            response
        );
    }

    
    /// <summary>
    /// Atualiza um registro de histórico clínico existente.
    /// </summary>
    /// <param name="id">Identificador único do histórico clínico.</param>
    /// <param name="request">Novos dados do histórico clínico.</param>
    /// <returns>Histórico clínico atualizado.</returns>
    /// <response code="200">Histórico clínico atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet ou histórico clínico não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoClinicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] HistoricoClinicoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_petRepository.ExistsById(request.PetId))
            return NotFound("Pet informado não existe.");

        var historico = _historicoClinicoRepository.GetById(id);

        if (historico is null)
            return NotFound("Histórico clínico não encontrado.");

        historico.AtualizarDados(
            request.TipoRegistro,
            request.Descricao,
            request.DataRegistro,
            request.DataRetorno,
            request.ProfissionalClinica,
            request.Observacoes
        );

        _historicoClinicoRepository.Update(historico);

        return Ok(ComLinks(historico));
    }

    /// <summary>
    /// Remove um registro de histórico clínico pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do histórico clínico.</param>
    /// <response code="204">Histórico clínico removido com sucesso.</response>
    /// <response code="404">Histórico clínico não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var removed = _historicoClinicoRepository.Delete(id);

        if (!removed)
            return NotFound("Histórico clínico não encontrado.");

        return NoContent();
    }

    /// <summary>
    /// Converte a entidade para DTO e anexa os links HATEOAS do recurso.
    /// </summary>
    private static HistoricoClinicoResponse ComLinks(HistoricoClinico historico) =>
        HistoricoClinicoResponse.FromDomain(historico) with { Links = HateoasLinks.HistoricoClinico(historico.Id, historico.PetId) };
}
