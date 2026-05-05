using System.ComponentModel.DataAnnotations;
using PetPulse.Domain.entites;

namespace PetPulse.Application.DTOs;

/// <summary>
/// DTO de requisição para cadastro de usuário/tutor.
/// </summary>
public record UsuarioRequest(
    [property: Required(ErrorMessage = "O nome é obrigatório")]
    [property: StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres")]
    string Nome,

    [property: Required(ErrorMessage = "O CPF é obrigatório")]
    [property: StringLength(14, MinimumLength = 11, ErrorMessage = "O CPF deve ter entre 11 e 14 caracteres")]
    string Cpf,

    [property: Required(ErrorMessage = "O e-mail é obrigatório")]
    [property: EmailAddress(ErrorMessage = "E-mail inválido")]
    string Email,

    [property: Required(ErrorMessage = "A senha é obrigatória")]
    [property: MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
    string Senha,

    [property: StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
    string? Telefone,

    [property: StringLength(255, ErrorMessage = "O endereço deve ter no máximo 255 caracteres")]
    string? Endereco
)
{
    /// <summary>
    /// Constrói a entidade de domínio Usuario.
    /// </summary>
    public Usuario ToDomain() => new(Nome, Cpf, Email, Senha, Telefone, Endereco);
}