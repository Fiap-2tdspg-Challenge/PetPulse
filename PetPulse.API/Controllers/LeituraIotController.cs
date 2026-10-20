using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Domain.Enum;

namespace PetPulse.Controllers;

/// <summary>
/// Controller do histórico de leituras da coleira IoT, armazenado no MongoDB.
/// </summary>
/// <remarks>
/// Base URL: /api/dispositivoiot/{dispositivoId}/leituras
/// Exemplo: http://localhost:5292/api/dispositivoiot/{dispositivoId}/leituras
///
/// Cada leitura é gravada no MongoDB (histórico completo) e também atualiza a última leitura do
/// dispositivo no Oracle (estado atual). Todos os endpoints exigem um token JWT válido.
/// </remarks>
[Route("api/dispositivoiot/{dispositivoId:guid}/leituras")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class LeituraIotController : ControllerBase
{
    public const int LimitePadrao = 50;
    public const int LimiteMaximo = 500;

    private readonly ILeituraIotRepository _leituraIotRepository;
    private readonly IDispositivoIotRepository _dispositivoIotRepository;

    /// <summary>
    /// Inicializa o controller com os repositórios necessários.
    /// </summary>
    /// <param name="leituraIotRepository">Repositório MongoDB do histórico de leituras.</param>
    /// <param name="dispositivoIotRepository">Repositório Oracle de dispositivos IoT.</param>
    public LeituraIotController(
        ILeituraIotRepository leituraIotRepository,
        IDispositivoIotRepository dispositivoIotRepository)
    {
        _leituraIotRepository = leituraIotRepository;
        _dispositivoIotRepository = dispositivoIotRepository;
    }

    /// <summary>
    /// Lista as leituras mais recentes de um dispositivo, da mais nova para a mais antiga.
    /// </summary>
    /// <param name="dispositivoId">Identificador único do dispositivo IoT.</param>
    /// <param name="limite">Quantidade máxima de leituras retornadas (1 a 500, padrão 50).</param>
    /// <returns>Histórico de leituras do dispositivo.</returns>
    /// <response code="200">Lista retornada com sucesso.</response>
    /// <response code="400">Limite fora do intervalo permitido.</response>
    /// <response code="404">Dispositivo IoT não encontrado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LeituraIotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetByDispositivo(Guid dispositivoId, [FromQuery] int limite = LimitePadrao)
    {
        if (limite < 1 || limite > LimiteMaximo)
            return BadRequest($"O limite deve estar entre 1 e {LimiteMaximo}.");

        if (!_dispositivoIotRepository.ExistsById(dispositivoId))
            return NotFound("Dispositivo IoT não encontrado.");

        var leituras = _leituraIotRepository
            .GetByDispositivoId(dispositivoId, limite)
            .Select(LeituraIotResponse.FromDocument)
            .ToList();

        return Ok(leituras);
    }

    /// <summary>
    /// Busca uma leitura específica de um dispositivo.
    /// </summary>
    /// <param name="dispositivoId">Identificador único do dispositivo IoT.</param>
    /// <param name="leituraId">Identificador único da leitura.</param>
    /// <returns>Leitura encontrada.</returns>
    /// <response code="200">Leitura encontrada com sucesso.</response>
    /// <response code="404">Leitura não encontrada para este dispositivo.</response>
    [HttpGet("{leituraId:guid}")]
    [ProducesResponseType(typeof(LeituraIotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid dispositivoId, Guid leituraId)
    {
        var leitura = _leituraIotRepository.GetById(leituraId);

        if (leitura is null || leitura.DispositivoId != dispositivoId)
            return NotFound("Leitura não encontrada para este dispositivo.");

        return Ok(LeituraIotResponse.FromDocument(leitura));
    }

    /// <summary>
    /// Registra uma nova leitura da coleira IoT.
    /// </summary>
    /// <remarks>
    /// Aceita o mesmo JSON que o firmware do ESP32 (PetPulse-Iot) expõe em <c>GET /api/dados</c>.
    /// A leitura é gravada no histórico (MongoDB) e passa a ser a última leitura do dispositivo
    /// no Oracle.
    /// </remarks>
    /// <param name="dispositivoId">Identificador único do dispositivo IoT.</param>
    /// <param name="request">Dados da leitura.</param>
    /// <returns>Leitura registrada.</returns>
    /// <response code="201">Leitura registrada com sucesso.</response>
    /// <response code="400">Dados inválidos ou dispositivo não está ativo.</response>
    /// <response code="404">Dispositivo IoT não encontrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LeituraIotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create(Guid dispositivoId, [FromBody] LeituraIotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dispositivo = _dispositivoIotRepository.GetById(dispositivoId);

        if (dispositivo is null)
            return NotFound("Dispositivo IoT não encontrado.");

        if (dispositivo.Status != StatusDispositivoEnum.Ativo)
            return BadRequest("O dispositivo não está ativo e não pode registrar leituras.");

        var leitura = request.ToDocument(dispositivo);

        // 1) Histórico completo no MongoDB.
        _leituraIotRepository.Add(leitura);

        // 2) Estado atual (última leitura) no Oracle — mesmo campo que o PUT do dispositivo atualiza.
        dispositivo.AtualizarLeitura(
            dispositivo.IntervaloColetaMinutos,
            leitura.FrequenciaCardiaca,
            leitura.NivelAtividade,
            leitura.PressaoSistolica,
            leitura.DataLeitura
        );

        _dispositivoIotRepository.Update(dispositivo);

        return CreatedAtAction(
            nameof(GetById),
            new { dispositivoId, leituraId = leitura.Id },
            LeituraIotResponse.FromDocument(leitura)
        );
    }

    /// <summary>
    /// Remove uma leitura do histórico.
    /// </summary>
    /// <param name="dispositivoId">Identificador único do dispositivo IoT.</param>
    /// <param name="leituraId">Identificador único da leitura.</param>
    /// <response code="204">Leitura removida com sucesso.</response>
    /// <response code="404">Leitura não encontrada para este dispositivo.</response>
    [HttpDelete("{leituraId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid dispositivoId, Guid leituraId)
    {
        var leitura = _leituraIotRepository.GetById(leituraId);

        if (leitura is null || leitura.DispositivoId != dispositivoId)
            return NotFound("Leitura não encontrada para este dispositivo.");

        _leituraIotRepository.Delete(leituraId);

        return NoContent();
    }
}
