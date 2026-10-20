using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Hateoas;
using PetPulse.Domain.entites;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de dispositivos IoT vinculados aos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/dispositivoiot
/// Exemplo: http://localhost:5292/api/dispositivoiot
///
/// Todos os endpoints exigem um token JWT válido (ver <c>POST /api/auth/login</c>).
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class DispositivoIotController : ControllerBase
{
    private readonly IDispositivoIotRepository _dispositivoIotRepository;
    private readonly IPetRepository _petRepository;

    public DispositivoIotController(
        IDispositivoIotRepository dispositivoIotRepository,
        IPetRepository petRepository)
    {
        _dispositivoIotRepository = dispositivoIotRepository;
        _petRepository = petRepository;
    }

    
    /// <summary>
    /// Lista os dispositivos IoT com paginação, ordenação e filtros.
    /// </summary>
    /// <remarks>
    /// Ordenação: sortBy = dataUltimaLeitura (padrão, decrescente), dataVinculacao, frequenciaCardiaca ou dataCadastro; sortDirection = asc ou desc.
    /// Filtros: petId e status.
    /// Exemplo: GET /api/DispositivoIot?status=Ativo&amp;sortBy=frequenciaCardiaca&amp;sortDirection=desc
    /// </remarks>
    /// <param name="query">Paginação (page, pageSize), ordenação (sortBy, sortDirection) e filtros.</param>
    /// <returns>Página de resultados com links HATEOAS de navegação.</returns>
    /// <response code="200">Página retornada com sucesso.</response>
    /// <response code="400">Parâmetros de paginação, ordenação ou filtro inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DispositivoIotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAll([FromQuery] DispositivoIotQuery query)
    {
        if (query.TryGetError(out var erro))
            return BadRequest(erro);

        var (dispositivos, total) = _dispositivoIotRepository.GetPaged(query);

        var pagina = new PagedResponse<DispositivoIotResponse>(
            dispositivos.Select(ComLinks).ToList(),
            query.Page,
            query.PageSize,
            total);

        return Ok(pagina with { Links = HateoasLinks.Paginacao(Rotas.DispositivoIot, query, pagina) });
    }

 
    /// <summary>
    /// Busca um dispositivo IoT pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do dispositivo IoT.</param>
    /// <returns>Dispositivo IoT encontrado.</returns>
    /// <response code="200">Dispositivo IoT encontrado com sucesso.</response>
    /// <response code="404">Dispositivo IoT não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DispositivoIotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var dispositivo = _dispositivoIotRepository.GetById(id);

        if (dispositivo is null)
            return NotFound("Dispositivo IoT não encontrado.");

        return Ok(ComLinks(dispositivo));
    }

    /// <summary>
    /// Busca o dispositivo IoT vinculado a um pet específico.
    /// </summary>
    /// <param name="petId">Identificador único do pet.</param>
    /// <returns>Dispositivo IoT vinculado ao pet.</returns>
    /// <response code="200">Dispositivo IoT encontrado com sucesso.</response>
    /// <response code="404">Pet ou dispositivo não encontrado.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(DispositivoIotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByPetId(Guid petId)
    {
        if (!_petRepository.ExistsById(petId))
            return NotFound("Pet não encontrado.");

        var dispositivo = _dispositivoIotRepository.GetByPetId(petId);

        if (dispositivo is null)
            return NotFound("Nenhum dispositivo IoT vinculado a este pet.");

        return Ok(ComLinks(dispositivo));
    }

   
    /// <summary>
    /// Cria um novo dispositivo IoT vinculado a um pet.
    /// </summary>
    /// <param name="request">Dados necessários para criação do dispositivo IoT.</param>
    /// <returns>Dispositivo IoT criado.</returns>
    /// <response code="201">Dispositivo IoT criado com sucesso.</response>
    /// <response code="400">Dados inválidos ou pet já possui dispositivo.</response>
    /// <response code="404">Pet informado não encontrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DispositivoIotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] DispositivoIotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_petRepository.ExistsById(request.PetId))
            return NotFound("Pet informado não existe.");

        if (_dispositivoIotRepository.ExistsByPetId(request.PetId))
            return BadRequest("Este pet já possui um dispositivo IoT vinculado.");

        var dispositivo = request.ToDomain();

        _dispositivoIotRepository.Add(dispositivo);

        var response = ComLinks(dispositivo);

        return CreatedAtAction(
            nameof(GetById),
            new { id = dispositivo.Id },
            response
        );
    }

    /// <summary>
    /// Atualiza um dispositivo IoT existente.
    /// </summary>
    /// <param name="id">Identificador único do dispositivo IoT.</param>
    /// <param name="request">Novos dados do dispositivo IoT.</param>
    /// <returns>Dispositivo IoT atualizado.</returns>
    /// <response code="200">Dispositivo IoT atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet ou dispositivo IoT não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DispositivoIotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] DispositivoIotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_petRepository.ExistsById(request.PetId))
            return NotFound("Pet informado não existe.");

        var dispositivo = _dispositivoIotRepository.GetById(id);

        if (dispositivo is null)
            return NotFound("Dispositivo IoT não encontrado.");

        dispositivo.AtualizarLeitura(
            request.IntervaloColetaMinutos,
            request.FrequenciaCardiaca,
            request.NivelAtividade,
            request.Pressao,
            request.DataUltimaLeitura
        );

        dispositivo.AlterarStatus(request.Status);

        _dispositivoIotRepository.Update(dispositivo);

        return Ok(ComLinks(dispositivo));
    }

    /// <summary>
    /// Remove um dispositivo IoT pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do dispositivo IoT.</param>
    /// <response code="204">Dispositivo IoT removido com sucesso.</response>
    /// <response code="404">Dispositivo IoT não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var removed = _dispositivoIotRepository.Delete(id);

        if (!removed)
            return NotFound("Dispositivo IoT não encontrado.");

        return NoContent();
    }

    /// <summary>
    /// Converte a entidade para DTO e anexa os links HATEOAS do recurso.
    /// </summary>
    private static DispositivoIotResponse ComLinks(DispositivoIot dispositivo) =>
        DispositivoIotResponse.FromDomain(dispositivo) with { Links = HateoasLinks.DispositivoIot(dispositivo.Id, dispositivo.PetId) };
}
