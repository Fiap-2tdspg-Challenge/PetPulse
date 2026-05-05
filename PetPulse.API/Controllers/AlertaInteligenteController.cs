using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Domain.Enum;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de alertas inteligentes dos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/alertainteligente
/// Exemplo: http://localhost:5292/api/alertainteligente
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
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
    /// Lista todos os alertas inteligentes cadastrados.
    /// </summary>
    /// <returns>Lista de alertas inteligentes.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AlertaInteligenteResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var alertas = _alertaInteligenteRepository
            .GetAll()
            .Select(AlertaInteligenteResponse.FromDomain)
            .ToList();

        return Ok(alertas);
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

        return Ok(AlertaInteligenteResponse.FromDomain(alerta));
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
            .Select(AlertaInteligenteResponse.FromDomain)
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
            .Select(AlertaInteligenteResponse.FromDomain)
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

        var response = AlertaInteligenteResponse.FromDomain(alerta);

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

        return Ok(AlertaInteligenteResponse.FromDomain(alerta));
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

        return Ok(AlertaInteligenteResponse.FromDomain(alerta));
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

        return Ok(AlertaInteligenteResponse.FromDomain(alerta));
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
}