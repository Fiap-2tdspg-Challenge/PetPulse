using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato de persistência para Usuario.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Lista paginada, filtrada e ordenada conforme <see cref="UsuarioQuery"/>.
    /// </summary>
    /// <returns>Itens da página solicitada e o total de itens que atendem aos filtros.</returns>
    (IReadOnlyList<Usuario> Items, int Total) GetPaged(UsuarioQuery query);

    Usuario? GetById(Guid id);

    /// <summary>
    /// Busca um usuário pelo e-mail (case-insensitive). Usado na autenticação (login).
    /// </summary>
    Usuario? GetByEmail(string email);

    Usuario Add(Usuario usuario);

    Usuario Update(Usuario usuario);

    bool Delete(Guid id);

    bool ExistsById(Guid id);

    bool ExistsByEmail(string email);

    bool ExistsByCpf(string cpf);
}