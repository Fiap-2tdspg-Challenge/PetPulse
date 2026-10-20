using PetPulse.Domain.entites.Mongo;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato de persistência (MongoDB) para o histórico de leituras da coleira IoT.
/// </summary>
/// <remarks>
/// Leituras são eventos imutáveis: uma vez registradas, não são alteradas — por isso não
/// existe operação de atualização, apenas inclusão, consulta e exclusão.
/// </remarks>
public interface ILeituraIotRepository
{
    LeituraIotDocument Add(LeituraIotDocument leitura);

    LeituraIotDocument? GetById(Guid id);

    /// <summary>
    /// Retorna as leituras mais recentes de um dispositivo, da mais nova para a mais antiga.
    /// </summary>
    IReadOnlyList<LeituraIotDocument> GetByDispositivoId(Guid dispositivoId, int limite);

    bool Delete(Guid id);
}
