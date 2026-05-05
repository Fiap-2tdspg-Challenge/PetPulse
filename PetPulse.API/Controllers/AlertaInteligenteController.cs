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
/// Exemplo: http://localhost:5000/api/alertainteligente
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AlertaInteligenteController : ControllerBase
{
    private readonly IAlertaInteligenteRepository _alertaInteligenteRepository;
    private readonly IPetRepository _petRepository;

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
    /// Lista os alertas inteligentes de um pet específico.
    /// </summary>
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