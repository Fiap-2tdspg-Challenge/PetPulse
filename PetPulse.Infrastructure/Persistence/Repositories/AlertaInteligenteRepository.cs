using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IAlertaInteligenteRepository"/>.
/// </summary>
public sealed class AlertaInteligenteRepository(PetPulseContext context) : IAlertaInteligenteRepository
{
    public IReadOnlyList<AlertaInteligente> GetAll()
    {
        return context.AlertasInteligentes
            .AsNoTracking()
            .OrderByDescending(alerta => alerta.DataGeracao)
            .ToList();
    }

    public AlertaInteligente? GetById(Guid id)
    {
        return context.AlertasInteligentes
            .FirstOrDefault(alerta => alerta.Id == id);
    }

    public IReadOnlyList<AlertaInteligente> GetByPetId(Guid petId)
    {
        return context.AlertasInteligentes
            .AsNoTracking()
            .Where(alerta => alerta.PetId == petId)
            .OrderByDescending(alerta => alerta.DataGeracao)
            .ToList();
    }

    public IReadOnlyList<AlertaInteligente> GetByStatus(StatusAlertaEnum status)
    {
        return context.AlertasInteligentes
            .AsNoTracking()
            .Where(alerta => alerta.Status == status)
            .OrderByDescending(alerta => alerta.DataGeracao)
            .ToList();
    }

    public AlertaInteligente Add(AlertaInteligente alerta)
    {
        ArgumentNullException.ThrowIfNull(alerta);

        context.AlertasInteligentes.Add(alerta);
        context.SaveChanges();

        return alerta;
    }

    public AlertaInteligente Update(AlertaInteligente alerta)
    {
        ArgumentNullException.ThrowIfNull(alerta);

        context.AlertasInteligentes.Update(alerta);
        context.SaveChanges();

        return alerta;
    }

    public bool Delete(Guid id)
    {
        var entity = context.AlertasInteligentes.FirstOrDefault(alerta => alerta.Id == id);

        if (entity is null)
            return false;

        context.AlertasInteligentes.Remove(entity);
        context.SaveChanges();

        return true;
    }

    public bool ExistsById(Guid id)
    {
        return context.AlertasInteligentes.Any(alerta => alerta.Id == id);
    }
}