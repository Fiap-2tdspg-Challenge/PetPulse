using PetPulse.Application.DTOs;
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
    public (IReadOnlyList<DispositivoIot> Items, int Total) GetPaged(DispositivoIotQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var dispositivos = context.DispositivosIot.AsNoTracking();

        if (query.PetId.HasValue)
        {
            var petId = query.PetId.Value;
            dispositivos = dispositivos.Where(dispositivo => dispositivo.PetId == petId);
        }

        if (query.Status.HasValue)
        {
            var status = query.Status.Value;
            dispositivos = dispositivos.Where(dispositivo => dispositivo.Status == status);
        }

        var descendente = query.EhDescendente();

        var ordenados = query.ObterCampoOrdenacao() switch
        {
            "datavinculacao" => dispositivos.OrdenarPor(dispositivo => dispositivo.DataVinculacao, descendente),
            "frequenciacardiaca" => dispositivos.OrdenarPor(dispositivo => dispositivo.FrequenciaCardiaca, descendente),
            "datacadastro" => dispositivos.OrdenarPor(dispositivo => dispositivo.CreatedAt, descendente),
            _ => dispositivos.OrdenarPor(dispositivo => dispositivo.DataUltimaLeitura, descendente)
        };

        return ordenados.Paginar(query.Page, query.PageSize);
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