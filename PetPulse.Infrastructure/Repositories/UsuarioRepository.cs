using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IUsuarioRepository"/>.
/// </summary>
public sealed class UsuarioRepository(PetPulseContext context) : IUsuarioRepository
{
    public IReadOnlyList<Usuario> GetAll()
    {
        return context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .ToList();
    }

    public Usuario? GetById(Guid id)
    {
        return context.Usuarios
            .FirstOrDefault(usuario => usuario.Id == id);
    }

    public Usuario Add(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        context.Usuarios.Add(usuario);
        context.SaveChanges();

        return usuario;
    }

    public Usuario Update(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        context.Usuarios.Update(usuario);
        context.SaveChanges();

        return usuario;
    }

    public bool Delete(Guid id)
    {
        var entity = context.Usuarios.FirstOrDefault(usuario => usuario.Id == id);

        if (entity is null)
            return false;

        context.Usuarios.Remove(entity);
        context.SaveChanges();

        return true;
    }

    public bool ExistsById(Guid id)
    {
        return context.Usuarios.Any(usuario => usuario.Id == id);
    }

    public bool ExistsByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        return context.Usuarios.Any(usuario => usuario.Email.ToLower() == normalizedEmail);
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var normalizedCpf = cpf.Trim();

        return context.Usuarios.Any(usuario => usuario.Cpf == normalizedCpf);
    }
}