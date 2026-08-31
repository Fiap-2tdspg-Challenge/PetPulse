using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.Tests;

public class AlertaInteligenteTests
{
    [Fact]
    public void CriarAlerta_ComDadosValidos_DeveIniciarComoAberto()
    {
        // Arrange
        var petId = Guid.NewGuid();

        // Act
        var alerta = new AlertaInteligente(
            petId,
            TipoAlertaEnum.Atividade,
            NivelRiscoEnum.Medio,
            OrigemAlertaEnum.DispositivoIot,
            "Nível de atividade abaixo do padrão.",
            "Observar nas próximas 24 horas.");

        // Assert
        Assert.Equal(StatusAlertaEnum.Aberto, alerta.Status);
        Assert.Equal(petId, alerta.PetId);
    }

    [Fact]
    public void Construtor_ComMensagemVazia_DeveLancarException()
    {
        // Arrange & Act
        var act = () => new AlertaInteligente(
            Guid.NewGuid(),
            TipoAlertaEnum.Vacina,
            NivelRiscoEnum.Baixo,
            OrigemAlertaEnum.Sistema,
            "",
            null);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Mensagem do alerta não pode ser vazia.", ex.Message);
    }

    [Fact]
    public void Visualizar_DeveAlterarStatusParaVisualizado()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        // Act
        alerta.Visualizar();

        // Assert
        Assert.Equal(StatusAlertaEnum.Visualizado, alerta.Status);
    }

    [Fact]
    public void Resolver_DeveAlterarStatusParaResolvido()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        // Act
        alerta.Resolver();

        // Assert
        Assert.Equal(StatusAlertaEnum.Resolvido, alerta.Status);
    }

    [Fact]
    public void AtualizarDados_ComDadosValidos_DeveAtualizarTodosOsCampos()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        // Act
        alerta.AtualizarDados(
            TipoAlertaEnum.FrequenciaCardiaca,
            NivelRiscoEnum.Alto,
            OrigemAlertaEnum.Usuario,
            "Frequência cardíaca muito alta.",
            "Procurar veterinário imediatamente.");

        // Assert
        Assert.Equal(TipoAlertaEnum.FrequenciaCardiaca, alerta.TipoAlerta);
        Assert.Equal(NivelRiscoEnum.Alto, alerta.NivelRisco);
        Assert.Equal("Frequência cardíaca muito alta.", alerta.Mensagem);
    }

    [Fact]
    public void AtualizarDados_ComMensagemVazia_DeveLancarException()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        // Act
        var act = () => alerta.AtualizarDados(
            TipoAlertaEnum.Vacina,
            NivelRiscoEnum.Baixo,
            OrigemAlertaEnum.Sistema,
            "",
            null);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Mensagem do alerta não pode ser vazia.", ex.Message);
    }

    private static AlertaInteligente CriarAlertaValido() =>
        new(
            Guid.NewGuid(),
            TipoAlertaEnum.Atividade,
            NivelRiscoEnum.Medio,
            OrigemAlertaEnum.DispositivoIot,
            "Nível de atividade abaixo do padrão.",
            "Observar nas próximas 24 horas.");
}
