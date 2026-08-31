using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Controllers;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.API.Tests.Controllers;

public class HistoricoClinicoControllerTests
{
    private readonly Mock<IHistoricoClinicoRepository> _historicoClinicoRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly HistoricoClinicoController _controller;

    public HistoricoClinicoControllerTests()
    {
        _controller = new HistoricoClinicoController(_historicoClinicoRepository.Object, _petRepository.Object);
    }

    [Fact]
    public void GetAll_DeveRetornarOkComListaDeHistoricos()
    {
        // Arrange
        var historicos = new List<HistoricoClinico> { CriarHistoricoValido() };
        _historicoClinicoRepository.Setup(r => r.GetAll()).Returns(historicos);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IReadOnlyList<HistoricoClinicoResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public void GetById_QuandoHistoricoNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _historicoClinicoRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((HistoricoClinico?)null);

        // Act
        var result = _controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetByPetId_QuandoPetNaoExiste_DeveRetornarNotFoundENaoConsultarHistoricos()
    {
        // Arrange
        _petRepository.Setup(r => r.ExistsById(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.GetByPetId(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _historicoClinicoRepository.Verify(r => r.GetByPetId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void Create_QuandoPetNaoExiste_DeveRetornarNotFoundENaoPersistir()
    {
        // Arrange
        var request = CriarRequestValido(Guid.NewGuid());
        _petRepository.Setup(r => r.ExistsById(request.PetId)).Returns(false);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _historicoClinicoRepository.Verify(r => r.Add(It.IsAny<HistoricoClinico>()), Times.Never);
    }

    [Fact]
    public void Create_ComDadosValidos_DeveRetornarCreatedEChamarAdd()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _historicoClinicoRepository
            .Setup(r => r.Add(It.IsAny<HistoricoClinico>()))
            .Returns((HistoricoClinico h) => h);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        _historicoClinicoRepository.Verify(r => r.Add(It.IsAny<HistoricoClinico>()), Times.Once);
    }

    [Fact]
    public void Update_QuandoPetNaoExiste_DeveRetornarNotFoundENaoAtualizar()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(false);

        // Act
        var result = _controller.Update(Guid.NewGuid(), request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _historicoClinicoRepository.Verify(r => r.Update(It.IsAny<HistoricoClinico>()), Times.Never);
    }

    [Fact]
    public void Update_QuandoHistoricoNaoExiste_DeveRetornarNotFoundENaoAtualizar()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _historicoClinicoRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((HistoricoClinico?)null);

        // Act
        var result = _controller.Update(Guid.NewGuid(), request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _historicoClinicoRepository.Verify(r => r.Update(It.IsAny<HistoricoClinico>()), Times.Never);
    }

    [Fact]
    public void Update_ComDadosValidos_DeveRetornarOkEChamarUpdate()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        var historicoExistente = CriarHistoricoValido(petId);

        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _historicoClinicoRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns(historicoExistente);

        // Act
        var result = _controller.Update(historicoExistente.Id, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _historicoClinicoRepository.Verify(r => r.Update(It.IsAny<HistoricoClinico>()), Times.Once);
    }

    [Fact]
    public void Delete_QuandoHistoricoNaoExiste_DeveRetornarNotFound()
    {
        // Arrange
        _historicoClinicoRepository.Setup(r => r.Delete(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Delete_QuandoHistoricoExiste_DeveRetornarNoContent()
    {
        // Arrange
        _historicoClinicoRepository.Setup(r => r.Delete(It.IsAny<Guid>())).Returns(true);

        // Act
        var result = _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    private static HistoricoClinicoRequest CriarRequestValido(Guid petId) =>
        new(
            petId,
            TipoRegistroClinicoEnum.Consulta,
            "Consulta de rotina",
            new DateOnly(2026, 1, 1),
            null,
            "Dr. João",
            "Sem observações");

    private static HistoricoClinico CriarHistoricoValido(Guid? petId = null) =>
        new(
            petId ?? Guid.NewGuid(),
            TipoRegistroClinicoEnum.Consulta,
            "Consulta de rotina",
            new DateOnly(2026, 1, 1),
            null,
            "Dr. João",
            "Sem observações");
}
