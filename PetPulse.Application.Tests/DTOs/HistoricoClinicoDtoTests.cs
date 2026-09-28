using System.ComponentModel.DataAnnotations;
using PetPulse.Application.DTOs;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.Application.Tests;

public class HistoricoClinicoDtoTests
{
    [Fact]
    public void ToDomain_ComDadosValidos_DeveMapearParaHistoricoClinico()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);

        // Act
        var historico = request.ToDomain();

        // Assert
        Assert.Equal(petId, historico.PetId);
        Assert.Equal(TipoRegistroClinicoEnum.Vacina, historico.TipoRegistro);
        Assert.Equal("Vacina antirrábica aplicada", historico.Descricao);
        Assert.Equal(new DateOnly(2026, 1, 10), historico.DataRegistro);
        Assert.Equal(new DateOnly(2027, 1, 10), historico.DataRetorno);
        Assert.Equal("Clínica PetSaúde", historico.ProfissionalClinica);
    }

    [Fact]
    public void FromDomain_ComHistoricoValido_DeveMapearParaResponse()
    {
        // Arrange
        var historico = new HistoricoClinico(
            Guid.NewGuid(), TipoRegistroClinicoEnum.Consulta, "Consulta de rotina",
            new DateOnly(2026, 2, 1), null, "Dr. João", "Sem observações");

        // Act
        var response = HistoricoClinicoResponse.FromDomain(historico);

        // Assert
        Assert.Equal(historico.Id, response.Id);
        Assert.Equal(historico.PetId, response.PetId);
        Assert.Equal(historico.TipoRegistro, response.TipoRegistro);
        Assert.Equal(historico.Descricao, response.Descricao);
        Assert.Equal(historico.DataRegistro, response.DataRegistro);
        Assert.Equal(historico.DataRetorno, response.DataRetorno);
        Assert.Equal(historico.ProfissionalClinica, response.ProfissionalClinica);
        Assert.Equal(historico.Observacoes, response.Observacoes);
        Assert.Equal(historico.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void HistoricoClinicoRequest_ComDadosValidos_DevePassarNaValidacao()
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
    [InlineData("ab")]
    public void HistoricoClinicoRequest_ComDescricaoInvalida_DeveFalharNaValidacao(string descricaoInvalida)
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid()) with { Descricao = descricaoInvalida };

        // Act
        var (isValid, results) = Validar(request);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Descricao"));
    }

    private static HistoricoClinicoRequest CriarRequestValido(Guid petId) =>
        new(
            petId,
            TipoRegistroClinicoEnum.Vacina,
            "Vacina antirrábica aplicada",
            new DateOnly(2026, 1, 10),
            new DateOnly(2027, 1, 10),
            "Clínica PetSaúde",
            null);

    private static (bool IsValid, List<ValidationResult> Results) Validar(object dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return (isValid, results);
    }
}
