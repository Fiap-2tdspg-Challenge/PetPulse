using PetPulse.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using PetPulse.Application.Services;
using PetPulse.Domain.entites;
using PetPulse.Infrastructure.Persistence;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IHistoricoClinicoRepository"/>.
/// </summary>
public sealed class HistoricoClinicoRepository(PetPulseContext context) : IHistoricoClinicoRepository
{
    public IReadOnlyList<HistoricoClinico> GetAll()
    {
        return context.HistoricosClinicos
            .AsNoTracking()
            .OrderByDescending(historico => historico.DataRegistro)
            .ToList();
    }

    public (IReadOnlyList<HistoricoClinico> Items, int Total) GetPaged(HistoricoClinicoQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var historicos = context.HistoricosClinicos.AsNoTracking();

        if (query.PetId.HasValue)
        {
            var petId = query.PetId.Value;
            historicos = historicos.Where(historico => historico.PetId == petId);
        }

        if (query.TipoRegistro.HasValue)
        {
            var tipo = query.TipoRegistro.Value;
            historicos = historicos.Where(historico => historico.TipoRegistro == tipo);
        }

        if (query.DataInicio.HasValue)
        {
            var dataInicio = query.DataInicio.Value;
            historicos = historicos.Where(historico => historico.DataRegistro >= dataInicio);
        }

        if (query.DataFim.HasValue)
        {
            var dataFim = query.DataFim.Value;
            historicos = historicos.Where(historico => historico.DataRegistro <= dataFim);
        }

        var descendente = query.EhDescendente();

        var ordenados = query.ObterCampoOrdenacao() switch
        {
            "tiporegistro" => historicos.OrdenarPor(historico => historico.TipoRegistro, descendente),
            "datacadastro" => historicos.OrdenarPor(historico => historico.CreatedAt, descendente),
            _ => historicos.OrdenarPor(historico => historico.DataRegistro, descendente)
        };

        return ordenados.Paginar(query.Page, query.PageSize);
    }

    public HistoricoClinico? GetById(Guid id)
    {
        return context.HistoricosClinicos
            .FirstOrDefault(historico => historico.Id == id);
    }

    public IReadOnlyList<HistoricoClinico> GetByPetId(Guid petId)
    {
        return context.HistoricosClinicos
            .AsNoTracking()
            .Where(historico => historico.PetId == petId)
            .OrderByDescending(historico => historico.DataRegistro)
            .ToList();
    }

    public HistoricoClinico Add(HistoricoClinico historico)
    {
        ArgumentNullException.ThrowIfNull(historico);

        context.HistoricosClinicos.Add(historico);
        context.SaveChanges();

        return historico;
    }

    public HistoricoClinico Update(HistoricoClinico historico)
    {
        ArgumentNullException.ThrowIfNull(historico);

        context.HistoricosClinicos.Update(historico);
        context.SaveChanges();

        return historico;
    }

    public bool Delete(Guid id)
    {
        var entity = context.HistoricosClinicos.FirstOrDefault(historico => historico.Id == id);

        if (entity is null)
            return false;

        context.HistoricosClinicos.Remove(entity);
        context.SaveChanges();

        return true;
    }

    public bool ExistsById(Guid id)
    {
        return context.HistoricosClinicos.Any(historico => historico.Id == id);
    }
}