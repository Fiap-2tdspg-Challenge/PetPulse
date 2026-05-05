using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pelas operações de pets na API.
/// Expõe endpoints para criar, listar, buscar, atualizar e remover pets.
/// </summary>
/// <remarks>
/// Base URL: /api/pet
/// Exemplo: http://localhost:5292/api/pet
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PetController : ControllerBase
{
    private readonly IPetRepository _petRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public PetController(
        IPetRepository petRepository,
        IUsuarioRepository usuarioRepository)
    {
        _petRepository = petRepository;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Lista todos os pets cadastrados.
    /// </summary>
    /// <returns>Lista de pets cadastrados.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var pets = _petRepository
            .GetAll()
            .Select(PetResponse.FromDomain)
            .ToList();

        return Ok(pets);
    }

    /// <summary>
    /// Busca um pet pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do pet.</param>
    /// <returns>Pet encontrado.</returns>
    /// <response code="200">Pet encontrado com sucesso.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var pet = _petRepository.GetById(id);

        if (pet is null)
            return NotFound("Pet não encontrado.");

        return Ok(PetResponse.FromDomain(pet));
    }

    /// <summary>
    /// Lista todos os pets vinculados a um usuário/tutor.
    /// </summary>
    /// <param name="usuarioId">Identificador único do usuário/tutor.</param>
    /// <returns>Lista de pets do usuário informado.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpGet("usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByUsuarioId(Guid usuarioId)
    {
        if (!_usuarioRepository.ExistsById(usuarioId))
            return NotFound("Usuário não encontrado.");

        var pets = _petRepository
            .GetByUsuarioId(usuarioId)
            .Select(PetResponse.FromDomain)
            .ToList();

        return Ok(pets);
    }

    /// <summary>
    /// Cria um novo pet.
    /// </summary>
    /// <param name="request">Dados necessários para criação do pet.</param>
    /// <returns>Pet criado.</returns>
    /// <response code="201">Pet criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Usuário informado não encontrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] PetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_usuarioRepository.ExistsById(request.UsuarioId))
            return NotFound("Usuário informado não existe.");

        var pet = request.ToDomain();

        _petRepository.Add(pet);

        var response = PetResponse.FromDomain(pet);

        return CreatedAtAction(
            nameof(GetById),
            new { id = pet.Id },
            response
        );
    }

    /// <summary>
    /// Atualiza os dados de um pet existente.
    /// </summary>
    /// <param name="id">Identificador único do pet.</param>
    /// <param name="request">Novos dados do pet.</param>
    /// <returns>Pet atualizado.</returns>
    /// <response code="200">Pet atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet ou usuário não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] PetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_usuarioRepository.ExistsById(request.UsuarioId))
            return NotFound("Usuário informado não existe.");

        var pet = _petRepository.GetById(id);

        if (pet is null)
            return NotFound("Pet não encontrado.");

        pet.AtualizarDados(
            request.Nome,
            request.Especie,
            request.Raca,
            request.DataNascimento,
            request.Peso,
            request.Sexo,
            request.Castrado,
            request.Porte
        );

        _petRepository.Update(pet);

        return Ok(PetResponse.FromDomain(pet));
    }

    /// <summary>
    /// Remove um pet pelo identificador único.
    /// </summary>
    /// <param name="id">Identificador único do pet.</param>
    /// <response code="204">Pet removido com sucesso.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var removed = _petRepository.Delete(id);

        if (!removed)
            return NotFound("Pet não encontrado.");

        return NoContent();
    }
}