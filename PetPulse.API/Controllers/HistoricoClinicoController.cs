using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de histórico clínico dos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/historicoclinico
/// Exemplo: http://localhost:5000/api/historicoclinico
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
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
    /// Lista todos os registros de histórico clínico.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoClinicoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var historicos = _historicoClinicoRepository
            .GetAll()
            .Select(HistoricoClinicoResponse.FromDomain)
            .ToList();

        return Ok(historicos);
    }

    /// <summary>
    /// Busca um histórico clínico pelo identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HistoricoClinicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var historico = _historicoClinicoRepository.GetById(id);

        if (historico is null)
            return NotFound("Histórico clínico não encontrado.");

        return Ok(HistoricoClinicoResponse.FromDomain(historico));
    }

    /// <summary>
    /// Lista os registros clínicos de um pet específico.
    /// </summary>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoClinicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByPetId(Guid petId)
    {
        if (!_petRepository.ExistsById(petId))
            return NotFound("Pet não encontrado.");

        var historicos = _historicoClinicoRepository
            .GetByPetId(petId)
            .Select(HistoricoClinicoResponse.FromDomain)
            .ToList();

        return Ok(historicos);
    }

    /// <summary>
    /// Cria um novo registro de histórico clínico.
    /// </summary>
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

        var response = HistoricoClinicoResponse.FromDomain(historico);

        return CreatedAtAction(
            nameof(GetById),
            new { id = historico.Id },
            response
        );
    }

    /// <summary>
    /// Atualiza um registro de histórico clínico existente.
    /// </summary>
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

        return Ok(HistoricoClinicoResponse.FromDomain(historico));
    }

    /// <summary>
    /// Remove um registro de histórico clínico pelo identificador único.
    /// </summary>
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
}