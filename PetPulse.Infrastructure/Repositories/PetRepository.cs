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