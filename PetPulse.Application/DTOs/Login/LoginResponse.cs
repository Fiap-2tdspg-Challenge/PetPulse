namespace PetPulse.Application.DTOs.Login;

/// <summary>
/// DTO de resposta da autenticação: token de acesso (JWT) e seus metadados.
/// </summary>
public record LoginResponse(
    string AccessToken,
    DateTime Expiracao,
    string Email,
    string Nome
);
