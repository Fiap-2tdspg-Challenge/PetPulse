using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPulse.Application.DTOs.Login;
using PetPulse.Application.Services;

namespace PetPulse.Controllers;

/// <summary>
/// Controller responsável pela autenticação. Emite um token JWT para um usuário
/// já cadastrado em <c>/api/usuario</c>.
/// </summary>
/// <remarks>
/// Base URL: /api/auth
/// Exemplo: http://localhost:5292/api/auth/login
/// </remarks>
[Route("api/auth")]
[ApiController]
[Produces("application/json")]
public class AuthController(ILoginService loginService) : ControllerBase
{
    /// <summary>
    /// Autentica um usuário pelo e-mail e senha e, se válidos, retorna um token JWT.
    /// </summary>
    /// <param name="request">Credenciais do usuário (e-mail e senha).</param>
    /// <returns>Token de acesso (JWT) e seus metadados, ou 401 se as credenciais forem inválidas.</returns>
    /// <response code="200">Autenticado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">E-mail ou senha incorretos.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = loginService.Login(request.Email, request.Senha);

        return token is null
            ? Unauthorized(new { title = "E-mail ou senha inválidos.", status = StatusCodes.Status401Unauthorized })
            : Ok(token);
    }
}
