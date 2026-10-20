using PetPulse.Application.DTOs;
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

    public (IReadOnlyList<AlertaInteligente> Items, int Total) GetPaged(AlertaInteligenteQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var alertas = context.AlertasInteligentes.AsNoTracking();

        if (query.PetId.HasValue)
        {
            var petId = query.PetId.Value;
            alertas = alertas.Where(alerta => alerta.PetId == petId);
        }

        if (query.Status.HasValue)
        {
            var status = query.Status.Value;
            alertas = alertas.Where(alerta => alerta.Status == status);
        }

        if (query.NivelRisco.HasValue)
        {
            var nivelRisco = query.NivelRisco.Value;
            alertas = alertas.Where(alerta => alerta.NivelRisco == nivelRisco);
        }

        if (query.TipoAlerta.HasValue)
        {
            var tipoAlerta = query.TipoAlerta.Value;
            alertas = alertas.Where(alerta => alerta.TipoAlerta == tipoAlerta);
        }

        var descendente = query.EhDescendente();

        var ordenados = query.ObterCampoOrdenacao() switch
        {
            "nivelrisco" => alertas.OrdenarPor(alerta => alerta.NivelRisco, descendente),
            "status" => alertas.OrdenarPor(alerta => alerta.Status, descendente),
            "tipoalerta" => alertas.OrdenarPor(alerta => alerta.TipoAlerta, descendente),
            _ => alertas.OrdenarPor(alerta => alerta.DataGeracao, descendente)
        };

        return ordenados.Paginar(query.Page, query.PageSize);
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