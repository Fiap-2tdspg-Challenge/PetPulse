using PetPulse.Domain.entites;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato de persistência para Usuario.
/// </summary>
public interface IUsuarioRepository
{
    IReadOnlyList<Usuario> GetAll();

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