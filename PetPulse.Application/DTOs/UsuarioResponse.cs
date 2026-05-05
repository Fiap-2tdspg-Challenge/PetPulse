using PetPulse.Domain.entites;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de resposta para usuário/tutor
/// </summary>
public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Cpf,
    string Email,
    string? Telefone,
    string? Endereco,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Mapeia a entidade Usuario para DTO.
    /// </summary>
    public static UsuarioResponse FromDomain(Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Nome,
            usuario.Cpf,
            usuario.Email,
            usuario.Telefone,
            usuario.Endereco,
            usuario.CreatedAt
        );
}