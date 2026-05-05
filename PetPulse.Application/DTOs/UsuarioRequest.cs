using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para cadastro de usuário/tutor.
/// </summary>
public record UsuarioRequest(
    [ Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres")]
    string Nome,

    [ Required(ErrorMessage = "O CPF é obrigatório")]
    [ StringLength(14, MinimumLength = 11, ErrorMessage = "O CPF deve ter entre 11 e 14 caracteres")]
    string Cpf,

    [ Required(ErrorMessage = "O e-mail é obrigatório")]
    [ EmailAddress(ErrorMessage = "E-mail inválido")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória")]
    [ MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
    string Senha,

    [ StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
    string? Telefone,

    [ StringLength(255, ErrorMessage = "O endereço deve ter no máximo 255 caracteres")]
    string? Endereco
)
{
    /// <summary>
    /// Constrói a entidade de domínio Usuario.
    /// </summary>
    public Usuario ToDomain() => new(Nome, Cpf, Email, Senha, Telefone, Endereco);
}