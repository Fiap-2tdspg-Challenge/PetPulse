using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IDispositivoIotRepository"/>.
/// </summary>
public sealed class DispositivoIotRepository(PetPulseContext context) : IDispositivoIotRepository
{
    public IReadOnlyList<DispositivoIot> GetAll()
    {
        return context.DispositivosIot
            .AsNoTracking()
            .OrderByDescending(dispositivo => dispositivo.DataUltimaLeitura)
            .ToList();
    }

    public DispositivoIot? GetById(Guid id)
    {
        return context.DispositivosIot
            .FirstOrDefault(dispositivo => dispositivo.Id == id);
    }

    public DispositivoIot? GetByPetId(Guid petId)
    {
        return context.DispositivosIot
            .AsNoTracking()
            .FirstOrDefault(dispositivo => dispositivo.PetId == petId);
    }

    public DispositivoIot Add(DispositivoIot dispositivo)
    {
        ArgumentNullException.ThrowIfNull(dispositivo);

        context.DispositivosIot.Add(dispositivo);
        context.SaveChanges();

        return dispositivo;
    }

    public DispositivoIot Update(DispositivoIot dispositivo)
    {
        ArgumentNullException.ThrowIfNull(dispositivo);

        context.DispositivosIot.Update(dispositivo);
        context.SaveChanges();

        return dispositivo;
    }

    public bool Delete(Guid id)
    {
        var entity = context.DispositivosIot.FirstOrDefault(dispositivo => dispositivo.Id == id);

        if (entity is null)
            return false;

        context.DispositivosIot.Remove(entity);
        context.SaveChanges();

        return true;
    }

    public bool ExistsById(Guid id)
    {
        return context.DispositivosIot.Any(dispositivo => dispositivo.Id == id);
    }

    public bool ExistsByPetId(Guid petId)
    {
        return context.DispositivosIot.Any(dispositivo => dispositivo.PetId == petId);
    }
}