using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;
using PetPulse.Domain.entites.Mongo;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Tests;

public class LeituraIotDtoTests
{
    [Fact]
    public void ToDocument_ComDadosValidos_DeveMapearParaDocumentoVinculadoAoDispositivo()
    {
        // Arrange
        var dispositivo = CriarDispositivo();
        var request = CriarRequestValido();

        // Act
        var documento = request.ToDocument(dispositivo);

        // Assert
        Assert.NotEqual(Guid.Empty, documento.Id);
        Assert.Equal(dispositivo.Id, documento.DispositivoId);
        Assert.Equal(dispositivo.PetId, documento.PetId);
        Assert.Equal("COLLAR-001", documento.CodigoDispositivo);
        Assert.Equal(95, documento.FrequenciaCardiaca);
        Assert.Equal(45, documento.NivelAtividade);
        Assert.Equal("caminhada", documento.DescAtividade);
        Assert.Equal(130, documento.PressaoSistolica);
        Assert.Equal(80, documento.PressaoDiastolica);
        Assert.Equal(38.5, documento.Temperatura);
        Assert.Equal("OK", documento.Estado);
        Assert.Equal(DateTimeKind.Utc, documento.DataLeitura.Kind);
    }

    [Fact]
    public void ToDocument_SemDataLeitura_DeveUsarOMomentoDoRecebimento()
    {
        // Arrange
        var antes = DateTime.UtcNow;

        // Act
        var documento = CriarRequestValido().ToDocument(CriarDispositivo());

        // Assert
        Assert.InRange(documento.DataLeitura, antes, DateTime.UtcNow);
        Assert.Equal(documento.RegistradoEm, documento.DataLeitura);
    }

    [Fact]
    public void FromDocument_DeveMapearParaResponse()
    {
        // Arrange
        var documento = CriarRequestValido().ToDocument(CriarDispositivo());

        // Act
        var response = LeituraIotResponse.FromDocument(documento);

        // Assert
        Assert.Equal(documento.Id, response.Id);
        Assert.Equal(documento.DispositivoId, response.DispositivoId);
        Assert.Equal(documento.FrequenciaCardiaca, response.FrequenciaCardiaca);
        Assert.Equal(documento.Temperatura, response.Temperatura);
        Assert.Equal(documento.DataLeitura, response.DataLeitura);
    }

    [Fact]
    public void LeituraIotRequest_ComDadosValidos_DevePassarNaValidacao()
    {
        // Act
        var (isValid, results) = Validar(CriarRequestValido());

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void LeituraIotRequest_ComFrequenciaCardiacaZero_DeveFalharNaValidacao()
    {
        // Arrange — campo ausente no JSON chega como 0, então também cai aqui
        var request = CriarRequestValido() with { FrequenciaCardiaca = 0 };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("FrequenciaCardiaca"));
    }

    [Fact]
    public void LeituraIotRequest_ComNivelAtividadeAcimaDe100_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido() with { NivelAtividade = 150 };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("NivelAtividade"));
    }

    [Fact]
    public void LeituraIotRequest_ComTemperaturaForaDoIntervalo_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido() with { Temperatura = 60 };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Temperatura"));
    }

    [Fact]
    public void LeituraIotRequest_SoComFrequenciaCardiaca_DevePassarNaValidacao()
    {
        // Arrange — os demais sensores são opcionais
        var request = new LeituraIotRequest(90, null, null, null, null, null, null, null, null, null, null, null, null);

        // Act
        var (isValid, _) = Validar(request);

        // Assert
        Assert.True(isValid);
    }

    private static DispositivoIot CriarDispositivo()
    {
        var dispositivo = new DispositivoIot(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today),
            5,
            null,
            null,
            null,
            null,
            StatusDispositivoEnum.Ativo);

        dispositivo.Id = Guid.NewGuid();

        return dispositivo;
    }

    private static LeituraIotRequest CriarRequestValido() =>
        new(
            FrequenciaCardiaca: 95,
            NivelAtividade: 45,
            DescAtividade: "caminhada",
            AceleracaoTotal: 1.23,
            PressaoSistolica: 130,
            PressaoDiastolica: 80,
            Temperatura: 38.5,
            Score: 87,
            Estado: "OK",
            Alerta: "",
            Uptime: 12345,
            IdDispositivo: "COLLAR-001",
            DataLeitura: null);

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        // Mesma estratégia dos demais DTOs (ver UsuarioDtoTests): os atributos de validação ficam
        // no parâmetro do construtor primário do record, então são lidos via reflection.
        var results = new List<ValidationResult>();
        var type = dto.GetType();
        var constructor = type.GetConstructors()[0];
        var context = new ValidationContext(dto);

        foreach (var parameter in constructor.GetParameters())
        {
            var property = type.GetProperty(parameter.Name!);
            if (property is null)
                continue;

            context.MemberName = property.Name;
            var value = property.GetValue(dto);

            foreach (var attribute in parameter.GetCustomAttributes<ValidationAttribute>(inherit: true))
            {
                var result = attribute.GetValidationResult(value, context);
                if (result is not null && result != ValidationResult.Success)
                    results.Add(result);
            }
        }

        return (results.Count == 0, results);
    }
}
