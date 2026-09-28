using System.ComponentModel.DataAnnotations;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Tests;

public class DispositivoIotDtoTests
{
    [Fact]
    public void ToDomain_ComDadosValidos_DeveMapearParaDispositivoIot()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);

        // Act
        var dispositivo = request.ToDomain();

        // Assert
        Assert.Equal(petId, dispositivo.PetId);
        Assert.Equal(new DateOnly(2026, 1, 1), dispositivo.DataVinculacao);
        Assert.Equal(15, dispositivo.IntervaloColetaMinutos);
        Assert.Equal(90, dispositivo.FrequenciaCardiaca);
        Assert.Equal(3.5m, dispositivo.NivelAtividade);
        Assert.Equal(120m, dispositivo.Pressao);
        Assert.Null(dispositivo.DataUltimaLeitura);
        Assert.Equal(StatusDispositivoEnum.Ativo, dispositivo.Status);
    }

    [Fact]
    public void FromDomain_ComDispositivoValido_DeveMapearParaResponse()
    {
        // Arrange
        var dispositivo = new DispositivoIot(
            Guid.NewGuid(), new DateOnly(2026, 1, 1), 15, 90, 3.5m, 120m,
            null, StatusDispositivoEnum.Ativo);

        // Act
        var response = DispositivoIotResponse.FromDomain(dispositivo);

        // Assert
        Assert.Equal(dispositivo.Id, response.Id);
        Assert.Equal(dispositivo.PetId, response.PetId);
        Assert.Equal(dispositivo.DataVinculacao, response.DataVinculacao);
        Assert.Equal(dispositivo.IntervaloColetaMinutos, response.IntervaloColetaMinutos);
        Assert.Equal(dispositivo.FrequenciaCardiaca, response.FrequenciaCardiaca);
        Assert.Equal(dispositivo.NivelAtividade, response.NivelAtividade);
        Assert.Equal(dispositivo.Pressao, response.Pressao);
        Assert.Equal(dispositivo.DataUltimaLeitura, response.DataUltimaLeitura);
        Assert.Equal(dispositivo.Status, response.Status);
        Assert.Equal(dispositivo.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void DispositivoIotRequest_ComDadosValidos_DevePassarNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid());

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void DispositivoIotRequest_ComFrequenciaCardiacaZero_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { FrequenciaCardiaca = 0 };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("FrequenciaCardiaca"));
    }

    [Fact]
    public void DispositivoIotRequest_ComPressaoNegativa_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { Pressao = -1m };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Pressao"));
    }

    private static DispositivoIotRequest CriarRequestValido(Guid petId) =>
        new(
            petId,
            new DateOnly(2026, 1, 1),
            15,
            90,
            3.5m,
            120m,
            null,
            StatusDispositivoEnum.Ativo);

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return (isValid, results);
    }
}
