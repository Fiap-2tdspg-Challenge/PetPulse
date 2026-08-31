using Microsoft.AspNetCore.Mvc;
using Moq;
using PetPulse.Application.DTOs;
using PetPulse.Application.Services;
using PetPulse.Controllers;
using PetPulse.Domain.entites;
using PetPulse.Domain.Enum;

namespace PetPulse.API.Tests.Controllers;

public class DispositivoIotControllerTests
{
    private readonly Mock<IDispositivoIotRepository> _dispositivoRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly DispositivoIotController _controller;

    public DispositivoIotControllerTests()
    {
        _controller = new DispositivoIotController(_dispositivoRepository.Object, _petRepository.Object);
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
        _dispositivoRepository.Verify(r => r.Add(It.IsAny<DispositivoIot>()), Times.Never);
    }

    [Fact]
    public void Create_QuandoPetJaPossuiDispositivo_DeveRetornarBadRequestENaoPersistir()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _dispositivoRepository.Setup(r => r.ExistsByPetId(petId)).Returns(true);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _dispositivoRepository.Verify(r => r.Add(It.IsAny<DispositivoIot>()), Times.Never);
    }

    [Fact]
    public void Create_ComDadosValidos_DeveRetornarCreatedEChamarAdd()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var request = CriarRequestValido(petId);
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _dispositivoRepository.Setup(r => r.ExistsByPetId(petId)).Returns(false);
        _dispositivoRepository.Setup(r => r.Add(It.IsAny<DispositivoIot>())).Returns((DispositivoIot d) => d);

        // Act
        var result = _controller.Create(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        _dispositivoRepository.Verify(r => r.Add(It.IsAny<DispositivoIot>()), Times.Once);
    }

    [Fact]
    public void GetByPetId_QuandoPetNaoExiste_DeveRetornarNotFoundENaoConsultarDispositivo()
    {
        // Arrange
        _petRepository.Setup(r => r.ExistsById(It.IsAny<Guid>())).Returns(false);

        // Act
        var result = _controller.GetByPetId(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _dispositivoRepository.Verify(r => r.GetByPetId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void GetByPetId_QuandoPetExisteMasSemDispositivo_DeveRetornarNotFound()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _petRepository.Setup(r => r.ExistsById(petId)).Returns(true);
        _dispositivoRepository.Setup(r => r.GetByPetId(petId)).Returns((DispositivoIot?)null);

        // Act
        var result = _controller.GetByPetId(petId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static DispositivoIotRequest CriarRequestValido(Guid petId) =>
        new(
            petId,
            new DateOnly(2026, 5, 5),
            30,
            95,
            72.5m,
            12.8m,
            DateTime.UtcNow,
            StatusDispositivoEnum.Ativo);
}
