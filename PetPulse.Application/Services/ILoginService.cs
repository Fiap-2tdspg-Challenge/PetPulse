using PetPulse.Application.DTOs.Login;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato do serviço de autenticação.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Autentica um usuário pelo e-mail e senha e, se válidos, emite um token JWT.
    /// </summary>
    /// <returns>O token de acesso (e metadados), ou <c>null</c> se as credenciais forem inválidas.</returns>
    LoginResponse? Login(string email, string senha);
}
