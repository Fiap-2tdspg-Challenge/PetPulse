using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.Tests;

public class HistoricoClinicoTests
{
    [Fact]
    public void CriarHistorico_ComDadosValidos_DeveCriarHistorico()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var dataRegistro = new DateOnly(2026, 5, 5);

        // Act
        var historico = new HistoricoClinico(
            petId,
            TipoRegistroClinicoEnum.Vacina,
            "Vacina V10 aplicada",
            dataRegistro,
            dataRegistro.AddYears(1),
            "Clínica Pet Vida",
            "Pet sem reação adversa.");

        // Assert
        Assert.Equal(petId, historico.PetId);
        Assert.Equal(TipoRegistroClinicoEnum.Vacina, historico.TipoRegistro);
        Assert.Equal("Vacina V10 aplicada", historico.Descricao);
    }

    [Fact]
    public void Construtor_ComDescricaoVazia_DeveLancarException()
    {
        // Arrange & Act
        var act = () => new HistoricoClinico(
            Guid.NewGuid(),
            TipoRegistroClinicoEnum.Consulta,
            "",
            new DateOnly(2026, 5, 5),
            null,
            null,
            null);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Descrição do histórico clínico não pode ser vazia.", ex.Message);
    }

    [Fact]
    public void AtualizarDados_ComDadosValidos_DeveAtualizarTodosOsCampos()
    {
        // Arrange
        var historico = new HistoricoClinico(
            Guid.NewGuid(),
            TipoRegistroClinicoEnum.Vacina,
            "Vacina V10 aplicada",
            new DateOnly(2026, 5, 5),
            null,
            "Clínica Pet Vida",
            null);

        // Act
        historico.AtualizarDados(
            TipoRegistroClinicoEnum.Consulta,
            "Consulta de rotina",
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 12, 1),
            "Dra. Camila",
            "Pet saudável");

        // Assert
        Assert.Equal(TipoRegistroClinicoEnum.Consulta, historico.TipoRegistro);
        Assert.Equal("Consulta de rotina", historico.Descricao);
        Assert.Equal("Dra. Camila", historico.ProfissionalClinica);
    }

    [Fact]
    public void AtualizarDados_ComDescricaoVazia_DeveLancarException()
    {
        // Arrange
        var historico = new HistoricoClinico(
            Guid.NewGuid(),
            TipoRegistroClinicoEnum.Vacina,
            "Vacina V10 aplicada",
            new DateOnly(2026, 5, 5),
            null,
            null,
            null);

        // Act
        var act = () => historico.AtualizarDados(
            TipoRegistroClinicoEnum.Consulta,
            "",
            new DateOnly(2026, 6, 1),
            null,
            null,
            null);

        // Assert
        var ex2 = Assert.Throws<Exception>(act);
        Assert.Equal("Descrição do histórico clínico não pode ser vazia.", ex2.Message);
    }
}
