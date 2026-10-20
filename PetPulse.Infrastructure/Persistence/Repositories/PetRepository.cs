using PetPulse.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IPetRepository"/>.
/// </summary>
public sealed class PetRepository(PetPulseContext context) : IPetRepository
{
    public IReadOnlyList<Pet> GetAll()
    {
        return context.Pets
            .AsNoTracking()
            .OrderBy(pet => pet.Nome)
            .ToList();
    }

    public (IReadOnlyList<Pet> Items, int Total) GetPaged(PetQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pets = context.Pets.AsNoTracking();

        if (query.UsuarioId.HasValue)
        {
            var usuarioId = query.UsuarioId.Value;
            pets = pets.Where(pet => pet.UsuarioId == usuarioId);
        }

        if (!string.IsNullOrWhiteSpace(query.Nome))
        {
            var nome = query.Nome.Trim().ToLower();
            pets = pets.Where(pet => pet.Nome.ToLower().Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(query.Especie))
        {
            var especie = query.Especie.Trim().ToLower();
            pets = pets.Where(pet => pet.Especie.ToLower() == especie);
        }

        if (query.Porte.HasValue)
        {
            var porte = query.Porte.Value;
            pets = pets.Where(pet => pet.Porte == porte);
        }

        if (query.Sexo.HasValue)
        {
            var sexo = query.Sexo.Value;
            pets = pets.Where(pet => pet.Sexo == sexo);
        }

        if (query.Castrado.HasValue)
        {
            var castrado = query.Castrado.Value;
            pets = pets.Where(pet => pet.Castrado == castrado);
        }

        var descendente = query.EhDescendente();

        var ordenados = query.ObterCampoOrdenacao() switch
        {
            "especie" => pets.OrdenarPor(pet => pet.Especie, descendente),
            "peso" => pets.OrdenarPor(pet => pet.Peso, descendente),
            "datanascimento" => pets.OrdenarPor(pet => pet.DataNascimento, descendente),
            "datacadastro" => pets.OrdenarPor(pet => pet.CreatedAt, descendente),
            _ => pets.OrdenarPor(pet => pet.Nome, descendente)
        };

        return ordenados.Paginar(query.Page, query.PageSize);
    }

    public Pet? GetById(Guid id)
    {
        return context.Pets
            .FirstOrDefault(pet => pet.Id == id);
    }

    public IReadOnlyList<Pet> GetByUsuarioId(Guid usuarioId)
    {
        return context.Pets
            .AsNoTracking()
            .Where(pet => pet.UsuarioId == usuarioId)
            .OrderBy(pet => pet.Nome)
            .ToList();
    }

    public Pet Add(Pet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);

        context.Pets.Add(pet);
        context.SaveChanges();

        return pet;
    }

    public Pet Update(Pet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);

        context.Pets.Update(pet);
        context.SaveChanges();

        return pet;
    }

    public bool Delete(Guid id)
    {
        var entity = context.Pets.FirstOrDefault(pet => pet.Id == id);

        if (entity is null)
            return false;

        context.Pets.Remove(entity);
        context.SaveChanges();

        return true;
    }

    public bool ExistsById(Guid id)
    {
        return context.Pets.Any(pet => pet.Id == id);
    }
}