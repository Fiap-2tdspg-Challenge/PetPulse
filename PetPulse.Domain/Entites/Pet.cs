using PetPulse.Domain.Commom;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.entites;

public class Pet : BaseEntity
{
    public string Nome { get; private set; } 

    public string Especie { get; private set; }

    public string? Raca { get; private set; }

    public DateOnly? DataNascimento { get; private set; }

    public decimal? Peso { get; private set; }

    public SexoPetEnum Sexo { get; private set; }

    public bool Castrado { get; private set; }

    public PortePetEnum Porte { get; private set; }

    public Guid UsuarioId { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public List<HistoricoClinico> HistoricosClinicos { get; private set; }

    public List<AlertaInteligente> AlertasInteligentes { get; private set; }

    public DispositivoIot? DispositivoIot { get; private set; }

    private Pet()
    {
    }

    public Pet(
        Guid usuarioId,
        string nome,
        string especie,
        string? raca,
        DateOnly? dataNascimento,
        decimal? peso,
        SexoPetEnum sexo,
        bool castrado,
        PortePetEnum porte)
    {
        UsuarioId = usuarioId;
        AtualizarNome(nome);
        AtualizarEspecie(especie);
        AtualizarRaca(raca);
        AtualizarDataNascimento(dataNascimento);
        AtualizarPeso(peso);
        Sexo = sexo;
        Castrado = castrado;
        Porte = porte;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new Exception("Nome do pet não pode ser vazio.");

        Nome = nome;
    }

    public void AtualizarEspecie(string especie)
    {
        if (string.IsNullOrWhiteSpace(especie))
            throw new Exception("Espécie do pet não pode ser vazia.");

        Especie = especie;
    }

    public void AtualizarRaca(string? raca)
    {
        Raca = raca;
    }

    public void AtualizarDataNascimento(DateOnly? dataNascimento)
    {
        if (dataNascimento.HasValue && dataNascimento.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new Exception("Data de nascimento não pode ser futura.");

        DataNascimento = dataNascimento;
    }

    public void AtualizarPeso(decimal? peso)
    {
        if (peso.HasValue && peso.Value <= 0)
            throw new Exception("Peso deve ser maior que zero.");

        Peso = peso;
    }

    public void AtualizarDados(
        string nome,
        string especie,
        string? raca,
        DateOnly? dataNascimento,
        decimal? peso,
        SexoPetEnum sexo,
        bool castrado,
        PortePetEnum porte)
    {
        AtualizarNome(nome);
        AtualizarEspecie(especie);
        AtualizarRaca(raca);
        AtualizarDataNascimento(dataNascimento);
        AtualizarPeso(peso);
        Sexo = sexo;
        Castrado = castrado;
        Porte = porte;
    }
}