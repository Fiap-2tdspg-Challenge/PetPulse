using MongoDB.Driver;
using PetPulse.Application.Services;
using PetPulse.Domain.entites.Mongo;

namespace PetPulse.Infrastructure.Repositories;

/// <summary>
/// Implementação MongoDB de <see cref="ILeituraIotRepository"/> — coleção <c>leituras_iot</c>.
/// </summary>
public sealed class LeituraIotRepository : ILeituraIotRepository
{
    public const string CollectionName = "leituras_iot";

    // O índice só precisa ser criado uma vez por execução da aplicação (CreateOne é idempotente).
    private static int _indiceCriado;

    private readonly IMongoCollection<LeituraIotDocument> _collection;

    public LeituraIotRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        _collection = database.GetCollection<LeituraIotDocument>(CollectionName);

        if (Interlocked.Exchange(ref _indiceCriado, 1) == 0)
            CriarIndice();
    }

    public LeituraIotDocument Add(LeituraIotDocument leitura)
    {
        ArgumentNullException.ThrowIfNull(leitura);

        _collection.InsertOne(leitura);

        return leitura;
    }

    public LeituraIotDocument? GetById(Guid id)
    {
        return _collection
            .Find(leitura => leitura.Id == id)
            .FirstOrDefault();
    }

    public IReadOnlyList<LeituraIotDocument> GetByDispositivoId(Guid dispositivoId, int limite)
    {
        return _collection
            .Find(leitura => leitura.DispositivoId == dispositivoId)
            .SortByDescending(leitura => leitura.DataLeitura)
            .Limit(limite)
            .ToList();
    }

    public bool Delete(Guid id)
    {
        var resultado = _collection.DeleteOne(leitura => leitura.Id == id);

        return resultado.DeletedCount > 0;
    }

    /// <summary>
    /// Índice composto (dispositivo + data decrescente): é exatamente o formato da consulta de
    /// histórico, então o MongoDB devolve as últimas leituras sem varrer a coleção inteira.
    /// </summary>
    private void CriarIndice()
    {
        try
        {
            var chaves = Builders<LeituraIotDocument>.IndexKeys
                .Ascending(leitura => leitura.DispositivoId)
                .Descending(leitura => leitura.DataLeitura);

            _collection.Indexes.CreateOne(new CreateIndexModel<LeituraIotDocument>(
                chaves,
                new CreateIndexOptions { Name = "ix_dispositivo_dataLeitura" }));
        }
        catch (MongoException)
        {
            // Sem conexão com o MongoDB neste momento: a própria operação que vier em seguida
            // vai falhar e ser tratada pelo GlobalExceptionHandler. O índice é recriado na
            // próxima execução da aplicação.
        }
    }
}
