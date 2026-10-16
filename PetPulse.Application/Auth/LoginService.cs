using Microsoft.Extensions.Configuration;
using PetPulse.Application.DTOs.Login;
using PetPulse.Application.Services;

namespace PetPulse.Application.Auth;

/// <summary>
/// Implementação do serviço de autenticação: valida e-mail e senha contra o
/// usuário cadastrado e, se corretos, delega a emissão do token ao <see cref="JwtTokenIssuer"/>.
/// </summary>
public sealed class LoginService(
    IUsuarioRepository usuarioRepository,
    IConfiguration configuration) : ILoginService
{
    public LoginResponse? Login(string email, string senha)
    {
        var usuario = usuarioRepository.GetByEmail(email);

        if (usuario is null || !usuario.VerificarSenha(senha))
            return null;

        return JwtTokenIssuer.Create(usuario, configuration);
    }
}
