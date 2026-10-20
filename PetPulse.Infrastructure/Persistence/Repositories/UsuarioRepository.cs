using PetPulse.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.Infrastructure.Repositories;

public sealed class UsuarioRepository(PetPulseContext context) : IUsuarioRepository
{
    public (IReadOnlyList<Usuario> Items, int Total) GetPaged(UsuarioQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var usuarios = context.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Nome))
        {
            var nome = query.Nome.Trim().ToLower();
            usuarios = usuarios.Where(usuario => usuario.Nome.ToLower().Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.Trim().ToLower();
            usuarios = usuarios.Where(usuario => usuario.Email.ToLower().Contains(email));
        }

        var descendente = query.EhDescendente();

        var ordenados = query.ObterCampoOrdenacao() switch
        {
            "email" => usuarios.OrdenarPor(usuario => usuario.Email, descendente),
            "datacadastro" => usuarios.OrdenarPor(usuario => usuario.CreatedAt, descendente),
            _ => usuarios.OrdenarPor(usuario => usuario.Nome, descendente)
        };

        return ordenados.Paginar(query.Page, query.PageSize);
    }

    public Usuario? GetById(Guid id)
    {
        return context.Usuarios
            .FirstOrDefault(usuario => usuario.Id == id);
    }

    public Usuario? GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        return context.Usuarios
            .FirstOrDefault(usuario => usuario.Email.ToLower() == normalizedEmail);
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
        var entity = context.Usuarios
            .AsNoTracking()
            .FirstOrDefault(usuario => usuario.Id == id);

        return entity is not null;
    }

    public bool ExistsByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var entity = context.Usuarios
            .AsNoTracking()
            .FirstOrDefault(usuario => usuario.Email.ToLower() == normalizedEmail);

        return entity is not null;
    }

    public bool ExistsByCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var normalizedCpf = cpf.Trim();

        var entity = context.Usuarios
            .AsNoTracking()
            .FirstOrDefault(usuario => usuario.Cpf == normalizedCpf);

        return entity is not null;
    }
}
