using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Tests;

public class AlertaInteligenteDtoTests
{
    [Fact]
    public void ToDomain_ComDadosValidos_DeveMapearParaAlertaInteligente()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);

        // Act
        var alerta = request.ToDomain();

        // Assert
        Assert.Equal(petId, alerta.PetId);
        Assert.Equal(TipoAlertaEnum.FrequenciaCardiaca, alerta.TipoAlerta);
        Assert.Equal(NivelRiscoEnum.Alto, alerta.NivelRisco);
        Assert.Equal(OrigemAlertaEnum.DispositivoIot, alerta.OrigemAlerta);
        Assert.Equal("Frequência cardíaca acima do normal detectada", alerta.Mensagem);
        Assert.Equal("Procurar atendimento veterinário", alerta.Recomendacao);
        Assert.Equal(StatusAlertaEnum.Aberto, alerta.Status);
    }

    [Fact]
    public void FromDomain_ComAlertaValido_DeveMapearParaResponse()
    {
        // Arrange
        var alerta = new AlertaInteligente(
            Guid.NewGuid(), TipoAlertaEnum.FrequenciaCardiaca, NivelRiscoEnum.Alto,
            OrigemAlertaEnum.DispositivoIot, "Frequência cardíaca acima do normal detectada",
            "Procurar atendimento veterinário");

        // Act
        var response = AlertaInteligenteResponse.FromDomain(alerta);

        // Assert
        Assert.Equal(alerta.Id, response.Id);
        Assert.Equal(alerta.PetId, response.PetId);
        Assert.Equal(alerta.TipoAlerta, response.TipoAlerta);
        Assert.Equal(alerta.NivelRisco, response.NivelRisco);
        Assert.Equal(alerta.OrigemAlerta, response.OrigemAlerta);
        Assert.Equal(alerta.Mensagem, response.Mensagem);
        Assert.Equal(alerta.Recomendacao, response.Recomendacao);
        Assert.Equal(alerta.DataGeracao, response.DataGeracao);
        Assert.Equal(StatusAlertaEnum.Aberto, response.Status);
        Assert.Equal(alerta.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void AlertaInteligenteRequest_ComDadosValidos_DevePassarNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid());

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ok")]
    public void AlertaInteligenteRequest_ComMensagemInvalida_DeveFalharNaValidacao(string mensagemInvalida)
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { Mensagem = mensagemInvalida };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Mensagem"));
    }

    [Fact]
    public void AlertaInteligenteRequest_ComRecomendacaoMuitoLonga_DeveFalharNaValidacao()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { Recomendacao = new string('a', 1001) };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Recomendacao"));
    }

    private static AlertaInteligenteRequest CriarRequestValido(Guid petId) =>
        new(
            petId,
            TipoAlertaEnum.FrequenciaCardiaca,
            NivelRiscoEnum.Alto,
            OrigemAlertaEnum.DispositivoIot,
            "Frequência cardíaca acima do normal detectada",
            "Procurar atendimento veterinário");

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        // Validator.TryValidateObject só enxerga atributos de validação em properties.
        // Nos DTOs (records), os atributos de validação ficam no parâmetro do construtor
        // primário — é dali que o ASP.NET Core também os lê para validar o ModelState —
        // então a validação aqui precisa ler os atributos do mesmo lugar: os parâmetros
        // do construtor, não as properties geradas.
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
