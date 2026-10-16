using System.ComponentModel.DataAnnotations;

namespace PetPulse.Application.DTOs.Login;

/// <summary>
/// DTO de requisição para autenticação (login) de um usuário já cadastrado em /api/usuario.
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória")]
    string Senha
);
