using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Domain.Tests;

public class DispositivoIotTests
{
    [Fact]
    public void CriarDispositivo_ComDadosValidos_DeveCriarDispositivo()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var dataVinculacao = new DateOnly(2026, 5, 5);

        // Act
        var dispositivo = new DispositivoIot(
            petId,
            dataVinculacao,
            30,
            95,
            72.5m,
            12.8m,
            DateTime.UtcNow,
            StatusDispositivoEnum.Ativo);

        // Assert
        Assert.Equal(petId, dispositivo.PetId);
        Assert.Equal(StatusDispositivoEnum.Ativo, dispositivo.Status);
        Assert.Equal(95, dispositivo.FrequenciaCardiaca);
    }

    [Fact]
    public void AtualizarLeitura_ComFrequenciaCardiacaZero_DeveLancarException()
    {
        // Arrange
        var dispositivo = CriarDispositivoValido();

        // Act
        var act = () => dispositivo.AtualizarLeitura(30, 0, 72.5m, 12.8m, DateTime.UtcNow);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Frequência cardíaca deve ser maior que zero.", ex.Message);
    }

    [Fact]
    public void AtualizarLeitura_ComNivelAtividadeNegativo_DeveLancarException()
    {
        // Arrange
        var dispositivo = CriarDispositivoValido();

        // Act
        var act = () => dispositivo.AtualizarLeitura(30, 95, -1m, 12.8m, DateTime.UtcNow);

        // Assert
        var ex = Assert.Throws<Exception>(act);
        Assert.Equal("Nível de atividade não pode ser negativo.", ex.Message);
    }

    [Fact]
    public void AlterarStatus_DeveAtualizarStatusDoDispositivo()
    {
        // Arrange
        var dispositivo = CriarDispositivoValido();

        // Act
        dispositivo.AlterarStatus(StatusDispositivoEnum.Manutencao);

        // Assert
        Assert.Equal(StatusDispositivoEnum.Manutencao, dispositivo.Status);
    }

    private static DispositivoIot CriarDispositivoValido() =>
        new(
            Guid.NewGuid(),
            new DateOnly(2026, 5, 5),
            30,
            95,
            72.5m,
            12.8m,
            DateTime.UtcNow,
            StatusDispositivoEnum.Ativo);
}
