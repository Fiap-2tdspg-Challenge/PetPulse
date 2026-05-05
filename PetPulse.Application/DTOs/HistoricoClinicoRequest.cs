using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para registro de histórico clínico.
/// </summary>
public record HistoricoClinicoRequest(
    [ Required(ErrorMessage = "O id do pet é obrigatório")]
    Guid PetId,

    [ Required(ErrorMessage = "O tipo de registro é obrigatório")]
    TipoRegistroClinicoEnum TipoRegistro,

    [ Required(ErrorMessage = "A descrição é obrigatória")]
    [ StringLength(500, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 500 caracteres")]
    string Descricao,

    [ Required(ErrorMessage = "A data do registro é obrigatória")]
    DateOnly DataRegistro,

    DateOnly? DataRetorno,

    [ StringLength(150, ErrorMessage = "O profissional/clínica deve ter no máximo 150 caracteres")]
    string? ProfissionalClinica,

    [ StringLength(1000, ErrorMessage = "As observações devem ter no máximo 1000 caracteres")]
    string? Observacoes
)
{
    /// <summary>
    /// Constrói a entidade de domínio HistoricoClinico.
    /// </summary>
    public HistoricoClinico ToDomain() =>
        new(
            PetId,
            TipoRegistro,
            Descricao,
            DataRegistro,
            DataRetorno,
            ProfissionalClinica,
            Observacoes
        );
}