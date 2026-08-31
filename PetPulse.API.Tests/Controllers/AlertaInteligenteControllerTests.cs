using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Controllers;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.API.Tests.Controllers;

public class AlertaInteligenteControllerTests
{
    private readonly Mock<IAlertaInteligenteRepository> _alertaRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly AlertaInteligenteController _controller;

    public AlertaInteligenteControllerTests()
    {
        _controller = new AlertaInteligenteController(_alertaRepository.Object, _petRepository.Object);
    }

    [Fact]
    public void Create_ComPetInexistente_DeveRetornarNotFoundENaoPersistir()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid());
        _petRepository.Setup(r => r.ExistsById(request.PetId)).Returns(false);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _alertaRepository.Verify(r => r.Add(It.IsAny<AlertaInteligente>()), Times.Never);
    }

    [Fact]
    public void Create_ComDadosValidos_DeveRetornarCreatedEChamarAdd()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _alertaRepository.Setup(r => r.Add(It.IsAny<AlertaInteligente>())).Returns((AlertaInteligente a) => a);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        _alertaRepository.Verify(r => r.Add(It.IsAny<AlertaInteligente>()), Times.Once);
    }

    [Fact]
    public void GetByPetId_QuandoPetNaoExiste_DeveRetornarNotFoundENaoConsultarAlertas()
    {
        // Arrange
        _petRepository.Setup(r => r.ExistsById(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.GetByPetId(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _alertaRepository.Verify(r => r.GetByPetId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void Visualizar_QuandoAlertaExiste_DeveMarcarComoVisualizadoEAtualizar()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        _alertaRepository.Setup(r => r.GetById(alerta.Id)).Returns(alerta);

        // Act
        var result = _controller.Visualizar(alerta.Id);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusAlertaEnum.Visualizado, alerta.Status);
        _alertaRepository.Verify(r => r.Update(alerta), Times.Once);
    }

    [Fact]
    public void Visualizar_QuandoAlertaNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _alertaRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((AlertaInteligente?)null);

        // Act
        var result = _controller.Visualizar(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Resolver_QuandoAlertaExiste_DeveMarcarComoResolvidoEAtualizar()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        _alertaRepository.Setup(r => r.GetById(alerta.Id)).Returns(alerta);

        // Act
        var result = _controller.Resolver(alerta.Id);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusAlertaEnum.Resolvido, alerta.Status);
        _alertaRepository.Verify(r => r.Update(alerta), Times.Once);
    }

    private static AlertaInteligenteRequest CriarRequestValido(Guid petId) =>
        new(
            petId,
            TipoAlertaEnum.Atividade,
            NivelRiscoEnum.Medio,
            OrigemAlertaEnum.DispositivoIot,
            "Nível de atividade abaixo do padrão.",
            "Observar nas próximas 24 horas.");

    private static AlertaInteligente CriarAlertaValido() =>
        new(
            Guid.NewGuid(),
            TipoAlertaEnum.Atividade,
            NivelRiscoEnum.Medio,
            OrigemAlertaEnum.DispositivoIot,
            "Nível de atividade abaixo do padrão.",
            "Observar nas próximas 24 horas.");
}
