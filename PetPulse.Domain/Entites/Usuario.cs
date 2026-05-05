using PetPulse.Domain.Commom;

namespace PetPulse.Domain.entites;

public class Usuario : BaseEntity
{
    public string Nome { get; private set; } 

    public string Cpf { get; private set; } 

    public string Email { get; private set; }
    public string Senha { get; private set; } 

    public string? Telefone { get; private set; }

    public string? Endereco { get; private set; }

    public List<Pet> Pets { get; private set; }

    private Usuario()
    {
    }

    public Usuario(
        string nome,
        string cpf,
        string email,
        string senha,
        string? telefone,
        string? endereco)
    {
        AtualizarNome(nome);
        AtualizarCpf(cpf);
        AtualizarEmail(email);
        AtualizarSenha(senha);
        AtualizarTelefone(telefone);
        AtualizarEndereco(endereco);
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new Exception("Nome do usuário não pode ser vazio.");

        Nome = nome;
    }

    public void AtualizarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new Exception("CPF não pode ser vazio.");

        Cpf = cpf;
    }

    public void AtualizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new Exception("E-mail inválido.");

        Email = email;
    }

    public void AtualizarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new Exception("Senha não pode ser vazia.");

        Senha = senha;
    }

    public void AtualizarTelefone(string? telefone)
    {
        Telefone = telefone;
    }

    public void AtualizarEndereco(string? endereco)
    {
        Endereco = endereco;
    }
}