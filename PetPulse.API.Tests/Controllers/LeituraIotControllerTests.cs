using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Controllers;
using PetPulse.Domain.entites;
using PetPulse.Domain.entites.Mongo;
using PetPulse.Domain.Enum;

namespace PetPulse.API.Tests.Controllers;

public class LeituraIotControllerTests
{
    private readonly Mock<ILeituraIotRepository> _leituraIotRepository = new();
    private readonly Mock<IDispositivoIotRepository> _dispositivoIotRepository = new();
    private readonly LeituraIotController _controller;

    public LeituraIotControllerTests()
    {
        _controller = new LeituraIotController(
            _leituraIotRepository.Object,
            _dispositivoIotRepository.Object);
    }

    [Fact]
    public void Create_ComDispositivoAtivo_DeveGravarNoMongoAtualizarOracleERetornarCreated()
    {
        // Arrange
        var dispositivo = CriarDispositivo(StatusDispositivoEnum.Ativo);
        _dispositivoIotRepository.Setup(r => r.GetById(dispositivo.Id)).Returns(dispositivo);

        var request = CriarRequestValido();

        // Act
        var result = _controller.Create(dispositivo.Id, request);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<LeituraIotResponse>(created.Value);
        Assert.Equal(dispositivo.Id, response.DispositivoId);
        Assert.Equal(dispositivo.PetId, response.PetId);
        Assert.Equal(95, response.FrequenciaCardiaca);

        _leituraIotRepository.Verify(r => r.Add(It.Is<LeituraIotDocument>(l => l.DispositivoId == dispositivo.Id)), Times.Once);
        _dispositivoIotRepository.Verify(r => r.Update(dispositivo), Times.Once);
        Assert.Equal(95, dispositivo.FrequenciaCardiaca);
        Assert.Equal(130m, dispositivo.Pressao);
    }

    [Fact]
    public void Create_QuandoDispositivoNaoExiste_DeveRetornarNotFoundSemGravar()
    {
        // Arrange
        _dispositivoIotRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((DispositivoIot?)null);

        // Act
        var result = _controller.Create(Guid.NewGuid(), CriarRequestValido());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _leituraIotRepository.Verify(r => r.Add(It.IsAny<LeituraIotDocument>()), Times.Never);
    }

    [Fact]
    public void Create_QuandoDispositivoInativo_DeveRetornarBadRequestSemGravar()
    {
        // Arrange
        var dispositivo = CriarDispositivo(StatusDispositivoEnum.Inativo);
        _dispositivoIotRepository.Setup(r => r.GetById(dispositivo.Id)).Returns(dispositivo);

        // Act
        var result = _controller.Create(dispositivo.Id, CriarRequestValido());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _leituraIotRepository.Verify(r => r.Add(It.IsAny<LeituraIotDocument>()), Times.Never);
        _dispositivoIotRepository.Verify(r => r.Update(It.IsAny<DispositivoIot>()), Times.Never);
    }

    [Fact]
    public void Create_ComModelStateInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("FrequenciaCardiaca", "A frequência cardíaca deve estar entre 1 e 400 bpm");

        // Act
        var result = _controller.Create(Guid.NewGuid(), CriarRequestValido());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _leituraIotRepository.Verify(r => r.Add(It.IsAny<LeituraIotDocument>()), Times.Never);
    }

    [Fact]
    public void GetByDispositivo_ComDispositivoExistente_DeveRetornarOkComHistorico()
    {
        // Arrange
        var dispositivoId = Guid.NewGuid();
        _dispositivoIotRepository.Setup(r => r.ExistsById(dispositivoId)).Returns(true);
        _leituraIotRepository
            .Setup(r => r.GetByDispositivoId(dispositivoId, LeituraIotController.LimitePadrao))
            .Returns(new List<LeituraIotDocument> { CriarDocumento(dispositivoId), CriarDocumento(dispositivoId) });

        // Act
        var result = _controller.GetByDispositivo(dispositivoId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var leituras = Assert.IsAssignableFrom<IReadOnlyList<LeituraIotResponse>>(ok.Value);
        Assert.Equal(2, leituras.Count);
    }

    [Fact]
    public void GetByDispositivo_QuandoDispositivoNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _dispositivoIotRepository.Setup(r => r.ExistsById(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.GetByDispositivo(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(LeituraIotController.LimiteMaximo + 1)]
    public void GetByDispositivo_ComLimiteForaDoIntervalo_DeveRetornarBadRequest(int limite)
    {
        // Act
        var result = _controller.GetByDispositivo(Guid.NewGuid(), limite);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _leituraIotRepository.Verify(r => r.GetByDispositivoId(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetById_QuandoLeituraPertenceAOutroDispositivo_DeveRetornarNotFound()
    {
        // Arrange
        var leitura = CriarDocumento(Guid.NewGuid());
        _leituraIotRepository.Setup(r => r.GetById(leitura.Id)).Returns(leitura);

        // Act
        var result = _controller.GetById(Guid.NewGuid(), leitura.Id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetById_QuandoLeituraExiste_DeveRetornarOk()
    {
        // Arrange
        var dispositivoId = Guid.NewGuid();
        var leitura = CriarDocumento(dispositivoId);
        _leituraIotRepository.Setup(r => r.GetById(leitura.Id)).Returns(leitura);

        // Act
        var result = _controller.GetById(dispositivoId, leitura.Id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LeituraIotResponse>(ok.Value);
        Assert.Equal(leitura.Id, response.Id);
    }

    [Fact]
    public void Delete_QuandoLeituraExiste_DeveRemoverERetornarNoContent()
    {
        // Arrange
        var dispositivoId = Guid.NewGuid();
        var leitura = CriarDocumento(dispositivoId);
        _leituraIotRepository.Setup(r => r.GetById(leitura.Id)).Returns(leitura);
        _leituraIotRepository.Setup(r => r.Delete(leitura.Id)).Returns(true);

        // Act
        var result = _controller.Delete(dispositivoId, leitura.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _leituraIotRepository.Verify(r => r.Delete(leitura.Id), Times.Once);
    }

    [Fact]
    public void Delete_QuandoLeituraNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _leituraIotRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((LeituraIotDocument?)null);

        // Act
        var result = _controller.Delete(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _leituraIotRepository.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
    }

    private static DispositivoIot CriarDispositivo(StatusDispositivoEnum status)
    {
        var dispositivo = new DispositivoIot(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today),
            5,
            80,
            20m,
            120m,
            DateTime.UtcNow.AddHours(-1),
            status);

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

    private static LeituraIotDocument CriarDocumento(Guid dispositivoId) =>
        new()
        {
            Id = Guid.NewGuid(),
            DispositivoId = dispositivoId,
            PetId = Guid.NewGuid(),
            FrequenciaCardiaca = 90,
            DataLeitura = DateTime.UtcNow,
            RegistradoEm = DateTime.UtcNow
        };
}
