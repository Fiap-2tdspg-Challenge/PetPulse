using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de dispositivos IoT vinculados aos pets.
/// </summary>
/// <remarks>
/// Base URL: /api/dispositivoiot
/// Exemplo: http://localhost:5292/api/dispositivoiot
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
    /// Inicializa o controller com os repositórios necessários.
    /// </summary>
    /// <param name="dispositivoIotRepository">Repositório de dispositivos IoT.</param>
    /// <param name="petRepository">Repositório de pets.</param>
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

        return Ok(DispositivoIotResponse.FromDomain(dispositivo));
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

        return Ok(DispositivoIotResponse.FromDomain(dispositivo));
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

        return Ok(DispositivoIotResponse.FromDomain(dispositivo));
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
}