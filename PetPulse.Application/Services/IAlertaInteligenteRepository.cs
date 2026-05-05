using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Services;

/// <summary>
/// Contrato de persistência para AlertaInteligente.
/// </summary>
public interface IAlertaInteligenteRepository
{   
    /// <summary>
    /// Retorna todos os Alertas
    /// </summary>
    /// <returns>Lista somente os alertas realizados</returns>
    IReadOnlyList<AlertaInteligente> GetAll();
    
    /// <summary>
    /// Busca um alerta pelo id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Retorna apenas um alerta ou null se não exister</returns>
    AlertaInteligente? GetById(Guid id);
    
    /// <summary>
    /// Retorna um Alerta através do id do pet
    /// </summary>
    /// <param name="petId"></param>
    /// <returns>Retorna Apenas um alerta ligado a um pet ou null se não exister</returns>
    IReadOnlyList<AlertaInteligente> GetByPetId(Guid petId);
    
    /// <summary>
    /// Retorna um Alerta pelo Status
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    IReadOnlyList<AlertaInteligente> GetByStatus(StatusAlertaEnum status);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="alerta"></param>
    /// <returns></returns>
    AlertaInteligente Add(AlertaInteligente alerta);
    
    /// <summary>
    /// Atualiza o alerta
    /// </summary>
    /// <param name="alerta"></param>
    /// <returns></returns>
    AlertaInteligente Update(AlertaInteligente alerta);
    
    /// <summary>
    /// Apaga um alerta através do id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool Delete(Guid id);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool ExistsById(Guid id);
}