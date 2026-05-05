using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de dispositivos IoT vinculados aos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/dispositivoiot
/// Exemplo: http://localhost:5000/api/dispositivoiot
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
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
    /// Lista todos os dispositivos IoT cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DispositivoIotResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var dispositivos = _dispositivoIotRepository
            .GetAll()
            .Select(DispositivoIotResponse.FromDomain)
            .ToList();

        return Ok(dispositivos);
    }

    /// <summary>
    /// Busca um dispositivo IoT pelo identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DispositivoIotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var dispositivo = _dispositivoIotRepository.GetById(id);

        if (dispositivo is null)
            return NotFound("Dispositivo IoT não encontrado.");

        return Ok(DispositivoIotResponse.FromDomain(dispositivo));
    }

    /// <summary>
    /// Busca o dispositivo IoT vinculado a um pet específico.
    /// </summary>
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

        return Ok(DispositivoIotResponse.FromDomain(dispositivo));
    }

    /// <summary>
    /// Cria um novo dispositivo IoT vinculado a um pet.
    /// </summary>
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

        var response = DispositivoIotResponse.FromDomain(dispositivo);

        return CreatedAtAction(
            nameof(GetById),
            new { id = dispositivo.Id },
            response
        );
    }

    /// <summary>
    /// Atualiza um dispositivo IoT existente.
    /// </summary>
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

        return Ok(DispositivoIotResponse.FromDomain(dispositivo));
    }

    /// <summary>
    /// Remove um dispositivo IoT pelo identificador único.
    /// </summary>
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
}